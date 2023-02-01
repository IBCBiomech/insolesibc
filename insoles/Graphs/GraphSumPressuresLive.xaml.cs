using insoles.TimeLine;
using ScottPlot;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Drawing;
using System.Timers;
using System.Diagnostics;

namespace insoles.Graphs
{
    /// <summary>
    /// Lógica de interacción para GraphSumPressuresLive.xaml
    /// </summary>
    public partial class GraphSumPressuresLive : Page
    {
        const int CAPACITY = 200;
        double[] valuesLeft = new double[CAPACITY];
        double[] valuesRight = new double[CAPACITY];

        private int nextIndex = 0;

        SignalPlot signalPlotLeft;
        SignalPlot signalPlotRight;

        private Color leftColor = Config.colorX;
        private Color rightColor = Config.colorY;

        private const string labelLeft = "Left";
        private const string labelRight = "Right";

        Timer timer = new Timer();
        public GraphSumPressuresLive()
        {
            InitializeComponent();
            signalPlotLeft = plot.Plot.AddSignal(valuesLeft, color: leftColor, label: labelLeft);
            signalPlotRight = plot.Plot.AddSignal(valuesRight, color: rightColor, label: labelRight);
            plot.Plot.AxisAutoX(margin: 0);
            plot.IsHitTestVisible = false;
            //plot.RightClicked -= plot.DefaultRightClickEvent;
            
            
            plot.Refresh();
        }
        public void initCapture()
        {
            /*
            timer.Elapsed += updateGraph;
            timer.Interval = 20;
            timer.Start();
            */
            /*
            double[] valuesLeft = new double[CAPACITY];
            double[] valuesRight = new double[CAPACITY];
            plot.Plot.Remove(signalPlotLeft);
            plot.Plot.Remove(signalPlotRight);
            signalPlotLeft = plot.Plot.AddSignal(valuesLeft, color: leftColor, label: labelLeft);
            signalPlotRight = plot.Plot.AddSignal(valuesRight, color: rightColor, label: labelRight);
            */
        }
        private void updateGraph(object sender, EventArgs e)
        {
            Random random = new Random();
            float[] left = new float[4];
            float[] right = new float[4];
            for(int i = 0; i < 4; i++)
            {
                left[i] = random.NextSingle() * 100;
                right[i] = random.NextSingle() * 100;
            }
            drawData(left, right);
        }
        public async void drawData(float[] left, float[] right)
        {
            if (nextIndex >= CAPACITY)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Array.Copy(valuesLeft, left.Length, valuesLeft, 0, valuesLeft.Length - left.Length);
                    Array.Copy(valuesRight, right.Length, valuesRight, 0, valuesRight.Length - right.Length);
                    /*
                    for(int i = 0; i < left.Length; i++)
                    {
                        valuesLeft[valuesLeft.Length - left.Length + i] = left[i];
                    }
                    for (int i = 0; i < left.Length; i++)
                    {
                        valuesRight[valuesRight.Length - right.Length + i] = right[i];
                    }
                    */
                    Array.Copy(left, 0, valuesLeft, valuesLeft.Length - left.Length, left.Length);
                    Array.Copy(right, 0, valuesRight, valuesRight.Length - right.Length, right.Length);
                    plot.Plot.SetAxisLimits(yMin: 0, yMax: Math.Max(valuesLeft.Max(), valuesRight.Max()) * 1.2);
                    plot.Render();
                });
            }
            else
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Array.Copy(left, 0, valuesLeft, nextIndex, left.Length);
                    Array.Copy(right, 0, valuesRight, nextIndex, right.Length);
                    plot.Plot.SetAxisLimits(yMin: 0, yMax: Math.Max(valuesLeft.Max(), valuesRight.Max()) * 1.2);
                    plot.Render();
                    nextIndex+= left.Length;
                });
            }
            // Si CAPACITY NO es multiplo del numero de datos del paquete 
            // Hay que poner otro caso
        }
        public async void clearData()
        {
            /*
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                nextIndex = 0;
                signalPlotRight.MaxRenderIndex = nextIndex;
                plot.Render();
            });
            */
        }
    }
}
