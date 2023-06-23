﻿using insoles.Controlls;
using insoles.DataHolders;
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
using System.Threading.Tasks;
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
        private Text L;
        private Text R;

        private int colorbarMax;

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
        private Metric _selectedMetric;
        public Metric selectedMetric {
            get
            {
                return _selectedMetric;
            }
            set
            {
                _selectedMetric = value;
                if(pressure_maps_metrics != null && !animate) 
                {
                    DrawData(pressure_maps_metrics[selectedMetric]);
                    DrawCenters(centersXs, centersYs);
                }
                NotifyPropertyChanged();
            }
        }
        public IEnumerable<Metric> metrics
        {
            get { return Enum.GetValues(typeof(Metric)).Cast<Metric>(); }
        }
        private bool _animate;
        public bool animate { 
            get
            {
                return _animate;
            }
            set
            {
                animate = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(graph_loaded));
            }
        }
        public bool graph_loaded { 
            get
            {
                if(animate)
                {
                    return pressure_maps_live != null;
                }
                else
                {
                    return pressure_maps_metrics != null;
                }
            } 
        }
        Dictionary<Metric, Matrix<float>>? _pressure_maps_metrics;
        public Dictionary<Metric, Matrix<float>>? pressure_maps_metrics
        {
            get
            {
                return _pressure_maps_metrics;
            }
            set
            {
                _pressure_maps_metrics = value;
                if (pressure_maps_metrics != null && !animate)
                {
                    DrawData(pressure_maps_metrics[selectedMetric]);
                    DrawCenters(centersXs, centersYs);
                }
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(graph_loaded));
            }
        }
        List<Matrix<float>>? _pressure_maps_live;
        public List<Matrix<float>>? pressure_maps_live
        {
            get
            {
                return _pressure_maps_live;
            }
            set
            {
                _pressure_maps_live = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(graph_loaded));
            }
        }
        private double[] centersXs;
        private double[] centersYs;
        public Heatmap()
        {
            InitializeComponent();
            DataContext = this;
            
        }
        public Task UpdateLimits(GraphData data)
        {
            int max = 0;
            for(int i = 0; i < data.length; i++)
            {
                FrameDataInsoles frame = (FrameDataInsoles)data[i];
                DataInsole left = frame.left;
                foreach(var pressure in left.pressures.Values)
                {
                    if(pressure > max)
                    {
                        max = (int)pressure;
                    }
                }
                DataInsole right = frame.right;
                foreach (var pressure in right.pressures.Values)
                {
                    if (pressure > max)
                    {
                        max = (int)pressure;
                    }
                }
            }
            colorbarMax = max;
            return Task.CompletedTask;
        }
        public void ClearData()
        {
            pressure_maps_metrics = null;
            pressure_maps_live = null;
        }
        public void DrawData(Matrix<float> data)
        {
            Matrix<double> dataDouble = data.Map(Convert.ToDouble);
            dataDouble = dataDouble.Transpose();
            double[,] dataArray = dataDouble.ToArray();
            double?[,] dataNull = HelperFunctions.replace(dataArray, BACKGROUND, null);
            var filtered = dataDouble.Enumerate().Where(x => x != BACKGROUND);
            avg = (int)filtered.Average();
            max = (int)filtered.Maximum();
            min = (int)filtered.Minimum();
            Dispatcher.Invoke(() => Draw(dataNull));
            if(L == null)
                L = plot.Plot.AddText("L", 0, data.RowCount / 2, size: 25, color: Color.DarkGray);
            if(R == null)
                R = plot.Plot.AddText("R", data.ColumnCount * 1.25, data.RowCount / 2, size: 25, color: Color.DarkGray);
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
            heatmap.Update(data, min: 0, max: colorbarMax);
            heatmap.Smooth = true;
            colorbar = plot.Plot.AddColorbar(heatmap);
            plot.Plot.Margins(0, 0);
            plot.Plot.MoveFirst(heatmap);
            plot.Refresh();
        }
        #region CPS
        public Task CalculateCenters(List<Tuple<double, double>> left, List<Tuple<double, double>> right)
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
            double CalculateSlope(Dictionary<Sensor, (float, float)> centers)
            {
                float item1Top = (centers[Sensor.HALLUX].Item1 + centers[Sensor.TOES].Item1) / 2;
                float item2Top = (centers[Sensor.HALLUX].Item2 + centers[Sensor.TOES].Item2) / 2;
                (float, float) top = (item1Top, item2Top);
                float item1Bot = (centers[Sensor.HEEL_L].Item1 + centers[Sensor.HEEL_R].Item1) / 2;
                float item2Bot = (centers[Sensor.HEEL_L].Item2 + centers[Sensor.HEEL_R].Item2) / 2;
                (float, float) bot = (item1Bot, item2Bot);
                return 0;
            }
            if(slopeLeft == null)
            {
                slopeLeft = CalculateSlope(pressureMap.centersLeft);
            }
            if(slopeRight == null)
            {
                slopeRight = CalculateSlope(pressureMap.centersRight);
            }
            ReduceByRanges(ref left, ref right, 1);
            ReduceSorting(ref left, ref right, 50);   
#else
            left = ReduceCPsByRanges(left);
            right = ReduceCPsByRanges(right);
#endif
            CPsToXsYs(left, right, out centersXs, out centersYs);
            return Task.CompletedTask;
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
            if (NumAverage < 1)
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
                    Tuple<double, double> cpReduced = new Tuple<double, double>(item1Sum / NumAverage, item2Sum / NumAverage);
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
            foreach (Tuple<double, double> cp in cps)
            {
                double key = ReduceFunc(cp, range);
                List<double> xs;
                if (cpsToReduce.TryGetValue(key, out xs))
                {
                    xs.Add(cp.Item1);
                }
                else
                {
                    xs = new List<double>
                    {
                        cp.Item1
                    };
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
        #endregion CPs
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
