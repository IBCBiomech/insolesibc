using insoles.Common;
using insoles.DeviceList.Enums;
using insoles.TimeLine;
using insoles.ToolBar;
using insoles.ToolBar.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using WisewalkSDK;
using static insoles.Common.Helpers;


namespace insoles.Graphs
{
    // Se encarga de manejar los grafos
    public class GraphManager
    {
        public CaptureManager captureManager;
        public ReplayManager replayManager;


        public Units unit { 
            set
            {
                captureManager.unit = value;
                Trace.WriteLine(captureManager.unit);
            }
            get
            {
                return captureManager.unit;
            }
        }
        public GraphManager()
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.initialized += (sender, args) => finishInit();
        }
        // Para solucionar problemas de dependencias
        private void finishInit()
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            VirtualToolBar virtualToolBar = mainWindow.virtualToolBar;
            replayManager = new ReplayManager();
            if (mainWindow.deviceList.Content == null)
            {
                mainWindow.deviceList.Navigated += delegate (object sender, NavigationEventArgs e)
                {
                    DeviceList.DeviceList deviceList = mainWindow.deviceList.Content as DeviceList.DeviceList;
                    captureManager = new CaptureManager(virtualToolBar, deviceList);
                };
            }
            else
            {
                DeviceList.DeviceList deviceList = mainWindow.deviceList.Content as DeviceList.DeviceList;
                captureManager = new CaptureManager(virtualToolBar, deviceList);
            }
        }
        public void initReplay(GraphData data)
        {
            if (captureManager.active)
            {
                captureManager.deactivate();
            }
            if (!replayManager.active)
            {
                replayManager.activate(data);
            }
            else
            {
                replayManager.reset(data);
            }
        }
        // Configura el modo capture
        public void initCapture()
        {
            if (replayManager.active)
            {
                replayManager.deactivate();
            }
            if (!captureManager.active)
            {
                captureManager.activate();
            }
            else
            {
                captureManager.reset();
            }
        }
    }
    public class CaptureManager
    {
        public bool active { get; private set; }
        private const int RENDER_MS = Config.RENDER_MS_CAPTUE;
        //private System.Timers.Timer timerRender;
        private VirtualToolBar virtualToolBar;
        private DeviceList.DeviceList deviceList;
        private TimeLine.TimeLine timeLine;

        GraphSumPressures graph;

        MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;


        //Begin Wise
        public Dictionary<string, WisewalkSDK.Device> devices_list = new Dictionary<string, WisewalkSDK.Device>();
        public List<int> counter = new List<int>();

        int numSoles = 0;

        public string frame2;
        public int sr;
        int timespan;
        string ts;
        int frame = 0;
        private bool resetFrame = false;

        string dataline;

        float fakets = 0.01f;
        float dt = 0.01f;

        string error = "";


        Vector3 v0, v1, v2, v3;

        const float G = 9.8f;

        private List<WisewalkSDK.SoleSensor> soleLeft;
        private List<WisewalkSDK.SoleSensor> soleRight;

        System.Timers.Timer fakeTimer;

        public Units unit = Units.mbar;
        //End Wise
        public CaptureManager(VirtualToolBar virtualToolBar, DeviceList.DeviceList deviceList)
        {
            active = false;
            this.virtualToolBar = virtualToolBar;
            this.deviceList = deviceList;
            saveTimeLine();

            mainWindow.virtualToolBar.saveEvent += onInitRecord;

            if(mainWindow.graphSumPressures.Content == null)
            {
                mainWindow.graphSumPressures.Navigated += (sender, e) =>
                {
                    graph = mainWindow.graphSumPressures.Content as GraphSumPressures;
                };
            }
            else
            {
                graph = mainWindow.graphSumPressures.Content as GraphSumPressures;
            }
        }

        private void saveTimeLine()
        {
            if (mainWindow.timeLine.Content == null)
            {
                mainWindow.timeLine.Navigated += delegate (object sender, NavigationEventArgs e)
                {
                    timeLine = mainWindow.timeLine.Content as TimeLine.TimeLine;
                };
            }
            else
            {
                timeLine = mainWindow.timeLine.Content as TimeLine.TimeLine;
            }
        }


        public void activate()
        {

            if (!active)
            {
                active = true;
                timeLine.endReplay(); // De momento cuando empieza a stremear apaga el replay

                mainWindow.api.dataReceived += Api_dataReceived;

                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    mainWindow.api.SetDeviceConfiguration(0, 100, 3, out error);

                    mainWindow.api.SetDeviceConfiguration(1, 100, 3, out error);

                    Task.Delay(2000);
                    mainWindow.api.StartStream(out error);

                    graph.initCapture();
                    virtualToolBar.stopEvent += onStop;
                });
            }
            //datos falsos
            /*
            fakeTimer = new System.Timers.Timer();
            fakeTimer.Interval = 40;
            fakeTimer.Elapsed += (s, e) => Fake_dataReceived();
            fakeTimer.Start();
            */
        }
        public void deactivate()
        {
            if (active)
            {
                active = false;
                graph.clearData();
                graph.initCapture();
                virtualToolBar.stopEvent -= onStop; //funcion local
                //mainWindow.api.StopStream(out error);
            }
        }
        public void reset()
        {
            if (active)
            {
                graph.clearData();
                graph.initCapture();
                
                mainWindow.api.StopStream(out error);
                /*
                if (virtualToolBar.pauseState == PauseState.Play)
                {
                    mainWindow.startActiveDevices();
                }
                */
            }
        }
        // Se ejecuta al clicar stop
        void onStop(object sender)
        {
            deactivate();
            mainWindow.api.dataReceived -= Api_dataReceived;
        }

        public void onInitRecord(object sender, EventArgs args)
        {
            fakets = 0.00f;
            frame = 0;
        }

        //Begin Wise
        //Callback para recoger datas del IMU

        //Método para transformar la presión a valor adecuado
        public int transform(int value)
        {
            return (4095 - value);
        }

        public float sumTransformSole(SoleSensor sole, Func<int, float> func)
        {
            return func(sole.arch) + func(sole.hallux) + func(sole.heel_R) +
                    func(sole.heel_L) + func(sole.met_1) + func(sole.met_3) +
                    func(sole.met_5) + func(sole.toes);
        }
        //function that sums all sensor pressures
        public float sumSole(WisewalkSDK.SoleSensor sole)
        {
            return transform(sole.arch) + transform(sole.hallux) + transform(sole.heel_R) +
                    transform(sole.heel_L) + transform(sole.met_1) + transform(sole.met_3) +
                    transform(sole.met_5) + transform(sole.toes);
        }
        public float avgSole(WisewalkSDK.SoleSensor sole)
        {
            return sumSole(sole) / Config.NUM_SENSORS;
        }
        public string stringTransformSole(SoleSensor sole, Func<int, float> func)
        {
            return func(sole.arch).ToString() + " " + func(sole.hallux).ToString() + " " +
                func(sole.heel_R).ToString() + " " + func(sole.heel_L).ToString() + " " +
                func(sole.met_1).ToString() + " " + func(sole.met_3).ToString() + " " +
                func(sole.met_5).ToString() + " " + func(sole.toes).ToString();
        }
        //function that concatenates all sensor pressure in a single line
        public string stringSole(WisewalkSDK.SoleSensor sole)
        {
            return transform(sole.arch).ToString() + " " + transform(sole.hallux).ToString() + " " +
                transform(sole.heel_R).ToString() + " " + transform(sole.heel_L).ToString() + " " +
                transform(sole.met_1).ToString() + " " + transform(sole.met_3).ToString() + " " +
                transform(sole.met_5).ToString() + " " + transform(sole.toes).ToString();
        }

       
        public void Api_dataReceived(byte deviceHandler, WisewalkSDK.WisewalkData data)
        {
                      
            if (deviceList.Side(deviceHandler) == Side.Left)
            {
                soleLeft = data.Sole;
                numSoles++;
            }
            else if (deviceList.Side(deviceHandler) == Side.Right)
            {
                soleRight = data.Sole;
                numSoles++;
            }
            if (numSoles % 2 == 0 && soleLeft != null && soleRight != null)
            {
                Trace.WriteLine("received");
                Trace.WriteLine("left");
                for(int i = 0; i < soleLeft.Count; i++)
                {
                    Trace.WriteLine(stringSole(soleLeft[i]));
                }
                Trace.WriteLine("right");
                for (int i = 0; i < soleRight.Count; i++)
                {
                    Trace.WriteLine(stringSole(soleRight[i]));
                }
                //transformPressures(ref soleLeft);
                float[] metric_left = new float[soleLeft.Count];
                float[] metric_right = new float[soleRight.Count];

                GraphSumPressures.Metric metric = graph.metricSelected;

                Func<int, float> transformFunc;
                switch (unit)
                {
                    case Units.mbar:
                        transformFunc = (VALUE_digital) => VALUE_mbar(ADC_neg(VALUE_digital));
                        break;
                    case Units.N:
                        transformFunc = (VALUE_digital) => N(VALUE_mbar(ADC_neg(VALUE_digital)));
                        break;
                    default:
                        throw new Exception("ninguna unidad seleccionada");
                }
                if (metric == GraphSumPressures.Metric.Avg) 
                {
                    for (int i = 0; i < Config.NUMPACKETS; i++)
                    {
                        metric_left[i] = avgSole(soleLeft[i]);
                        metric_right[i] = avgSole(soleRight[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < Config.NUMPACKETS; i++)
                    {
                        metric_left[i] = sumTransformSole(soleLeft[i], transformFunc);
                        metric_right[i] = sumTransformSole(soleRight[i], transformFunc);
                    }
                }
                //GraphSumPressures graph = new GraphSumPressures(); // Cambiar esto. Iván: esta línea la tengo que quitar para que funcione el gráfico
                graph.drawData(metric_left, metric_right);

                if (virtualToolBar.recordState == RecordState.Recording)
                {

                    dataline = "";
                    for (int j = 0; j < Config.NUMPACKETS; j++)
                    {
                        // He dejado la de release de momento porque sino otras partes no funcionaran bien
/*
                        string leftSide = $" {4095 - soleLeft[j].arch} {4095 - soleLeft[j].hallux} {4095 - soleLeft[j].heel_L} {4095 - soleLeft[j].heel_R} {4095 - soleLeft[j].met_1} {4095 - soleLeft[j].met_3} {4095 - soleLeft[j].met_5} {4095 - soleLeft[j].toes}";
                        string rightSide = $" {4095 - soleRight[j].arch} {4095 - soleRight[j].hallux} {4095 - soleRight[j].heel_L} {4095 - soleRight[j].heel_R} {4095 - soleRight[j].met_1} {4095 - soleRight[j].met_3} {4095 - soleRight[j].met_5} {4095 - soleRight[j].toes}";

                        int leftSum = ( 4095 * 8 ) - ( soleLeft[j].arch + soleLeft[j].hallux + soleLeft[j].heel_L + soleLeft[j].heel_R + soleLeft[j].met_1 + soleLeft[j].met_3 + soleLeft[j].met_5 + soleLeft[j].toes);
                        int rightSum = ( 4095 * 8 ) - ( soleRight[j].arch + soleRight[j].hallux + soleRight[j].heel_L + soleRight[j].heel_R + soleRight[j].met_1 + soleRight[j].met_3 + soleRight[j].met_5 + soleRight[j].toes);

                        dataline = "1 " + (fakets).ToString("F2") + " " + (frame).ToString() + " " + leftSide.ToString() + " " + rightSide.ToString() + " " +
                           " " + leftSum.ToString() + " " + rightSum.ToString() + " " +"\n";
*/
                        
                        dataline = "1 " + (fakets).ToString("F2") + " " + (frame).ToString() + " " + 
                            stringTransformSole(soleLeft[j], transformFunc) + " " + 
                            stringTransformSole(soleRight[j], transformFunc) + " " +
                            metric_right[j].ToString() + " " + metric_left[j].ToString() + " " +"\n";
                        fakets += 0.01f;
                        frame += 1;
                        mainWindow.fileSaver.appendCSVManual(dataline);
                        
                    }
              
                }
            }
        }
    }
        //End Wise
    public class ReplayManager
    {
        public bool active { get; private set; }

        public delegate void FrameEventHandler(object sender, int frame);
        public event FrameEventHandler frameEvent;

        private TimeLine.TimeLine timeLine;
        private Butterfly butterfly;
        private PressureMap pressureMap;
        private AlgLib algLib;
        private GraphSumPressures sumPressures;

        private GraphData graphData;
        public ReplayManager()
        {
            active = false;
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow.timeLine.Content == null)
            {
                mainWindow.timeLine.Navigated += delegate (object sender, NavigationEventArgs e)
                {
                    timeLine = mainWindow.timeLine.Content as TimeLine.TimeLine;
                };
            }
            else
            {
                timeLine = mainWindow.timeLine.Content as TimeLine.TimeLine;
            }
            if (mainWindow.butterfly == null)
            {
                mainWindow.initialized += (s, e) =>
                {
                    butterfly = mainWindow.butterfly;
                };
            }
            else
            {
                butterfly = mainWindow.butterfly;
            }
            if (Config.HeatmapMethodUsed == Config.HeatmapMethod.Alglib)
            {
                if (mainWindow.algLib == null)
                {
                    mainWindow.initialized += (s, e) =>
                    {
                        algLib = mainWindow.algLib;
                    };
                }
                else
                {
                    algLib = mainWindow.algLib;
                }
            }
            else if(Config.HeatmapMethodUsed == Config.HeatmapMethod.IDW)
            {
                if (mainWindow.pressureMap == null)
                {
                    mainWindow.initialized += (s, e) =>
                    {
                        pressureMap = mainWindow.pressureMap;
                    };
                }
                else
                {
                    pressureMap = mainWindow.pressureMap;
                }
            }
            if (mainWindow.graphSumPressures.Content == null)
            {
                mainWindow.graphSumPressures.Navigated += delegate (object sender, NavigationEventArgs e)
                {
                    sumPressures = mainWindow.graphSumPressures.Content as GraphSumPressures;
                };
            }
            else
            {
                sumPressures = mainWindow.graphSumPressures.Content as GraphSumPressures;
            }
        }
        public void activate(GraphData graphData)
        {
            if (!active)
            {
                active = true;
                this.graphData = graphData;
                butterfly.Calculate(graphData);
                if (Config.HeatmapMethodUsed == Config.HeatmapMethod.Alglib)
                {
                    algLib.Calculate(graphData);
                }
                else if(Config.HeatmapMethodUsed == Config.HeatmapMethod.IDW)
                {
                    pressureMap.Calculate(graphData);
                }
                sumPressures.drawData(graphData);
                frameEvent += sumPressures.onUpdateTimeLine;
                timeLine.model.timeEvent += onUpdateTimeLine;
                timeLine.startReplay();
            }
        }
        public void deactivate()
        {
            if (active)
            {
                active = false;
                timeLine.endReplay();
                timeLine.model.timeEvent -= onUpdateTimeLine;
                sumPressures.clearData();
                frameEvent -= sumPressures.onUpdateTimeLine;
            }
        }
        public void reset(GraphData graphData)
        {
            if (active)
            {
                this.graphData = graphData;
                butterfly.Calculate(graphData);
                if (Config.HeatmapMethodUsed == Config.HeatmapMethod.Alglib)
                {
                    algLib.Calculate(graphData);
                }
                else if (Config.HeatmapMethodUsed == Config.HeatmapMethod.IDW)
                {
                    pressureMap.Calculate(graphData);
                }        
                sumPressures.clearData();
                sumPressures.drawData(graphData);
            }
        }
        
        public void onUpdateTimeLine(object sender, double time)
        {
            int initialEstimation(double time)
            {
                double timePerFrame = graphData.maxTime / graphData.maxFrame;
                int expectedFrame = (int)Math.Round(time / timePerFrame);
                return expectedFrame;
            }
            int searchFrameLineal(double time, int currentFrame, int previousFrame, double previousDiference)
            {
                double currentTime = graphData.time(currentFrame);
                double currentDiference = Math.Abs(time - currentTime);
                if (currentDiference >= previousDiference)
                {
                    return previousFrame;
                }
                else if (currentTime < time)
                {
                    if (currentFrame == graphData.maxFrame) //Si es el ultimo frame devolverlo
                    {
                        return graphData.maxFrame;
                    }
                    else
                    {
                        return searchFrameLineal(time, currentFrame + 1, currentFrame, currentDiference);
                    }
                }
                else if (currentTime > time)
                {
                    if (currentFrame == graphData.minFrame) //Si es el primer frame devolverlo
                    {
                        return graphData.minFrame;
                    }
                    else
                    {
                        return searchFrameLineal(time, currentFrame - 1, currentFrame, currentDiference);
                    }
                }
                else //currentTime == time muy poco probable (decimales) pero puede pasar
                {
                    return currentFrame;
                }
            }
            int estimatedFrame = initialEstimation(time);
            estimatedFrame = Math.Max(estimatedFrame, graphData.minFrame); // No salirse del rango
            estimatedFrame = Math.Min(estimatedFrame, graphData.maxFrame); // No salirse del rango
            frameEvent?.Invoke(this, searchFrameLineal(time, estimatedFrame, -1, double.MaxValue));
        }
        
    }
}
