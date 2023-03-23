//#define CENTROS_SEPARADOS
#define CENTER_LEFT
#define CENTER_RIGHT

using ScottPlot.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;

namespace insoles.Graphs
{
    /// <summary>
    /// Lógica de interacción para GraphButterflyScottplot.xaml
    /// </summary>
    public partial class GraphButterflyScottplot : Page
    {
        private Foot foot;
        public ModelButterfly model { get; private set; }
        public GraphButterflyScottplot()
        {
            InitializeComponent();
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow.foot == null)
            {
                mainWindow.initialized += (s, e) =>
                {
                    foot = mainWindow.foot;
                    model = new ModelButterfly(plot, foot);
                };
            }
            else
            {
                foot = mainWindow.foot;
                model = new ModelButterfly(plot, foot);
            }
            DataContext = this;
        }
        public void DrawData(FramePressures[] data)
        {
            List<double> x = new List<double>();
            List<double> y = new List<double>();
            List<Color> colors = new List<Color>();
            int index = 0;
            Colormap colormap = Config.colormap;
#if !CENTROS_SEPARADOS
            while (data[index].totalCenter == null)
            {
                index++;
            }
            FramePressures firstFrame = data[index];
            Tuple<double, double> firstPoint = firstFrame.totalCenter;
            x.Add(firstPoint.Item1);
            y.Add(firstPoint.Item2);
            colors.Add(colormap.GetColor(firstFrame.totalPressure / FramePressures.maxPressure));
            for (int i = index + 1; i < data.Length; i++)
            {
                if (data[i].totalCenter != null)
                {
                    FramePressures frame = data[i];
                    Tuple<double, double> point = frame.totalCenter;
                    x.Add(point.Item1);
                    y.Add(point.Item2);
                    colors.Add(colormap.GetColor(frame.totalPressure / FramePressures.maxPressure));
                }
            }
            model.DrawData(x, y, colors);
#else
#if CENTER_LEFT
            index = 0;
            while (data[index].centerLeft == null)
            {
                index++;
            }
            Tuple<double, double> firstPoint = data[index].centerLeft;
            x.Add(firstPoint.Item1);
            y.Add(firstPoint.Item2);
            for (int i = index + 1; i < data.Length; i++)
            {
                if (data[i].centerLeft != null)
                {
                    Tuple<double, double> point = data[i].centerLeft;
                    x.Add(point.Item1);
                    y.Add(point.Item2);
                }
            }
#endif
#if CENTER_RIGHT
            index = 0;
            while (data[index].centerRight == null)
            {
                index++;
            }
            firstPoint = data[index].centerRight;
            x.Add(firstPoint.Item1);
            y.Add(firstPoint.Item2);
            for (int i = index + 1; i < data.Length; i++)
            {
                if (data[i].centerRight != null)
                {
                    Tuple<double, double> point = data[i].centerRight;
                    x.Add(point.Item1);
                    y.Add(point.Item2);
                }
            }
#endif
            model.DrawData(x, y);
#endif
        }
    }
}
