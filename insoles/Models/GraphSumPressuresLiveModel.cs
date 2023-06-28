using insoles.Utilities;
using ScottPlot;
using ScottPlot.Plottable;
using System.Drawing;
using System.Windows;

namespace insoles.Model
{
    public class GraphSumPressuresLiveModel : ViewModelBase
    {
        private WpfPlot plot;
        private double[] left;
        private double[] right;
        private const int CAPACITY = 200;
        private int nextIndex = 0;
        private SignalPlot signalPlotLeft;
        private SignalPlot signalPlotRight;
        private VLine lineFrame;
        public GraphSumPressuresLiveModel(WpfPlot plot) 
        { 
            this.plot = plot;
            left = new double[CAPACITY];
            right = new double[CAPACITY];
            signalPlotLeft = plot.Plot.AddSignal(left, color: Color.Red, label: "Left");
            signalPlotRight = plot.Plot.AddSignal(right, color: Color.Blue, label: "Right");
            signalPlotLeft.MarkerSize = 0;
            signalPlotRight.MarkerSize = 0;
            plot.Plot.AxisAutoY();
            plot.Plot.SetAxisLimitsX(xMin: 0, xMax: CAPACITY);
            plot.Plot.Legend(location : Alignment.UpperRight);
            lineFrame = plot.Plot.AddVerticalLine(0, color: Color.Green, width: 1, style: LineStyle.Dash);
            plot.Refresh();
        }
        public void UpdateData(float[] dataLeft, float[] dataRight, bool render = true)
        {
            for (int i = 0; i < dataLeft.Length; i++)
            {
                int index = (nextIndex + i) % CAPACITY;
                left[index] = dataLeft[i];
                right[index] = dataRight[i];
            }
            signalPlotLeft.Label = "Left = " + dataLeft[dataLeft.Length - 1].ToString("0.##");
            signalPlotRight.Label = "Right = " + dataRight[dataRight.Length - 1].ToString("0.##");
            nextIndex += dataLeft.Length;
            lineFrame.X = nextIndex % CAPACITY;
            plot.Plot.AxisAutoY();
            if (render)
            {
                Application.Current.Dispatcher.Invoke(() => plot.Render());
            }
        }
    }
}
