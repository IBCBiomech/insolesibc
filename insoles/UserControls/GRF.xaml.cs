using insoles.DataHolders;
using ScottPlot.Styles;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Diagnostics;

namespace insoles.UserControls
{
    /// <summary>
    /// Lógica de interacción para GRF.xaml
    /// </summary>
    public partial class GRF : UserControl
    {
        private List<double> xs_temp_left;
        private List<double> ys_temp_left;
        private List<double> xs_temp_right;
        private List<double> ys_temp_right;
        List<double> XPoints = new List<double>();
        public GRF()
        {
            InitializeComponent();
            DataContext = this;
        }
        public Task Update(GraphData data)
        {
            xs_temp_left = new List<double>();
            ys_temp_left = new List<double>();
            xs_temp_right = new List<double>();
            ys_temp_right = new List<double>();
            foreach (var frame in data.frames)
            {
                FrameDataInsoles frameInsoles = (FrameDataInsoles)frame;
                DataInsole left = frameInsoles.left;
                DataInsole right = frameInsoles.right;
                xs_temp_left.Add(frameInsoles.time);
                xs_temp_right.Add(frameInsoles.time);
                ys_temp_left.Add(left.totalPressure);
                ys_temp_right.Add(right.totalPressure);
            }
            double[] dts_left = StdDevPointCalculation(ys_temp_left);
            double[] dts_right = StdDevPointCalculation(ys_temp_right);

            plot.MouseLeftButtonUp += new MouseButtonEventHandler(MouseTracking);

            plot.Plot.Palette = ScottPlot.Palette.Amber;
            plot.Plot.Style(ScottPlot.Style.Seaborn);

            double[] xs_left = xs_temp_left.ToArray();
            double[] ys_left = ys_temp_left.ToArray();
            plot.Plot.AddScatterLines(xs_left, ys_left, System.Drawing.Color.DarkOrange, 5, label:"left");
            //plot.Plot.AddFillError(xs_left, ys_left, dts_left, System.Drawing.Color.FromArgb(50, System.Drawing.Color.IndianRed));

            double[] xs_right = xs_temp_right.ToArray();
            double[] ys_right = ys_temp_right.ToArray();
            plot.Plot.AddScatterLines(xs_right, ys_right, System.Drawing.Color.DarkBlue, 5, label:"right");
            //plot.Plot.AddFillError(xs_right, ys_right, dts_right, System.Drawing.Color.FromArgb(50, System.Drawing.Color.SkyBlue));

            plot.Plot.Legend();

            plot.Render();

            return Task.CompletedTask;
        }
        // Este método es el que añade las barras verticales
        private void MouseTracking(object sender, MouseButtonEventArgs e)
        {

            (double x, double y) = plot.GetMouseCoordinates();
            var vlabel = plot.Plot.AddVerticalLine(x, color: System.Drawing.Color.IndianRed, style: ScottPlot.LineStyle.Solid);
            vlabel.PositionLabel = true;
            vlabel.DragEnabled = true;
            plot.Render();


            XPoints.Add(x);


        }
        /// <summary>
        /// Cálculo de la desviación estándar de cada punto
        /// </summary>
        /// <param name="ys_temp">Array de entrada: valores YE</param>
        /// <returns>double[]</returns>
        private double[] StdDevPointCalculation(List<double> ys_temp)
        {
            double media = ys_temp.Average();

            // Calcular la desviación típica para cada punto
            List<double> desviacionesTipicas = new List<double>();
            foreach (double valor in ys_temp)
            {
                // Calcular la diferencia entre el valor y la media
                double diferencia = valor - media;

                // Elevar al cuadrado la diferencia
                double diferenciaCuadrada = Math.Pow(diferencia, 2);

                // Agregar la diferencia cuadrada a la lista
                desviacionesTipicas.Add(diferenciaCuadrada);
            }

            double[] dts = desviacionesTipicas.ToArray();

            return dts;
        }
        private void StdDevButton1_Click(object sender, RoutedEventArgs e)
        {
            WpfPlot plot2 = new WpfPlot();
            //double[] dataX = new double[] { 1, 2, 3, 4, 5 };
            //double[] dataY = new double[] { 1, 4, 9, 16, 25 };
            Trace.WriteLine(XPoints[0]);
            Trace.WriteLine(XPoints[1]);

            double FirstClosest = FindClosest(xs_temp_left, XPoints[0]);
            double LastClosest = FindClosest(xs_temp_left, XPoints[1]);

            int indexFirstClosest = xs_temp_left.IndexOf(FirstClosest);
            int indexLastClosest = xs_temp_left.IndexOf(LastClosest);

            List<double> listXleft = xs_temp_left.GetRange(indexFirstClosest, indexLastClosest);
            List<double> listYleft = ys_temp_left.GetRange(indexFirstClosest, indexLastClosest);

            double[] dataXleft = listXleft.ToArray();
            double[] dataYleft = listYleft.ToArray();

            double[] stddevleft = StdDevPointCalculation(listYleft);
            plot2.Plot.AddScatterLines(dataXleft, dataYleft, System.Drawing.Color.DarkOrange, 5);
            plot2.Plot.AddFillError(dataXleft, dataYleft, stddevleft, System.Drawing.Color.FromArgb(50, System.Drawing.Color.Green));

            List<double> listXright = xs_temp_right.GetRange(indexFirstClosest, indexLastClosest);
            List<double> listYright = ys_temp_right.GetRange(indexFirstClosest, indexLastClosest);

            double[] dataXright = listXright.ToArray();
            double[] dataYright = listYright.ToArray();

            double[] stddevright = StdDevPointCalculation(listYleft);
            plot2.Plot.AddScatterLines(dataXright, dataYright, System.Drawing.Color.DarkBlue, 5);
            plot2.Plot.AddFillError(dataXright, dataYright, stddevright, System.Drawing.Color.FromArgb(50, System.Drawing.Color.SkyBlue));

            plot2.Render();

            plot2.SetValue(Grid.RowProperty, 1);
            grid.Children.Add(plot2);



        }
        // Función que sí utilizamos para obtener el número más cercano de una lista
        private double FindClosest(List<double> data, double value)
        {
            if (value <= data[0])
            {
                return data[0];
            }
            for (var i = 0; i < data.Count - 1; i++)
            {
                if (data[i] <= value && value <= data[i + 1])
                {
                    return data[i];
                }

            }

            return data.Last();
        }

        // Este Código pertenece a una función que retonra el número más próximo de una lista. 
        // Est una segunda opción que no utilizamos por ahora
        private static double? FindNearestValue(IEnumerable<double> arr, double d)
        {
            var minDist = double.MaxValue;
            double? nearestValue = null;

            foreach (var x in arr)
            {
                var dist = Math.Abs(x - d);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearestValue = x;
                }
            }
            return nearestValue;
        }
    }
}
