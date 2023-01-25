using ScottPlot;
using System;
using System.Drawing;
using ScottPlot.Plottable;
using System.Windows.Controls;

namespace insoles.TimeLine
{
    public class Model
    {
        public delegate void TimeEventHandler(object sender, double time);
        public event TimeEventHandler timeEvent;
        public event TimeEventHandler dragEvent;
        public event EventHandler endEvent;

        private TextBlock timer;
        private WpfPlot plot;
        private HSpan line;
        private double pos = 0;
        private const double PERCENT_WIDTH = 0.5; 
        private double width = 0.5;
        private double minX = 0;
        private double maxX = 100;
        private double minY = 0;
        private double maxY = 1;
        private double lastTime;
        // Tiempo de actualizacio del timeline
        private double MIN_CHANGE_TO_NOTIFY;

        // Crea los ejes y las lineas
        public Model(WpfPlot plot, TextBlock timer, double UPDATE_TIME_MS)
        {
            this.timer = timer;
            this.plot = plot;
            plot.Plot.XAxis2.SetSizeLimit(pad:0);
            plot.Plot.XAxis2.SetSizeLimit(pad:0);
            plot.Plot.SetInnerViewLimits(xMin: minX, xMax: maxX, yMin:minY, yMax:maxY);
            plot.Plot.SetOuterViewLimits(xMin: minX, xMax: maxX, yMin: minY, yMax: maxY);
            line = plot.Plot.AddHorizontalSpan(pos - width, pos + width, Color.LightSkyBlue);
            line.DragEnabled = true;
            line.DragFixedSize = true;
            line.DragLimitMin = minX;
            line.DragLimitMax = maxX;
            plot.Plot.SetAxisLimitsX(minX, maxX);
            plot.Plot.SetAxisLimitsY(minY, maxY);
            plot.Plot.XAxis.TickLabelFormat(formatAxis);
            plot.Plot.YAxis.TickLabelFormat((_) => 
            {
                return "";
            });
            plot.Refresh();
            line.Dragged += (sender, e) =>
            {
                dragEvent?.Invoke(this, time);
                timeEvent?.Invoke(this, time);
                NotifyTimeChanged();
            };
            MIN_CHANGE_TO_NOTIFY = UPDATE_TIME_MS / 1000;
        }
        public void updateLimits(double minTime, double maxTime)
        {
            width = (maxTime - minTime) * PERCENT_WIDTH / 100.0;
            minX = minTime;
            maxX = maxTime;
            plot.Plot.SetInnerViewLimits(xMin: minX, xMax: maxX, yMin: minY, yMax: maxY);
            plot.Plot.SetOuterViewLimits(xMin: minX, xMax: maxX, yMin: minY, yMax: maxY);
            line.DragLimitMin = minX;
            line.DragLimitMax = maxX;
            plot.Plot.SetAxisLimitsX(minX, maxX);
            time = minTime;
        }
        // Mueve la linea al principio
        public void moveToStart()
        {
            time = minX;
            dragEvent?.Invoke(this, time);
            timeEvent?.Invoke(this, time);
            plot.Render();
            NotifyTimeChanged();
        }
        // Mueve la linea al final
        public void moveToEnd()
        {
            time = maxX;
            dragEvent?.Invoke(this, time);
            timeEvent?.Invoke(this, time);
            endEvent?.Invoke(this, EventArgs.Empty);
            plot.Render();
            NotifyTimeChanged();
        }
        // Sirve para actualzar el tiempo
        public void setTime(double time)
        {
            if (time > maxX)
            {
                this.time = maxX;
                endEvent?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                this.time = time;
            }
            plot.Render();
            timeEvent?.Invoke(this, time);
            if (hasToNotify())
            {
                NotifyTimeChanged();
            }
        }
        // Sirve para actualizar el contador
        private void NotifyTimeChanged()
        {
            timer.Text = formatTimer(TimeSpan.FromSeconds(time));
            lastTime = time;
        }
        // Condicion para actualizar el contador (sirve para no actualizarlo cada tick)
        // (No hace falta actualizarlo cada tick ya que no se nota)
        private bool hasToNotify()
        {
            return Math.Abs(time - lastTime) > MIN_CHANGE_TO_NOTIFY;
        }
        // Formato del eje x
        private string formatAxis(double time)
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
        }
        // Definicion del valor del tiempo. Se puede modificar pero set y get tienen que ser consistentes
        public double time
        {
            get
            {
                return line.X1 + width;
            }
            private set
            {
                line.X1 = value - width;
                line.X2 = value + width;
            }
        }
        // Devuelve el texto del timer
        public string formatTimer(TimeSpan timeSpan)
        {
            return string.Format("{0:D2}:{1:D2}:{2:D2}:{3:D3}",
            timeSpan.Hours,
            timeSpan.Minutes,
            timeSpan.Seconds,
            timeSpan.Milliseconds);
        }
    }
}
