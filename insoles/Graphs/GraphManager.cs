﻿using insoles.ToolBar;
using insoles.ToolBar.Enums;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

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
            if (mainWindow.timeLine.Content == null)
            {
                mainWindow.timeLine.Navigated += delegate (object sender, NavigationEventArgs e)
                {
                    TimeLine.TimeLine timeLine = mainWindow.timeLine.Content as TimeLine.TimeLine;
                    replayManager = new ReplayManager(timeLine);
                };
            }
            else
            {
                TimeLine.TimeLine timeLine = mainWindow.timeLine.Content as TimeLine.TimeLine;
                replayManager = new ReplayManager(timeLine);
            }

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
        public void initReplay()
        {
            if (captureManager.active)
            {
                captureManager.deactivate();
            }
            if (!replayManager.active)
            {
                replayManager.activate();
            }
            else
            {
                replayManager.reset();
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

        int id_left = 0;
        int id_right = 1;

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

                mainWindow.api.SetDeviceConfiguration(0, 100, 3, out error);
               
                mainWindow.api.SetDeviceConfiguration(1, 100, 3, out error);
                Task.Delay(3000);

                Application.Current.Dispatcher.BeginInvoke(() =>
                {
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

        //Begin Wise
        //Callback para recoger datas del IMU
        public void Api_dataReceived(byte deviceHandler, WisewalkSDK.WisewalkData data)
        {
            float sumSole(WisewalkSDK.SoleSensor sole)
            {
                return sole.arch + sole.hallux + sole.heel_R +
                        sole.heel_L + sole.met_1 + sole.met_3 +
                        sole.met_5 + sole.toes;
            }
            string stringSole(WisewalkSDK.SoleSensor sole)
            {
                return sole.arch.ToString("F2") + " " + sole.hallux.ToString("F2") + " " +
                    sole.heel_R.ToString("F2") + " " + sole.heel_L.ToString("F2") + " " +
                    sole.met_1.ToString("F2") + " " + sole.met_3.ToString("F2") + " " +
                    sole.met_5.ToString("F2") + " " + sole.toes.ToString("F2");
            }
            if (deviceHandler == id_left)
            {
                soleLeft = data.Sole;
                numSoles++;
            }
            else if (deviceHandler == id_right)
            {
                soleRight = data.Sole;
                numSoles++;
            }
            if (numSoles % 2 == 0)
            {
                float[] sum_left = new float[soleLeft.Count];
                for (int i = 0; i < soleLeft.Count; i++)
                {
                    sum_left[i] = sumSole(soleLeft[i]);
                }
                float[] sum_right = new float[soleRight.Count];
                for (int i = 0; i < soleRight.Count; i++)
                {
                    sum_right[i] = sumSole(soleRight[i]);
                }
                GraphSumPressures graph = new GraphSumPressures(); // Cambiar esto
                graph.drawData(sum_left, sum_right);
                if (virtualToolBar.recordState == RecordState.Recording)
                {
                    dataline = "";
                    for (int i = 0; i < soleLeft.Count; i++)
                    {
                        dataline += "1 " + (fakets + i * 0.01f).ToString("F2") + " " +
                            (frame + i).ToString() + " " + stringSole(soleLeft[i]) + " " +
                            stringSole(soleRight[i]) + "\n";
                    }
                    mainWindow.fileSaver.appendCSVManual(dataline);
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
        public ReplayManager(TimeLine.TimeLine timeLine)
        {
            active = false;
            this.timeLine = timeLine;
        }
        public void activate()
        {
            if (!active)
            {
                active = true;
                //this.graphData = graphData;
                //timeLine.model.timeEvent += onUpdateTimeLine;
                timeLine.startReplay();
            }
        }
        public void deactivate()
        {
            if (active)
            {
                active = false;
                timeLine.endReplay();
                //timeLine.model.timeEvent -= onUpdateTimeLine;
                //graph.clearData();
                //frameEvent -= graph.onUpdateTimeLine;
            }
        }
        public void reset()
        {
            if (active)
            {
                //this.graphData = graphData;
                List<Frame>? graphs = null;
                //graph.clearData();
                //graph.drawData(graphData);
            }
        }
        /*
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
        */
    }
}
