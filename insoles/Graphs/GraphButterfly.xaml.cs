using ScottPlot.Statistics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace insoles.Graphs
{
    /// <summary>
    /// Lógica de interacción para GraphButterfly.xaml
    /// </summary>
    public partial class GraphButterfly : Page
    {
        private WriteableBitmap writeableBitmap;
        private Foot foot;
        public GraphButterfly()
        {
            InitializeComponent();
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            if(mainWindow.foot == null)
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
            int[] buffer = foot.getImage();
            writeableBitmap = new WriteableBitmap(foot.getLength(0), foot.getLength(1), 96, 96, PixelFormats.Bgr32, null);
            graph.Source = writeableBitmap;
            Int32Rect rect = new Int32Rect(
                    0,
                    0,
                    foot.getLength(0),
                    foot.getLength(1));
            var stride = (rect.Width * writeableBitmap.Format.BitsPerPixel + 7) / 8;
            var bufferSize = rect.Height * stride;
            writeableBitmap.WritePixels(rect, buffer, stride, 0, 0);
        }
        public void DrawData(FramePressures[] data)
        {
            int index = 0;
            while (data[index].totalCenter == null)
            {
                index++;
            }
            Tuple<double, double> lastPointD = data[index].totalCenter;
            Tuple<int, int> lastPoint = toInt(lastPointD);
            for (int i = index + 1; i < data.Length; i++)
            {
                if (data[i].totalCenter != null)
                {
                    Tuple<double, double> currentPointD = data[i].totalCenter;
                    Tuple<int, int> currentPoint = toInt(currentPointD);
                    writeableBitmap.DrawLine(lastPoint.Item1, lastPoint.Item2, currentPoint.Item1, currentPoint.Item2, Colors.Purple);
                    lastPoint = currentPoint;
                }
            }
        }
        public Tuple<int, int> toInt(Tuple<double, double> point)
        {
            int item1 = Convert.ToInt32(Math.Floor(point.Item1));
            int item2 = Convert.ToInt32(Math.Floor(point.Item2));
            return new Tuple<int, int>(item1, item2);
        }
    }
}
