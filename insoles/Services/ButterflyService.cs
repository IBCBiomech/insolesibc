using insoles.DataHolders;
using insoles.Enums;
using insoles.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace insoles.Services
{
    public class ButterflyService : IButterflyService
    {
        private Dictionary<Sensor, Tuple<double, double>> cp_sensors_left;
        private Dictionary<Sensor, Tuple<double, double>> cp_sensors_right;

        private Dictionary<Sensor, int> area_sensors_left;
        private Dictionary<Sensor, int> area_sensors_right;


        private IPlantillaService foot;

        private double MIN_N = 20;
        public ButterflyService(IPlantillaService foot)
        {
            this.foot = foot;
            Init();
        }
        public void Init()
        {
            cp_sensors_left = new Dictionary<Sensor, Tuple<double, double>>();
            Dictionary<Sensor, List<Tuple<int, int>>> sensor_positions_left = foot.CalculateSensorPositionsLeft();
            area_sensors_left = new Dictionary<Sensor, int>();
            foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
            {
                cp_sensors_left[sensor] = HelperFunctions.Average(sensor_positions_left[sensor]);
                area_sensors_left[sensor] = sensor_positions_left[sensor].Count;
            }



            cp_sensors_right = new Dictionary<Sensor, Tuple<double, double>>();
            Dictionary<Sensor, List<Tuple<int, int>>> sensor_positions_right = foot.CalculateSensorPositionsRight();
            area_sensors_right = new Dictionary<Sensor, int>();
            foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
            {
                cp_sensors_right[sensor] = HelperFunctions.Average(sensor_positions_right[sensor]);
                area_sensors_right[sensor] = sensor_positions_right[sensor].Count;
            }

        }
        public Task Calculate(GraphData graphData, out FramePressures[] frames, 
            out List<Tuple<double, double>> cps_left, out List<Tuple<double, double>> cps_right)
        {
            FramePressures.Reset();
            frames = new FramePressures[graphData.length];
            cps_left = new List<Tuple<double, double>>();
            cps_right = new List<Tuple<double, double>>();
            for (int i = 0; i < graphData.length; i++)
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
                if (total_pressure_left > MIN_N)
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
                if (total_pressure_right > MIN_N)
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


                frames[i] = new FramePressures(i, pressure_center_left, pressure_center_right, total_pressure_left, total_pressure_right);
            }
            return Task.CompletedTask;
        }
    }
}
