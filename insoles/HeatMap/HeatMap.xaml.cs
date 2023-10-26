using ConsoleDebug;
using HeatMap;
using insoles.DataHolders;
using insoles.Enums;
using insoles.Services;
using insoles.States;
using insoles.WPFHeatmap;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace insoles.HeatMap
{
    /// <summary>
    /// Lógica de interacción para HeatMap.xaml
    /// </summary>
    public partial class HeatMap : UserControl
    {
        private Dictionary<Sensor, (float, float)> sensors_left;
        private Dictionary<Sensor, (float, float)> sensors_right;

        private GraphData graphData;

        private WriteableBitmap footMap;
        private WriteableBitmap alphaMap;

        const float FACTOR_COLORBAR_ANIMATE = 2f;
        const float FACTOR_COLORBAR_MAX = 5f;
        const float FACTOR_COLORBAR_AVG = 1f;

        private int colorbarMax;
        private int colorbarAvg;
        private int colorbarAnimate;

        private int width;
        private int height;

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
                    Update(value);
                }
            }
        }
        public HeatMap(AnalisisState state, IPlantillaService plantilla, ICodesService codes)
        {
            InitializeComponent();
            CalculateCenters(plantilla.CalculateSensorPositionsLeft(),
                plantilla.CalculateSensorPositionsRight(),
                out sensors_left, out sensors_right);
            width = plantilla.getLength(0);
            height = plantilla.getLength(1);
            float peso = 70;
            int numSensors = Enum.GetValues(typeof(Sensor)).Length;
            colorbarAnimate = (int)(peso * 9.8f / numSensors * FACTOR_COLORBAR_ANIMATE);
            colorbarMax = (int)(peso * 9.8f / numSensors * FACTOR_COLORBAR_MAX);
            colorbarAvg = (int)(peso * 9.8f / numSensors * FACTOR_COLORBAR_AVG);
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
        public async void Update(GraphData graphData)
        {
            this.graphData = graphData;
        }
        private async void Update(int frame)
        {
            List<DataType> datas = new List<DataType>();
            frame = Math.Max(Math.Min(frame, graphData.length), 0);
            DataInsole left = ((FrameDataInsoles)graphData[frame]).left;
            DataInsole right = ((FrameDataInsoles)graphData[frame]).right;
            foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
            {
                (float, float) point = sensors_left[sensor];
                double intensity = CalculateIntensity(left[sensor]);
                datas.Add(new DataType() { X = (int)point.Item1, Y = (int)point.Item2, Weight = intensity});
            }
            foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
            {
                (float, float) point = sensors_right[sensor];
                double intensity = CalculateIntensity(right[sensor]);
                datas.Add(new DataType() { X = (int)point.Item1, Y = (int)point.Item2, Weight = intensity });
            }
            HeatMapImage heatMapImage = new HeatMapImage(width, height, 200, 50);
            heatMapImage.SetDatas(datas);
            Bitmap bitmap = heatMapImage.GetHeatMap();
            BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            image.Source = bitmapSource;
        }
        private double CalculateIntensity(double pressure)
        {
            double percent = pressure / colorbarAnimate;
            if (percent >= 1)
            {
                return 10;
            }
            else
            {
                return percent * 10;
            }
        }
    }
}
