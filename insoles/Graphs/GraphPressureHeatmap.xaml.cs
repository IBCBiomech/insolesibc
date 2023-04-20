#define REDUCE_SORT

using insoles.Common;
using insoles.Graphs.Converters;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Statistics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace insoles.Graphs
{
    /// <summary>
    /// Lógica de interacción para GraphPressureHeatmap.xaml
    /// </summary>
    public enum Metric { Avg, Max, Min}
    public class MetricEventArgs : EventArgs
    {
        public Metric metric { get; set; }
    }
    public partial class GraphPressureHeatmap : Page, INotifyPropertyChanged
    {
        private Foot foot;
        private PressureMap pressureMap;
        private AlgLib algLib;

        public event PropertyChangedEventHandler PropertyChanged;

        public delegate void MetricChangedEventHandler(object sender, MetricEventArgs e);
        public MetricChangedEventHandler MetricChanged;

        public ModelHeatmap model { get; private set; }
        private int avg_ = int.MinValue;
        private int max_ = int.MinValue;
        private int min_ = int.MinValue;
        public int avg
        {
            get
            {
                return avg_;
            }
            set
            {
                avg_ = value;
                NotifyPropertyChanged();
            }
        }
        public int max
        {
            get
            {
                return max_;
            }
            set
            {
                max_ = value;
                NotifyPropertyChanged();
            }
        }
        public int min
        {
            get
            {
                return min_;
            }
            set
            {
                min_ = value;
                NotifyPropertyChanged();
            }
        }
        public bool calculating 
        {
            set { 
                if (value)
                {
                    calculating_visibility = Visibility.Visible;
                }
                else
                {
                    calculating_visibility = Visibility.Collapsed;
                }
            }
            get
            {
                return calculating_visibility == Visibility.Visible;
            }
        }
        private Visibility calculating_visibility_ = Visibility.Collapsed;
        public Visibility calculating_visibility 
        { 
            get { return calculating_visibility_; }
            set
            {
                calculating_visibility_ = value;
                NotifyPropertyChanged();
            }
        }
        private Visibility graph_visibility_ = Visibility.Hidden;
        public Visibility graph_visibility
        {
            get
            {
                return graph_visibility_;
            }
            set
            {
                graph_visibility_ = value;
                NotifyPropertyChanged();
            }
        }
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public GraphPressureHeatmap()
        {
            InitializeComponent();
            calculating = false;
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            model = new ModelHeatmap(plot);
            if (mainWindow.foot == null)
            {
                mainWindow.initialized += (s, e) =>
                {
                    foot = mainWindow.foot;
                };
            }
            else
            {
                foot = mainWindow.foot;
            }
            /*
            if (mainWindow.pressureMap == null)
            {
                mainWindow.initialized += (s, e) =>
                {
                    pressureMap = mainWindow.pressureMap;
                };
            }
            else
            {
                pressureMap = mainWindow.pressureMap;
            }
            */
            if (mainWindow.algLib == null)
            {
                mainWindow.initialized += (s, e) =>
                {
                    algLib = mainWindow.algLib;
                };
            }
            else
            {
                algLib = mainWindow.algLib;
            }
            metric.SelectionChanged += (s, e) =>
            {
                selectionChanged();
            };
            metric.SelectedIndex = 0;
            DataContext = this;
        }
        private void selectionChanged()
        {
            string selected = metric.SelectedValue.ToString();
            Binding binding;
            switch (selected)
            {
                case var value when value == (string)FindResource("avgStr"):
                    binding = new Binding("avg");
                    binding.Source = this;
                    binding.Converter = (PreTextConverter)FindResource("avgConverter");
                    metricValue.SetBinding(TextBlock.TextProperty, binding);
                    NotifyPropertyChanged(nameof(avg));
                    MetricChanged?.Invoke(this, new MetricEventArgs { metric = Metric.Avg });
                    break;
                case var value when value == (string)FindResource("maxStr"):
                    binding = new Binding("max");
                    binding.Source = this;
                    binding.Converter = (PreTextConverter)FindResource("maxConverter");
                    metricValue.SetBinding(TextBlock.TextProperty, binding);
                    NotifyPropertyChanged(nameof(max));
                    MetricChanged?.Invoke(this, new MetricEventArgs { metric = Metric.Max });
                    break;
                case var value when value == (string)FindResource("minStr"):
                    binding = new Binding("min");
                    binding.Source = this;
                    binding.Converter = (PreTextConverter)FindResource("minConverter");
                    metricValue.SetBinding(TextBlock.TextProperty, binding);
                    NotifyPropertyChanged(nameof(min));
                    MetricChanged?.Invoke(this, new MetricEventArgs { metric = Metric.Min });
                    break;
                default:
                    Trace.WriteLine(selected);
                    Trace.WriteLine((string)FindResource("avgStr"));
                    Trace.WriteLine((string)FindResource("maxStr"));
                    Trace.WriteLine((string)FindResource("minStr"));
                    throw new Exception("seleccion max min avg error");
            }
        }
        public void DrawData(Matrix<float> data)
        {
            Matrix<double> dataDouble = data.Map(Convert.ToDouble);
            dataDouble = dataDouble.Transpose();
            double[,] dataArray = dataDouble.ToArray();
            double?[,] dataNull = Helpers.replace(dataArray, Config.BACKGROUND, null);
            Dispatcher.Invoke(() => model.Draw(dataNull));
            var filtered = dataDouble.Enumerate().Where(x => x != Config.BACKGROUND);
            avg = (int)filtered.Average();
            max = (int)filtered.Maximum();
            min = (int)filtered.Minimum();
            graph_visibility = Visibility.Visible;
        }
        public void DrawCPs(List<Tuple<double, double>> left, List<Tuple<double, double>> right)
        {
            void ReduceSorting(ref List<Tuple<double, double>> left, ref List<Tuple<double, double>> right, int NumResult)
            {
                left = ReduceCPsSorting(left, NumResult);
                right = ReduceCPsSorting(right, NumResult);
            }
            void ReduceByRanges(ref List<Tuple<double, double>> left, ref List<Tuple<double, double>> right, int range)
            {
                left = ReduceCPsByRanges(left, range);
                right = ReduceCPsByRanges(right, range);
            }
#if REDUCE_SORT
            ReduceByRanges(ref left, ref right, 1);
            ReduceSorting(ref left, ref right, 50);   
#else
            left = ReduceCPsByRanges(left);
            right = ReduceCPsByRanges(right);
#endif
            double[] xs;
            double[] ys;
            CPsToXsYs(left, right, out xs, out ys);
            Dispatcher.Invoke(() => model.DrawCenters(xs, ys));
        }
        private void CPsToXsYs(List<Tuple<double, double>> left, List<Tuple<double, double>> right, 
            out double[] xs, out double[] ys)
        {
            xs = new double[left.Count + right.Count];
            ys = new double[left.Count + right.Count];
            for (int i = 0; i < left.Count; i++)
            {
                xs[i] = left[i].Item1;
                ys[i] = left[i].Item2;
            }
            for (int i = 0; i < right.Count; i++)
            {
                xs[left.Count + i] = right[i].Item1;
                ys[left.Count + i] = right[i].Item2;
            }
        }
        private List<Tuple<double, double>> ReduceCPsSorting(List<Tuple<double, double>> cps, int NumResult, float minFactor = 0.75f)
        {
            cps.Sort((x, y) => y.Item2.CompareTo(x.Item2));
            List<Tuple<double, double>> cpsReduced = new List<Tuple<double, double>>();
            int NumAverage = cps.Count / NumResult;
            if(NumAverage < 1)
            {
                NumAverage = 1;
            }
            int index = 0;
            double item1Sum = 0;
            double item2Sum = 0;
            foreach (Tuple<double, double> cp in cps)
            {
                item1Sum += cp.Item1;
                item2Sum += cp.Item2;
                index++;
                if (index % NumAverage == 0)
                {
                    Tuple<double, double> cpReduced = new Tuple<double, double>(item1Sum / NumAverage, item2Sum/NumAverage);
                    cpsReduced.Add(cpReduced);
                    item1Sum = 0;
                    item2Sum = 0;
                }
            }
            int lastNumAverage = index % NumAverage;
            if (lastNumAverage > NumAverage * minFactor)
            {
                Tuple<double, double> cpReduced = new Tuple<double, double>(item1Sum / lastNumAverage, item2Sum / lastNumAverage);
                cpsReduced.Add(cpReduced);
            }
            return cpsReduced;
        }
        private List<Tuple<double, double>> ReduceCPsByRanges(List<Tuple<double, double>> cps, int range = 10)
        {
            Dictionary<double, List<double>> cpsToReduce = new Dictionary<double, List<double>>();
            foreach(Tuple<double, double> cp in cps)
            {
                double key = ReduceFunc(cp, range);
                List<double> xs;
                if(cpsToReduce.TryGetValue(key, out xs))
                {
                    xs.Add(cp.Item1);
                }
                else
                {
                    xs=new List<double>();
                    xs.Add(cp.Item1);
                    cpsToReduce[key] = xs;
                }
            }
            List<Tuple<double, double>> cpsReduced = new List<Tuple<double, double>>();
            foreach (double key in cpsToReduce.Keys)
            {
                List<double> cpsOfKey = cpsToReduce[key];
                cpsReduced.Add(new Tuple<double, double>(cpsOfKey.Average(), key));
            }
            return cpsReduced;
        }
        private double ReduceFunc(Tuple<double, double> cp, int range = 10)
        {
            int yInt = (int)Math.Round(cp.Item2);
            return (yInt / range) * range + (range / 2);
            // Al hacer la division entera corta por abajo. Para que quede la media de los puntos le sumo la mitad del rango
        }
    }
}
