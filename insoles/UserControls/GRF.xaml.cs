﻿using insoles.DataHolders;
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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ScottPlot.Plottable;
using System.Xml.Linq;
using System.Data;
using MathNet.Numerics;

namespace insoles.UserControls
{
    /// <summary>
    /// Lógica de interacción para GRF.xaml
    /// </summary>
    public enum Units { N, Kg}
   
    /// <summary>
    /// Clase para controlar GRF
    /// </summary>
    public partial class GRF : UserControl, INotifyPropertyChanged
    {
        private List<double> xs_temp_left;
        private List<double> ys_temp_left;
        private List<double> xs_temp_right;
        private List<double> ys_temp_right;

        private List<double> xs_left_N;
        private List<double> ys_left_N;
        private List<double> xs_right_N;
        private List<double> ys_right_N;

        private List<double> xs_left_Kg;
        private List<double> ys_left_Kg;
        private List<double> xs_right_Kg;
        private List<double> ys_right_Kg;

        List<double> XPoints = new List<double>();

        private Units _selectedUnits;
        private WpfPlot rangePlot;
        private VLine rangeVlabel;
        private List<VLine> listOfVlabels = new List<VLine>();

        public Units selectedUnits
        {
            get { return _selectedUnits; }
            set
            {
                _selectedUnits = value;
                NotifyPropertyChanged();
                if(value == Units.N)
                {
                    setN();
                    plot.Plot.Clear();
                    render();
                }
                else if(value == Units.Kg)
                {
                    setKg();
                    plot.Plot.Clear();
                    render();
                }
            }
        }
        public IEnumerable<Units> units
        {
            get { return Enum.GetValues(typeof(Units)).Cast<Units>(); }
        }
        private const double MIN_REFRESH = 0.1;
        private double lastTime = 0;
        public double time
        {
            set
            {
                timeLine.X = value;
                if (Math.Abs(value - lastTime) > MIN_REFRESH)
                {
                    lastTime = value;
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        plot.Refresh();
                    }));
                    double closest = FindClosest(xs_temp_left, value);
                    int indexClosest = xs_temp_left.IndexOf(closest);
                    leftPlot.Label = "Left = " + ys_temp_left[indexClosest].ToString("0.##");
                    rightPlot.Label = "Right = " + ys_temp_right[indexClosest].ToString("0.##");
                    total = (ys_temp_left[indexClosest] + ys_temp_right[indexClosest]).Round(2).ToString() +
                        selectedUnits.ToString();
                }
            }
        }
        private string _total;
        public string total
        {
            get { return _total; }
            set { _total = value; NotifyPropertyChanged(); }
        }
        private VLine timeLine;
        private ScatterPlot leftPlot;
        private ScatterPlot rightPlot;


        // Inicializa eventos
        public GRF()
        {
            InitializeComponent();
            DataContext = this;
            plot.MouseDown += PlotControl_MouseDown;
            plot.MouseMove += Plot_MouseMove;

            plot.Plot.Render();
        }

        // Imprime en el Xlabel las coordenadas XY (temporalmente)
        private void Plot_MouseMove(object sender, MouseEventArgs e)
        {
            (double x, double y) = plot.GetMouseCoordinates();
            plot.Plot.XLabel($"X: {x:F2}, Y: {y:F2}");
        }

        // Desactiva el efecto por defecto de left click para que no se dibujen 2 lineas
        private void PlotControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                e.Handled = true;
        }
        private void setN()
        {
            xs_temp_left = xs_left_N;
            ys_temp_left = ys_left_N;
            xs_temp_right = xs_right_N;
            ys_temp_right = ys_right_N;
        }
        private void setKg()
        {
            xs_temp_left = xs_left_Kg;
            ys_temp_left = ys_left_Kg;
            xs_temp_right = xs_right_Kg;
            ys_temp_right = ys_right_Kg;
        }
        public Task Update(GraphData data)
        {
            plot.Plot.Clear();
            xs_left_N = new();
            ys_left_N = new();
            xs_right_N = new();
            ys_right_N = new();

            xs_left_Kg = new();
            ys_left_Kg = new();
            xs_right_Kg = new();
            ys_right_Kg = new();

            foreach (var frame in data.frames)
            {
                FrameDataInsoles frameInsoles = (FrameDataInsoles)frame;
                DataInsole left = frameInsoles.left;
                DataInsole right = frameInsoles.right;

                xs_left_N.Add(frameInsoles.time);
                xs_right_N.Add(frameInsoles.time);
                ys_left_N.Add(left.totalPressure);
                ys_right_N.Add(right.totalPressure);

                xs_left_Kg.Add(frameInsoles.time);
                xs_right_Kg.Add(frameInsoles.time);
                ys_left_Kg.Add(left.totalPressure / 9.8);
                ys_right_Kg.Add(right.totalPressure / 9.8);
            }
            if (selectedUnits == Units.N)
                setN();
            else if (selectedUnits == Units.Kg) 
                setKg();

            render();

            return Task.CompletedTask;
        }

        //Este método habría que refactorizar el nombre, se confunde con plot.render()
        private void render()
        {
            if (ys_temp_left == null)
                return;
            double[] dts_left = StdDevCalculation(ys_temp_left);
            double[] dts_right = StdDevCalculation(ys_temp_right);


            plot.MouseDoubleClick += new MouseButtonEventHandler(MouseTracking);

            plot.Plot.Palette = ScottPlot.Palette.Amber;
            plot.Plot.Style(ScottPlot.Style.Seaborn);

            double[] xs_left = xs_temp_left.ToArray();
            double[] ys_left = ys_temp_left.ToArray();
            leftPlot = plot.Plot.AddScatterLines(xs_left, ys_left, System.Drawing.Color.DarkOrange, 2, label: "left");
            //plot.Plot.AddFillError(xs_left, ys_left, dts_left, System.Drawing.Color.FromArgb(50, System.Drawing.Color.IndianRed));

            double[] xs_right = xs_temp_right.ToArray();
            double[] ys_right = ys_temp_right.ToArray();
            rightPlot = plot.Plot.AddScatterLines(xs_right, ys_right, System.Drawing.Color.DarkBlue, 2, label: "right");
            //plot.Plot.AddFillError(xs_right, ys_right, dts_right, System.Drawing.Color.FromArgb(50, System.Drawing.Color.SkyBlue));

            plot.Plot.Legend();

            timeLine = plot.Plot.AddVerticalLine(0, color: System.Drawing.Color.DeepSkyBlue);

            plot.Render();
        }

        // Este método es el que añade las barras verticales
        private void MouseTracking(object sender, MouseButtonEventArgs e)
        {
            if ( XPoints.Count >= 0 && XPoints.Count <2)
            {
                (double x, double y) = plot.GetMouseCoordinates();
                rangeVlabel = plot.Plot.AddVerticalLine(x, color: System.Drawing.Color.IndianRed, style: ScottPlot.LineStyle.Solid);
                rangeVlabel.PositionLabel = true;
                rangeVlabel.DragEnabled = true;
                plot.Render();
                listOfVlabels.Add(rangeVlabel);
                XPoints.Add(x);
            }
            else
            {
                MessageBox.Show("No se puede hacer rango con más de dos puntos. Elimina la STDDEV actual","Info",MessageBoxButton.OK, MessageBoxImage.Warning);
            }
       

        }

        // Cálculo de la desviación estándar de cada punto
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

                //El sqrt para que el gráfico no se vaya con valores grandes
                double diferenciaSqrt = Math.Sqrt(diferenciaCuadrada);

                double std = Math.Min(diferenciaSqrt, valor);
                // Agregar la diferencia cuadrada a la lista
                desviacionesTipicas.Add(std);
            }

            double[] dts = desviacionesTipicas.ToArray();

            return dts;
        }
        // Cálculo de la desviación estándar
        private double[] StdDevCalculation(List<double> ys_temp)
        {
            double avg = ys_temp.Average();
            double sum = ys_temp.Select(x => (avg - x) * (avg - x)).Sum();

            double std = Math.Sqrt(sum / ys_temp.Count);
            return Enumerable.Repeat(std, ys_temp.Count).ToArray();
        }
        private void StdDevButton1_Click(object sender, RoutedEventArgs e)
        {
            rangePlot = new WpfPlot();  
        
            double FirstClosest = FindClosest(xs_temp_left, XPoints[0]);
            double LastClosest = FindClosest(xs_temp_left, XPoints[1]);

            int indexFirstClosest = xs_temp_left.IndexOf(FirstClosest);
            int indexLastClosest = xs_temp_left.IndexOf(LastClosest);

            List<double> listXleft = xs_temp_left.GetRange(indexFirstClosest, (indexLastClosest - indexFirstClosest));
            List<double> listYleft = ys_temp_left.GetRange(indexFirstClosest, (indexLastClosest - indexFirstClosest));

            double[] dataXleft = listXleft.ToArray();
            double[] dataYleft = listYleft.ToArray();

            double[] stddevleft = StdDevCalculation(listYleft);
            rangePlot.Plot.AddScatterLines(dataXleft, dataYleft, System.Drawing.Color.DarkOrange, 2);
            rangePlot.Plot.AddFillError(dataXleft, dataYleft, stddevleft, System.Drawing.Color.FromArgb(50, System.Drawing.Color.Green));

            List<double> listXright = xs_temp_right.GetRange(indexFirstClosest, (indexLastClosest - indexFirstClosest));
            List<double> listYright = ys_temp_right.GetRange(indexFirstClosest, (indexLastClosest - indexFirstClosest));

            double[] dataXright = listXright.ToArray();
            double[] dataYright = listYright.ToArray();

            double[] stddevright = StdDevCalculation(listYright);
            rangePlot.Plot.AddScatterLines(dataXright, dataYright, System.Drawing.Color.DarkBlue, 2);
            rangePlot.Plot.AddFillError(dataXright, dataYright, stddevright, System.Drawing.Color.FromArgb(50, System.Drawing.Color.SkyBlue));

            rangePlot.Render();

            rangePlot.SetValue(Grid.RowProperty, 1);
            grid.Children.Add(rangePlot);

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
        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void ClearGraphButton_Click(object sender, RoutedEventArgs e)
        {
            
            grid.Children.Remove(rangePlot);
          
            foreach (var el in listOfVlabels)
            {
                plot.Plot.Remove(el);

            }
            listOfVlabels.Clear();
            XPoints.Clear();
            plot.Render();
            rangePlot.Render();
        }
    }
}
