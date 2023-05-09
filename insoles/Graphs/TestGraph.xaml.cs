using ScottPlot;
using System;
using System.Windows.Controls;
using System.Drawing;

namespace insoles.Graphs
{
    /// <summary>
    /// Lógica de interacción para TestGraph.xaml
    /// </summary>
    public partial class TestGraph : Page
    {
        public TestGraph()
        {
            InitializeComponent();
            RenderSignal();
        }
        private void RenderScatter()
        {
            int pointCount = 20;
            Random rand = new Random(0);
            double[] xs = DataGen.Consecutive(pointCount);
            double[] ys = DataGen.RandomWalk(rand, pointCount, 2.0);
            double[] yErr = DataGen.Random(rand, pointCount, 1.0, 1.0);

            plot.Plot.AddScatter(xs, ys, Color.Blue);
            plot.Plot.AddFillError(xs, ys, yErr, Color.FromArgb(50, Color.Blue));
            plot.Refresh();
        }
        private void RenderSignal()
        {
            int pointCount = 20;
            Random rand = new Random(0);
            double[] xs = DataGen.Consecutive(pointCount);
            double[] ys = DataGen.RandomWalk(rand, pointCount, 2.0);
            double[] yErr = DataGen.Random(rand, pointCount, 1.0, 1.0);

            plot.Plot.AddSignal(ys, color:Color.Blue);
            plot.Plot.AddFillError(xs, ys, yErr, Color.FromArgb(50, Color.Blue));
            plot.Refresh();
        }
    }
}
