using ScottPlot.Plottable;
using System.Windows.Controls;
using System.Drawing;
using System;
using System.Timers;
using System.Diagnostics;
using System.Windows.Threading;
using insoles.States;
using System.Windows;

namespace insoles.UserControls
{
    /// <summary>
    /// Lógica de interacción para TimeLine.xaml
    /// </summary>
    public partial class TimeLine : UserControl
    {
        private AnalisisState state;

        private const int TICK_MS = 10;
        private Timer timer;
        private Stopwatch stopwatch;
        private double deltaTime = 0;

        private HSpan line;
        private double pos = 0;
        private const double PERCENT_WIDTH = 0.5;
        private double width = 0.5;
        private double minX = 0;
        private double maxX = 100;
        private double minY = 0;
        private double maxY = 1;
        private double lastTime;
        private double MIN_CHANGE_TO_NOTIFY;
        public TimeLine(AnalisisState state)
        {
            InitializeComponent();
            this.state = state;
            plot.Plot.XAxis2.SetSizeLimit(pad: 0);
            plot.Plot.XAxis2.SetSizeLimit(pad: 0);
            plot.Plot.SetInnerViewLimits(xMin: minX, xMax: maxX, yMin: minY, yMax: maxY);
            plot.Plot.SetOuterViewLimits(xMin: minX, xMax: maxX, yMin: minY, yMax: maxY);
            line = plot.Plot.AddHorizontalSpan(pos - width, pos + width, Color.DeepSkyBlue);
            line.DragEnabled = true;
            line.DragFixedSize = true;
            line.DragLimitMin = minX;
            line.DragLimitMax = maxX;
            plot.Plot.SetAxisLimitsX(minX, maxX);
            plot.Plot.SetAxisLimitsY(minY, maxY);
            plot.Plot.XAxis.TickLabelFormat((time) =>
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds(time);
                // Si es menor que una hora mostrar solo minutos y segundos
                if (time < 60 * 60)
                {
                    return string.Format("{0:D2}:{1:D2}",
                    timeSpan.Minutes,
                    timeSpan.Seconds);
                }
                else
                {
                    return string.Format("{0:D2}:{1:D2}:{2:D2}",
                    timeSpan.Hours,
                    timeSpan.Minutes,
                    timeSpan.Seconds);
                }
            });
            plot.Plot.YAxis.TickLabelFormat((_) =>
            {
                return "";
            });
            line.Dragged += (sender, e) =>
            {
                deltaTime = line.X1;
                if (stopwatch.IsRunning)
                {
                    stopwatch.Restart();
                }
                else
                {
                    stopwatch.Reset();
                }
            };
            plot.Refresh();

            timer = new Timer();
            timer.Interval = TICK_MS;
            timer.Elapsed += (object sender, ElapsedEventArgs e)=>
            {
                TimeSpan time = stopwatch.Elapsed;
                double totalTime = time.TotalSeconds + deltaTime;
                if(totalTime > maxX)
                {
                    totalTime = maxX;
                    state.paused = true;
                    stopwatch.Reset();
                    timer.Stop();
                }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    line.X1 = totalTime - width / 2;
                    line.X2 = totalTime + width / 2;
                    plot.Refresh();
                });
            };
            stopwatch = new Stopwatch();
        }
        public void Play()
        {
            timer.Start();
            stopwatch.Start();
            state.paused = false;
        }
        public void Pause()
        {
            timer.Stop();
            stopwatch.Stop();
            state.paused = true;
        }
        public void FastForward()
        {
            Trace.WriteLine("FastForward from TimeLine");
            if (stopwatch.IsRunning)
            {
                stopwatch.Restart();
            }
            else
            {
                stopwatch.Reset();
            }
            deltaTime = maxX;
            line.X1 = maxX - width;
            line.X2 = maxX;
            plot.Refresh();
        }
        public void FastBackward()
        {
            Trace.WriteLine("FastBackward from TimeLine");
            if (stopwatch.IsRunning)
            {
                stopwatch.Restart();
            }
            else
            {
                stopwatch.Reset();
            }
            deltaTime = minX;
            line.X1 = minX;
            line.X2 = minX + width;
            plot.Refresh();
        }
    }
}
