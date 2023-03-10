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

namespace insoles.Graphs
{
    // Se encarga de manejar los grafos
    public class GraphManager
    {
        public CaptureManager captureManager;
        public ReplayManager replayManager;


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
            /*
            mainWindow.api.StopStream(out error);
            if (virtualToolBar.pauseState == PauseState.Play)
            {
                mainWindow.startActiveDevices();
            }
            */
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
        // Se ejecuta al clicar pause
        void onPause(object sender, PauseState pauseState)
        {
            if (pauseState == PauseState.Pause)
            {
                //timerRender.Stop();
            }
            else if (pauseState == PauseState.Play)
            {
                //timerRender.Start();
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
            fakets = 0;
            frame = 0;
        }

        private void Fake_dataReceived()
        {
            int n = 4;
            Random random = new Random();
            float[] sum_left = new float[n];
            for (int i = 0; i < n; i++)
            {
                sum_left[i] = random.Next(10000);
            }
            float[] sum_right = new float[n];
            for (int i = 0; i < n; i++)
            {
                sum_right[i] = random.Next(10000);
            }
            graph.drawData(sum_left, sum_right);
        }

        //Begin Wise
        //Callback para recoger datas del IMU

        //Método para transformar la presión a valor adecuado
        public int transform(int value)
        {
            return (4095 - value);
        }

        //function that sums all sensor pressures
        public float sumSole(WisewalkSDK.SoleSensor sole)
        {
            return transform(sole.arch) + transform(sole.hallux) + transform(sole.heel_R) +
                    transform(sole.heel_L) + transform(sole.met_1) + transform(sole.met_3) +
                    transform(sole.met_5) + transform(sole.toes);
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
           
            // No usar esto
            /*
            void transformPressures(ref List<SoleSensor> data)
            {
                for(int i = 0; i < data.Count; i++)
                {
                    data[i].arch = transform(data[i].arch);
                    data[i].hallux = transform(data[i].hallux);
                    data[i].heel_L = transform(data[i].heel_L);
                    data[i].heel_R = transform(data[i].heel_R);
                    data[i].met_1 = transform(data[i].met_1);
                    data[i].met_3 = transform(data[i].met_3);
                    data[i].met_5 = transform(data[i].met_5);
                    data[i].toes = transform(data[i].toes);
                }
            }
            */
          
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
            if (numSoles % 2 == 0)
            {
                //transformPressures(ref soleLeft);
                float[] sum_left = new float[soleLeft.Count];
                for (int i = 0; i < soleLeft.Count; i++)
                {
                    sum_left[i] = sumSole(soleLeft[i]);
                }
                //transformPressures(ref soleRight);
                float[] sum_right = new float[soleRight.Count];
                for (int i = 0; i < soleRight.Count; i++)
                {
                    sum_right[i] = sumSole(soleRight[i]);
                }

                //GraphSumPressures graph = new GraphSumPressures(); // Cambiar esto. Iván: esta línea la tengo que quitar para que funcione el gráfico
                graph.drawData(sum_left, sum_right);
                if (virtualToolBar.recordState == RecordState.Recording)
                {
                    dataline = "";
                    for (int i = 0; i < soleLeft.Count; i++)
                    {
                        dataline += "1 " + (fakets + i * 0.01f).ToString("F2") + " " +
                            (frame + i).ToString() + " " + stringSole(soleLeft[i]) + " " + stringSole(soleRight[i]) + " " +
                            sumSole(soleLeft[i]).ToString() + " " +
                            sumSole(soleRight[i]).ToString() + " " +"\n";
                    }


                    //dataline = "1 " + (fakets).ToString("F2") + " " +
                    //        (frame).ToString() + " " + sum_left.Average().ToString() + " " +
                    //        sum_right.Average().ToString() + "\n";

                    mainWindow.fileSaver.appendCSVManual(dataline);
                    fakets += 0.01f;
                    frame += 1;
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
                pressureMap.Calculate(graphData);
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
                pressureMap.Calculate(graphData);
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
