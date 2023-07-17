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

        private const int TICK_MS = 20;
        private Timer timer;
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

        public delegate void TimeEventHandler(object sender, double time);
        public event TimeEventHandler TimeChanged;
        public TimeLine(AnalisisState state)
        {
            InitializeComponent();
            this.state = state;
            /*
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
            */
            slider.ValueChanged += (sender, e) =>
            {
                InvokeTimeChanged(e.NewValue);
            };

            timer = new Timer();
            timer.Interval = TICK_MS;
            timer.Elapsed += (object sender, ElapsedEventArgs e)=>
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    slider.Value += TICK_MS / 1000.0;
                }));
            };
        }
        public void Play()
        {
            timer.Start();
            state.paused = false;
        }
        public void Pause()
        {
            timer.Stop();
            state.paused = true;
        }
        public void FastForward()
        {
            Trace.WriteLine("FastForward from TimeLine");
            deltaTime = slider.Maximum;
            slider.Value = slider.Maximum;
        }
        public void FastBackward()
        {
            Trace.WriteLine("FastBackward from TimeLine");
            deltaTime = slider.Minimum;
            slider.Value = slider.Minimum;
        }
        private void InvokeTimeChanged(double time)
        {
            if(Math.Abs(time - lastTime) > MIN_CHANGE_TO_NOTIFY)
            {
                TimeChanged?.Invoke(this, time);
                lastTime = time;
            }
        }
        public void ChangeLimits(double max)
        {
            slider.Maximum = max;
            slider.TickFrequency = Math.Round(slider.Maximum / 10);
        }
    }
}
