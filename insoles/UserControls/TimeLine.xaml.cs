using ScottPlot.Plottable;
using System.Windows.Controls;
using System.Drawing;
using System;
using System.Timers;
using System.Diagnostics;
using System.Windows.Threading;
using insoles.States;
using System.Windows;
using System.Windows.Input;
using insoles.Messages;

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
        private double deltaTime = 0;

        private HSpan line;
        private double pos = 0;
        private const double PERCENT_WIDTH = 0.5;
        private double min = 0;
        private double max;
        private double lastTime;
        private double MIN_CHANGE_TO_NOTIFY;

        private double MOUSE_SENSITIVITY = 0.5;

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
            this.max = max;
            slider.Maximum = max;
            slider.TickFrequency = Math.Round(slider.Maximum / 10);
        }

        private void slider_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left || e.Key == Key.Right)
            {
                double increment = 0.01; // Set your desired increment value

                if (e.Key == Key.Left)
                {
                    slider.Value -= increment;
                }
                else if (e.Key == Key.Right)
                {
                    slider.Value += increment;
                }

                e.Handled = true; // Set to true to prevent default slider behavior
            }
        }
        public void ChangeRange(GraphRange graphRange)
        {
            slider.Minimum = graphRange.first * TICK_MS / 1000.0;
            slider.Maximum = graphRange.last * TICK_MS / 1000.0;
        }
        public void ClearRange()
        {
            slider.Minimum = 0;
            slider.Maximum = max;
        }
        /*
private void slider_PreviewMouseMove(object sender, MouseEventArgs e)
{
   if (e.LeftButton == MouseButtonState.Pressed)
   {
       var mousePosition = e.GetPosition(slider);
       double totalWidth = slider.ActualWidth;
       double valueRange = slider.Maximum - slider.Minimum;

       double relativeMousePosition = mousePosition.X / totalWidth;
       double mouseMovement = relativeMousePosition * valueRange - slider.Value;
       double newValue = slider.Value + mouseMovement * MOUSE_SENSITIVITY;

       // Ensure the new value stays within the valid range
       newValue = Math.Min(Math.Max(newValue, slider.Minimum), slider.Maximum);

       // Update the value
       slider.Value = newValue;
   }
}
*/
    }
}
