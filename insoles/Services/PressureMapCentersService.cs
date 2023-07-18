using insoles.DataHolders;
using insoles.Enums;
using insoles.Utilities;
using MathNet.Numerics.Data.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Resources;
using System.Windows;
using MathNet.Numerics.LinearAlgebra;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Controls;
using insoles.States;
using System.Threading;

namespace insoles.Services
{
    [Serializable]
    public enum SensorHeelReduced : int
    {
        HALLUX,
        TOES,
        MET1,
        MET3,
        MET5,
        ARCH,
        HEEL
    }
    public class PressureMapCentersService : IPressureMapService
    {
        private delegate void ActionRef<T1, T2, T3>(T1 arg1, ref T2 arg2, ref T3 arg3);
        private Matrix<float> sensor_map;
        private ICodesService codes;
        private Dictionary<SensorHeelReduced, Matrix<float>> inverse_reduced_distances;
        private Matrix<float> inverse_distances_background;

        const int BACKGROUND = -1;
        public int N_FRAMES { get; } = 10;

        private bool isInitialized = false;

        private AnalisisState state;
        public PressureMapCentersService(AnalisisState state, Matrix<float> sensor_map, ICodesService codes,
            Dictionary<Sensor, List<Tuple<int, int>>> sensor_positions_left,
            Dictionary<Sensor, List<Tuple<int, int>>> sensor_positions_right)
        {
            this.state = state;
            this.sensor_map = sensor_map;
            this.codes = codes;
            Task.Run(() =>
            {
                (float, float) reduceSensorsFunc(List<(float, float)> centers)
                {
                    float sum1 = 0;
                    float sum2 = 0;
                    foreach (var c in centers)
                    {
                        sum1 += c.Item1;
                        sum2 += c.Item2;
                    }
                    return (sum1 / centers.Count, sum2 / centers.Count);
                }
                try
                {
                    inverse_reduced_distances = LoadMatrixDictionary();
                }
                catch (IOException) {
                    MessageBox.Show("No se ha encontrado el fichero de la matrix\nSe va a proceder a recalcularla", "inverse_distances_from_reduced.mtx not found", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.Yes);
                    Dictionary<Sensor, (float, float)> centersLeft;
                    Dictionary<Sensor, (float, float)> centersRight;
                    CalculateCenters(sensor_positions_left, sensor_positions_right,
                        out centersLeft, out centersRight);
                    Dictionary<SensorHeelReduced, (float, float)> centersLeftReduced =
                    ReduceSensorsHeel(centersLeft, reduceSensorsFunc);
                    Dictionary<SensorHeelReduced, (float, float)> centersRightReduced =
                    ReduceSensorsHeel(centersRight, reduceSensorsFunc);
                    inverse_reduced_distances = CalculateMinDistances(sensor_map, codes,
                        centersLeftReduced, centersRightReduced);
                    SaveMatrixDictionary(inverse_reduced_distances, "C:\\Users\\" + Environment.UserName + "\\Documents" + "\\inverse_distances_from_reduced.mtx");
                }
                try
                {
                    Uri uri = new Uri("pack://application:,,,/Precalculus/inverse_distances_background_from_reduced.mtx");
                    StreamResourceInfo sri = Application.GetResourceStream(uri);
                    Stream stream = sri.Stream;
                    inverse_distances_background = MatrixMarketReader.ReadMatrix<float>(stream);
                }
                catch (IOException)
                {
                    MessageBox.Show("No se ha encontrado el fichero de la matrix\nSe va a proceder a recalcularla", "inverse_distances_background.mtx not found", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.Yes);
                    inverse_distances_background = CalculateMinDistancesBackground(sensor_map, codes);
                    MatrixMarketWriter.WriteMatrix("C:\\Users\\" + Environment.UserName + "\\Documents" + "\\inverse_distances_background_from_reduced.mtx", inverse_distances_background);
                }
                isInitialized = true;
            });
        }
        private void SaveMatrixDictionary(Dictionary<SensorHeelReduced, Matrix<float>> dictionary, string filePath)
        {
            using (FileStream stream = File.OpenWrite(filePath))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, dictionary);
            }
        }
        private Dictionary<SensorHeelReduced, Matrix<float>> LoadMatrixDictionary()
        {
            Dictionary<SensorHeelReduced, Matrix<float>> dictionary;

            Uri uri = new Uri("pack://application:,,,/Precalculus/inverse_distances_reduced_from_reduced.mtx");
            StreamResourceInfo sri = Application.GetResourceStream(uri);

            using (Stream stream = sri.Stream)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                dictionary = (Dictionary<SensorHeelReduced, Matrix<float>>)formatter.Deserialize(stream);
            }

            return dictionary;
        }
        private Dictionary<SensorHeelReduced, T> ReduceSensorsHeel<T>(Dictionary<Sensor, T> centers, Func<List<T>, T> transformFuncion)
        {
            Dictionary<SensorHeelReduced, T> centersReduced = new Dictionary<SensorHeelReduced, T>();
            centersReduced[SensorHeelReduced.HALLUX] = centers[Sensor.Hallux];
            centersReduced[SensorHeelReduced.TOES] = centers[Sensor.Toes];

            centersReduced[SensorHeelReduced.MET1] = centers[Sensor.Met1];
            centersReduced[SensorHeelReduced.MET3] = centers[Sensor.Met3];
            centersReduced[SensorHeelReduced.MET5] = centers[Sensor.Met5];

            centersReduced[SensorHeelReduced.ARCH] = centers[Sensor.Arch];

            List<T> heels = new List<T>
            {
                centers[Sensor.HeelL],
                centers[Sensor.HeelR]
            };
            centersReduced[SensorHeelReduced.HEEL] = transformFuncion(heels);

            return centersReduced;
        }
        private void CalculateCenters(Dictionary<Sensor, List<Tuple<int, int>>> sensor_positions_left,
            Dictionary<Sensor, List<Tuple<int, int>>> sensor_positions_right,
            out Dictionary<Sensor, (float, float)> centersLeft, 
            out Dictionary<Sensor, (float, float)> centersRight)
        {
            (float, float) CalculateCenter(List<Tuple<int, int>> sensor_positions)
            {
                int rowSum = 0;
                int colSum = 0;
                foreach (Tuple<int, int> position in sensor_positions)
                {
                    rowSum += position.Item1;
                    colSum += position.Item2;
                }
                float centerRow = (float)rowSum / sensor_positions.Count;
                float centerCol = (float)colSum / sensor_positions.Count;
                return (centerRow, centerCol);
            }
            centersLeft = new();
            centersRight = new();
            foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
            {
                centersLeft[sensor] = CalculateCenter(sensor_positions_left[sensor]);
                centersRight[sensor] = CalculateCenter(sensor_positions_right[sensor]);
            }
        }
        private Dictionary<T, Matrix<float>> CalculateMinDistances<T>(Matrix<float> sensor_map,
            ICodesService codes,
            Dictionary<T, (float, float)> centersLeft,
            Dictionary<T, (float, float)> centersRight) where T : Enum
        {
            Dictionary<T, Matrix<float>> inverse_distances = new Dictionary<T, Matrix<float>>();
            foreach (T sensor in (T[])Enum.GetValues(typeof(T)))
            {
                inverse_distances[sensor] = sensor_map.MapIndexed((row, col, code) =>
                {
                    if (code != codes.Background())
                    {
                        if (row < sensor_map.RowCount / 2)
                        {
                            (float, float) point = centersLeft[sensor];
                            float distance = HelperFunctions.SquareDistance(row, col, point.Item1, point.Item2);
                            return 1.0f / distance;
                        }
                        else
                        {
                            (float, float) point = centersRight[sensor];
                            float distance = HelperFunctions.SquareDistance(row, col, point.Item1, point.Item2);
                            return 1.0f / distance;
                        }
                    }
                    else
                    {
                        return 0.0f;
                    }
                });
            }
            return inverse_distances;
        }
        private Matrix<float> CalculateMinDistancesBackground(Matrix<float> sensor_map, ICodesService codes)
        {
            List<(int, int, float)> backgroundPoints = MathNetHelpers.FindAll(sensor_map, (value) => value == codes.Background());
            // Añadir los bordes de la imagen
            for (int i = 0; i < sensor_map.RowCount; i++)
            {
                backgroundPoints.Add((i, -1, codes.Background()));
                backgroundPoints.Add((i, sensor_map.ColumnCount, codes.Background()));
            }
            for (int i = 0; i < sensor_map.ColumnCount; i++)
            {
                backgroundPoints.Add((-1, i, codes.Background()));
                backgroundPoints.Add((sensor_map.RowCount, i, codes.Background()));
            }
            Matrix<float> inverse_distances = sensor_map.MapIndexed((row, col, code) =>
            {
                if (code == codes.Background())
                {
                    return 0f;
                }
                else
                {
                    int min_distance = int.MaxValue;
                    foreach ((int, int, float) point in backgroundPoints)
                    {
                        int distance = HelperFunctions.SquareDistance(row, col, point.Item1, point.Item2);
                        if (distance < min_distance)
                        {
                            min_distance = distance;
                        }
                    }
                    return 1.0f / min_distance;
                }
            });
            return inverse_distances;
        }
        public async Task<Dictionary<UserControls.Metric, Matrix<float>>> CalculateMetrics(GraphData graphData)
        {
            Task<Matrix<float>> CalculateOne(GraphData graphData, ActionRef<GraphData, DataInsole, DataInsole> func, UserControls.Metric metric)
            {
                double reduceFunc(List<double> pressures)
                {
                    return pressures.Sum() / pressures.Count;
                }
                DataInsole leftInsole = new DataInsole();
                DataInsole rightInsole = new DataInsole();
                func(graphData, ref leftInsole, ref rightInsole);
                Dictionary<Sensor, double> pressuresLeft = leftInsole.pressures;
                Dictionary<Sensor, double> pressuresRight = rightInsole.pressures;
                Dictionary<SensorHeelReduced, double> pressuresLeftReduced = ReduceSensorsHeel(pressuresLeft, reduceFunc);
                Dictionary<SensorHeelReduced, double> pressuresRightReduced = ReduceSensorsHeel(pressuresRight, reduceFunc);


                return CalculateFromPoint(sensor_map, codes,
                    pressuresLeftReduced, pressuresRightReduced, inverse_reduced_distances,
                    inverse_distances_background);
            }
            while (!isInitialized)
            {
                await Task.Delay(1000);
            }
            Dictionary<UserControls.Metric, Matrix<float>> pressure_maps = new();
            pressure_maps[UserControls.Metric.Avg] = await CalculateOne(graphData, average, UserControls.Metric.Avg);
            pressure_maps[UserControls.Metric.Max] = await CalculateOne(graphData, max, UserControls.Metric.Max);
            pressure_maps[UserControls.Metric.Min] = await CalculateOne(graphData, min, UserControls.Metric.Min);
            return pressure_maps;
        }
        public async Task<List<Matrix<float>>> CalculateLive(GraphData graphData)
        {
            Task<Matrix<float>> CalculateOne(DataInsole leftInsole, DataInsole rightInsole)
            {
                double reduceFunc(List<double> pressures)
                {
                    return pressures.Sum() / pressures.Count;
                }
                Dictionary<Sensor, double> pressuresLeft = leftInsole.pressures;
                Dictionary<Sensor, double> pressuresRight = rightInsole.pressures;
                Dictionary<SensorHeelReduced, double> pressuresLeftReduced = ReduceSensorsHeel(pressuresLeft, reduceFunc);
                Dictionary<SensorHeelReduced, double> pressuresRightReduced = ReduceSensorsHeel(pressuresRight, reduceFunc);

                return CalculateFromPoint(sensor_map, codes,
                    pressuresLeftReduced, pressuresRightReduced, 
                    inverse_reduced_distances, inverse_distances_background);
            }
            while (!isInitialized)
            {
                await Task.Delay(1000);
            }
            List<Matrix<float>> pressureMapsLive = new();
            for (int i = 0; i < graphData.length; i += state.framesTaken)
            {
                DataInsole left = new();
                DataInsole right = new();
                // AVG
                /*
                for (int j = i; j < Math.Min(i + state.framesTaken, graphData.length); j++)
                {
                    FrameDataInsoles frameData = (FrameDataInsoles)graphData[j];
                    foreach (Sensor sensor in Enum.GetValues(typeof(Sensor)))
                    {
                        left[sensor] += frameData.left[sensor];
                        right[sensor] += frameData.right[sensor];
                    }
                }
                foreach (Sensor sensor in Enum.GetValues(typeof(Sensor)))
                {
                    left[sensor] /= Math.Min(state.framesTaken, graphData.length - i);
                    right[sensor] /= Math.Min(state.framesTaken, graphData.length - i);
                }
                */
                //Max
                for (int j = i; j < Math.Min(i + state.framesTaken, graphData.length); j++)
                {
                    FrameDataInsoles frameData = (FrameDataInsoles)graphData[j];
                    foreach (Sensor sensor in Enum.GetValues(typeof(Sensor)))
                    {
                        if (frameData.left[sensor] > left[sensor])
                        {
                            left[sensor] = frameData.left[sensor];
                        }
                        if (frameData.right[sensor] > right[sensor])
                        {
                            right[sensor] = frameData.right[sensor];
                        }
                    }
                }
                pressureMapsLive.Add(CalculateOne(left, right).Result);
            }
            return pressureMapsLive;
        }
        private async Task<Matrix<float>> CalculateFromPoint<T>(Matrix<float> sensor_map, ICodesService codes,
            Dictionary<T, double> left, Dictionary<T, double> right,
            Dictionary<T, Matrix<float>> inverse_distances,
            Matrix<float> inverse_distances_background) where T : Enum
        {
            Matrix<float> pressure_map = sensor_map.MapIndexed((row, col, code) => {
                if (code == codes.Background())
                {
                    return BACKGROUND;
                }
                else
                {
                    float numerator = 0.0f;
                    float denominator = 0.0f;
                    foreach (T sensor in (T[])Enum.GetValues(typeof(T)))
                    {
                        float inverse_distance = inverse_distances[sensor][row, col];
                        if (row < sensor_map.RowCount / 2)
                            numerator += (float)left[sensor] * inverse_distance;
                        else
                            numerator += (float)right[sensor] * inverse_distance;
                        denominator += inverse_distance;
                    }
                    denominator += inverse_distances_background[row, col];
                    return numerator / denominator;
                }
            });

            return pressure_map;
        }
        private void average(GraphData data, ref DataInsole left, ref DataInsole right)
        {
            for (int i = 0; i < data.length; i++)
            {
                FrameDataInsoles frameData = (FrameDataInsoles)data[i];
                foreach (Sensor sensor in Enum.GetValues(typeof(Sensor)))
                {
                    left[sensor] += frameData.left[sensor];
                    right[sensor] += frameData.right[sensor];
                }
            }
            foreach (Sensor sensor in Enum.GetValues(typeof(Sensor)))
            {
                left[sensor] /= data.length;
                right[sensor] /= data.length;
            }
        }
        private void max(GraphData data, ref DataInsole left, ref DataInsole right)
        {
            foreach (Sensor sensor in Enum.GetValues(typeof(Sensor)))
            {
                left[sensor] = int.MinValue;
                right[sensor] = int.MinValue;
            }
            for (int i = 0; i < data.length; i++)
            {
                FrameDataInsoles frameData = (FrameDataInsoles)data[i];
                foreach (Sensor sensor in Enum.GetValues(typeof(Sensor)))
                {
                    if (frameData.left[sensor] > left[sensor])
                    {
                        left[sensor] = frameData.left[sensor];
                    }
                    if (frameData.right[sensor] > right[sensor])
                    {
                        right[sensor] = frameData.right[sensor];
                    }
                }
            }
        }
        private void min(GraphData data, ref DataInsole left, ref DataInsole right)
        {
            foreach (Sensor sensor in Enum.GetValues(typeof(Sensor)))
            {
                left[sensor] = int.MaxValue;
                right[sensor] = int.MaxValue;
            }
            for (int i = 0; i < data.length; i++)
            {
                FrameDataInsoles frameData = (FrameDataInsoles)data[i];
                foreach (Sensor sensor in Enum.GetValues(typeof(Sensor)))
                {
                    if (frameData.left[sensor] < left[sensor])
                    {
                        left[sensor] = frameData.left[sensor];
                    }
                    if (frameData.right[sensor] < right[sensor])
                    {
                        right[sensor] = frameData.right[sensor];
                    }
                }
            }
        }
    }
}
