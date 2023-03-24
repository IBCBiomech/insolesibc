using DirectShowLib;
using insoles.Common;
using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using MathNet.Numerics.Data.Text;
using static insoles.Graphs.Foot;
using System.IO;
using System.Windows.Resources;
using System.Windows;

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
            Uri uri = new Uri("pack://application:,,,/Assets/bitmap_heatmap_30_closest.png");
            StreamResourceInfo sri = Application.GetResourceStream(uri);
            Stream stream = sri.Stream;
            Bitmap bmp = new Bitmap(stream);
            sensor_map = Helpers.ImageToMatrix(bmp);

            codes = new Codes();
            length[0] = sensor_map.RowCount;
            length[1] = sensor_map.ColumnCount;
        }
        public int getLength(int index)
        {
            return length[index];
        }
        // Esto es al reves por como esta leyendo el bitmap
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
            Trace.WriteLine("Columns: " + sensor_map.ColumnCount);
            Trace.WriteLine("Rows: " + sensor_map.RowCount);
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
