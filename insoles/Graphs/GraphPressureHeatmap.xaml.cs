using MathNet.Numerics.LinearAlgebra;
using System;
using System.Windows;
using System.Windows.Controls;

namespace insoles.Graphs
{
    /// <summary>
    /// Lógica de interacción para GraphPressureHeatmap.xaml
    /// </summary>
    public partial class GraphPressureHeatmap : Page
    {
        private Foot foot;
        public ModelHeatmap model { get; private set; }
        public GraphPressureHeatmap()
        {
            InitializeComponent();
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
            model.Draw(dataNull);
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
