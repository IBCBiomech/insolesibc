#define REDUCE_SENSORS //Comentar esto para que no haga la media de MET y HEEL
#define BAKGROUND_DISTANCES //Comentar esto para no usar las distancias al borde

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
    public class PressureMap
    {
        delegate void ActionRef<T1, T2, T3>(T1 arg1, ref T2 arg2, ref T3 arg3);

        private Dictionary<Sensor, Matrix<float>> inverse_distances = new Dictionary<Sensor, Matrix<float>>();
        private Dictionary<SensorReduced, Matrix<float>> inverse_reduced_distances = new Dictionary<SensorReduced, Matrix<float>>();
        private Matrix<float> inverse_distances_background;
        //private GraphPressureMap graph;
        private GraphPressureHeatmap graph;
        private Foot foot;

        private Dictionary<Metric, Matrix<float>> pressureMaps = new Dictionary<Metric, Matrix<float>>();

        private bool isInitialized = false;
        public event EventHandler initialized;

        private Metric metric;
        private bool dirty = true;

        private Dictionary<Sensor, (float, float)> centersLeft = new Dictionary<Sensor, (float, float)>();
        private Dictionary<Sensor, (float, float)> centersRight = new Dictionary<Sensor, (float, float)>();

        private Dictionary<SensorReduced, (float, float)> centersLeftReduced = new Dictionary<SensorReduced, (float, float)>();
        private Dictionary<SensorReduced, (float, float)> centersRightReduced = new Dictionary<SensorReduced, (float, float)>();
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
                CalculateCenters();
#if REDUCE_SENSORS
                centersLeftReduced = ReduceSensors(centersLeft, reduceSensorsFunc);
                centersRightReduced = ReduceSensors(centersRight, reduceSensorsFunc);
                inverse_reduced_distances = CalculateMinDistances(centersLeftReduced, centersRightReduced);
#else
                inverse_distances = CalculateMinDistances(centersLeft, centersRight);
#endif
#if BAKGROUND_DISTANCES
                inverse_distances_background = CalculateMinDistancesBackground();
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
                    graph.MetricChanged += changeMetric;
                };
            }
            else
            {
                graph = mainWindow.graphPressures.Content as GraphPressureHeatmap;
                graph.MetricChanged += changeMetric;
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
        private void drawSensorMap()
        {
            Matrix<float> pressureMap = foot.sensor_map.Map((value) =>
            {
                if(value == 10 || value == 30 || value == 50 || value == 70)
                {
                    return 1000;
                }
                if(value == 20 || value == 40 || value == 80)
                {
                    //return 1000;
                }
                if (value < 100)
                {
                    return value * 2.5f;
                }
                else
                {
                    return value;
                }
            });
            graph.DrawData(pressureMap);
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
            return inverse_distances;
        }
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
        public void Calculate(GraphData graphData)
        {
            //drawSensorMap();
            //return;
            void CalculateAll()
            {
                void CalculateOne(GraphData graphData, ActionRef<GraphData, DataInsole, DataInsole> func, Metric metric)
                {
                    int reduceFunc(List<int> pressures)
                    {
                        return pressures.Sum() / pressures.Count;
                    }
                    DataInsole leftInsole = new DataInsole();
                    DataInsole rightInsole = new DataInsole();
                    func(graphData, ref leftInsole, ref rightInsole);
                    Dictionary<Sensor, int> pressuresLeft = leftInsole.pressures;
                    Dictionary<Sensor, int> pressuresRight = rightInsole.pressures;
#if REDUCE_SENSORS
                    Dictionary<SensorReduced, int> pressuresLeftReduced = ReduceSensors(pressuresLeft, reduceFunc);
                    Dictionary<SensorReduced, int> pressuresRightReduced = ReduceSensors(pressuresRight, reduceFunc);
                    pressureMaps[metric] = CalculateFromPoint(pressuresLeftReduced, pressuresRightReduced, inverse_reduced_distances);
#else
                    pressureMaps[metric] = CalculateFromPoint(pressuresLeft, pressuresRight, inverse_distances);
#endif
                }
                CalculateOne(graphData, average, Metric.Avg);
                CalculateOne(graphData, average, Metric.Max);
                CalculateOne(graphData, average, Metric.Min);
            }
            if (isInitialized)
            {
                graph.calculating = true;
                CalculateAll();
                graph.DrawData(pressureMaps[metric]);
                graph.calculating = false;
            }
            else
            {
                graph.calculating = true;
                initialized += (s, e) =>
                {
                    CalculateAll();
                    graph.DrawData(pressureMaps[metric]);
                    graph.calculating = false;
                };
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
        public void changeMetric(object sender, MetricEventArgs e)
        {
            metric = e.metric;
            if (pressureMaps.ContainsKey(metric) && !graph.calculating)
            {
                graph.DrawData(pressureMaps[metric]);
            }
        }
    }
}
