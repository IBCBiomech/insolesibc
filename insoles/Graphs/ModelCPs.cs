using ScottPlot;
using ScottPlot.Plottable;
using ScottPlot.Renderable;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.Graphs
{
    public class ModelCPs
    {
        private WpfPlot plotLeftX;
        private WpfPlot plotLeftY;
        private WpfPlot plotRightX;
        private WpfPlot plotRightY;

        private ScatterPlot graphLeftX;
        private ScatterPlot graphLeftY;
        private ScatterPlot graphRightX;
        private ScatterPlot graphRightY;
        public ModelCPs(WpfPlot plotLeftX, WpfPlot plotLeftY, WpfPlot plotRightX, WpfPlot plotRightY)
        {
            this.plotLeftX = plotLeftX;
            this.plotLeftY = plotLeftY;
            this.plotRightX = plotRightX;
            this.plotRightY = plotRightY;

            plotLeftX.Plot.XAxis2.IsVisible = false;
            plotLeftX.Plot.YAxis2.IsVisible = false;
            plotRightX.Plot.XAxis2.IsVisible = false;
            plotRightX.Plot.YAxis2.IsVisible = false;
            plotLeftY.Plot.XAxis2.IsVisible = false;
            plotLeftY.Plot.YAxis2.IsVisible = false;
            plotRightY.Plot.XAxis2.IsVisible = false;
            plotRightY.Plot.YAxis2.IsVisible = false;


            plotLeftX.Plot.XAxis.IsVisible = false;
            plotRightX.Plot.XAxis.IsVisible = false;

            plotLeftX.IsHitTestVisible = false;
            plotLeftY.IsHitTestVisible = false;
            plotRightX.IsHitTestVisible = false;
            plotRightY.IsHitTestVisible = false;

            plotLeftY.Plot.XLabel("Tiempo(s)");
            plotRightY.Plot.XLabel("Tiempo(s)");

            plotLeftX.Plot.YLabel("Xcp(izq)(mm)");
            plotLeftY.Plot.YLabel("Ycp(izq)(mm)");
            plotRightX.Plot.YLabel("Xcp(der)(mm)");
            plotRightY.Plot.YLabel("Ycp(der)(mm)");
            /*
            plotLeftX.AxesChanged += (sender, args) =>
            {
                AxisLimits leftLimits = plotLeftX.Plot.GetAxisLimits();
                plotLeftY.Plot.SetAxisLimitsX(leftLimits.XMin, leftLimits.XMax);
                plotRightX.Plot.SetAxisLimitsY(leftLimits.YMin, leftLimits.YMax);
            };
            */
        }
        private void Refresh()
        {
            plotLeftX.Refresh();
            plotLeftY.Refresh();
            plotRightX.Refresh();
            plotRightY.Refresh();
        }
        private void Clear()
        {
            plotLeftX.Plot.Clear();
            plotLeftY.Plot.Clear();
            plotRightX.Plot.Clear();
            plotRightY.Plot.Clear();
        }
        public void DrawData(FramePressures[] frames)
        {
            Clear();

            List<double> timeLeft = new List<double>();
            List<double> timeRight = new List<double>();
            List<double> xLeft = new List<double>();
            List<double> xRight = new List<double>();
            List<double> yLeft = new List<double>();
            List<double> yRight = new List<double>();
            for (int i = 0; i < frames.Length; i++)
            {
                FramePressures frame = frames[i];
                if (frame.centerLeft != null)
                {
                    timeLeft.Add(frame.frame * 0.01);
                    xLeft.Add(frame.centerLeft.Item1);
                    yLeft.Add(frame.centerLeft.Item2);
                }
                if (frame.centerRight != null)
                {
                    timeRight.Add(frame.frame * 0.01);
                    xRight.Add(frame.centerRight.Item1);
                    yRight.Add(frame.centerRight.Item2);
                }
            }
            graphLeftX = plotLeftX.Plot.AddScatterLines(timeLeft.ToArray(), xLeft.ToArray());
            graphLeftY = plotLeftY.Plot.AddScatterLines(timeLeft.ToArray(), yLeft.ToArray());
            graphRightX = plotRightX.Plot.AddScatterLines(timeRight.ToArray(), xRight.ToArray());
            graphRightY = plotRightY.Plot.AddScatterLines(timeRight.ToArray(), yRight.ToArray());

            AxisLimits axisLimitsLeftX = graphLeftX.GetAxisLimits();
            AxisLimits axisLimitsLeftY = graphLeftY.GetAxisLimits();

            AxisLimits axisLimitsRightX = graphRightX.GetAxisLimits();
            AxisLimits axisLimitsRightY = graphRightY.GetAxisLimits();

            plotLeftX.Plot.SetAxisLimitsX(
                Math.Min(axisLimitsLeftX.XMin, axisLimitsLeftY.XMin)
                , Math.Max(axisLimitsLeftX.XMax, axisLimitsLeftY.XMax));
            plotLeftY.Plot.SetAxisLimitsX(
                Math.Min(axisLimitsLeftX.XMin, axisLimitsLeftY.XMin)
                , Math.Max(axisLimitsLeftX.XMax, axisLimitsLeftY.XMax));

            plotRightX.Plot.SetAxisLimitsX(
                Math.Min(axisLimitsRightX.XMin, axisLimitsRightY.XMin)
                , Math.Max(axisLimitsRightX.XMax, axisLimitsRightY.XMax));
            plotRightY.Plot.SetAxisLimitsX(
                Math.Min(axisLimitsRightX.XMin, axisLimitsRightY.XMin)
                , Math.Max(axisLimitsRightX.XMax, axisLimitsRightY.XMax));
            Refresh();
        }
    }
}
