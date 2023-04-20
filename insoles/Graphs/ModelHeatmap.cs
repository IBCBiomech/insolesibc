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
            heatmap = plot.Plot.AddHeatmap(data, colormap:Config.colormap);
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
    }
}
