#define PLANTILLA
#define ALPHA //Incrementa alpha a medida que añade nuevos puntos al butterfly. Asi los puntos mas nuevos se ven por encima

using ScottPlot;
using Style = ScottPlot.Style;
using System.Drawing;
using System.IO;
using System.Diagnostics;
using MathNet.Numerics.LinearAlgebra;
using insoles.Common;
using System.Windows.Media.Media3D;
using System;
using System.Drawing.Imaging;
using Microsoft.VisualBasic.Logging;
using ScottPlot.Drawing;
using static System.Net.WebRequestMethods;
using System.Windows.Documents;
using System.Collections.Generic;
using ScottPlot.Plottable;
using System.Windows.Resources;
using System.Windows;

namespace insoles.Graphs
{
    public class ModelButterfly
    {
        private WpfPlot plot;
        private Foot foot;
        private ScottPlot.Plottable.Image image;
        private ScatterPlot cps;
        private double scale = 1;
#if PLANTILLA
        string file = "Assets/bitmap_butterfly_white_smoke.png";
#else
        string file = "bitmap.png";
#endif
        public ModelButterfly(WpfPlot plot, Foot foot)
        {
            this.plot = plot;
            this.foot = foot;
            plot.Plot.Style(dataBackground: Color.White);
            plot.Plot.XAxis2.IsVisible = false;
            plot.Plot.YAxis2.IsVisible = false;
            plot.Plot.XAxis.Label("Xcp(combo)(mm)");
            plot.Plot.YAxis.Label("Ycp(combo)(mm)");
            saveBitmap(foot);
            drawFoot();
        }
        private void drawFoot()
        {
            Uri uri = new Uri("pack://application:,,,/Assets/bitmap_butterfly_white_smoke.png");
            StreamResourceInfo sri = Application.GetResourceStream(uri);
            Stream stream = sri.Stream;
            Bitmap bitmap = new Bitmap(stream);
            image = plot.Plot.AddImage(bitmap, 0, 0, anchor: Alignment.LowerCenter);
            image.HeightInAxisUnits = Config.PLANTILLA_HEIGHT * scale;
            image.WidthInAxisUnits = Config.PLANTILLA_WIDTH * scale;
            //plot.Plot.SetAxisLimitsX(-WIDTH * scale / 2, WIDTH * scale / 2);
            //plot.Plot.SetAxisLimitsY(0, HEIGHT * scale);
            double xMin = -Config.PLANTILLA_WIDTH * scale / 2;
            double xMax = Config.PLANTILLA_WIDTH * scale / 2;
            double yMin = 0;
            double yMax = Config.PLANTILLA_HEIGHT * scale;
            plot.Plot.SetInnerViewLimits(xMin, xMax, yMin, yMax);
            plot.Plot.SetOuterViewLimits(yMin: 0);

            plot.Plot.SetAxisLimits(xMin, xMax, yMin, yMax);
            plot.Plot.AxisScaleLock(true);
            plot.IsHitTestVisible = false;
            plot.Refresh();

            plot.SizeChanged += (sender, args) =>
            {
                plot.Plot.SetAxisLimits(xMin, xMax, yMin, yMax);
            };
        }
        public void saveBitmap(Foot foot)
        {
            Matrix<float> sensor_map = foot.sensor_map.Transpose();
            Codes codes = foot.codes;
            int height = sensor_map.RowCount;
            int width = sensor_map.ColumnCount;
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (sensor_map[i, j] == codes.Background())
                    {
                        bitmap.SetPixel(j, i, Color.FromArgb(0, Color.White));
                    }
                    else
                    {
                        bitmap.SetPixel(j, i, Color.FromArgb(255, Color.WhiteSmoke));
                    }
                }
            }
            bitmap.Save(Config.INITIAL_PATH + "\\bitmap_butterfly_white_smoke.png", ImageFormat.Png);
        }
        public void DrawData(List<double> x_list, List<double> y_list, List<Color> colors)
        {
            int calculateAlpha(int i, int length, int min = 128)
            {
                float percent = (float)i / length;
                return Math.Max(min, (int)(percent * byte.MaxValue));
            }
            plot.Plot.Clear(typeof(ScatterPlot));
            double[] x = new double[x_list.Count];
            double[] y = new double[y_list.Count];
#if PLANTILLA
            double qualityMult = 1;
#else
            double qualityMult = 1 / Config.qualitySizes[Config.footQuality];
#endif
            for (int i = 0; i < x_list.Count; i++)
            {
                x[i] = (x_list[i] * qualityMult - Config.PLANTILLA_WIDTH / 2) * scale;
            }
            for(int i = 0; i < y_list.Count; i++)
            {
                y[i] = y_list[i] * qualityMult * scale;
            }
            for(int i = 0; i < Math.Min(x.Length, y.Length) - 1; i++)
            {
#if ALPHA
                Color color = Helpers.Interpolate(colors[i], colors[i + 1], calculateAlpha(i, colors.Count));
#else
                Color color = Helpers.Interpolate(colors[i], colors[i + 1]);
#endif
                plot.Plot.AddScatterLines(new double[] { x[i], x[i + 1] }, new double[] { y[i], y[i + 1] },
                    color);
            }
            plot.Refresh();
        }
    }
}
