using ScottPlot.Statistics;
using System.Collections.Generic;
using System;

namespace insoles.Graphs
{
    public class Butterfly
    {
        private Dictionary<Sensor, Vector2> cp_sensors_left;
        private Dictionary<Sensor, Vector2> cp_sensors_right;

        private Dictionary<Sensor, int> area_sensors_left;
        private Dictionary<Sensor, int> area_sensors_right;

        private FramePressures[] frames;
        public Butterfly()
        {

        }
        public void Calculate(GraphData graphData)
        {
            frames = new FramePressures[graphData.length];
            for(int i = 0; i < graphData.length; i++)
            {
                FrameDataInsoles frameData = (FrameDataInsoles)graphData[i];
                DataInsole pressure_left = frameData.left;
                DataInsole pressure_right = frameData.right;

                Vector2? pressure_center_left = null;
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
                    double x_right = 0;
                    double y_right = 0;
                    foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
                    {
                        x_right += cp_sensors_right[sensor].X * pressure_right[sensor] * area_sensors_right[sensor];
                        y_right += cp_sensors_right[sensor].Y * pressure_right[sensor] * area_sensors_right[sensor];
                    }
                    x_right /= total_pressure_right;
                    y_right /= total_pressure_right;
                    pressure_center_right = new Vector2(x_right, y_right);
                }
                frames[i] = new FramePressures(pressure_center_left, pressure_center_right, total_pressure_left, total_pressure_right);
            }
        }
        private class FramePressures
        {
            public Vector2? totalCenter { get; private set; }
            public Vector2? centerLeft { get; private set; }
            public Vector2? centerRight {get; private set; }
            public FramePressures(Vector2? centerLeft, Vector2? centerRight, int totalPressureLeft, int totalPressureRight)
            {
                this.centerLeft = centerLeft;
                this.centerRight = centerRight;
                if (centerLeft == null && centerRight == null)
                {
                    totalCenter = null;
                }
                else if(centerLeft == null)
                {
                    totalCenter = centerRight;
                }
                else if(centerRight == null)
                {
                    totalCenter = centerLeft;
                }
                else
                {
                    double x = (totalPressureLeft * centerLeft.Value.X + totalPressureRight * centerRight.Value.X) / (totalPressureLeft + totalPressureRight);
                    double y = (totalPressureLeft * centerLeft.Value.Y + totalPressureRight * centerRight.Value.Y) / (totalPressureLeft + totalPressureRight);
                    totalCenter = new Vector2(x, y);
                }
            }
        }
    }
    
}
