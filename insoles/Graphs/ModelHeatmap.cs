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
        public ModelHeatmap(WpfPlot plot)
        {
            this.plot = plot;
            plot.Plot.Style(Style.Seaborn);
        }
        public void Draw(double?[,] data)
        {
            plot.Plot.Clear();
            Heatmap heatmap = plot.Plot.AddHeatmap(data, colormap:Colormap.Amp);
            heatmap.Update(data, min: 0);
            Colorbar colorbar = plot.Plot.AddColorbar(heatmap);
            plot.Plot.Margins(0, 0);
            plot.Refresh();
        }
    }
}
