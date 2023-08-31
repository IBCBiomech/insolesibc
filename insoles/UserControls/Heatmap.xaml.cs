using insoles.Controlls;
using insoles.DataHolders;
using insoles.States;
using insoles.Utilities;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Statistics;
using ScottPlot;
using ScottPlot.Drawing;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.DirectoryServices;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Markup;

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
    public partial class Heatmap : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {
        const int BACKGROUND = -1;
        const int PERCENTIL_MAX = 10;

        private ScottPlot.Plottable.Heatmap heatmap;
        private Colorbar colorbar;
        private ScatterPlot centers;
        private Text L;
        private Text R;

        private object drawingCentersLock = new object();

        private double maxTime;

        private int colorbarMax;
        private int colorbarPercentile;

        double xMin;
        double xMax;
        double yMin;
        double yMax;

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
                if(pressure_maps_metrics_range != null && !animate)
                {
                    DrawDataWPF(pressure_maps_metrics_range[selectedMetric], plot);
                    DrawCenters(centersXsRange, centersYsRange, plot);
                }
                else if(pressure_maps_metrics != null && !animate) 
                {
                    DrawDataWPF(pressure_maps_metrics[selectedMetric], plot);
                    DrawCenters(centersXs, centersYs, plot);
                }
                NotifyPropertyChanged();
            }
        }
        public IEnumerable<Metric> metrics
        {
            get { return Enum.GetValues(typeof(Metric)).Cast<Metric>(); }
        }
        public IEnumerable<int> posibleFramesTaken
        {
            get
            {
                return new List<int>() { 1, 10, 50, 100, 500 };
            }
        }
        public int framesTaken
        {
            get
            {
                return state.framesTaken;
            }
            set
            {
                Trace.WriteLine(value);
                state.framesTaken = value;
                NotifyPropertyChanged();
            }
        }
        private bool _animate;
        public bool animate { 
            get
            {
                return _animate;
            }
            set
            {
                _animate = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(graph_loaded));
                if (value)
                {
                    if (graph_loaded)
                        DrawDataWPF(pressure_maps_live[Math.Min(frame, pressure_maps_live.Count - 1)], plot);
                }
                else
                {
                    if (graph_loaded)
                        if (pressure_maps_metrics_range == null)
                        {
                            DrawDataWPF(pressure_maps_metrics[selectedMetric], plot);
                            DrawCenters(centersXs, centersYs, plot);
                        }
                        else
                        {
                            DrawDataWPF(pressure_maps_metrics_range[selectedMetric], plot);
                            DrawCenters(centersXsRange, centersYsRange, plot);
                        }
                }         
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
        private int _frame = 0;
        private int frame
        {
            get
            {
                return _frame;
            }
            set
            {
                if(value != _frame)
                {
                    _frame = value;
                    if(animate && graph_loaded)
                    {
                        DrawDataWPF(pressure_maps_live[Math.Min(value, pressure_maps_live.Count - 1)], plot);
                    }
                }
            }
        }
        private const double TIME_PER_FRAME = 0.01;
        public double time
        {
            set
            {
                frame = (int)Math.Floor(value / (state.framesTaken * TIME_PER_FRAME));
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
                if(pressure_maps_metrics != null)
                    SetAxisLimits(pressure_maps_metrics[Metric.Avg]);
                if (pressure_maps_metrics != null && !animate)
                {
                    DrawDataWPF(pressure_maps_metrics[selectedMetric], plot);
                    DrawCenters(centersXs, centersYs, plot);
                }
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(graph_loaded));
            }
        }
        Dictionary<Metric, Matrix<float>>? _pressure_maps_metrics_range;
        public Dictionary<Metric, Matrix<float>>? pressure_maps_metrics_range
        {
            get
            {
                return _pressure_maps_metrics_range;
            }
            set
            {
                _pressure_maps_metrics_range = value;
                if (value != null)
                {
                    SetAxisLimits(value[Metric.Avg]);
                    if (!animate)
                    {
                        DrawDataWPF(pressure_maps_metrics_range[selectedMetric], plot);
                        DrawCenters(centersXsRange, centersYsRange, plot);
                    }
                }
                else
                {
                    if (pressure_maps_metrics != null)
                        SetAxisLimits(pressure_maps_metrics[Metric.Avg]);
                    if (pressure_maps_metrics != null && !animate)
                    {
                        DrawDataWPF(pressure_maps_metrics[selectedMetric], plot);
                        DrawCenters(centersXs, centersYs, plot);
                    }
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
        private double[] centersXsRange;
        private double[] centersYsRange;
        public AnalisisState state { get;set; }
        public Heatmap(AnalisisState state)
        {
            InitializeComponent();
            this.state = state;
            framesTaken = 500;
            DataContext = this;
            plot.Plot.Style(dataBackground: Color.White);
        }
        public Task UpdateLimits(GraphData data)
        {
            List<int> pressures = new List<int>();
            for (int i = 0; i < data.length; i++)
            {
                FrameDataInsoles frame = (FrameDataInsoles)data[i];
                DataInsole left = frame.left;
                foreach (var pressure in left.pressures.Values)
                {
                    pressures.Add((int)pressure);
                }
                DataInsole right = frame.right;
                foreach (var pressure in right.pressures.Values)
                {
                    pressures.Add((int)pressure);
                }
            }
            pressures.Sort((x, y) => y.CompareTo(x));
            colorbarPercentile = pressures[pressures.Count * PERCENTIL_MAX / 100];
            colorbarMax = pressures[0];
            /* MAX Pressure
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
            */
            maxTime = data.maxTime;
            return Task.CompletedTask;
        }
        private void SetAxisLimits(Matrix<float> matrix)
        {
            xMin = matrix.ColumnCount * 0.15;
            xMax = matrix.ColumnCount * 0.75;
            yMin = 0;
            yMax = matrix.RowCount * 0.75;
            plot.Plot.SetInnerViewLimits(xMin, xMax, yMin, yMax);
            plot.Plot.SetOuterViewLimits(yMin: 0);

            plot.Plot.SetAxisLimits(xMin, xMax, yMin, yMax);
            plot.Plot.AxisScaleLock(true);
            if (L == null)
                L = plot.Plot.AddText("L", matrix.ColumnCount * 0.2, matrix.RowCount * 0.25, size: 50, color: Color.DarkGray);
            if (R == null)
                R = plot.Plot.AddText("R", matrix.ColumnCount * 1.05, matrix.RowCount * 0.25, size: 50, color: Color.DarkGray);
        }
        public void ClearData()
        {
            pressure_maps_metrics = null;
            pressure_maps_live = null;
        }
        private void DrawDataWPF(Matrix<float> data, WpfPlot plot) 
        {
            int max;
            if (selectedMetric == Metric.Max && !animate)
            {
                max = colorbarMax;
            }
            else
            {
                max = colorbarPercentile;
            }
            DrawData(data, plot.Plot, ref heatmap, ref colorbar, max);
            Dispatcher.Invoke(() => plot.Refresh());
        }
        public void DrawData(Matrix<float> data, Plot plot,
            ref ScottPlot.Plottable.Heatmap heatmap, ref Colorbar colorbar, int maxBar)
        {
            Matrix<double> dataDouble = data.Map(Convert.ToDouble);
            dataDouble = dataDouble.Transpose();
            double[,] dataArray = dataDouble.ToArray();
            double?[,] dataNull = HelperFunctions.replace(dataArray, BACKGROUND, null);
            var filtered = dataDouble.Enumerate().Where(x => x != BACKGROUND);
            avg = (int)filtered.Average();
            max = (int)filtered.Maximum();
            min = (int)filtered.Minimum();
            Draw(dataNull, plot, ref heatmap, ref colorbar, maxBar);
        }
        private void Draw(double?[,] data, Plot plot, 
            ref ScottPlot.Plottable.Heatmap heatmap, ref Colorbar colorbar, int maxBar)
        {
            if (heatmap != null)
            {
                plot.Clear(heatmap.GetType());
            }
            if (colorbar != null)
            {
                plot.Clear(colorbar.GetType());
            }
            IColormap colormap = extendColormap(Colormap.Jet, HelperFunctions.MakeColorDarker(Color.WhiteSmoke, 10), HelperFunctions.noInterpolate, extendSize: 10, totalSize: 256);
            heatmap = plot.AddHeatmap(data, colormap: new Colormap(colormap));
            heatmap.Update(data, min: 0, max: maxBar);
            heatmap.Smooth = true;
            colorbar = plot.AddColorbar(heatmap);
            plot.Margins(0, 0);
            plot.MoveFirst(heatmap);
            plot.SetAxisLimits(xMin, xMax, yMin, yMax);
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
        public Task CalculateCentersRange(List<Tuple<double, double>> left, List<Tuple<double, double>> right)
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
            CPsToXsYs(left, right, out centersXsRange, out centersYsRange);
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
        public void DrawCenters(double[] xs, double[] ys, ScottPlot.WpfPlot plot)
        {
            lock (drawingCentersLock)
            {
                if (centers != null)
                {
                    plot.Plot.Clear(centers.GetType());
                }
                centers = plot.Plot.AddScatter(xs, ys, lineWidth: 0, color: Color.WhiteSmoke);
                plot.Plot.MoveLast(centers);
                Dispatcher.Invoke(() => plot.Refresh());
            }
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
        public Task SaveFigs()
        {
            ScottPlot.Plottable.Heatmap heatmap = null;
            Colorbar colorbar = null;

            Form hiddenForm = new Form
            {
                WindowState = FormWindowState.Minimized,
                ShowInTaskbar = false,
                Opacity = 0
            };

            FormsPlot plot = new();
            plot.Height = 480;
            plot.Width = 640;
            plot.Plot.SetInnerViewLimits(xMin, xMax, yMin, yMax);
            plot.Plot.SetOuterViewLimits(yMin: 0);

            // Add the plot control to the hidden form
            hiddenForm.Controls.Add(plot);

            hiddenForm.Show();

            plot.Plot.SetAxisLimits(xMin, xMax, yMin, yMax);
            plot.Plot.AxisScaleLock(true);
            L = plot.Plot.AddText("L", pressure_maps_metrics[Metric.Avg].ColumnCount * 0.2, 
                pressure_maps_metrics[Metric.Avg].RowCount * 0.25, size: 50, color: Color.DarkGray);
            R = plot.Plot.AddText("R", pressure_maps_metrics[Metric.Avg].ColumnCount * 1.05,
                pressure_maps_metrics[Metric.Avg].RowCount * 0.25, size: 50, color: Color.DarkGray);
            DrawData(pressure_maps_metrics[Metric.Avg], plot.Plot, ref heatmap, ref colorbar, colorbarPercentile);
            plot.Plot.SetAxisLimits(xMin, xMax * 1.5, yMin, yMax);
            plot.Refresh();
            plot.Render();
            plot.Plot.SaveFig("heatmap_avg.png");  
            DrawData(pressure_maps_metrics[Metric.Max], plot.Plot, ref heatmap, ref colorbar, colorbarMax);
            plot.Plot.SetAxisLimits(xMin, xMax * 1.5, yMin, yMax);
            plot.Refresh();
            plot.Render();
            plot.Plot.SaveFig("heatmap_max.png");
            DrawData(pressure_maps_metrics[Metric.Min], plot.Plot, ref heatmap, ref colorbar, colorbarPercentile);
            plot.Plot.SetAxisLimits(xMin, xMax * 1.5, yMin, yMax);
            plot.Refresh();
            plot.Render();
            plot.Plot.SaveFig("heatmap_min.png");
            plot.Dispose();
            hiddenForm.Close();
            return Task.CompletedTask;
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
