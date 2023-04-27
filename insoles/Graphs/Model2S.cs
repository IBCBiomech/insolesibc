﻿using ScottPlot;
using ScottPlot.Plottable;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Windows.Input;
using System.Windows.Markup;

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
        public WpfPlot plot { get; private set; }

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

        private CaptureModel captureModel;
        private ReplayModel replayModel;

        public double displacement { get; set; } = 0;

        public Model2S(WpfPlot plot, double minY, double maxY)
        {
            captureModel = new CaptureModel(this);
            replayModel = new ReplayModel(this);

            this.minY = minY;
            this.maxY = maxY;
            this.plot = plot;

            lineFrame = plot.Plot.AddVerticalLine(0, color: frameColor, width: verticalLineWidth, style: LineStyle.Dash);

            plot.Plot.SetAxisLimitsY(minY, maxY);
            plot.Plot.Legend(location: Alignment.UpperRight);
            plot.Plot.XLabel("s");
            plot.Plot.YLabel("N/m\xB2");


            plot.Plot.Style(Style.Seaborn);


            plot.Refresh();
        }
        static string customFormatter(double position, double displacement)
        {
            if (position == 0)
                return "zero";
            else if (position > 0)
                return $"+{position:F2}";
            else
                return $"({Math.Abs(position):F2})";
        }
        private string displacementFormatter(double position)
        {
            return $"{(position + displacement):F0}";
        }
        public void initCapture()
        {
            clear();
            plot.Plot.XAxis.AutomaticTickPositions();
            captureModel.initCapture();
        }
        #region Replay
        // Añade todos los datos de golpe (solo para replay)
        public void updateData(double[] left, double[] right)
        {
            clear();
            plot.Plot.XAxis.TickLabelFormat(displacementFormatter);
            replayModel.updateData(left, right);
        }
        // Cambia los datos a mostrar
        public void updateIndex(int index)
        {
            replayModel.updateIndex(index);
        }
        #endregion Replay
        // Esta version funciona mejor pero usa mas memoria. Si se sobrepasa la memoria incial hay que modificar el tamaño de las arrays.

        // Actualiza los datos
        public void updateData(float[] dataLeft, float[] dataRight, bool render = true)
        {
            captureModel.updateData(dataLeft, dataRight, render);
        }
        // Borra todos los puntos de todas las lineas
        public void clear()
        {
            plot.Plot.Clear(typeof(SignalPlot));
            plot.Render();
        }
        class CaptureModel
        {
            Model2S model;
            private const int CAPACITY = 200;
            double[] valuesLeft;
            double[] valuesRight;
            SignalPlot signalPlotLeft;
            SignalPlot signalPlotRight;
            private int nextIndex = 0;

            public CaptureModel(Model2S model)
            {
                this.model = model;
            }
            public void initCapture()
            {
                valuesLeft = new double[CAPACITY];
                valuesRight = new double[CAPACITY];
                signalPlotLeft = model.plot.Plot.AddSignal(valuesLeft, color: model.leftColor, label: labelLeft);
                signalPlotRight = model.plot.Plot.AddSignal(valuesRight, color: model.rightColor, label: labelRight);
                signalPlotLeft.MarkerSize = 0;
                signalPlotRight.MarkerSize = 0;
                nextIndex = 0;
                model.plot.Plot.SetAxisLimitsX(xMin: 0, xMax: CAPACITY);
            }
            public void updateData(float[] dataLeft, float[] dataRight, bool render = true)
            {
                for (int i = 0; i < dataLeft.Length; i++)
                {
                    int index = (nextIndex + i) % CAPACITY;
                    valuesLeft[index] = dataLeft[i];
                    valuesRight[index] = dataRight[i];
                }
                signalPlotLeft.Label = labelLeft + "= " + dataLeft[dataLeft.Length - 1].ToString("0.##");
                signalPlotRight.Label = labelRight + "= " + dataRight[dataRight.Length - 1].ToString("0.##");
                nextIndex += dataLeft.Length;
                model.lineFrame.X = nextIndex % CAPACITY;
                if (render)
                {
                    model.plot.Render();
                }
            }
        }
        class ReplayModel
        {
            Model2S model;
            double[] bufferLeft;
            double[] bufferRight;
            double[] valuesLeft;
            double[] valuesRight;
            SignalPlot signalPlotLeft;
            SignalPlot signalPlotRight;
            private const int CAPACITY = 200;
            private const float EXTRA_SPACE = 0.1f;
            public ReplayModel(Model2S model)
            {
                this.model = model;
            }
            public void updateData(double[] left, double[] right)
            {
                bufferLeft = left;
                bufferRight = right;
                valuesLeft = new double[CAPACITY];
                valuesRight = new double[CAPACITY];
                signalPlotLeft = model.plot.Plot.AddSignal(valuesLeft, color: model.leftColor, label: "X");
                signalPlotRight = model.plot.Plot.AddSignal(valuesRight, color: model.rightColor, label: "Y");
                signalPlotLeft.MarkerSize = 0;
                signalPlotRight.MarkerSize = 0;
                maxRenderIndex = 0;
                model.plot.Plot.SetAxisLimitsX(xMin: 0, xMax: valuesLeft.Length + valuesLeft.Length * EXTRA_SPACE);
                model.plot.Render();
            }
            public void updateIndex(int index)
            {
                if (index < CAPACITY)
                {
                    Array.Copy(bufferLeft, 0, valuesLeft, 0, index);
                    Array.Copy(bufferRight, 0, valuesRight, 0, index);

                    signalPlotLeft.Label = labelLeft + "= " + bufferLeft[index].ToString("0.##");
                    signalPlotRight.Label = labelRight + "= " + bufferRight[index].ToString("0.##");

                    maxRenderIndex = index;

                    model.displacement = 0;
                    model.plot.Render();
                }
                else
                {
                    int startIndex = index - CAPACITY;
                    Array.Copy(bufferLeft, startIndex, valuesLeft, 0, CAPACITY);
                    Array.Copy(bufferRight, startIndex, valuesRight, 0, CAPACITY);

                    maxRenderIndex = CAPACITY - 1;

                    signalPlotLeft.Label = labelLeft + "= " + bufferLeft[index].ToString("0.##");
                    signalPlotRight.Label = labelRight + "= " + bufferRight[index].ToString("0.##");

                    model.displacement = startIndex;
                    model.plot.Render();
                }
            }
            private int maxRenderIndex
            {
                get
                {
                    return (int)model.lineFrame.X;
                }
                set
                {
                    signalPlotLeft.MaxRenderIndex = value;
                    signalPlotRight.MaxRenderIndex = value;
                    model.lineFrame.X = value;
                }
            }
        }
    }
}