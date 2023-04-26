using insoles.Common;
using ScottPlot;
using ScottPlot.Drawing;
using ScottPlot.Plottable;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Media.Animation;

namespace insoles.Graphs
{
    public class ModelHeatmap
    {
        private WpfPlot plot;

        private Heatmap heatmap;
        private Colorbar colorbar;

        private ScatterPlot centers;

        Colormap[] viableColormaps = new Colormap[] {Colormap.Jet, Colormap.Turbo };
        public ModelHeatmap(WpfPlot plot)
        {
            this.plot = plot;
            plot.Plot.Style(Style.Seaborn);
            double xMin = 0;
            double xMax = Config.PLANTILLA_WIDTH;
            double yMin = 0;
            double yMax = Config.PLANTILLA_HEIGHT;
            plot.Plot.SetInnerViewLimits(xMin, xMax, yMin, yMax);
            plot.Plot.SetOuterViewLimits(yMin: 0);
            plot.Plot.SetAxisLimits(xMin, xMax, yMin, yMax);
            plot.Plot.AxisScaleLock(true);
            plot.Refresh();
            plot.SizeChanged += (sender, args) =>
            {
                plot.Plot.SetAxisLimits(xMin, xMax, yMin, yMax);
            };
        }
        private Color noInterpolate(Color color, Color extended, float ratio)
        {
            return extended;
        }
        public void Draw(double?[,] data)
        {
            if(heatmap != null)
            {
                plot.Plot.Clear(heatmap.GetType());
            }
            if(colorbar != null)
            {
                plot.Plot.Clear(colorbar.GetType());
            }
            //IColormap colormap = extendColormap(Colormap.Jet, Color.LightGray, Helpers.Interpolate, extendSize:25, totalSize:256);
            IColormap colormap = extendColormap(Colormap.Jet, Color.LightGray, noInterpolate, extendSize:15, totalSize:256);
            //IColormap colormap = extendColormap(Colormap.Jet, Color.LightGray, Helpers.CustomInterpolate((ratio) => (float)Math.Pow(ratio, 3)), extendSize:25, totalSize:256);
            heatmap = plot.Plot.AddHeatmap(data, colormap:new Colormap(colormap));
            heatmap.Update(data, min: 0);
            heatmap.Smooth = true;
            colorbar = plot.Plot.AddColorbar(heatmap);
            plot.Plot.Margins(0, 0);
            plot.Plot.MoveFirst(heatmap);
            plot.Refresh();
        }
        public void DrawCenters(double[] xs, double[] ys)
        {
            if (centers != null)
            {
                plot.Plot.Clear(centers.GetType());
            }
            centers = plot.Plot.AddScatter(xs, ys, lineWidth: 0, color: Color.WhiteSmoke);
            plot.Plot.MoveLast(centers);
        }
        private IColormap extendColormap(Colormap colormapBase, Color colorExtend, 
            Func<Color, Color, float, Color> interpolationFunction, 
            int extendSize = 20, int totalSize = 100)
        {
            Color[] colors = new Color[totalSize];
            for(int i = extendSize; i < totalSize; i++)
            {
                colors[i] = colormapBase.GetColor((double)(i - extendSize) / (totalSize - extendSize));
            }
            Color color0 = colormapBase.GetColor(0);
            for(int i = 0; i < extendSize; i++)
            {
                colors[i] = interpolationFunction(color0, colorExtend, (float)i / extendSize);
            }
            Color function(double value)
            {
                int index = (int)(value * totalSize);
                index = Math.Min(Math.Max(index, 0), totalSize - 1);
                return colors[index];
            }
            return new CustomColormap(new Func<double, Color>(function), 
                colormapBase.Name + "Extended");
        }
    }
}
