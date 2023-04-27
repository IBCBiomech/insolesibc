#define CENTER_SENSORS
#define BAKGROUND_DISTANCES //Comentar esto para no usar las distancias al borde
#define AVERAGE

using System;
using System.Drawing;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using insoles.Common;
using System.Windows;
using System.Windows.Markup;
using System.Threading.Tasks;
using DirectShowLib;
using System.Diagnostics;
using System.Linq;
using MathNet.Numerics.Data.Text;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using System.IO;
using System.Windows.Resources;
using MathNet.Numerics.Interpolation;
using System.Windows.Navigation;

namespace insoles.Graphs
{
    public enum SensorReduced
    {
        HALLUX,
        TOES,
        MET,
        ARCH,
        HEEL
    }
    public enum SensorHeelReduced
    {
        HALLUX,
        TOES,
        MET1,
        MET3,
        MET5,
        ARCH,
        HEEL
    }
    public class PressureMap
    {
        delegate void ActionRef<T1, T2, T3>(T1 arg1, ref T2 arg2, ref T3 arg3);

        private Dictionary<Sensor, Matrix<float>> inverse_distances = new Dictionary<Sensor, Matrix<float>>();
        private Dictionary<SensorHeelReduced, Matrix<float>> inverse_reduced_distances = new Dictionary<SensorHeelReduced, Matrix<float>>();
        private Matrix<float> inverse_distances_background;
        //private GraphPressureMap graph;
        private GraphPressureHeatmap graph;
        private Foot foot;

        private List<Matrix<float>> pressureMaps = new();

        private bool isInitialized = false;
        public event EventHandler initialized;

        private Metric metric;
        private bool dirty = true;

        private const int N_FRAMES = Config.N_FRAMES_HEATMAP;
        private int lastFrame = -1;

        private TimeLine.TimeLine timeLine;

        private GraphData graphData;

        public Dictionary<Sensor, (float, float)> centersLeft { get; private set; } = new Dictionary<Sensor, (float, float)>();
        public Dictionary<Sensor, (float, float)> centersRight { get; private set; } = new Dictionary<Sensor, (float, float)>();

