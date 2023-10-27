using HeatMap;
using insoles.DataHolders;
using insoles.Enums;
using insoles.Services;
using insoles.States;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using MathNet.Numerics.LinearAlgebra;
using System.Windows.Media.Imaging;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Threading.Tasks;

namespace insoles.HeatMap
{
    /// <summary>
    /// Lógica de interacción para HeatMap.xaml
    /// </summary>
    public enum Metric { Avg, Max, Min }
    public class MetricEventArgs : EventArgs
    {
        public Metric metric { get; set; }
    }
    public partial class HeatMap : UserControl
    {
        private delegate void ActionRef<T1, T2, T3, T4, T5>(T1 arg1, ref T2 arg2, ref T3 arg3, T4 arg4, T5 arg5);
        private Dictionary<Sensor, (float, float)> sensors_left;
        private Dictionary<Sensor, (float, float)> sensors_right;

        private GraphData graphData;

        private AnalisisState state;

        private Bitmap footMap;
        private Bitmap alphaMap;

        const float FACTOR_COLORBAR_ANIMATE = 2f;
        const float FACTOR_COLORBAR_MAX = 5f;
        const float FACTOR_COLORBAR_AVG = 1f;

        const double MAX_INTENSITY = 10;

        private int colorbarMax;
        private int colorbarAvg;
        private int colorbarAnimate;

        private int width;
        private int height;

        private (int, int) range;

