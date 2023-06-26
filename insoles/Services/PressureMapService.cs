using insoles.Enums;
using System.Collections.Generic;
using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Data.Text;
using insoles.Utilities;
using insoles.DataHolders;
using insoles.UserControls;
using System.Linq;
using System.Threading.Tasks;
using Emgu.CV.Ocl;
using System.Windows.Resources;
using System.Windows;
using System.IO;
using insoles.States;

namespace insoles.Services
{
    public class PressureMapService : IPressureMapService
    {
        private delegate void ActionRef<T1, T2, T3>(T1 arg1, ref T2 arg2, ref T3 arg3);
        private Matrix<float> sensor_map;
        private ICodesService codes;
        private Dictionary<Sensor, Matrix<float>> inverse_distances;
        private Matrix<float> inverse_distances_background;
        private Dictionary<UserControls.Metric, Matrix<float>> pressureMaps = new();
        private List<Matrix<float>> pressureMapsLive;

        const int BACKGROUND = -1;
        public int N_FRAMES { get; } = 10;
        const int MIN_VALUE = 1;

        private bool isInitialized = false;

        public PressureMapService(AnalisisState state,
            Matrix<float> sensor_map, ICodesService codes,
            Dictionary<Sensor, List<Tuple<int, int>>> sensor_positions_left,
            Dictionary<Sensor, List<Tuple<int, int>>> sensor_positions_right)
        {
            Task.Run(() =>
            {
                inverse_distances = CalculateMinDistances(sensor_map, codes,
                    sensor_positions_left, sensor_positions_right);                
                try
                {
                    Uri uri = new Uri("pack://application:,,,/Precalculus/inverse_distances_background.mtx");
                    StreamResourceInfo sri = Application.GetResourceStream(uri);
                    Stream stream = sri.Stream;
                    inverse_distances_background = MatrixMarketReader.ReadMatrix<float>(stream);
                }
                catch (IOException)
                {
                    MessageBox.Show("No se ha encontrado el fichero de la matrix\nSe va a proceder a recalcularla", "inverse_distances_background.mtx not found", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.Yes);
                    inverse_distances_background = CalculateMinDistancesBackground(sensor_map, codes);
                    //MatrixMarketWriter.WriteMatrix("C:\\Users\\" + Environment.UserName + "\\Documents" + "\\inverse_distances_background.mtx", inverse_distances_background);
                }
                isInitialized = true;
            });
        }
        private Dictionary<Sensor, Matrix<float>> CalculateMinDistances(Matrix<float> sensor_map,
            ICodesService codes,
            Dictionary<Sensor, List<Tuple<int, int>>> sensor_positions_left,
            Dictionary<Sensor, List<Tuple<int, int>>> sensor_positions_right)
        {
            this.sensor_map = sensor_map;
            this.codes = codes;
            Dictionary<Sensor, Matrix<float>> inverse_distances = new Dictionary<Sensor, Matrix<float>>();
            foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
            {
                inverse_distances[sensor] = sensor_map.MapIndexed((row, col, code) =>
                {
                    if (code != codes.Background())
                    {
                        if (row < sensor_map.RowCount / 2)
                        {
                            int min_distance = sensor_map.RowCount * sensor_map.RowCount + sensor_map.ColumnCount * sensor_map.ColumnCount;
                            foreach (Tuple<int, int> point in sensor_positions_left[sensor])
                            {
                                int distance = HelperFunctions.SquareDistance(row, col, point.Item1, point.Item2);
                                if (distance < min_distance)
                                {
                                    min_distance = distance;
                                }
                            }
                            if(min_distance < MIN_VALUE)
                                min_distance = MIN_VALUE;
                            return 1.0f / min_distance;
                        }
                        else
                        {
                            int min_distance = sensor_map.RowCount * sensor_map.RowCount + sensor_map.ColumnCount * sensor_map.ColumnCount;
                            foreach (Tuple<int, int> point in sensor_positions_right[sensor])
                            {
                                int distance = HelperFunctions.SquareDistance(row, col, point.Item1, point.Item2);
                                if (distance < min_distance)
                                {
                                    min_distance = distance;
                                }
                            }
                            if (min_distance < MIN_VALUE)
                                min_distance = MIN_VALUE;
                            return 1.0f / min_distance;
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
            Matrix<float> CalculateOne(GraphData graphData, ActionRef<GraphData, DataInsole, DataInsole> func, UserControls.Metric metric)
            {
                int reduceFunc(List<int> pressures)
                {
                    return pressures.Sum() / pressures.Count;
                }
                DataInsole leftInsole = new DataInsole();
                DataInsole rightInsole = new DataInsole();
                func(graphData, ref leftInsole, ref rightInsole);
                Dictionary<Sensor, double> pressuresLeft = leftInsole.pressures;
                Dictionary<Sensor, double> pressuresRight = rightInsole.pressures;

                return CalculateFromPoint(sensor_map, codes,
                    pressuresLeft, pressuresRight, inverse_distances,
                    inverse_distances_background);
            }
            while (!isInitialized)
            {
                await Task.Delay(1000);
            }
            Dictionary<UserControls.Metric, Matrix<float>> pressure_maps = new();
            pressure_maps[UserControls.Metric.Avg] = CalculateOne(graphData, average, UserControls.Metric.Avg);
            pressure_maps[UserControls.Metric.Max] = CalculateOne(graphData, max, UserControls.Metric.Max);
            pressure_maps[UserControls.Metric.Min] = CalculateOne(graphData, min, UserControls.Metric.Min);
            return pressure_maps;
        }
        public async Task<List<Matrix<float>>> CalculateLive(GraphData graphData)
        {
            Matrix<float> CalculateOne(DataInsole leftInsole, DataInsole rightInsole)
            {
                Dictionary<Sensor, double> pressuresLeft = leftInsole.pressures;
                Dictionary<Sensor, double> pressuresRight = rightInsole.pressures;

                return CalculateFromPoint(sensor_map, codes,
                    pressuresLeft, pressuresRight, inverse_distances, inverse_distances_background);
            }
            while (!isInitialized)
            {
                await Task.Delay(1000);
            }
            List<Matrix<float>> pressureMapsLive = new();
            for (int i = 0; i < graphData.length; i += N_FRAMES)
            {
                DataInsole left = new();
                DataInsole right = new();
                for (int j = i; j < Math.Min(i + N_FRAMES, graphData.length); j++)
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
                    left[sensor] /= Math.Min(N_FRAMES, graphData.length - i);
                    right[sensor] /= Math.Min(N_FRAMES, graphData.length - i);
                }
                pressureMapsLive.Add(CalculateOne(left, right));
            }
            return pressureMapsLive;
        }
        private Matrix<float> CalculateFromPoint(Matrix<float> sensor_map, ICodesService codes,
            Dictionary<Sensor, double> left, Dictionary<Sensor, double> right,
            Dictionary<Sensor, Matrix<float>> inverse_distances,
            Matrix<float> inverse_distances_background)
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
                    foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
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
