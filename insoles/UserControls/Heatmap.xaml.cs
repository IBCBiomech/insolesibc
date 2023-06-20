using insoles.Controlls;
using insoles.Utilities;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Statistics;
using ScottPlot.Drawing;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace insoles.UserControls
{
    /// <summary>
    /// Lógica de interacción para Heatmap.xaml
    /// </summary>
    public enum Metric { Avg, Max, Min}
    public class MetricEventArgs : EventArgs
    {
        public Metric metric { get; set; }
    }
    public partial class Heatmap : UserControl, INotifyPropertyChanged
    {
        const int BACKGROUND = -1;

        private ScottPlot.Plottable.Heatmap heatmap;
        private Colorbar colorbar;
        private ScatterPlot centers;

        Dictionary<Metric, Matrix<float>> pressure_maps_metrics;
        List<Matrix<float>> pressure_maps_live;

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
        public Heatmap()
        {
            InitializeComponent();
            
        }
        public void SetDataMetrics(Dictionary<Metric, Matrix<float>> pressure_maps)
        {
            pressure_maps_metrics = pressure_maps;
        }
        public void SetDataLive(List<Matrix<float>> pressure_maps)
        {
            pressure_maps_live = pressure_maps;
        }
        public void DrawData(Matrix<float> data)
        {
            Matrix<double> dataDouble = data.Map(Convert.ToDouble);
            dataDouble = dataDouble.Transpose();
            double[,] dataArray = dataDouble.ToArray();
            double?[,] dataNull = HelperFunctions.replace(dataArray, BACKGROUND, null);
            Dispatcher.Invoke(() => Draw(dataNull));
            var filtered = dataDouble.Enumerate().Where(x => x != BACKGROUND);
            avg = (int)filtered.Average();
            max = (int)filtered.Maximum();
            min = (int)filtered.Minimum();
            //graph_visibility = Visibility.Visible;
        }
        private void Draw(double?[,] data)
        {
            if (heatmap != null)
            {
                plot.Plot.Clear(heatmap.GetType());
            }
            if (colorbar != null)
            {
                plot.Plot.Clear(colorbar.GetType());
            }
            IColormap colormap = extendColormap(Colormap.Jet, Color.LightGray, (color, extended, ratio) => extended, extendSize: 15, totalSize: 256);
            heatmap = plot.Plot.AddHeatmap(data, colormap: new Colormap(colormap));
            heatmap.Update(data, min: 0, max: max);
            heatmap.Smooth = true;
            colorbar = plot.Plot.AddColorbar(heatmap);
            plot.Plot.Margins(0, 0);
            plot.Plot.MoveFirst(heatmap);
            plot.Refresh();
        }
        public void DrawCenters(double[] xs, double[] ys)
        {
            if (centers != null)
            {
                plot.Plot.Clear(centers.GetType());
            }
            centers = plot.Plot.AddScatter(xs, ys, lineWidth: 0, color: Color.WhiteSmoke);
            plot.Plot.MoveLast(centers);
        }
        private IColormap extendColormap(Colormap colormapBase, Color colorExtend,
            Func<Color, Color, float, Color> interpolationFunction,
            int extendSize = 20, int totalSize = 100)
        {
            Color[] colors = new Color[totalSize];
            for (int i = extendSize; i < totalSize; i++)
            {
                colors[i] = colormapBase.GetColor((double)(i - extendSize) / (totalSize - extendSize));
            }
            Color color0 = colormapBase.GetColor(0);
            for (int i = 0; i < extendSize; i++)
            {
                colors[i] = interpolationFunction(color0, colorExtend, (float)i / extendSize);
            }
            Color function(double value)
            {
                int index = (int)(value * totalSize);
                index = Math.Min(Math.Max(index, 0), totalSize - 1);
                return colors[index];
            }
            return new CustomColormap(new Func<double, Color>(function),
                colormapBase.Name + "Extended");
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
