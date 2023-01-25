using DirectShowLib;
using insoles.DeviceList.TreeClasses;
using insoles.ToolBar;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace insoles
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public VirtualToolBar virtualToolBar;
        public FileSaver.FileSaver fileSaver;
        public event EventHandler initialized;
        public MainWindow()
        {
            InitializeComponent();

            virtualToolBar = new VirtualToolBar();
            fileSaver = new FileSaver.FileSaver();

            initToolBarHandlers();

            initialized?.Invoke(this, EventArgs.Empty);
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
                // Añade las camaras al TreeView
                async void addCameras(DeviceList.DeviceList deviceListClass)
                {
                    // Devuelve el nombre de todas las camaras conectadas
                    List<string> cameraNames()
                    {
                        List<DsDevice> devices = new List<DsDevice>(DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice));
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
                    //names.ForEach(n => Trace.WriteLine(n));
                    List<int> indices = await Task.Run(() => cameraIndices(names.Count));
                    //indices.ForEach(i => Trace.WriteLine(i));

                    List<CameraInfo> cameras = new List<CameraInfo>();
                    for (int i = 0; i < names.Count; i++)
                    {
                        if (indices.Contains(i))
                        {
                            cameras.Add(new CameraInfo(i, names[i]));
                        }
                    }
                    deviceListClass.setCameras(cameras);

                    //MessageBox.Show(scanDevices.Count + " IMUs encontrados", "Scan Devices", MessageBoxButton.OK, MessageBoxImage.Information);
                }


                DeviceList.DeviceList deviceListClass = deviceList.Content as DeviceList.DeviceList;
                //deviceListClass.clearAll();
                addCameras(deviceListClass);
                deviceListClass.showCameras();
                // Añade datos inventados quitar
                deviceListClass.showInsoles();

                deviceListClass.addInsole(new InsolesInfo(0, "WISEWALK", "Left", "ASDFG"));
                InsolesInfo insoleRight = new InsolesInfo(1, "WISEWALK", "Right", "ASTF");
                insoleRight.battery = 56;
                insoleRight.fw = "1.19";
                deviceListClass.addInsole(insoleRight);

            }
            deviceListLoadedCheck(onScanFunction);
            virtualToolBar.onScanClick();
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
                List<string> InsolesToDisconnect = new List<string>();
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
                    }
                }
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
                foreach (object selected in selectedItems)
                {
                    if (selected != null && selected is CameraInfo)
                    {
                        MultiSelectTreeViewItem treeViewItem = (MultiSelectTreeViewItem)deviceListClass.cameras.ItemContainerGenerator.ContainerFromItem(selected);
                        CameraInfo cameraInfo = treeViewItem.DataContext as CameraInfo;
                        int id = cameraInfo.number; //Id de la camara
                        CamaraViewport.CamaraViewport camaraViewportClass = camaraViewport.Content as CamaraViewport.CamaraViewport;
                        if (!camaraViewportClass.someCameraOpened())
                        {
                            camaraViewportClass.Title = cameraInfo.name + " CAM " + id;
                            camaraViewportClass.initializeCamara(id);
                        }
                        break;
                    }
                }
            }
            deviceListLoadedCheck(onOpenCameraFunction);
        }
        // Funcion que se ejecuta al clicar el boton Capture
        private void onCapture(object sender, EventArgs e)
        {
            virtualToolBar.captureClick();
            //graphManager.initCapture();
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
            //virtualToolBar.openClick();
        }
    }
}