        private Dictionary<SensorHeelReduced, (float, float)> centersLeftReduced = new Dictionary<SensorHeelReduced, (float, float)>();
        private Dictionary<SensorHeelReduced, (float, float)> centersRightReduced = new Dictionary<SensorHeelReduced, (float, float)>();
        public PressureMap()
        {
            void initFunc()
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
#if CENTER_SENSORS
                CalculateCenters();
                centersLeftReduced = ReduceSensorsHeel(centersLeft, reduceSensorsFunc);
                centersRightReduced = ReduceSensorsHeel(centersRight, reduceSensorsFunc);
                inverse_reduced_distances = CalculateMinDistances(centersLeftReduced, centersRightReduced);
#else
                Dictionary<Sensor, List<Tuple<int, int>>> sensor_positions_left = foot.CalculateSensorPositionsLeft();
                Dictionary<Sensor, List<Tuple<int, int>>> sensor_positions_right = foot.CalculateSensorPositionsRight();
                inverse_distances = CalculateMinDistances(sensor_positions_left, sensor_positions_right);
#endif
#if BAKGROUND_DISTANCES
                try
                {
                    Uri uri = new Uri("pack://application:,,,/Assets/inverse_distances_background.mtx");
                    StreamResourceInfo sri = Application.GetResourceStream(uri);
                    Stream stream = sri.Stream;
                    inverse_distances_background = MatrixMarketReader.ReadMatrix<float>(stream);
                }
                catch (System.IO.IOException)
                {
                    MessageBox.Show("No se ha encontrado el fichero de la matrix\nSe va a proceder a recalcularla", "inverse_distances_background.mtx not found", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.Yes);
                    inverse_distances_background = CalculateMinDistancesBackground();
                    MatrixMarketWriter.WriteMatrix(Config.INITIAL_PATH + "\\inverse_distances_background.mtx", inverse_distances_background);
                }
#endif
                isInitialized = true;
                initialized?.Invoke(this, EventArgs.Empty);
            }
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow.graphPressures.Content == null)
            {
                mainWindow.graphPressures.Navigated += (s, e) =>
                {
                    graph = mainWindow.graphPressures.Content as GraphPressureHeatmap;
                    
                };
            }
            else
            {
                graph = mainWindow.graphPressures.Content as GraphPressureHeatmap;
                
            }
            if (mainWindow.timeLine.Content == null)
            {
                mainWindow.timeLine.Navigated += delegate (object sender, NavigationEventArgs e)
                {
                    timeLine = mainWindow.timeLine.Content as TimeLine.TimeLine;
                };
            }
            else
            {
                timeLine = mainWindow.timeLine.Content as TimeLine.TimeLine;
            }
            if (mainWindow.foot == null)
            {
                mainWindow.initialized += (s, e) =>
                {
                    foot = mainWindow.foot;
                    Task.Run(() => {
                        initFunc();
                    });
                };
            }
            else
            {
                foot = mainWindow.foot;
                Task.Run(() => {
                    initFunc();
                });
            }
        }
        private Dictionary<SensorReduced, T> ReduceSensors<T>(Dictionary<Sensor, T> centers, Func<List<T>, T> transformFuncion)
        {
            Dictionary<SensorReduced, T> centersReduced = new Dictionary<SensorReduced, T>();
            centersReduced[SensorReduced.HALLUX] = centers[Sensor.HALLUX];
            centersReduced[SensorReduced.TOES] = centers[Sensor.TOES];

            List<T> mets = new List<T>();
            mets.Add(centers[Sensor.MET1]);
            mets.Add(centers[Sensor.MET3]);
            mets.Add(centers[Sensor.MET5]);
            centersReduced[SensorReduced.MET] = transformFuncion(mets);

            centersReduced[SensorReduced.ARCH] = centers[Sensor.ARCH];

            List<T> heels = new List<T>();
            heels.Add(centers[Sensor.HEEL_L]);
            heels.Add(centers[Sensor.HEEL_R]);
            centersReduced[SensorReduced.HEEL] = transformFuncion(heels);

            return centersReduced;
        }
        private Dictionary<SensorHeelReduced, T> ReduceSensorsHeel<T>(Dictionary<Sensor, T> centers, Func<List<T>, T> transformFuncion)
        {
            Dictionary<SensorHeelReduced, T> centersReduced = new Dictionary<SensorHeelReduced, T>();
            centersReduced[SensorHeelReduced.HALLUX] = centers[Sensor.HALLUX];
            centersReduced[SensorHeelReduced.TOES] = centers[Sensor.TOES];

            centersReduced[SensorHeelReduced.MET1] = centers[Sensor.MET1];
            centersReduced[SensorHeelReduced.MET3] = centers[Sensor.MET3];
            centersReduced[SensorHeelReduced.MET5] = centers[Sensor.MET5];

            centersReduced[SensorHeelReduced.ARCH] = centers[Sensor.ARCH];

            List<T> heels = new List<T>
            {
                centers[Sensor.HEEL_L],
                centers[Sensor.HEEL_R]
            };
            centersReduced[SensorHeelReduced.HEEL] = transformFuncion(heels);

            return centersReduced;
        }
        private void CalculateCenters()
        {
            (float, float) CalculateCenter(List<Tuple<int,int>> sensor_positions)
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
            Dictionary<Sensor, List<Tuple<int, int>>> sensor_positions_left = foot.CalculateSensorPositionsLeft();
            Dictionary<Sensor, List<Tuple<int, int>>> sensor_positions_right = foot.CalculateSensorPositionsRight();
            foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
            {
                centersLeft[sensor] = CalculateCenter(sensor_positions_left[sensor]);
                centersRight[sensor] = CalculateCenter(sensor_positions_right[sensor]);
            }
        }

        private Dictionary<T, Matrix<float>> CalculateMinDistances<T>(Dictionary<T, (float, float)> centersLeft, Dictionary<T, (float, float)> centersRight) where T : Enum
        {
            Dictionary<T, Matrix<float>> inverse_distances = new Dictionary<T, Matrix<float>>();
            foreach (T sensor in (T[])Enum.GetValues(typeof(T)))
            {
                inverse_distances[sensor] = foot.sensor_map.MapIndexed((row, col, code) =>
                {
                    if (code != foot.codes.Background())
                    {
                        if (row < foot.sensor_map.RowCount / 2)
                        {
                            (float, float) point = centersLeft[sensor];
                            float distance = Helpers.SquareDistance(row, col, point.Item1, point.Item2);
                            return 1.0f / distance;
                        }
                        else
                        {
                            (float, float) point = centersRight[sensor];
                            float distance = Helpers.SquareDistance(row, col, point.Item1, point.Item2);
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
        private Matrix<float> CalculateMinDistancesBackground()
        {
            List<(int, int, float)> backgroundPoints = Helpers.FindAll(foot.sensor_map, (value) => value == foot.codes.Background());
            // Añadir los bordes de la imagen
            for(int i = 0; i < foot.sensor_map.RowCount; i++)
            {
                backgroundPoints.Add((i, -1, foot.codes.Background()));
                backgroundPoints.Add((i, foot.sensor_map.ColumnCount, foot.codes.Background()));
            }
            for (int i = 0; i < foot.sensor_map.ColumnCount; i++)
            {
                backgroundPoints.Add((-1, i, foot.codes.Background()));
                backgroundPoints.Add((foot.sensor_map.RowCount, i, foot.codes.Background()));
            }
            Matrix<float> inverse_distances = foot.sensor_map.MapIndexed((row, col, code) =>
            {
                if (code == foot.codes.Background())
                {
                    return 0f;
                }
                else
                {
                    int min_distance = int.MaxValue;
                    foreach((int, int, float) point in backgroundPoints)
                    {
                        int distance = Helpers.SquareDistance(row, col, point.Item1, point.Item2);
                        if (distance < min_distance)
                        {
                            min_distance = distance;
                        }
                    }
                    return 1.0f / min_distance;
                }
            });
            MatrixMarketWriter.WriteMatrix(Config.INITIAL_PATH + "\\inverse_distances_background.mtx", inverse_distances);
            return inverse_distances;
        }
        // Esto se usa si los sensores ocupan mas de 1 pixel
        private void CalculateMinDistances()
        {
            Dictionary<Sensor, List<Tuple<int, int>>> sensor_positions_left = foot.CalculateSensorPositionsLeft();
            Dictionary<Sensor, List<Tuple<int, int>>> sensor_positions_right = foot.CalculateSensorPositionsRight();
            foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
            {
                inverse_distances[sensor] = foot.sensor_map.MapIndexed((row, col, code) =>
                {
                    if (code == foot.codes.Foot())
                    {
                        if (row < foot.sensor_map.RowCount / 2)
                        {
                            int min_distance = foot.sensor_map.RowCount * foot.sensor_map.RowCount + foot.sensor_map.ColumnCount * foot.sensor_map.ColumnCount;
                            foreach (Tuple<int, int> point in sensor_positions_left[sensor])
                            {
                                int distance = Helpers.SquareDistance(row, col, point.Item1, point.Item2);
                                if (distance < min_distance)
                                {
                                    min_distance = distance;
                                }
                            }
                            return 1.0f / min_distance;
                        }
                        else
                        {
                            int min_distance = foot.sensor_map.RowCount * foot.sensor_map.RowCount + foot.sensor_map.ColumnCount * foot.sensor_map.ColumnCount;
                            foreach (Tuple<int, int> point in sensor_positions_right[sensor])
                            {
                                int distance = Helpers.SquareDistance(row, col, point.Item1, point.Item2);
                                if (distance < min_distance)
                                {
                                    min_distance = distance;
                                }
                            }
                            return 1.0f / min_distance;
                        }
                    }
                    else
                    {
                        return 0.0f;
                    }
                });
            }
        }
        private Dictionary<Sensor, Matrix<float>> CalculateMinDistances(Dictionary<Sensor, List<Tuple<int, int>>> sensor_positions_left,
            Dictionary<Sensor, List<Tuple<int, int>>> sensor_positions_right)
        {
            Dictionary<Sensor, Matrix<float>> inverse_distances = new Dictionary<Sensor, Matrix<float>>();
            foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
            {
                inverse_distances[sensor] = foot.sensor_map.MapIndexed((row, col, code) =>
                {
                    if(code == foot.codes.Background())
                    {
                        return 0.0f;
                    }
                    else
                    {
                        if (row < foot.sensor_map.RowCount / 2)
                        {
                            int min_distance = foot.sensor_map.RowCount * foot.sensor_map.RowCount + foot.sensor_map.ColumnCount * foot.sensor_map.ColumnCount;
                            foreach (Tuple<int, int> point in sensor_positions_left[sensor])
                            {
                                int distance = Helpers.SquareDistance(row, col, point.Item1, point.Item2);
                                if (distance < min_distance)
                                {
                                    if(distance <= 1)
                                    {
                                        distance = 1;
                                        break;
                                    }
                                    min_distance = distance;
                                }
                            }
                            return 1.0f / min_distance;
                        }
                        else
                        {
                            int min_distance = foot.sensor_map.RowCount * foot.sensor_map.RowCount + foot.sensor_map.ColumnCount * foot.sensor_map.ColumnCount;
                            foreach (Tuple<int, int> point in sensor_positions_right[sensor])
                            {
                                int distance = Helpers.SquareDistance(row, col, point.Item1, point.Item2);
                                if (distance < min_distance)
                                {
                                    if (distance <= 1)
                                    {
                                        distance = 1;
                                        break;
                                    }
                                    min_distance = distance;
                                }
                            }
                            return 1.0f / min_distance;
                        }
                    }
                });
            }
            return inverse_distances;
        }
        public async void Calculate(GraphData graphData)
        {
            void CalculateAll()
            {
                Matrix<float> CalculateOne(DataInsole leftInsole, DataInsole rightInsole)
                {
                    Dictionary<Sensor, int> pressuresLeft = leftInsole.pressures;
                    Dictionary<Sensor, int> pressuresRight = rightInsole.pressures;
#if CENTER_SENSORS
                    Dictionary<SensorHeelReduced, int> pressuresLeftReduced = ReduceSensorsHeel(pressuresLeft, (l) => (int)l.Average());
                    Dictionary<SensorHeelReduced, int> pressuresRightReduced = ReduceSensorsHeel(pressuresRight, (l) => (int)l.Average());

                    return CalculateFromPoint(pressuresLeftReduced, pressuresRightReduced, inverse_reduced_distances);
#else
                    return CalculateFromPoint(pressuresLeft, pressuresRight, inverse_distances);
#endif
                }
                pressureMaps = new();
                for (int i = 0; i < graphData.length; i+= N_FRAMES)
                {
#if !AVERAGE
                    DataInsole left_i = ((FrameDataInsoles)graphData[i]).left;
                    DataInsole right_i = ((FrameDataInsoles)graphData[i]).right;
                    pressureMaps.Add(CalculateOne(left_i, right_i));
#else
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
                    pressureMaps.Add(CalculateOne(left, right));
#endif
                }
            }
            this.graphData = graphData;
            if (isInitialized)
            {
                graph.calculating = true;
                await Task.Run(() => CalculateAll());
                await Task.Run(() => graph.InitData(graphData));
                graph.DrawData(pressureMaps[0]);
                graph.calculating = false;
                timeLine.model.timeEvent -= onUpdateTime;
                timeLine.model.timeEvent += onUpdateTime;
            }
            else
            {
                graph.calculating = true;
                initialized += (s, e) =>
                {
                    CalculateAll();
                    graph.InitData(graphData);
                    graph.DrawData(pressureMaps[0]);
                    graph.calculating = false;
                    timeLine.model.timeEvent -= onUpdateTime;
                    timeLine.model.timeEvent += onUpdateTime;
                };
            }
        }
        private void onUpdateTime(object sender, double time)
        {
            int initialEstimation(double time)
            {
                double timePerFrame = graphData.maxTime / graphData.maxFrame;
                int expectedFrame = (int)Math.Round(time / timePerFrame);
                return expectedFrame;
            }
            int searchFrameLineal(double time, int currentFrame, int previousFrame, double previousDiference)
            {
                double currentTime = graphData.time(currentFrame);
                double currentDiference = Math.Abs(time - currentTime);
                if (currentDiference >= previousDiference)
                {
                    return previousFrame;
                }
                else if (currentTime < time)
                {
                    if (currentFrame == graphData.maxFrame) //Si es el ultimo frame devolverlo
                    {
                        return graphData.maxFrame;
                    }
                    else
                    {
                        return searchFrameLineal(time, currentFrame + 1, currentFrame, currentDiference);
                    }
                }
                else if (currentTime > time)
                {
                    if (currentFrame == graphData.minFrame) //Si es el primer frame devolverlo
                    {
                        return graphData.minFrame;
                    }
                    else
                    {
                        return searchFrameLineal(time, currentFrame - 1, currentFrame, currentDiference);
                    }
                }
                else //currentTime == time muy poco probable (decimales) pero puede pasar
                {
                    return currentFrame;
                }
            }
            int estimatedFrame = initialEstimation(time);
            estimatedFrame = Math.Max(estimatedFrame, graphData.minFrame); // No salirse del rango
            estimatedFrame = Math.Min(estimatedFrame, graphData.maxFrame); // No salirse del rango
            estimatedFrame /= N_FRAMES;
            if (estimatedFrame != lastFrame)
            {
                lastFrame = estimatedFrame;
                graph.DrawData(pressureMaps[estimatedFrame]);
            }
        }
        private Matrix<float> CalculateFromPoint<T>(Dictionary<T, int> left, Dictionary<T, int> right, 
            Dictionary<T, Matrix<float>> inverse_distances) where T : Enum
        {
            Matrix<float> pressure_map = foot.sensor_map.MapIndexed((row, col, code) => {
                if (code == foot.codes.Background())
                {
                    return Config.BACKGROUND;
                }
                else
                {
                    float numerator = 0.0f;
                    float denominator = 0.0f;
                    foreach (T sensor in (T[])Enum.GetValues(typeof(T)))
                    {
                        float inverse_distance = inverse_distances[sensor][row, col];
                        if (row < foot.sensor_map.RowCount / 2)
                            numerator += left[sensor] * inverse_distance;
                        else
                            numerator += right[sensor] * inverse_distance;
                        denominator += inverse_distance;
                    }
#if BAKGROUND_DISTANCES
                    denominator += inverse_distances_background[row, col];
#endif
                    return numerator / denominator;
                }
            });

            return pressure_map;
        }
        // Esto se usa si los sensores ocupan mas de 1 pixel
        private Matrix<float> Calculate_(DataInsole left, DataInsole right)
        {
            Matrix<float> pressure_map = foot.sensor_map.MapIndexed((row, col, code) => {
                if (code == foot.codes.Background())
                {
                    return Config.BACKGROUND;
                }
                else if (code == foot.codes.Foot())
                {
                    float numerator = 0.0f;
                    float denominator = 0.0f;
                    foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
                    {
                        float inverse_distance = inverse_distances[sensor][row, col];
                        if (row < foot.sensor_map.RowCount / 2)
                            numerator += left[sensor] * inverse_distance;
                        else
                            numerator += right[sensor] * inverse_distance;
                        denominator += inverse_distance;
                    }
                    return numerator / denominator;
                }
                else
                {
                    Sensor sensor = foot.codes.GetSensor(code);
                    if (row < foot.sensor_map.RowCount / 2)
                        return left[sensor];
                    else
                        return right[sensor];
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
