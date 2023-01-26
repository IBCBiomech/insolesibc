using System;
using System.Drawing;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using insoles.Common;

namespace insoles.Graphs
{
    internal class PressureMap
    {
        Codes codes;
        Matrix<float> sensor_map;
        Matrix<float> pressure_map;
        Dictionary<Sensor, Matrix<float>> inverse_distances = new Dictionary<Sensor, Matrix<float>>();
        Dictionary<Sensor, Tuple<double, double>> cp_sensors_left;
        Dictionary<Sensor, Tuple<double, double>> cp_sensors_right;
        Dictionary<Sensor, int> area_sensors_left;
        Dictionary<Sensor, int> area_sensors_right;

        public PressureMap(string image_path)
        {
            Bitmap bmp = new Bitmap(image_path);
            sensor_map = Helpers.ImageToMatrix(bmp);
            pressure_map = Matrix<float>.Build.Dense(sensor_map.RowCount, sensor_map.ColumnCount);
            (float, int)[] frequences = Helpers.CountFrequences(sensor_map);
            codes = new Codes(frequences);
            CalculateMinDistances();
        }
        private void CalculateMinDistances()
        {
            Dictionary<Sensor, List<Tuple<int, int>>> sensor_positions_left = CalculateSensorPositions(new Tuple<int, int>(0, sensor_map.RowCount / 2));
            Dictionary<Sensor, List<Tuple<int, int>>> sensor_positions_right = CalculateSensorPositions(new Tuple<int, int>(sensor_map.RowCount / 2, sensor_map.RowCount));
            foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
            {
                inverse_distances[sensor] = sensor_map.MapIndexed((row, col, code) =>
                {
                    if (code == codes.Foot())
                    {
                        if (row < sensor_map.RowCount / 2)
                        {
                            int min_distance = sensor_map.RowCount * sensor_map.RowCount + sensor_map.ColumnCount * sensor_map.ColumnCount;
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
                            int min_distance = sensor_map.RowCount * sensor_map.RowCount + sensor_map.ColumnCount * sensor_map.ColumnCount;
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
        private Dictionary<Sensor, List<Tuple<int, int>>> CalculateSensorPositions(Tuple<int, int> axis1_range)
        {
            Dictionary<Sensor, List<Tuple<int, int>>> sensor_positions = new Dictionary<Sensor, List<Tuple<int, int>>>();
            for (int i = axis1_range.Item1; i < axis1_range.Item2; i++)
            {
                for (int j = 0; j < sensor_map.ColumnCount; j++)
                {
                    float code = sensor_map[i, j];
                    if (codes.IsSensor(code))
                    {
                        Sensor sensor = codes.GetSensor(code);
                        if (sensor_positions.ContainsKey(sensor))
                        {
                            sensor_positions[sensor].Add(new Tuple<int, int>(i, j));
                        }
                        else
                        {
                            sensor_positions[sensor] = new List<Tuple<int, int>>();
                            sensor_positions[sensor].Add(new Tuple<int, int>(i, j));
                        }
                    }
                }
            }
            return sensor_positions;
        }

        public void Update(Dictionary<Sensor, int> pressure_left, Dictionary<Sensor, int> pressure_right)
        {
            foreach (Sensor sensor in pressure_left.Keys)
            {
                pressure_left[sensor] = 4095 - pressure_left[sensor];
            }
            foreach (Sensor sensor in pressure_right.Keys)
            {
                pressure_right[sensor] = 4095 - pressure_right[sensor];
            }
            pressure_map = sensor_map.MapIndexed((row, col, code) => {
                if (code == codes.Background())
                {
                    return -1;
                }
                else if (code == codes.Foot())
                {
                    float numerator = 0.0f;
                    float denominator = 0.0f;
                    foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
                    {
                        float inverse_distance = inverse_distances[sensor][row, col];
                        if (row < sensor_map.RowCount / 2)
                            numerator += pressure_left[sensor] * inverse_distance;
                        else
                            numerator += pressure_right[sensor] * inverse_distance;
                        denominator += inverse_distance;
                    }
                    return numerator / denominator;
                }
                else
                {
                    Sensor sensor = codes.GetSensor(code);
                    if (row < sensor_map.RowCount / 2)
                        return pressure_left[sensor];
                    else
                        return pressure_right[sensor];
                }

            });
        }
        public Tuple<Tuple<double, double>, Tuple<double, double>, Tuple<double, double>> GetPressureCenter(Dictionary<Sensor, int> pressure_left, Dictionary<Sensor, int> pressure_right)
        {
            Tuple<double, double> pressure_center_left;
            Tuple<double, double> pressure_center_right;
            if (cp_sensors_left == null)
            {
                cp_sensors_left = new Dictionary<Sensor, Tuple<double, double>>();
                Dictionary<Sensor, List<Tuple<int, int>>> sensor_positions_left = CalculateSensorPositions(new Tuple<int, int>(0, sensor_map.RowCount / 2));
                area_sensors_left = new Dictionary<Sensor, int>();
                foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
                {
                    cp_sensors_left[sensor] = Helpers.Average(sensor_positions_left[sensor]);
                    area_sensors_left[sensor] = sensor_positions_left[sensor].Count;
                }



                cp_sensors_right = new Dictionary<Sensor, Tuple<double, double>>();
                Dictionary<Sensor, List<Tuple<int, int>>> sensor_positions_right = CalculateSensorPositions(new Tuple<int, int>(sensor_map.RowCount / 2, sensor_map.RowCount));
                area_sensors_right = new Dictionary<Sensor, int>();
                foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
                {
                    cp_sensors_right[sensor] = Helpers.Average(sensor_positions_right[sensor]);
                    area_sensors_right[sensor] = sensor_positions_right[sensor].Count;
                }
            }

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
            }
            else
            {
                pressure_center_left = new Tuple<double, double>(0, 0);
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
            }
            else
            {
                pressure_center_right = new Tuple<double, double>(0, 0);
            }
            if (total_pressure_left == 0 && total_pressure_right == 0)
            {
                return null;
            }
            double x = (total_pressure_left * pressure_center_left.Item1 + total_pressure_right * pressure_center_right.Item1) / (total_pressure_left + total_pressure_right);
            double y = (total_pressure_left * pressure_center_left.Item2 + total_pressure_right * pressure_center_right.Item2) / (total_pressure_left + total_pressure_right);
            if (total_pressure_left == 0)
            {
                pressure_center_left = null;
            }
            if (total_pressure_right == 0)
            {
                pressure_center_right = null;
            }
            return new Tuple<Tuple<double, double>, Tuple<double, double>, Tuple<double, double>>(new Tuple<double, double>(x, y), pressure_center_left, pressure_center_right);
        }
        public int[,] GetPressureMap()
        {
            int[,] result = new int[pressure_map.RowCount, pressure_map.ColumnCount];
            for (int i = 0; i < pressure_map.RowCount; i++)
            {
                for (int j = 0; j < pressure_map.ColumnCount; j++)
                {
                    result[i, j] = (int)Math.Round(pressure_map[i, j]);
                }
            }
            return result;
        }
        public Matrix<float> GetPressureMapMatrix()
        {
            return pressure_map;
        }
        public int[] GetImage()
        {
            int[] image = new int[sensor_map.RowCount * sensor_map.ColumnCount];
            for (int i = 0; i < sensor_map.ColumnCount; i++)
            {
                for (int j = 0; j < sensor_map.RowCount; j++)
                {
                    if (sensor_map[j, i] == codes.Background())
                    {
                        image[i * sensor_map.RowCount + j] = Helpers.ColorToInt(Color.White);
                    }
                    else
                    {
                        image[i * sensor_map.RowCount + j] = Helpers.ColorToInt(Color.Gray);
                    }
                }
            }
            return image;
        }
        public int GetLength(int dimension)
        {
            if (dimension == 0)
            {
                return sensor_map.RowCount;
            }
            if (dimension == 1)
            {
                return sensor_map.ColumnCount;
            }
            return -1;
        }
    }
}
