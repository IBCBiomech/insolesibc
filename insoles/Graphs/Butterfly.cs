//#define STATS

using System.Collections.Generic;
using System;
using System.Windows;
using insoles.Common;
using System.Numerics;
using DirectShowLib;
using System.Diagnostics;

namespace insoles.Graphs
{
    public class Butterfly
    {
        Plantilla plantilla;
        private Dictionary<Sensor, Tuple<double, double>> cp_sensors_left;
        private Dictionary<Sensor, Tuple<double, double>> cp_sensors_right;

        private List<Tuple<double, double>> cps_left;
        private List<Tuple<double, double>> cps_right;

        private Dictionary<Sensor, double> area_sensors_left;
        private Dictionary<Sensor, double> area_sensors_right;

        private FramePressures[] frames;

        private GraphButterflyScottplot graph;
        private GraphPressureHeatmap pressureGraph;
        private Foot foot;
        public Butterfly()
        {
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow.graphButterfly.Content == null)
            {
                mainWindow.graphButterfly.Navigated += (s, e) =>
                {
                    graph = mainWindow.graphButterfly.Content as GraphButterflyScottplot;
                };
            }
            else
            {
                graph = mainWindow.graphButterfly.Content as GraphButterflyScottplot;
            }
            if (mainWindow.graphPressures.Content == null)
            {
                mainWindow.graphPressures.Navigated += (s, e) =>
                {
                    pressureGraph = mainWindow.graphPressures.Content as GraphPressureHeatmap;
                };
            }
            else
            {
                pressureGraph = mainWindow.graphPressures.Content as GraphPressureHeatmap;
            }
            if (mainWindow.foot == null)
            {
                mainWindow.initialized += (s, e) =>
                {
                    foot = mainWindow.foot;
                    init();
                };
            }
            else
            {
                foot = mainWindow.foot;
                init();
            }
        }
        public void init()
        {
            plantilla = new PlantillaWiseware(foot.CalculateHeight());
            cp_sensors_left = new Dictionary<Sensor, Tuple<double, double>>();
            Dictionary<Sensor, List<Tuple<int, int>>> sensor_positions_left = foot.CalculateSensorPositionsLeft();
            area_sensors_left = new Dictionary<Sensor, double>();
            foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
            {
                cp_sensors_left[sensor] = Helpers.Average(sensor_positions_left[sensor]);
                //area_sensors_left[sensor] = sensor_positions_left[sensor].Count;
                area_sensors_left[sensor] = plantilla.GetArea(sensor);
            }



            cp_sensors_right = new Dictionary<Sensor, Tuple<double, double>>();
            Dictionary<Sensor, List<Tuple<int, int>>> sensor_positions_right = foot.CalculateSensorPositionsRight();
            area_sensors_right = new Dictionary<Sensor, double>();
            foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
            {
                cp_sensors_right[sensor] = Helpers.Average(sensor_positions_right[sensor]);
                //area_sensors_right[sensor] = sensor_positions_right[sensor].Count;
                area_sensors_right[sensor] = plantilla.GetArea(sensor);
            }
            
        }
        public void Calculate(GraphData graphData)
        {
            FramePressures.Reset();
            frames = new FramePressures[graphData.length];
            cps_left = new List<Tuple<double, double>>();
            cps_right = new List<Tuple<double, double>>();
            for(int i = 0; i < graphData.length; i++)
            {
                FrameDataInsoles frameData = (FrameDataInsoles)graphData[i];
                DataInsole pressure_left = frameData.left;
                DataInsole pressure_right = frameData.right;

                Tuple<double, double>? pressure_center_left;
                Tuple<double, double>? pressure_center_right;

                double total_pressure_left = 0;
                foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
                {
                    total_pressure_left += pressure_left[sensor] * area_sensors_left[sensor];
                }
                if (total_pressure_left > 0)
                {
                    double row_left = 0;
                    double col_left = 0;
                    foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
                    {
                        row_left += cp_sensors_left[sensor].Item1 * pressure_left[sensor] * area_sensors_left[sensor];
                        col_left += cp_sensors_left[sensor].Item2 * pressure_left[sensor] * area_sensors_left[sensor];
                    }
                    row_left /= total_pressure_left;
                    col_left /= total_pressure_left;
                    pressure_center_left = new Tuple<double, double>(row_left, col_left);
                    cps_left.Add(pressure_center_left);
                }
                else
                {
                    pressure_center_left = null;
                }

                double total_pressure_right = 0;
                foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
                {
                    total_pressure_right += pressure_right[sensor] * area_sensors_right[sensor];
                }
                if (total_pressure_right > 0)
                {
                    double row_right = 0;
                    double col_right = 0;
                    foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
                    {
                        row_right += cp_sensors_right[sensor].Item1 * pressure_right[sensor] * area_sensors_right[sensor];
                        col_right += cp_sensors_right[sensor].Item2 * pressure_right[sensor] * area_sensors_right[sensor];
                    }
                    row_right /= total_pressure_right;
                    col_right /= total_pressure_right;
                    pressure_center_right = new Tuple<double, double>(row_right, col_right);
                    cps_right.Add(pressure_center_right);
                }
                else
                {
                    pressure_center_right = null;
                }


                frames[i] = new FramePressures(i, pressure_center_left, pressure_center_right, (int)total_pressure_left, (int)total_pressure_right);
            }
#if STATS
            FramePressures.PrintStats();
#endif
            graph.DrawData(frames);
            pressureGraph.DrawCPs(cps_left, cps_right);
        }
    }
    public class FramePressures
    {
#if STATS
        public static int leftWins = 0;
        public static int rightWins = 0;
        public static int leftZeros = 0;
        public static int rightZeros = 0;
        public static double totalCenterRow = 0;
        public static double totalCenterCol = 0;
        public static int totalCenters = 0;
#endif
        public static double maxPressure { get; private set; } = 0;
        public int frame { get; private set; }
        public Tuple<double, double>? totalCenter { get; private set; }
        public Tuple<double, double>? centerLeft { get; private set; }
        public Tuple<double, double>? centerRight { get; private set; }
        public double totalPressure { get; private set; }
        public FramePressures(int frame, Tuple<double, double>? centerLeft, Tuple<double, double>? centerRight, int total_pressure_left, int total_pressure_right)
        {
            this.frame = frame;
            this.centerLeft = centerLeft;
            this.centerRight = centerRight;
            totalPressure = total_pressure_left + total_pressure_right;
            if(totalPressure > maxPressure)
            {
                maxPressure = totalPressure;
            }
            if (centerLeft == null && centerRight == null)
            {
                totalCenter = null;
            }
            else if (centerLeft == null)
            {
                totalCenter = centerRight;
#if STATS
                leftZeros++;
#endif
            }
            else if (centerRight == null)
            {
                totalCenter = centerLeft;
#if STATS
                rightZeros++;
#endif
            }
            else
            {   
                double row = (total_pressure_left * centerLeft.Item1 + total_pressure_right * centerRight.Item1) / (total_pressure_left + total_pressure_right);
                double col = (total_pressure_left * centerLeft.Item2 + total_pressure_right * centerRight.Item2) / (total_pressure_left + total_pressure_right);
                totalCenter = new Tuple<double, double>(row, col);
#if STATS
                if (total_pressure_left > total_pressure_right)
                {
                    leftWins++;
                }
                else
                {
                    rightWins++;
                }
                totalCenterRow += row;
                totalCenterCol += col;
                totalCenters++;
#endif
            }
        }
        // Nuevo fichero
        public static void Reset()
        {
            maxPressure = 0;
        }
#if STATS
        public static void PrintStats()
        {
            Trace.WriteLine("Left zeros: " + leftZeros);
            Trace.WriteLine("Right zeros: " + rightZeros);
            Trace.WriteLine("Left wins: " + leftWins);
            Trace.WriteLine("Right wins: " + rightWins);
            Trace.WriteLine("Total Center Row Avg" + totalCenterRow / totalCenters);
            Trace.WriteLine("Total Center Col Avg" + totalCenterCol / totalCenters);
            int rows = ((MainWindow)Application.Current.MainWindow).foot.sensor_map.RowCount;
            int cols = ((MainWindow)Application.Current.MainWindow).foot.sensor_map.ColumnCount;
            Trace.WriteLine("Total Rows " + rows);
            Trace.WriteLine("Total Cols " + cols);
        }
#endif
    }
}
