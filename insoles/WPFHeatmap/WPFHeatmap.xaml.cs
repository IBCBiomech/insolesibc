using insoles.DataHolders;
using insoles.Enums;
using insoles.Services;
using insoles.States;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MathNet.Numerics.LinearAlgebra;

namespace insoles.WPFHeatmap
{
    /// <summary>
    /// Lógica de interacción para WPFHeatmap.xaml
    /// </summary>
    public partial class WPFHeatmap : UserControl
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

        private const double TIME_PER_FRAME = 0.01;
        public double time
        {
            set
            {
                frame = (int)Math.Floor(value /  TIME_PER_FRAME);
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
                    Update(value, 1);
                }
            }
        }
        public WPFHeatmap(AnalisisState state, IPlantillaService plantilla, ICodesService codes)
        {
            InitializeComponent();
            footMap = CreateFootImage(plantilla.sensor_map, codes);
            alphaMap = CreateAlphaOverlayImage(plantilla.sensor_map, codes);
            CalculateCenters(plantilla.CalculateSensorPositionsLeft(), 
                plantilla.CalculateSensorPositionsRight(),
                out sensors_left, out sensors_right);
            float peso = 70;
            int numSensors = Enum.GetValues(typeof(Sensor)).Length;
            colorbarAnimate = (int)(peso * 9.8f / numSensors * FACTOR_COLORBAR_ANIMATE);
            colorbarMax = (int)(peso * 9.8f / numSensors * FACTOR_COLORBAR_MAX);
            colorbarAvg = (int)(peso * 9.8f / numSensors * FACTOR_COLORBAR_AVG);
            cHeatMap.Width = plantilla.getLength(0);
            cHeatMap.Height = plantilla.getLength(1);
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
        private async void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            await Dispatcher.Invoke(async () => { RenderContent(); });
        }
        private async void RenderContent()
        {
            await Dispatcher.Invoke(async () =>
            {

                cHeatMap.Clear();



                // Lets loop few times and create a random point each iteration

                // Pick random locations and intensity
                //x = rRand.Next(0, (int)(cHeatMap.ActualWidth - 1));
                //y = rRand.Next(0, (int)(cHeatMap.ActualHeight - 1));
                //intense = (byte)rRand.Next(0, 255);
                //cHeatMap.AddHeatPoint(new HeatPoint(x, y, intense));


                //cHeatMap.Render();


                //for (int i = 0; i < 10; i++)
                //{
                //	x = rRand.Next(0, (int)(cHeatMap.ActualWidth - 1));
                //	y = rRand.Next(0, (int)(cHeatMap.ActualHeight - 1));
                //	intense = (byte)rRand.Next(0, 255);
                //	cHeatMap.AddHeatPoint(new HeatPoint(x, y, intense));


                //	cHeatMap.Render();

                //}

            });
        }
        public async void Update(GraphData graphData)
        {
            this.graphData = graphData;
        }
        private async void Update(int frame)
        {
            await Dispatcher.Invoke(async () =>
            {
                cHeatMap.Clear();
                frame = Math.Max(Math.Min(frame, graphData.length), 0);
                DataInsole left = ((FrameDataInsoles)graphData[frame]).left;
                DataInsole right = ((FrameDataInsoles)graphData[frame]).right;
                foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
                {
                    (float, float) point = sensors_left[sensor];
                    byte intensity = CalculateIntensity(left[sensor]);
                    cHeatMap.AddHeatPoint(new HeatPoint((int)point.Item1, (int)point.Item2, intensity));
                }
                foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
                {
                    (float, float) point = sensors_right[sensor];
                    byte intensity = CalculateIntensity(right[sensor]);
                    cHeatMap.AddHeatPoint(new HeatPoint((int)point.Item1, (int)point.Item2, intensity));
                }
                cHeatMap.Render(alphaMap);
            });
        }
        private async void Update(int frame, int distance)
        {
            await Dispatcher.Invoke(async () =>
            {
                cHeatMap.Clear();
                frame = Math.Max(Math.Min(frame, graphData.length), 0);
                DataInsole left = ((FrameDataInsoles)graphData[frame]).left;
                DataInsole right = ((FrameDataInsoles)graphData[frame]).right;
                foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
                {
                    (float, float) point = sensors_left[sensor];
                    byte intensity = CalculateIntensity(left[sensor]);
                    cHeatMap.AddHeatPoint(new HeatPoint((int)point.Item1 + distance, (int)point.Item2 + distance, intensity));
                    cHeatMap.AddHeatPoint(new HeatPoint((int)point.Item1 + distance, (int)point.Item2 - distance, intensity));
                    cHeatMap.AddHeatPoint(new HeatPoint((int)point.Item1 - distance, (int)point.Item2 + distance, intensity));
                    cHeatMap.AddHeatPoint(new HeatPoint((int)point.Item1 - distance, (int)point.Item2 - distance, intensity));
                }
                foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
                {
                    (float, float) point = sensors_right[sensor];
                    byte intensity = CalculateIntensity(right[sensor]);
                    cHeatMap.AddHeatPoint(new HeatPoint((int)point.Item1 + distance, (int)point.Item2 + distance, intensity));
                    cHeatMap.AddHeatPoint(new HeatPoint((int)point.Item1 + distance, (int)point.Item2 - distance, intensity));
                    cHeatMap.AddHeatPoint(new HeatPoint((int)point.Item1 - distance, (int)point.Item2 + distance, intensity));
                    cHeatMap.AddHeatPoint(new HeatPoint((int)point.Item1 - distance, (int)point.Item2 - distance, intensity));
                }
                cHeatMap.Render(alphaMap);
            });
        }
        private byte CalculateIntensity(double pressure)
        {
            double percent = pressure / colorbarAnimate;
            if (percent >= 1)
            {
                return byte.MaxValue;
            }
            else
            {
                return (byte)(byte.MaxValue * percent);
            }
        }
        private WriteableBitmap CreateAlphaOverlayImage(Matrix<float> matrix, ICodesService codes)
        {
            int width = matrix.RowCount;
            int height = matrix.ColumnCount;

            byte[] pixelData = new byte[width * height * 4];

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
                    int baseIndex = (i + j * width) * 4;

                    pixelData[baseIndex + 3] = alphaByte;
                }
            }

            WriteableBitmap alphaOverlay = new WriteableBitmap(width, height, 96, 96, PixelFormats.Pbgra32, null);
            alphaOverlay.WritePixels(new Int32Rect(0, 0, width, height), pixelData, width * 4, 0);

            return alphaOverlay;
        }
        private WriteableBitmap CreateFootImage(Matrix<float> matrix, ICodesService codes)
        {
            byte color = 128;
            int width = matrix.RowCount;
            int height = matrix.ColumnCount;

            byte[] pixelData = new byte[width * height * 4];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    byte alphaByte;
                    if (matrix[i, j] != codes.Background())
                    {
                        alphaByte = 255;
                    }
                    else
                    {
                        alphaByte = 0;
                    }
                    int baseIndex = (i + j * width) * 4;

                    pixelData[baseIndex] = color;
                    pixelData[baseIndex + 1] = color;
                    pixelData[baseIndex + 2] = color;
                    pixelData[baseIndex + 3] = alphaByte;
                }
            }

            WriteableBitmap foot = new WriteableBitmap(width, height, 96, 96, PixelFormats.Pbgra32, null);
            foot.WritePixels(new Int32Rect(0, 0, width, height), pixelData, width * 4, 0);

            return foot;
        }
    }
}
