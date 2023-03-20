using insoles.Common;
using insoles.Graphs.Converters;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Statistics;
using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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
    }
}
