using ScottPlot;
using ScottPlot.Plottable;

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
            Heatmap heatmap = plot.Plot.AddHeatmap(data);
            Colorbar colorbar = plot.Plot.AddColorbar(heatmap);
            plot.Plot.Margins(0, 0);
            plot.Refresh();
        }
    }
}