        private const double TIME_PER_FRAME = 0.01;
        public double time
        {
            set
            {
                frame = (int)Math.Floor(value / TIME_PER_FRAME);
            }
        }
        private int _frame = 0;
        private int frame
        {
            get { return _frame; }
            set
            {
                if (value != _frame)
                {
                    _frame = value;
                    if(animate)
                        Update(value);
                }
            }
        }
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
        public Metric selectedMetric
        {
            get
            {
                return _selectedMetric;
            }
            set
            {
                _selectedMetric = value;
                if (graphData != null && !animate)
                {
                    switch (value)
                    {
                        case Metric.Min:
                            Update(Min, colorbarAvg);
                            break;
                        case Metric.Max:
                            Update(Max, colorbarMax);
                            break;
                        case Metric.Avg:
                            Update(Average, colorbarAvg);
                            break;
                    }
                    //DrawCenters(centersXs, centersYs, plot);
                }
                NotifyPropertyChanged();
            }
        }
        public IEnumerable<Metric> metrics
        {
            get { return Enum.GetValues(typeof(Metric)).Cast<Metric>(); }
        }
        private bool _animate;
        public bool animate
        {
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
                        Update(frame);
                }
                else
                {
                    if (graph_loaded)
                        switch (selectedMetric)
                        {
                            case Metric.Min:
                                Update(Min, colorbarAvg);
                                break;
                            case Metric.Max:
                                Update(Max, colorbarMax);
                                break;
                            case Metric.Avg:
                                Update(Average, colorbarAvg);
                                break;
                        }
                }
            }
        }
        public bool graph_loaded
        {
            get
            {
                return graphData != null;
            }
        }
        public HeatMap(AnalisisState state, IPlantillaService plantilla, ICodesService codes)
        {
            InitializeComponent();
            alphaMap = CreateAlphaOverlayImage(plantilla.sensor_map, codes);
            CalculateCenters(plantilla.CalculateSensorPositionsLeft(),
                plantilla.CalculateSensorPositionsRight(),
                out sensors_left, out sensors_right);
            width = plantilla.getLength(0);
            height = plantilla.getLength(1);
            this.state = state;
            DataContext = this;
        }
        private void CalculateCenters(Dictionary<Sensor, List<Tuple<int, int>>> sensor_positions_left,
            Dictionary<Sensor, List<Tuple<int, int>>> sensor_positions_right,
            out Dictionary<Sensor, (float, float)> centersLeft,
            out Dictionary<Sensor, (float, float)> centersRight)
        {
            (float, float) CalculateCenter(List<Tuple<int, int>> sensor_positions)
            {
                int rowSum = 0;
                int colSum = 0;
                foreach (Tuple<int, int> position in sensor_positions)
                {
                    rowSum += position.Item1;
                    colSum += position.Item2;
                }
                float centerRow = (float)rowSum / sensor_positions.Count;
                float centerCol = (float)colSum / sensor_positions.Count;
                return (centerRow, centerCol);
            }
            centersLeft = new();
            centersRight = new();
            foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
            {
                centersLeft[sensor] = CalculateCenter(sensor_positions_left[sensor]);
                centersRight[sensor] = CalculateCenter(sensor_positions_right[sensor]);
            }
        }
        public async Task Update(GraphData graphData)
        {
            this.graphData = graphData;
            float peso = state.peso;
            int numSensors = Enum.GetValues(typeof(Sensor)).Length;
            colorbarAnimate = (int)(peso * 9.8f / numSensors * FACTOR_COLORBAR_ANIMATE);
            colorbarMax = (int)(peso * 9.8f / numSensors * FACTOR_COLORBAR_MAX);
            colorbarAvg = (int)(peso * 9.8f / numSensors * FACTOR_COLORBAR_AVG);
            range = (0, graphData.length - 1);
            if (animate)
            {
                await Update(frame);
            }
            else
            {
                switch (selectedMetric)
                {
                    case Metric.Min:
                        await Update(Min, colorbarAvg);
                        break;
                    case Metric.Max:
                        await Update(Max, colorbarMax);
                        break;
                    case Metric.Avg:
                        await Update(Average, colorbarAvg);
                        break;
                }
            }
        }
        private async Task Update(int frame)
        {
            List<DataType> datas = new List<DataType>();
            frame = Math.Max(Math.Min(frame, graphData.length), 0);
            DataInsole left = ((FrameDataInsoles)graphData[frame]).left;
            DataInsole right = ((FrameDataInsoles)graphData[frame]).right;
            foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
            {
                (float, float) point = sensors_left[sensor];
                double intensity = CalculateIntensity(left[sensor], colorbarAnimate);
                datas.Add(new DataType() { X = (int)point.Item1, Y = (int)point.Item2, Weight = intensity});
            }
            foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
            {
                (float, float) point = sensors_right[sensor];
                double intensity = CalculateIntensity(right[sensor], colorbarAnimate);
                datas.Add(new DataType() { X = (int)point.Item1, Y = (int)point.Item2, Weight = intensity });
            }
            HeatMapImage heatMapImage = new HeatMapImage(width, height, 40, 10);
            heatMapImage.SetDatas(datas);
            Bitmap bitmap = heatMapImage.GetHeatMap();
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.DrawImage(alphaMap, new System.Drawing.Point(0, 0));
            }
            BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            await Dispatcher.BeginInvoke(() =>
            {
                image.Source = bitmapSource;
            });
        }
        private async Task Update(ActionRef<GraphData, DataInsole, DataInsole, int, int> func, int colorbar)
        {
            List<DataType> datas = new List<DataType>();
            DataInsole left = new();
            DataInsole right = new();
            func(graphData, ref left, ref right, range.Item1, range.Item2);
            foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
            {
                (float, float) point = sensors_left[sensor];
                double intensity = CalculateIntensity(left[sensor], colorbar);
                datas.Add(new DataType() { X = (int)point.Item1, Y = (int)point.Item2, Weight = intensity });
            }
            foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
            {
                (float, float) point = sensors_right[sensor];
                double intensity = CalculateIntensity(right[sensor], colorbar);
                datas.Add(new DataType() { X = (int)point.Item1, Y = (int)point.Item2, Weight = intensity });
            }
            HeatMapImage heatMapImage = new HeatMapImage(width, height, 40, 10);
            heatMapImage.SetDatas(datas);
            Bitmap bitmap = heatMapImage.GetHeatMap();
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.DrawImage(alphaMap, new System.Drawing.Point(0, 0));
            }
            BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            await Dispatcher.BeginInvoke(() =>
            {
                image.Source = bitmapSource;
            });
        }
        private double CalculateIntensity(double pressure, int colorbar)
        {
            double percent = pressure / colorbar;
            if (percent >= 1)
            {
                return MAX_INTENSITY;
            }
            else
            {
                return percent * MAX_INTENSITY;
            }
        }
        private Bitmap CreateAlphaOverlayImage(Matrix<float> matrix, ICodesService codes)
        {
            int width = matrix.RowCount;
            int height = matrix.ColumnCount;

            Bitmap alphaOverlay = new Bitmap(width, height);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    byte alphaByte;
                    if (matrix[i, j] == codes.Background())
                    {
                        alphaByte = 255;
                    }
                    else
                    {
                        alphaByte = 0;
                    }

                    Color pixelColor = Color.FromArgb(alphaByte, 255, 255, 255);
                    alphaOverlay.SetPixel(i, j, pixelColor);
                }
            }

            return alphaOverlay;
        }

        private Bitmap CreateFootImage(Matrix<float> matrix, ICodesService codes)
        {
            int width = matrix.RowCount;
            int height = matrix.ColumnCount;

            Bitmap alphaOverlay = new Bitmap(width, height);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Color pixelColor;
                    if (matrix[i, j] == codes.Background())
                    {
                        pixelColor = Color.FromArgb(255, 255, 255, 255);
                    }
                    else
                    {
                        pixelColor = Color.FromArgb(255, 0, 0, 0);
                    }
                    alphaOverlay.SetPixel(i, j, pixelColor);
                }
            }

            return alphaOverlay;
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        private void Average(GraphData data, ref DataInsole left, ref DataInsole right, int start, int end)
        {
            for (int i = start; i <= end; i++)
            {
                FrameDataInsoles frameData = (FrameDataInsoles)data[i];
                foreach (Sensor sensor in Enum.GetValues(typeof(Sensor)))
                {
                    left[sensor] += frameData.left[sensor];
                    right[sensor] += frameData.right[sensor];
                }
            }
            foreach (Sensor sensor in Enum.GetValues(typeof(Sensor)))
            {
                left[sensor] /= data.length;
                right[sensor] /= data.length;
            }
        }
        private void Max(GraphData data, ref DataInsole left, ref DataInsole right, int start, int end)
        {
            foreach (Sensor sensor in Enum.GetValues(typeof(Sensor)))
            {
                left[sensor] = int.MinValue;
                right[sensor] = int.MinValue;
            }
            for (int i = start; i <= end; i++)
            {
                FrameDataInsoles frameData = (FrameDataInsoles)data[i];
                foreach (Sensor sensor in Enum.GetValues(typeof(Sensor)))
                {
                    if (frameData.left[sensor] > left[sensor])
                    {
                        left[sensor] = frameData.left[sensor];
                    }
                    if (frameData.right[sensor] > right[sensor])
                    {
                        right[sensor] = frameData.right[sensor];
                    }
                }
            }
        }
        private void Min(GraphData data, ref DataInsole left, ref DataInsole right, int start, int end)
        {
            foreach (Sensor sensor in Enum.GetValues(typeof(Sensor)))
            {
                left[sensor] = int.MaxValue;
                right[sensor] = int.MaxValue;
            }
            for (int i = start; i <= end; i++)
            {
                FrameDataInsoles frameData = (FrameDataInsoles)data[i];
                foreach (Sensor sensor in Enum.GetValues(typeof(Sensor)))
                {
                    if (frameData.left[sensor] < left[sensor])
                    {
                        left[sensor] = frameData.left[sensor];
                    }
                    if (frameData.right[sensor] < right[sensor])
                    {
                        right[sensor] = frameData.right[sensor];
                    }
                }
            }
        }
    }
}
