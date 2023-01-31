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

        private bool isInitialized = false;
        public event EventHandler initialized;
        public PressureMap()
        {
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
            if (isInitialized)
            {
                graph.calculating = true;
                Calculate_(graphData);
                graph.calculating = false;
            }
            else
            {
                graph.calculating = true;
                initialized += (s, e) =>
                {
                    Calculate_(graphData);
                    graph.calculating = false;
                };
            }
        }
        private void Calculate_(GraphData graphData)
        {
            DataInsole left = new DataInsole();
            DataInsole right = new DataInsole();
            average(graphData, ref left, ref right);

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

            graph.DrawData(pressure_map);
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
    }
}
