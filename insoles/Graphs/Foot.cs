using DirectShowLib;
using insoles.Common;
using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;

namespace insoles.Graphs
{
    public class Foot
    {
        private int[] length = new int[2];
        public Matrix<float> sensor_map { get; private set; }
        public Codes codes { get; private set; }

        public enum Quality { HIGH, MID, LOW};

        private Dictionary<Quality, string> resolutions = new Dictionary<Quality, string>();
        public Foot()
        {
            resolutions[Quality.HIGH] = "foot_preprocess.png";
            resolutions[Quality.MID] = "foot2q_preprocess.png";
            resolutions[Quality.LOW] = "foot4q_preprocess.png";
            Quality quality = Config.footQuality;
            string file = resolutions[quality];
            string path = Helpers.GetFilePath(file);
            Bitmap bmp = new Bitmap(path);
            sensor_map = Helpers.ImageToMatrix(bmp);
            length[0] = sensor_map.RowCount;
            length[1] = sensor_map.ColumnCount;
            (float, int)[] frequences = Helpers.CountFrequences(sensor_map);
            codes = new Codes(frequences);
        }
        public int[] getImage()
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
        public int getLength(int index)
        {
            return length[index];
        }
        public Dictionary<Sensor, List<Tuple<int, int>>> CalculateSensorPositionsLeft()
        {
            return CalculateSensorPositions(new Tuple<int, int>(0, sensor_map.RowCount / 2));
        }
        public Dictionary<Sensor, List<Tuple<int, int>>> CalculateSensorPositionsRight()
        {
            return CalculateSensorPositions(new Tuple<int, int>(sensor_map.RowCount / 2, sensor_map.RowCount));
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
    }
}
