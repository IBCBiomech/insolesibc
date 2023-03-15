#define PLANTILLA

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

namespace insoles.Graphs
{
    public class ModelButterfly
    {
        private WpfPlot plot;
        private Foot foot;
        private ScottPlot.Plottable.Image image;
        private ScatterPlot cps;
        private const double HEIGHT = 605;
        private const double WIDTH = 474;
        private double scale = 1;
#if PLANTILLA
        string file = "bitmap_butterfly.png";
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
            //saveBitmap(foot);
            drawFoot();
        }
        private void drawFoot()
        {
            Bitmap bitmap = new Bitmap(Helpers.GetFilePath(file));
            image = plot.Plot.AddImage(bitmap, 0, 0, anchor: Alignment.LowerCenter);
            image.HeightInAxisUnits = HEIGHT * scale;
            image.WidthInAxisUnits = WIDTH * scale;
            //plot.Plot.SetAxisLimitsX(-WIDTH * scale / 2, WIDTH * scale / 2);
            //plot.Plot.SetAxisLimitsY(0, HEIGHT * scale);
            plot.Plot.SetInnerViewLimits(-WIDTH * scale / 2, WIDTH * scale / 2, 0, HEIGHT * scale);
            //plot.Plot.SetOuterViewLimits(-WIDTH * scale, WIDTH * scale, 0, HEIGHT * scale * 2);
            plot.Plot.SetOuterViewLimits(yMin: 0);
            plot.Plot.AxisScaleLock(true);
            plot.IsHitTestVisible = false;
            plot.Refresh();
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
                        bitmap.SetPixel(j, i, Color.FromArgb(255, Color.Gray));
                    }
                }
            }
            bitmap.Save(Config.INITIAL_PATH + "\\bitmap.png", ImageFormat.Png);
        }
        public void DrawData(List<double> x_list, List<double> y_list)
        {
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
                x[i] = (x_list[i] * qualityMult - WIDTH / 2) * scale;
            }
            for(int i = 0; i < y_list.Count; i++)
            {
                y[i] = y_list[i] * qualityMult * scale;
            }
            cps = plot.Plot.AddScatterLines(x, y, Color.Purple);
            plot.Refresh();
        }
    }
}
