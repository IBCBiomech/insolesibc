﻿#define ALPHA

using insoles.DataHolders;
using insoles.Utilities;
using ScottPlot;
using ScottPlot.Drawing;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Resources;
using static System.Windows.Forms.AxHost;

namespace insoles.UserControls
{
    /// <summary>
    /// Lógica de interacción para GrafoMariposa.xaml
    /// </summary>
    public partial class GrafoMariposa : UserControl, INotifyPropertyChanged
    {
        private ScottPlot.Plottable.Image image;
        private double scale = 1;
        private const double PLANTILLA_HEIGHT = 445;
        private const double PLANTILLA_WIDTH = 615;
        private const int N_FRAMES_ANIMATE = 200;

        private FramePressures[]? _framePressures = null;
        public FramePressures[]? framePressures
        {
            get
            {
                return _framePressures;
            }
            set
            {
                _framePressures = value;
                if (animate)
                {
                    if (value != null) 
                        DrawData(value, Math.Max(0, frame - N_FRAMES_ANIMATE), frame);
                }
                else
                {
                    if (value != null)
                        DrawData(value, 0, value.Length - 1);
                }
            }
        }

        private bool _animate;
        public bool animate
        {
            get
            {
                return _animate;
            }
            set
            {
                _animate = value;
                NotifyPropertyChanged();
                //NotifyPropertyChanged(nameof(graph_loaded));
                if (value)
                {
                    if (framePressures != null)
                        DrawData(framePressures, Math.Max(0, frame - N_FRAMES_ANIMATE), frame);
                }
                else
                {
                    if (framePressures != null)
                        DrawData(framePressures, 0, framePressures.Length - 1);
                }
            }
        }
        private const double TIME_PER_FRAME = 0.01;
        public double time
        {
            set
            {
                frame = (int)Math.Floor(value / TIME_PER_FRAME);
            }
        }

        private int _frame = 0;
        private int frame
        {
            get
            {
                return _frame;
            }
            set
            {
                if (value != _frame)
                {
                    _frame = value;
                    if (animate && framePressures != null && !drawing)
                    {
                        DrawData(framePressures, Math.Max(0, frame - N_FRAMES_ANIMATE), frame);
                    }
                }
            }
        }
        private bool drawing = false;
        public GrafoMariposa()
        {
            InitializeComponent();
            ConfigurePlot();
            DibujarPlantilla();
            plot.Refresh();
            DataContext = this;
        }
        public void ConfigurePlot()
        {
            plot.Plot.Style(dataBackground: Color.White);
            plot.Plot.XAxis2.IsVisible = false;
            plot.Plot.YAxis2.IsVisible = false;
            plot.Plot.XAxis.Label("Xcp(combo)(mm)");
            plot.Plot.YAxis.Label("Ycp(combo)(mm)");
            plot.Plot.Title("Butterfly");
        }
        public void DibujarPlantilla()
        {
            Uri uri = new Uri("pack://application:,,,/Images/bitmap_mariposa.png");
            StreamResourceInfo sri = Application.GetResourceStream(uri);
            Stream stream = sri.Stream;
            Bitmap bitmap = new Bitmap(stream);
            image = plot.Plot.AddImage(bitmap, 0, 0, anchor: Alignment.LowerCenter);
            image.HeightInAxisUnits = PLANTILLA_HEIGHT * scale;
            image.WidthInAxisUnits = PLANTILLA_WIDTH * scale;
            double xMin = -PLANTILLA_WIDTH * scale / 2;
            double xMax = PLANTILLA_WIDTH * scale / 2;
            double yMin = 0;
            double yMax = PLANTILLA_HEIGHT * scale;
            plot.Plot.SetInnerViewLimits(xMin, xMax, yMin, yMax);
            plot.Plot.SetOuterViewLimits(yMin: 0);

            plot.Plot.SetAxisLimits(xMin, xMax, yMin, yMax);
            plot.Plot.AxisScaleLock(true);
            plot.IsHitTestVisible = false;

            plot.SizeChanged += (sender, args) =>
            {
                plot.Plot.SetAxisLimits(xMin, xMax, yMin, yMax);
            };

            plot.Plot.AddText("L", xMin + (xMax - xMin) * 0.2, yMin + (yMax - yMin) * 0.25, size:50, color:Color.DarkGray);
            plot.Plot.AddText("R", xMin + (xMax - xMin) * 0.75, yMin + (yMax - yMin) * 0.25, size:50, color: Color.DarkGray);
        }
        public async void DrawData(FramePressures[] data, int initFrame, int lastFrame)
        {
            drawing = true;
            List<double> x = new List<double>();
            List<double> y = new List<double>();
            List<Color> colors = new List<Color>();
            int index = initFrame;
            Colormap colormap = Colormap.Jet;
            while (data[index].totalCenter == null)
            {
                index++;
                if(index == lastFrame + 1)
                {
                    return;
                }
            }
            FramePressures firstFrame = data[index];
            Tuple<double, double> firstPoint = firstFrame.totalCenter;
            x.Add(firstPoint.Item1);
            y.Add(firstPoint.Item2);
            colors.Add(colormap.GetColor(firstFrame.totalPressure / FramePressures.maxPressure));
            for (int i = index + 1; i < lastFrame; i++)
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
            await DrawData(x, y, colors);
            drawing = false;
        }
        private async Task DrawData(List<double> x_list, List<double> y_list, List<Color> colors)
        {
            int calculateAlpha(int i, int length, int min = 128)
            {
                float percent = (float)i / length;
                return Math.Max(min, (int)(percent * byte.MaxValue));
            }
            plot.Plot.Clear(typeof(ScatterPlot));
            double[] x = new double[x_list.Count];
            double[] y = new double[y_list.Count];

            double qualityMult = 1;
            for (int i = 0; i < x_list.Count; i++)
            {
                x[i] = (x_list[i] * qualityMult - PLANTILLA_WIDTH / 2) * scale;
            }
            for (int i = 0; i < y_list.Count; i++)
            {
                y[i] = y_list[i] * qualityMult * scale;
            }
            for (int i = 0; i < Math.Min(x.Length, y.Length) - 1; i++)
            {
#if ALPHA
                Color color = HelperFunctions.Interpolate(colors[i], colors[i + 1], calculateAlpha(i, colors.Count));
#else
                Color color = HelperFunctions.Interpolate(colors[i], colors[i + 1]);
#endif
                plot.Plot.AddScatterLines(new double[] { x[i], x[i + 1] }, new double[] { y[i], y[i + 1] },
                    color);
            }
            await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                    plot.Refresh();
            }));
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
