using insoles.Common;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Statistics;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace insoles.Graphs
{
    /// <summary>
    /// Lógica de interacción para GraphPressureHeatmap.xaml
    /// </summary>
    public partial class GraphPressureHeatmap : Page, INotifyPropertyChanged
    {
        private Foot foot;

        public event PropertyChangedEventHandler PropertyChanged;

        public ModelHeatmap model { get; private set; }
        private int max_ = int.MinValue;
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
            DataContext = this;
        }
        public void DrawData(Matrix<float> data)
        {
            Matrix<double> dataDouble = data.Map(Convert.ToDouble);
            dataDouble = dataDouble.Transpose();
            double[,] dataArray = dataDouble.ToArray();
            double?[,] dataNull = Helpers.replace(dataArray, Config.BACKGROUND, null);
            Dispatcher.Invoke(() => model.Draw(dataNull));
            max = (int)dataDouble.Enumerate().Maximum();
            graph_visibility = Visibility.Visible;
        }
    }
}
