using insoles.TimeLine;
using MathNet.Numerics.LinearAlgebra;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static alglib;
using static insoles.Graphs.GraphSumPressures;
using static ScottPlot.Plottable.PopulationPlot;

namespace insoles.Graphs
{
    public class AlgLib
    {
        delegate void ActionRef<T1, T2, T3>(T1 arg1, ref T2 arg2, ref T3 arg3);
        private rbfmodel leftModel;
        private rbfmodel rightModel;
        private double[,] pointsLeft;
        private double[,] pointsRight;
        private Foot foot;
        private GraphPressureHeatmap graph;

        private bool isInitialized = false;
        public event EventHandler initialized;

        private Metric metric;

        private Dictionary<Sensor, List<Tuple<int, int>>> sensorPositionsLeft;
        private Dictionary<Sensor, List<Tuple<int, int>>> sensorPositionsRight;

        private Dictionary<Sensor, (float, float)> centersLeft = new Dictionary<Sensor, (float, float)>();
        private Dictionary<Sensor, (float, float)> centersRight = new Dictionary<Sensor, (float, float)>();

        private int numLeftSensorPoints;
        private int numRightSensorPoints;

        private int numLeftFootPoints;
        private int numRightFootPoints;

        private Dictionary<Metric, Matrix<float>> pressureMaps = new Dictionary<Metric, Matrix<float>>();
        public AlgLib()
        {
            void initFunc()
            {
                /*
                void initModel(out rbfmodel model, out double[,] points, Dictionary<Sensor, List<Tuple<int, int>>> sensorPositions)
                {
                    int NX = 2;
                    int NY = 1;
                    rbfcreate(NX, NY, out model);
                    int N = 0;
                    foreach(List<Tuple<int, int>> concretSensorPositions in sensorPositions.Values)
                    {
                        N += concretSensorPositions.Count;
                    }
                    points = new double[N, NX + NY];
                    int i = 0;
                    int item1Index = 0;
                    int item2Index = 1;
                    int valueIndex = 2;
                    foreach(Sensor sensor in sensorPositions.Keys)
                    {
                        double code = foot.codes.GetCode(sensor);
                        foreach (Tuple<int, int> position in sensorPositions[sensor])
                        {
                            points[i, item1Index] = position.Item1;
                            points[i, item2Index] = position.Item2;
                            points[i, valueIndex] = code;
                            i++;
                        }
                    }
                }
                */
                int countSensorPoints(Dictionary<Sensor, List<Tuple<int, int>>> sensorPositions)
                {
                    int N = 0;
                    foreach (List<Tuple<int, int>> concretSensorPositions in sensorPositions.Values)
                    {
                        N += concretSensorPositions.Count;
                    }
                    return N;
                }
                sensorPositionsLeft = foot.CalculateSensorPositionsLeft();
                sensorPositionsRight = foot.CalculateSensorPositionsRight();
                numLeftSensorPoints = countSensorPoints(sensorPositionsLeft);
                numRightSensorPoints = countSensorPoints(sensorPositionsRight);
                rbfcreate(2, 1, out leftModel);
                rbfcreate(2, 1, out rightModel);
                CalculateCenters();
                isInitialized = true;
                initialized?.Invoke(this, EventArgs.Empty);
            }          
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow.graphPressures.Content == null)
            {
                mainWindow.graphPressures.Navigated += (s, e) =>
                {
                    graph = mainWindow.graphPressures.Content as GraphPressureHeatmap;
                    graph.MetricChanged += changeMetric;
                };
            }
            else
            {
                graph = mainWindow.graphPressures.Content as GraphPressureHeatmap;
                graph.MetricChanged += changeMetric;
            }
            if (mainWindow.foot == null)
            {
                mainWindow.initialized += (s, e) =>
                {
                    foot = mainWindow.foot;
                    Task.Run(() => {
                        initFunc();
                    });
                };
            }
            else
            {
                foot = mainWindow.foot;
                Task.Run(() => {
                    initFunc();
                });
            }
        }
        private void CalculateCenters()
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
            Dictionary<Sensor, List<Tuple<int, int>>> sensor_positions_left = foot.CalculateSensorPositionsLeft();
            Dictionary<Sensor, List<Tuple<int, int>>> sensor_positions_right = foot.CalculateSensorPositionsRight();
            foreach (Sensor sensor in (Sensor[])Enum.GetValues(typeof(Sensor)))
            {
                centersLeft[sensor] = CalculateCenter(sensor_positions_left[sensor]);
                centersRight[sensor] = CalculateCenter(sensor_positions_right[sensor]);
            }
        }
        public void Calculate(GraphData graphData)
        {
            void CalculateAll()
            {
                void CalculateOne(GraphData graphData, ActionRef<GraphData, DataInsole, DataInsole> func, Metric metric)
                {
                    rbfmodel CalculateModel(Dictionary<Sensor, int> pressures, 
                        Dictionary<Sensor, List<Tuple<int, int>>> sensorPositions, int numSensorPoints)
                    {
                        rbfmodel model;
                        rbfcreate(2, 1, out model);
                        double [,] points = new double[numSensorPoints, 3];
                        int i = 0;
                        foreach (Sensor sensor in sensorPositions.Keys)
                        {
                            double value = pressures[sensor];
                            foreach (Tuple<int, int> position in sensorPositions[sensor])
                            {
                                points[i, 0] = position.Item1;
                                points[i, 1] = position.Item2;
                                points[i, 2] = value;
                                i++;
                            }
                        }
                        rbfsetpoints(model, points);
                        rbfreport report;
                        rbfbuildmodel(model, out report);
                        return model;
                    }
                    DataInsole leftInsole = new DataInsole();
                    DataInsole rightInsole = new DataInsole();
                    func(graphData, ref leftInsole, ref rightInsole);
                    Dictionary<Sensor, int> pressuresLeft = leftInsole.pressures;
                    Dictionary<Sensor, int> pressuresRight = rightInsole.pressures;
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Restart();
                    rbfmodel modelLeft = CalculateModel(pressuresLeft, sensorPositionsLeft, numLeftSensorPoints);
                    Trace.WriteLine("model left " + stopwatch.Elapsed.TotalSeconds);
                    stopwatch.Restart();
                    rbfmodel modelRight = CalculateModel(pressuresRight, sensorPositionsRight, numRightSensorPoints);
                    Trace.WriteLine("model right " + stopwatch.Elapsed.TotalSeconds);
                    Matrix<float> data = foot.sensor_map.Clone();
                    float background = foot.codes.Background();
                    stopwatch.Restart();
                    for(int i = 0; i < data.RowCount / 2; i++)
                    {
                        for(int j = 0; j < data.ColumnCount; j++)
                        {
                            if(data[i, j] != background)
                            {
                                data[i, j] = (float)rbfcalc2(modelLeft, i, j);
                            }
                        }
                    }
                    Trace.WriteLine("left foot " + stopwatch.Elapsed.TotalSeconds);
                    stopwatch.Restart();
                    for (int i = data.RowCount / 2; i < data.RowCount; i++)
                    {
                        for (int j = 0; j < data.ColumnCount; j++)
                        {
                            if (data[i, j] != background)
                            {
                                data[i, j] = (float)rbfcalc2(modelRight, i, j);
                            }
                        }
                    }
                    Trace.WriteLine("right foot " + stopwatch.Elapsed.TotalSeconds);
                    pressureMaps[metric] = data;
                }
                void CalculateOneFromCenters(GraphData graphData, ActionRef<GraphData, DataInsole, DataInsole> func, Metric metric)
                {
                    rbfmodel CalculateModel(Dictionary<Sensor, int> pressures,
                        Dictionary<Sensor, (float, float)> sensorPositions)
                    {
                        rbfmodel model;
                        rbfcreate(2, 1, out model);
                        double[,] points = new double[sensorPositions.Values.Count, 3];
                        int i = 0;
                        foreach (Sensor sensor in sensorPositions.Keys)
                        {
                            double value = pressures[sensor];
                            points[i, 0] = sensorPositions[sensor].Item1;
                            points[i, 1] = sensorPositions[sensor].Item2;
                            points[i, 2] = value;
                            i++;
                        }
                        rbfsetpoints(model, points);
                        rbfreport report;
                        rbfbuildmodel(model, out report);
                        Trace.WriteLine(report);
                        return model;
                    }
                    Trace.WriteLine("Start CalculateOneFromCenters");
                    DataInsole leftInsole = new DataInsole();
                    DataInsole rightInsole = new DataInsole();
                    func(graphData, ref leftInsole, ref rightInsole);
                    Dictionary<Sensor, int> pressuresLeft = leftInsole.pressures;
                    Dictionary<Sensor, int> pressuresRight = rightInsole.pressures;
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Restart();
                    rbfmodel modelLeft = CalculateModel(pressuresLeft, centersLeft);
                    Trace.WriteLine("model left " + stopwatch.Elapsed.TotalSeconds);
                    stopwatch.Restart();
                    rbfmodel modelRight = CalculateModel(pressuresRight, centersRight);
                    Trace.WriteLine("model right " + stopwatch.Elapsed.TotalSeconds);
                    Matrix<float> data = foot.sensor_map.Clone();
                    float background = foot.codes.Background();
                    stopwatch.Restart();
                    for (int i = 0; i < data.RowCount / 2; i++)
                    {
                        for (int j = 0; j < data.ColumnCount; j++)
                        {
                            if (data[i, j] == background)
                            {
                                data[i, j] = Config.BACKGROUND;
                            }
                            else
                            {
                                data[i, j] = (float)rbfcalc2(modelLeft, i, j);
                            }
                        }
                    }
                    Trace.WriteLine("left foot " + stopwatch.Elapsed.TotalSeconds);
                    stopwatch.Restart();
                    for (int i = data.RowCount / 2; i < data.RowCount; i++)
                    {
                        for (int j = 0; j < data.ColumnCount; j++)
                        {
                            if (data[i, j] == background)
                            {
                                data[i, j] = Config.BACKGROUND;
                            }
                            else
                            {
                                data[i, j] = (float)rbfcalc2(modelRight, i, j);
                            }
                        }
                    }
                    Trace.WriteLine("right foot " + stopwatch.Elapsed.TotalSeconds);
                    pressureMaps[metric] = data;
                }
                CalculateOneFromCenters(graphData, average, Metric.Avg);
                CalculateOneFromCenters(graphData, max, Metric.Max);
                CalculateOneFromCenters(graphData, min, Metric.Min);
            }
            if (isInitialized)
            {
                graph.calculating = true;
                CalculateAll();
                graph.DrawData(pressureMaps[metric]);
                graph.calculating = false;
            }
            else
            {
                graph.calculating = true;
                initialized += (s, e) =>
                {
                    CalculateAll();
                    graph.DrawData(pressureMaps[metric]);
                    graph.calculating = false;
                };
            }
        }
        private void average(GraphData data, ref DataInsole left, ref DataInsole right)
        {
            for (int i = 0; i < data.length; i++)
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
        private void max(GraphData data, ref DataInsole left, ref DataInsole right)
        {
            foreach (Sensor sensor in Enum.GetValues(typeof(Sensor)))
            {
                left[sensor] = int.MinValue;
                right[sensor] = int.MinValue;
            }
            for (int i = 0; i < data.length; i++)
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
        private void min(GraphData data, ref DataInsole left, ref DataInsole right)
        {
            foreach (Sensor sensor in Enum.GetValues(typeof(Sensor)))
            {
                left[sensor] = int.MaxValue;
                right[sensor] = int.MaxValue;
            }
            for (int i = 0; i < data.length; i++)
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
        public void changeMetric(object sender, MetricEventArgs e)
        {
            metric = e.metric;
            if (pressureMaps.ContainsKey(metric) && !graph.calculating)
            {
                graph.DrawData(pressureMaps[metric]);
            }
        }
    }
}
