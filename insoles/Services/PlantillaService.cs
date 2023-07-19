using insoles.Enums;
using insoles.Utilities;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Resources;

namespace insoles.Services
{
    public class PlantillaService : IPlantillaService
    {
        private int[] length = new int[2];
        public Matrix<float> sensor_map { get; private set; }
        private ICodesService codes { get; set; }
        public PlantillaService(ICodesService codes)
        {
            Uri uri = new Uri("pack://application:,,,/Images/bitmap_reduced2.png");
            StreamResourceInfo sri = Application.GetResourceStream(uri);
            Stream stream = sri.Stream;
            Bitmap bmp = new Bitmap(stream);
            sensor_map = MathNetHelpers.ImageToMatrix(bmp);

            this.codes = codes;
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
                            sensor_positions[sensor] = new List<Tuple<int, int>>
                            {
                                new Tuple<int, int>(i, j)
                            };
                        }
                    }
                }
            }
            return sensor_positions;
        }
        public List<Tuple<int, int>> CalculateFootPositionsLeft()
        {
            return CalculateFootPositions(new Tuple<int, int>(0, sensor_map.RowCount / 2));
        }
        public List<Tuple<int, int>> CalculateFootPositionsRight()
        {
            return CalculateFootPositions(new Tuple<int, int>(sensor_map.RowCount / 2, sensor_map.RowCount));
        }
        private List<Tuple<int, int>> CalculateFootPositions(Tuple<int, int> axis1_range)
        {
            Trace.WriteLine("Columns: " + sensor_map.ColumnCount);
            Trace.WriteLine("Rows: " + sensor_map.RowCount);
            List<Tuple<int, int>> foot_positions = new List<Tuple<int, int>>();
            for (int i = axis1_range.Item1; i < axis1_range.Item2; i++)
            {
                for (int j = 0; j < sensor_map.ColumnCount; j++)
                {
                    float code = sensor_map[i, j];
                    if (codes.Foot() == code)
                    {
                        foot_positions.Add(new Tuple<int, int>(i, j));
                    }
                }
            }
            return foot_positions;
        }
    }
}
