using ScottPlot;
using ScottPlot.Plottable;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;

namespace insoles.Graphs
{
    // Modelo de los graficos del acelerometro, giroscopio y magnetometro
    public class Model2S
    {
        private const int RIGHT_SEPARATION = 20;
        private const int MAX_POINTS = 100;
        private int CAPACITY = 100000; //Usar un valor sufientemente grande para que en la mayoria de los casos no haya que cambiar el tamaño de los arrays
        private const int GROW_FACTOR = 2;

        double[] valuesLeft;
        double[] valuesRight;
        SignalPlot signalPlotLeft;
        SignalPlot signalPlotRight;

        private int nextIndex = 0;
        private WpfPlot plot;

        private double minY;
        private double maxY;

        private Color frameColor = Color.Black;
        private VLine lineFrame;
        private const float verticalLineWidth = 0.5f;

        private const float horizontalLineWidth = 0.5f;
        private Color leftColor = Config.colorX;
        private HLine lineLeft;
        private Color rightColor = Config.colorY;
        private HLine lineRight;

        private const string labelLeft = "Left";
        private const string labelRight = "Right";

        public Model2S(WpfPlot plot, double minY, double maxY, string title = "", string units = "")
        {
            this.minY = minY;
            this.maxY = maxY;
            this.plot = plot;

            lineFrame = plot.Plot.AddVerticalLine(0, color: frameColor, width: verticalLineWidth, style: LineStyle.Dash);

            plot.Plot.SetAxisLimitsX(xMin: 0, MAX_POINTS);
            plot.Plot.Legend(location: Alignment.UpperRight);


            plot.Plot.Style(Style.Seaborn);


            plot.Refresh();
        }
        static string customFormatter(double position)
        {
            if (position == 0)
                return "zero";
            else if (position > 0)
                return $"+{position:F2}";
            else
                return $"({Math.Abs(position):F2})";
        }
        public void initCapture()
        {
            valuesLeft = new double[CAPACITY];
            valuesRight = new double[CAPACITY];
            plot.Plot.Remove(signalPlotLeft);
            plot.Plot.Remove(signalPlotRight);
            signalPlotLeft = plot.Plot.AddSignal(valuesLeft, color: leftColor, label: labelLeft);
            signalPlotRight = plot.Plot.AddSignal(valuesRight, color: rightColor, label: labelRight);
            signalPlotLeft.MarkerSize = 0;
            signalPlotRight.MarkerSize = 0;
            plot.Plot.SetAxisLimitsY(yMin: minY, yMax: maxY);
            nextIndex = 0;
            maxRenderIndex = nextIndex;
        }
        #region Replay
        // Añade todos los datos de golpe (solo para replay)
        public void updateData(double[] left, double[] right)
        {
            valuesLeft = left;
            valuesRight = right;
            plot.Plot.Remove(signalPlotLeft);
            plot.Plot.Remove(signalPlotRight);
            signalPlotLeft = plot.Plot.AddSignal(valuesLeft, color: leftColor, label: labelLeft);
            signalPlotRight = plot.Plot.AddSignal(valuesRight, color: rightColor, label: labelRight);
            signalPlotLeft.MarkerSize = 0;
            signalPlotRight.MarkerSize = 0;
            maxRenderIndex = 0;
            //plot.Plot.SetAxisLimitsX(xMin: 0, xMax: Math.Min(MAX_POINTS, valuesX.Length));
            plot.Plot.SetAxisLimitsX(xMin: 0, xMax: valuesLeft.Length);
            plot.Plot.SetAxisLimitsY(yMin: minY, yMax: maxY);
        }
        // Cambia los datos a mostrar
        public void updateIndex(int index)
        {
            index = Math.Min(index, valuesLeft.Length); //Por si acaso
            maxRenderIndex = index;

            signalPlotLeft.Label = "Left= " + valuesLeft[index].ToString("0.##");
            signalPlotRight.Label = "Right= " + valuesRight[index].ToString("0.##");

            //plot.Plot.SetAxisLimits(xMin: Math.Max(0, index - MAX_POINTS),
            //    xMax: Math.Max(index + RIGHT_SEPARATION, Math.Min(MAX_POINTS, valuesX.Length)));
            plot.Render();
        }
        #endregion Replay
        // Esta version funciona mejor pero usa mas memoria. Si se sobrepasa la memoria incial hay que modificar el tamaño de las arrays.

        // Actualiza los datos
        public async void updateData(float[] dataLeft, float[] dataRight, bool render = true)
        {
            if (nextIndex + Math.Max(dataLeft.Length, dataRight.Length) >= CAPACITY) // No deberia pasar
            {
                CAPACITY = CAPACITY * GROW_FACTOR;
                Array.Resize(ref valuesLeft, CAPACITY);
                Array.Resize(ref valuesRight, CAPACITY);
                plot.Plot.Remove(signalPlotLeft);
                plot.Plot.Remove(signalPlotRight);
                signalPlotLeft = plot.Plot.AddSignal(valuesLeft, color: Color.Red, label: "X");
                signalPlotRight = plot.Plot.AddSignal(valuesRight, color: Color.Green, label: "Y");
                signalPlotLeft.MarkerSize = 0;
                signalPlotRight.MarkerSize = 0;
            }
            for (int i = 0; i < dataLeft.Length; i++)
            {
                valuesLeft[nextIndex + i] = dataLeft[i];
            }
            for (int i = 0; i < dataRight.Length; i++)
            {
                valuesRight[nextIndex + i] = dataRight[i];
            }
            signalPlotLeft.Label = "Left= " + dataLeft[dataLeft.Length - 1].ToString("0.##");
            signalPlotRight.Label = "Right= " + dataRight[dataRight.Length - 1].ToString("0.##");
            nextIndex += Math.Max(dataLeft.Length, dataRight.Length);
            if (render)
            {
                this.render();
            }
        }
        // Actualiza el renderizado
        public async void render()
        {
            int index = nextIndex - 1;
            if (index < 0)
            {
                index = 0;
            }
            maxRenderIndex = index;
            plot.Plot.SetAxisLimits(xMin: Math.Max(0, index - MAX_POINTS),
                xMax: Math.Max(index + RIGHT_SEPARATION, Math.Min(MAX_POINTS, valuesLeft.Length)));
            plot.Render();
        }
        // Borra todos los puntos de todas las lineas
        public void clear()
        {
            nextIndex = 0;
            maxRenderIndex = nextIndex;
            plot.Render();
        }
        // Usar esto para actualizar la line tambien
        private int maxRenderIndex
        {
            get
            {
                return (int)lineFrame.X;
            }
            set
            {
                signalPlotLeft.MaxRenderIndex = value;
                signalPlotRight.MaxRenderIndex = value;
                lineFrame.X = value;
            }
        }
    }
}