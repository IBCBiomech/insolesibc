﻿using System.Collections.Generic;
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
        private Dictionary<Sensor, Tuple<double, double>> cp_sensors_left;
        private Dictionary<Sensor, Tuple<double, double>> cp_sensors_right;

        private List<Tuple<double, double>> cps_left;
        private List<Tuple<double, double>> cps_right;

        private Dictionary<Sensor, int> area_sensors_left;
        private Dictionary<Sensor, int> area_sensors_right;

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
            cp_sensors_left = new Dictionary<Sensor, Tuple<double, double>>();
            Dictionary<Sensor, List<Tuple<int, int>>> sensor_positions_left = foot.CalculateSensorPositionsLeft();
            area_sensors_left = new Dictionary<Sensor, int>();
            foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
            {
                cp_sensors_left[sensor] = Helpers.Average(sensor_positions_left[sensor]);
                area_sensors_left[sensor] = sensor_positions_left[sensor].Count;
            }



            cp_sensors_right = new Dictionary<Sensor, Tuple<double, double>>();
            Dictionary<Sensor, List<Tuple<int, int>>> sensor_positions_right = foot.CalculateSensorPositionsRight();
            area_sensors_right = new Dictionary<Sensor, int>();
            foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
            {
                cp_sensors_right[sensor] = Helpers.Average(sensor_positions_right[sensor]);
                area_sensors_right[sensor] = sensor_positions_right[sensor].Count;
            }
            
        }
        public void Calculate(GraphData graphData)
        {
            frames = new FramePressures[graphData.length];
            cps_left = new List<Tuple<double, double>>();
            cps_right = new List<Tuple<double, double>>();
            for(int i = 0; i < graphData.length; i++)
            {
                FrameDataInsoles frameData = (FrameDataInsoles)graphData[i];
                DataInsole pressure_left = frameData.left;
                DataInsole pressure_right = frameData.right;
                /*
                Random random = new Random();
                DataInsole pressure_left = new DataInsole();
                pressure_left[Sensor.MET1] = random.Next(0, 4000);
                DataInsole pressure_right = new DataInsole();
                pressure_right[Sensor.MET1] = random.Next(0, 4000);
                pressure_right[Sensor.HALLUX] = random.Next(0, 4000);
                */
                /*
                Vector2? pressure_center_left = null;
                int total_pressure_left = 0;
                foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
                {
                    total_pressure_left += pressure_left[sensor] * area_sensors_left[sensor];
                }
                if (total_pressure_left > 0)
                {
                    float x_left = 0;
                    float y_left = 0;
                    foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
                    {
                        x_left += cp_sensors_left[sensor].X * pressure_left[sensor] * area_sensors_left[sensor];
                        y_left += cp_sensors_left[sensor].Y * pressure_left[sensor] * area_sensors_left[sensor];
                    }
                    x_left /= total_pressure_left;
                    y_left /= total_pressure_left;
                    pressure_center_left = new Vector2(x_left, y_left);
                }

                Vector2? pressure_center_right = null;
                int total_pressure_right = 0;
                foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
                {
                    total_pressure_right += pressure_right[sensor] * area_sensors_right[sensor];
                }
                if (total_pressure_right > 0)
                {
                    float x_right = 0;
                    float y_right = 0;
                    foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
                    {
                        x_right += cp_sensors_right[sensor].X * pressure_right[sensor] * area_sensors_right[sensor];
                        y_right += cp_sensors_right[sensor].Y * pressure_right[sensor] * area_sensors_right[sensor];
                    }
                    x_right /= total_pressure_right;
                    y_right /= total_pressure_right;
                    pressure_center_right = new Vector2(x_right, y_right);
                }
                */
                Tuple<double, double>? pressure_center_left;
                Tuple<double, double>? pressure_center_right;

                int total_pressure_left = 0;
                foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
                {
                    total_pressure_left += pressure_left[sensor] * area_sensors_left[sensor];
                }
                if (total_pressure_left > 0)
                {
                    double x_left = 0;
                    double y_left = 0;
                    foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
                    {
                        x_left += cp_sensors_left[sensor].Item1 * pressure_left[sensor] * area_sensors_left[sensor];
                        y_left += cp_sensors_left[sensor].Item2 * pressure_left[sensor] * area_sensors_left[sensor];
                    }
                    x_left /= total_pressure_left;
                    y_left /= total_pressure_left;
                    pressure_center_left = new Tuple<double, double>(x_left, y_left);
                    cps_left.Add(pressure_center_left);
                }
                else
                {
                    pressure_center_left = null;
                }

                int total_pressure_right = 0;
                foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
                {
                    total_pressure_right += pressure_right[sensor] * area_sensors_right[sensor];
                }
                if (total_pressure_right > 0)
                {
                    double x_right = 0;
                    double y_right = 0;
                    foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
                    {
                        x_right += cp_sensors_right[sensor].Item1 * pressure_right[sensor] * area_sensors_right[sensor];
                        y_right += cp_sensors_right[sensor].Item2 * pressure_right[sensor] * area_sensors_right[sensor];
                    }
                    x_right /= total_pressure_right;
                    y_right /= total_pressure_right;
                    pressure_center_right = new Tuple<double, double>(x_right, y_right);
                    cps_right.Add(pressure_center_right);
                }
                else
                {
                    pressure_center_right = null;
                }


                frames[i] = new FramePressures(i, pressure_center_left, pressure_center_right, total_pressure_left, total_pressure_right);
            }
            graph.DrawData(frames);
            pressureGraph.DrawCPs(cps_left, cps_right);
        }
    }
    public class FramePressures
    {
        public int frame { get; private set; }
        public Tuple<double, double>? totalCenter { get; private set; }
        public Tuple<double, double>? centerLeft { get; private set; }
        public Tuple<double, double>? centerRight { get; private set; }
        public FramePressures(int frame, Tuple<double, double>? centerLeft, Tuple<double, double>? centerRight, int total_pressure_left, int total_pressure_right)
        {
            this.frame = frame;
            this.centerLeft = centerLeft;
            this.centerRight = centerRight;
            if (centerLeft == null && centerRight == null)
            {
                totalCenter = null;
            }
            else if (centerLeft == null)
            {
                totalCenter = centerRight;
            }
            else if (centerRight == null)
            {
                totalCenter = centerLeft;
            }
            else
            {
                double x = (total_pressure_left * centerLeft.Item1 + total_pressure_right * centerRight.Item1) / (total_pressure_left + total_pressure_right);
                double y = (total_pressure_left * centerLeft.Item2 + total_pressure_right * centerRight.Item2) / (total_pressure_left + total_pressure_right);
                totalCenter = new Tuple<double, double>(x, y);
            }
        }
    }
}
