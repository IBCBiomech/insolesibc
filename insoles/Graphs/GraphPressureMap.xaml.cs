using insoles.Common;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Color = System.Drawing.Color;

namespace insoles.Graphs
{
    /// <summary>
    /// Lógica de interacción para GraphPressureMap.xaml
    /// </summary>
    public partial class GraphPressureMap : Page
    {
        private WriteableBitmap writeableBitmap;
        private Foot foot;

        private static Color[] colors = new Color[4] { Color.Blue, Color.Green, Color.Yellow, Color.Red };
        private static int[] ranges = new int[4] { 0, 1500, 3000, 4500 };
        public GraphPressureMap()
        {
            InitializeComponent();
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
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
        private void init()
        {
            writeableBitmap = new WriteableBitmap(foot.getLength(0), foot.getLength(1), 96, 96, PixelFormats.Bgr32, null);
            graph.Source = writeableBitmap;
        }
        public void DrawData(Matrix<float> data)
        {
            int[] buffer = GetHeatmap(data);
            Int32Rect rect = new Int32Rect(
                   0,
                   0,
                   foot.sensor_map.RowCount,
                   foot.sensor_map.ColumnCount);
                   //foot.sensor_map.GetLength(0),
                   //pressureMap.GetLength(1));
            var stride = (rect.Width * writeableBitmap.Format.BitsPerPixel + 7) / 8;
            var bufferSize = rect.Height * stride;
            writeableBitmap.WritePixels(rect, buffer, stride, 0, 0);
        }  
        private int[] GetHeatmap(Matrix<float> array)
        {
            Matrix<float> heatmap = array.Map((value) =>
            {
                Color color = IntToColor((int)value);
                return (float)Helpers.ColorToInt(color);
            });

            int[] buffer = new int[array.RowCount * array.ColumnCount];
            for (int i = 0; i < array.ColumnCount; i++)
            {
                for (int j = 0; j < array.RowCount; j++)
                {
                    buffer[i * array.RowCount + j] = (int)heatmap[j, i];
                }
            }
            return buffer;
        }
        private Color IntToColor(int value)
        {
            if (value < 0)
            {
                return Color.White;
            }
            for (int i = 1; i < ranges.Length; i++)
            {
                if (value < ranges[i])
                {
                    return InterpolateColors(value, colors[i - 1], colors[i], ranges[i - 1], ranges[i]);
                }
            }
            return colors[colors.Length - 1];
        }
        private Color InterpolateColors(int value, Color color1, Color color2, int value1, int value2)
        {
            double division = (double)(value - value1) / (double)(value2 - value1);
            int[] substract = { color2.R - color1.R, color2.G - color1.G, color2.B - color1.B };
            for (int i = 0; i < substract.Length; i++)
            {
                substract[i] = (int)(substract[i] * division);
            }
            int[] result = { color1.R + substract[0], color1.G + substract[1], color1.B + substract[2] };
            return Color.FromArgb(result[0], result[1], result[2]);
        }
    }
}
