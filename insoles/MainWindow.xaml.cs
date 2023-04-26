using DirectShowLib;
using insoles.DeviceList.TreeClasses;
using insoles.Graphs;
using insoles.ToolBar;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Xml.Linq;
using WisewalkSDK;
using static WisewalkSDK.Wisewalk;
using System.Runtime.InteropServices;
using MathNet.Numerics.LinearAlgebra.Solvers;

using AForge.Video.DirectShow;
using FilterCategory = DirectShowLib.FilterCategory;

namespace insoles
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public Wisewalk api;

        public GraphManager graphManager;
        public VirtualToolBar virtualToolBar;
        public FileSaver.FileSaver fileSaver;
        public event EventHandler initialized;

        public Foot foot;
        public Butterfly butterfly;
        public PressureMap pressureMap;
        public AlgLib algLib;

        private List<Wisewalk.ComPort> ports = new List<Wisewalk.ComPort>();
        private List<Wisewalk.Dev> scanDevices = new List<Wisewalk.Dev>();
        private string port_selected;
        private string error;
        private List<int> counter = new List<int>();
        public Dictionary<string, WisewalkSDK.Device> devices_list
        {
            get
            {
                return api.GetDevicesConnected();
            }
        }
        public MainWindow()
        {
            //Transformers.transformImageHeatmap();
            //Transformers.transformImageButterfly();
            InitializeComponent();

            Application.Current.MainWindow = this;
            virtualToolBar = new VirtualToolBar();
            fileSaver = new FileSaver.FileSaver();
            graphManager = new GraphManager();
            butterfly = new Butterfly();
            if(Config.HeatmapMethodUsed == Config.HeatmapMethod.Alglib)
                algLib = new AlgLib();
            else if(Config.HeatmapMethodUsed == Config.HeatmapMethod.IDW)
                pressureMap = new PressureMap();
            foot = new Foot();

            api = new Wisewalk();
            api.scanFinished += Api_scanFinished;
            api.deviceConnected += Api_deviceConnected;
            api.onError += Api_onError;
            api.deviceDisconnected += Api_onDisconnect;

            initToolBarHandlers();

            initCameraAnchorables();

            /* //generar imagenes
            Transformers.transformImageButterfly();
            Transformers.transformImageHeatmap();
            */

            initialized?.Invoke(this, EventArgs.Empty);

            //fileSaver.saveFakeFile();
            //virtualToolBar.transformFiles();
        }
        private void initCameraAnchorables()
        {
            if (camaraViewport1.Content == null)
            {
                camaraViewport1.Navigated += delegate (object sender, NavigationEventArgs e)
                {
                    ((CamaraViewport.CamaraViewport)camaraViewport1.Content).layoutAnchorable = camaraAnchorable1;
                };
            }
            else
            {
                ((CamaraViewport.CamaraViewport)camaraViewport1.Content).layoutAnchorable = camaraAnchorable1;
            }
            if (camaraViewport2.Content == null)
            {
                camaraViewport2.Navigated += delegate (object sender, NavigationEventArgs e)
                {
                    ((CamaraViewport.CamaraViewport)camaraViewport2.Content).layoutAnchorable = camaraAnchorable2;
                };
            }
            else
            {
                ((CamaraViewport.CamaraViewport)camaraViewport2.Content).layoutAnchorable = camaraAnchorable2;
            }
        }
        // Conecta los botones de la ToolBar
        private void initToolBarHandlers()
        {
            toolBar.Navigated += delegate (object sender, NavigationEventArgs e)
            {
                ToolBar.ToolBar toolBarClass = toolBar.Content as ToolBar.ToolBar;
                toolBarClass.scan.Click += new RoutedEventHandler(onScan);
                toolBarClass.connect.Click += new RoutedEventHandler(onConnect);
                toolBarClass.disconnect.Click += new RoutedEventHandler(onDisconnect);
                toolBarClass.openCamera.Click += new RoutedEventHandler(onOpenCamera);
                toolBarClass.capture.Click += new RoutedEventHandler(onCapture);
                toolBarClass.pause.Click += new RoutedEventHandler(onPause);
                toolBarClass.stop.Click += new RoutedEventHandler(onStop);
                toolBarClass.record.Click += new RoutedEventHandler(onRecord);
                toolBarClass.capturedFiles.Click += new RoutedEventHandler(onCapturedFiles);
            };
        }
        // Funcion que llaman todos los handlers del ToolBar. Por si acaso el device list no se ha cargado.
        private void deviceListLoadedCheck(Action func)
        {
            if (deviceList.Content == null)
            {
                deviceList.Navigated += delegate (object sender, NavigationEventArgs e)
                {
                    func();
                };
            }
            else
            {
                func();
            }
        }
        private void onScan(object sender, EventArgs e)
        {
            // Funcion que se ejecuta al clicar el boton scan
            void onScanFunction()
            {
                void getInsoles()
                {

                    ShowPorts();
                    api.Open(port_selected, out error);

                    if (!api.ScanDevices(out error))
                    {
                        // Error
                        Trace.WriteLine("", "Error to scan devices - " + error);
                    }
                    else
                    {
                        Thread.Sleep(2000);
                    }

                }
                // Añade las camaras al TreeView
                async void addCameras(DeviceList.DeviceList deviceListClass)
                {
                    List<int>[] cameraFps()
                    {
                        Stopwatch stopwatch = Stopwatch.StartNew();
                        var devices = new FilterInfoCollection(AForge.Video.DirectShow.FilterCategory.VideoInputDevice);
                        List<int>[] cameraFps = new List<int>[devices.Count];
                        for (int i = 0; i < devices.Count; i++)
                        {

                            cameraFps[i] = new List<int>();
                            var captureDevice = new VideoCaptureDevice(devices[i].MonikerString);
                            foreach (var capability in captureDevice.VideoCapabilities)
                            {
                                if (!cameraFps[i].Contains(capability.AverageFrameRate))
                                {
                                    cameraFps[i].Add(capability.AverageFrameRate);
                                }
                            }
                            cameraFps[i].Add(60);
                        }
                        Trace.WriteLine(stopwatch.Elapsed.TotalSeconds);
                        return cameraFps;
                    }
                    Dictionary<int, int> DirectshowAforgeMap()
                    {
                        var aforgeDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

                        var directshowDevices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

                        var deviceMap = new Dictionary<int, int>();

                        for (int ai = 0; ai < aforgeDevices.Count; ai++)
                        {
                            for (int dsi = 0; dsi < directshowDevices.Count(); dsi++)
                            {
                                string monikerDs;
                                directshowDevices[dsi].Mon.GetDisplayName(null, null, out monikerDs);
                                if (aforgeDevices[ai].MonikerString == monikerDs)
                                {
                                    deviceMap.Add(dsi, ai);
                                    break;
                                }
                            }
                        }
                        return deviceMap;
                    }
                    // Devuelve el nombre de todas las camaras conectadas
                    List<string> cameraNames()
                    {
                        List<DsDevice> devices = new List<DsDevice>(DsDevice.GetDevicesOfCat(DirectShowLib.FilterCategory.VideoInputDevice));
                        List<string> cameraNames = new List<string>();
                        foreach (DsDevice device in devices)
                        {
                            cameraNames.Add(device.Name);
                        }
                        return cameraNames;
                    }
                    // Devuelve una lista de indice OpenCV de las camaras disponibles
                    List<int> cameraIndices(int maxIndex = 10)
                    {
                        List<int> indices = new List<int>();
                        VideoCapture capture = new VideoCapture();
                        for (int index = 0; index < maxIndex; index++)
                        {
                            capture.Open(index, VideoCaptureAPIs.DSHOW);
                            if (capture.IsOpened())
                            {
                                indices.Add(index);
                                capture.Release();
                            }
                        }
                        return indices;
                    }
                    List<string> names = await Task.Run(() => cameraNames());
                    List<int>[] fps = await Task.Run(() => cameraFps());
                    Dictionary<int, int> directshowToAforge = await Task.Run(() => DirectshowAforgeMap());
                    List<int> indices = await Task.Run(() => cameraIndices(names.Count));
                    //indices.ForEach(i => Trace.WriteLine(i));
                    await Task.Run(() => getInsoles()); //necesario para escanear IMUs

                    List<CameraInfo> cameras = new List<CameraInfo>();
                    for (int i = 0; i < names.Count; i++)
                    {
                        Trace.WriteLine("i = " + i);
                        if (indices.Contains(i))
                        {
                            Trace.WriteLine("indices.Contains " + i);
                            List<int> camFps = fps[directshowToAforge[i]];
                            Trace.WriteLine("List<double> fps = await Task.Run(() => fpsValues(i)); i = " + i);
                            cameras.Add(new CameraInfo(i, names[i], camFps));
                        }
                    }
                    deviceListClass.setCameras(cameras);

                    await Task.Delay(4000);

                    List<InsolesInfo> insoles = new List<InsolesInfo>();
                    for (int i = 0; i < scanDevices.Count; i++)
                    {
                        string name = "Wisewalk";
                        insoles.Add(new InsolesInfo(name, GetMacAddress(scanDevices, i)));
                    }
                    //insoles.Add(new InsolesInfo("Wisewalk", "AD:EF:GH"));
                    //insoles.Add(new InsolesInfo("Wisewalk", "TD:CK:PO"));
                    deviceListClass.setInsoles(insoles);
                    //MessageBox.Show(scanDevices.Count + " IMUs encontrados", "Scan Devices", MessageBoxButton.OK, MessageBoxImage.Information);
                }


                DeviceList.DeviceList deviceListClass = deviceList.Content as DeviceList.DeviceList;
                //deviceListClass.clearAll();
                addCameras(deviceListClass);
                deviceListClass.showCameras();
                deviceListClass.showInsoles();
            }
            deviceListLoadedCheck(onScanFunction);
            virtualToolBar.onScanClick();
        }
        private byte handler(InsolesInfo insole)
        {
            string handler = devices_list.Where(d => d.Value.Id == insole.address).FirstOrDefault().Key;
            return byte.Parse(handler);
        }
        private Dev findInsole(InsolesInfo insoleInfo)
        {
            return scanDevices.FirstOrDefault(de => GetMacAddress(de) == insoleInfo.address);
        }
        // Conecta el boton connect
        private void onConnect(object sender, EventArgs e)
        {
            // Funcion que se ejecuta al clicar el boton connect
            async void onConnectFunction()
            {
                DeviceList.DeviceList deviceListClass = deviceList.Content as DeviceList.DeviceList;
                IList<object> selectedItems = (IList<object>)deviceListClass.treeView.SelectedItems;
                List<InsolesInfo> connectedInsoles = new List<InsolesInfo>();
                List<object> selectedIMUs = new List<object>(); // Necesario porque deviceListClass.treeView.SelectedItems puede cambiar despues de clicar connect
                foreach (object selected in selectedItems)
                {
                    if (selected != null) // No se si se puede quitar
                    {
                        if (selected is InsolesInfo)
                        {
                            selectedIMUs.Add(selected);
                            MultiSelectTreeViewItem treeViewItem = (MultiSelectTreeViewItem)deviceListClass.insoles.ItemContainerGenerator.ContainerFromItem(selected);

                            //´Wise connecting
                            InsolesInfo insoleInfo = treeViewItem.DataContext as InsolesInfo;
                            connectedInsoles.Add(insoleInfo);
                        }
                        else if (selected is CameraInfo)
                        {
                            //MultiSelectTreeViewItem treeViewItem = (MultiSelectTreeViewItem)deviceListClass.cameras.ItemContainerGenerator.ContainerFromItem(selected);
                            //deviceListClass.connectCamera(treeViewItem);
                        }
                    }
                }
                List<Dev> conn_list_dev = new List<Dev>();
                foreach (InsolesInfo insole in connectedInsoles)
                {
                    conn_list_dev.Add(findInsole(insole));
                }
                if (!api.Connect(conn_list_dev, out error))
                {
                    Trace.WriteLine("Connect error " + error);
                }
            }
            deviceListLoadedCheck(onConnectFunction);
        }
        // Conecta el boton disconnect
        private void onDisconnect(object sender, EventArgs e)
        {
            // Funcion que se ejecuta al clicar el boton disconnect
            async void onDisconnectFunction()
            {
                DeviceList.DeviceList deviceListClass = deviceList.Content as DeviceList.DeviceList;
                IList<object> selectedItems = (IList<object>)deviceListClass.treeView.SelectedItems;
                List<InsolesInfo> insolesToDisconnect = new List<InsolesInfo>();
                List<int> devHandlers = new List<int>();
                Trace.WriteLine("before disconnect");
                foreach (object selected in selectedItems)
                {
                    if (selected != null && selected is InsolesInfo)
                    {
                        MultiSelectTreeViewItem treeViewItem = (MultiSelectTreeViewItem)deviceListClass.insoles.ItemContainerGenerator.ContainerFromItem(selected);
                        //Begin Wise
                        InsolesInfo insoleInfo = treeViewItem.DataContext as InsolesInfo;

                        //devHandlers.Add(insoleInfo.handler);
                        insolesToDisconnect.Add(insoleInfo);
                        devHandlers.Add(handler(insoleInfo));
                    }
                }

                if (!api.Disconnect(devHandlers, out error))
                {
                    Trace.WriteLine("Disconnect error " + error);
                }
                await Task.Delay(4000);
                deviceListClass.disconnectInsoles(insolesToDisconnect);
            }
            deviceListLoadedCheck(onDisconnectFunction);
        }
        // Conecta el boton Open Camera
        private void onOpenCamera(object sender, EventArgs e)
        {
            // Funcion que se ejecuta al clicar el boton Open Camera
            void onOpenCameraFunction()
            {
                DeviceList.DeviceList deviceListClass = deviceList.Content as DeviceList.DeviceList;
                IList<object> selectedItems = (IList<object>)deviceListClass.treeView.SelectedItems;
                List<Frame> camaraViewportFrames = new List<Frame>() { camaraViewport1, camaraViewport2 };
                int frameIndex = 0;
                foreach (object selected in selectedItems)
                {
                    if (selected != null && selected is CameraInfo)
                    {
                        MultiSelectTreeViewItem treeViewItem = (MultiSelectTreeViewItem)deviceListClass.cameras.ItemContainerGenerator.ContainerFromItem(selected);
                        CameraInfo cameraInfo = treeViewItem.DataContext as CameraInfo;
                        int id = cameraInfo.number; //Id de la camara
                        int fps = cameraInfo.fps;
                        while (frameIndex < camaraViewportFrames.Count)
                        {
                            CamaraViewport.CamaraViewport camaraViewportClass = camaraViewportFrames[frameIndex].Content as CamaraViewport.CamaraViewport;
                            frameIndex++;
                            if (!camaraViewportClass.someCameraOpened())
                            {
                                //camaraViewportClass.Title = cameraInfo.name + " CAM " + id;
                                camaraViewportClass.initializeCamara(id, fps);
                                break;
                            }
                        }
                    }
                }
            }
            deviceListLoadedCheck(onOpenCameraFunction);
        }
        // Funcion que se ejecuta al clicar el boton Capture
        private void onCapture(object sender, EventArgs e)
        {
            virtualToolBar.captureClick();
            graphManager.initCapture();
        }
        // Funcion que se ejecuta al clicar el boton Pause
        private void onPause(object sender, EventArgs e)
        {
            virtualToolBar.pauseClick();
        }
        // Funcion que se ejecuta al clicar el boton Stop
        private void onStop(object sender, EventArgs e)
        {
            virtualToolBar.stopClick();
        }
        // Funcion que se ejecuta al clicar el boton Record
        private void onRecord(object sender, EventArgs e)
        {

            virtualToolBar.recordClick();
        }
        // Funcion que se ejecuta al clicar el boton Show Captured Files
        private void onCapturedFiles(object sender, EventArgs e)
        {
            virtualToolBar.openClick();
        }
        private void ShowPorts()
        {


            ports = api.GetUsbDongles();
            foreach (Wisewalk.ComPort port in ports)
            {
                Match match1 = Regex.Match(port.description, "nRF52 USB CDC BLE*", RegexOptions.IgnoreCase);
                if (match1.Success)
                {
                    port_selected = port.name;
                    Trace.WriteLine(port.description);

                }
            }
        }
        public DateTime GetDateTime()
        {
            DateTime dateTime = new DateTime(2022, 11, 8, 13, 0, 0, 0);
            return dateTime;
        }
        private void Api_scanFinished(List<Wisewalk.Dev> devices)
        {
            scanDevices = devices;
            Trace.WriteLine("# of devices: " + devices.Count);
            ShowScanList(scanDevices);
        }
        private async void Api_deviceConnected(byte handler, WisewalkSDK.Device dev)
        {
            // Esta funcion tiene que ser LOCAL
            void setRTCDevice(byte deviceHandler, byte sampleRate, byte packetType)
            {
                if (deviceHandler == handler)
                {
                    api.SetRTCDevice(deviceHandler, GetDateTime(), out error);
                    Dispatcher.BeginInvoke(
                    () => (deviceList.Content as DeviceList.DeviceList).
                    updateHeaderInfo(dev.Id, handler)
                );
                    api.updateDeviceConfiguration -= setRTCDevice;
                }
            }

            await Dispatcher.BeginInvoke(
                () => (deviceList.Content as DeviceList.DeviceList).
                connectInsole(dev.Id, handler)
            );

            api.SetDeviceConfiguration(handler, 100, 3, out error);
            api.updateDeviceConfiguration += setRTCDevice;


            counter.Add(0);
        }
        private void ShowScanList(List<Wisewalk.Dev> devices)
        {

            for (int idx = 0; idx < devices.Count; idx++)
            {
                string macAddress = devices[idx].mac[5].ToString("X2") + ":" + devices[idx].mac[4].ToString("X2") + ":" + devices[idx].mac[3].ToString("X2") + ":" +
                                    devices[idx].mac[2].ToString("X2") + ":" + devices[idx].mac[1].ToString("X2") + ":" + devices[idx].mac[0].ToString("X2");


                Trace.WriteLine("MacAddress: ", " * " + macAddress);
            }

        }
        private string GetMacAddress(List<Wisewalk.Dev> devices, int idx)
        {
            string mac = "";

            mac = devices[idx].mac[5].ToString("X2") + ":" + devices[idx].mac[4].ToString("X2") + ":" + devices[idx].mac[3].ToString("X2") + ":" +
                                    devices[idx].mac[2].ToString("X2") + ":" + devices[idx].mac[1].ToString("X2") + ":" + devices[idx].mac[0].ToString("X2");

            return mac;
        }
        private string GetMacAddress(Wisewalk.Dev device)
        {
            string mac = "";

            mac = device.mac[5].ToString("X2") + ":" + device.mac[4].ToString("X2") + ":" + device.mac[3].ToString("X2") + ":" +
                                    device.mac[2].ToString("X2") + ":" + device.mac[1].ToString("X2") + ":" + device.mac[0].ToString("X2");

            return mac;
        }
        private void Api_onDisconnect(byte deviceHandler)
        {
            Trace.WriteLine("Api_onDisconnect");
            Dispatcher.BeginInvoke(
                    () => (deviceList.Content as DeviceList.DeviceList).
                    disconnectInsole(deviceHandler)
                );
            //devices_list.Remove(deviceHandler.ToString());
        }
        private void Api_onError(byte deviceHandler, string error)
        {
            if (deviceHandler != 0xFF)
            {
                SetLogText(devices_list[deviceHandler.ToString()].Id, error);
            }
            else
            {
                SetLogText("", error);
            }
        }
        private void SetLogText(string device, string text)
        {


            var output = "";
            var message = "";

            if (device != "")
            {
                message = $"{DateTime.Now:HH:mm:ss.fff}   RX from [{device}]:   {text} ";
            }
            else
            {
                message = $"{DateTime.Now:HH:mm:ss.fff}:   {text} ";
            }

            output += message + Environment.NewLine;

            Trace.Write(output);


        }
    }
}
