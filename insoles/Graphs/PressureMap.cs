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

namespace insoles.Graphs
{
    public class PressureMap
    {
        private Dictionary<Sensor, Matrix<float>> inverse_distances = new Dictionary<Sensor, Matrix<float>>();
        //private GraphPressureMap graph;
        private GraphPressureHeatmap graph;
        private Foot foot;

        private Dictionary<Metric, Matrix<float>> pressureMaps = new Dictionary<Metric, Matrix<float>>();

        private bool isInitialized = false;
        public event EventHandler initialized;

        private Metric metric;
        private bool dirty = true;
        public PressureMap()
        {
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
                    Task.Run(() => CalculateMinDistances());
                };
            }
            else
            {
                foot = mainWindow.foot;
                Task.Run(() => CalculateMinDistances());
            }
        }
        private void drawSensorMap()
        {
            Matrix<float> pressureMap = foot.sensor_map.Map((value) =>
            {
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
            isInitialized = true;
            initialized?.Invoke(this, EventArgs.Empty);
        }
        public void Calculate(GraphData graphData)
        {
            void CalculateAll()
            {
                DataInsole leftAvg = new DataInsole();
                DataInsole rightAvg = new DataInsole();
                average(graphData, ref leftAvg, ref rightAvg);
                pressureMaps[Metric.Avg] = Calculate_(leftAvg, rightAvg);

                DataInsole leftMax = new DataInsole();
                DataInsole rightMax = new DataInsole();
                max(graphData, ref leftMax, ref rightMax);
                pressureMaps[Metric.Max] = Calculate_(leftMax, rightMax);

                DataInsole leftMin = new DataInsole();
                DataInsole rightMin = new DataInsole();
                min(graphData, ref leftMin, ref rightMin);
                pressureMaps[Metric.Min] = Calculate_(leftMin, rightMin);
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
