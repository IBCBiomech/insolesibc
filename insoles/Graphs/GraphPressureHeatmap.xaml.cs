using MathNet.Numerics.LinearAlgebra;
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
            double?[,] dataNull = replaceWithNull(dataArray, Config.BACKGROUND);
            Dispatcher.Invoke(() => model.Draw(dataNull));
        }
        private double?[,] replaceWithNull(double[,] array, double value)
        {
            double?[,] result = new double?[array.GetLength(0), array.GetLength(1)];
            for(int i = 0; i < array.GetLength(0); i++)
            {
                for(int j = 0; j < array.GetLength(1); j++)
                {
                    if(array[i, j] == value)
                    {
                        result[i, j] = null;
                    }
                    else
                    {
                        result[i, j] = array[i, j];
                    }
                }
            }
            return result;
        }
    }
}
