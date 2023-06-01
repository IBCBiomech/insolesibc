using insoles.Enums;
using insoles.Messages;
using insoles.Model;
using insoles.Services;
using insoles.Utilities;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using ScottPlot;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WisewalkSDK;

namespace insoles.ViewModel
{
    public class RegistroVM : ViewModelBase
    {
        private IApiService apiService;
        private ICameraService cameraService;
        private ILiveCalculationsService liveCalculationsService;
        private ISaveService saveService;
        public ICommand ScanCommand { get; set; }
        public ICommand ConnectCommand { get; set; }
        public ICommand ConnectSelectedCommand { get; set; }
        public ICommand ConnectAllCommand { get; set; }
        public ICommand CaptureCommand { get; set; }
        public ICommand OpenCameraCommand { get; set; }
        public ICommand RecordCommand { get; set; }
        public ICommand StopCommand { get; set; }

        private void Scan(object obj)
        {
            apiService.Scan();
            cameraService.Scan();
        }
        private void Connect(object obj)
        {
            InsoleModel insole = obj as InsoleModel;
            List<string> macs = new List<string> { insole.MAC };
            apiService.Connect(macs);
        }
        private void ConnectAll(object obj)
        {
            apiService.ConnectAll();
        }
        private void ConnectSelected(object obj)
        {
            List<string> macs = new List<string>();
            var selected = obj as IList<object>;
            foreach(var selectedItem in selected)
            {
                InsoleModel insole = selectedItem as InsoleModel;
                macs.Add(insole.MAC);
            }
            apiService.Connect(macs);
        }
        private void Capture(object obj) => apiService.Capture();
        private bool CaptureCanExecute(object obj)
        {
            return Insoles.Where((InsoleModel insole) => insole.connected).Count() == 2
                && !apiService.capturing;
        }
        private void OpenCamera(object obj)
        {
            Trace.WriteLine("OpenCamera");
            CameraModel camera = obj as CameraModel;
            cameraService.OpenCamera(camera.number, camera.fps, camera.resolution);
        }
        private void Record(object obj)
        {
            saveService.Start(cameraService.getFps(0), cameraService.getResolution(0));
        }
        private void Stop(object obj)
        {
            saveService.Stop();
        }
        public ObservableCollection<CameraModel> Cameras { get; set; }
        public ObservableCollection<InsoleModel> Insoles { get; set; }
        private BitmapSource currentFrame;
        public BitmapSource CurrentFrame
        {
            get 
            { 
                return currentFrame;
            }
            set
            {
                currentFrame = value;
                OnPropertyChanged();
            }
        }
        public WpfPlot Plot { get; set; }
        private GraphSumPressuresLiveModel GraphModel;
        public RegistroVM()
        {
            //Init services
            apiService = new ApiService();
            cameraService = new CameraService();
            liveCalculationsService = new LiveCalculationsService();
            saveService = new SaveService();
            //Init commands
            ScanCommand = new RelayCommand(Scan);
            ConnectCommand = new RelayCommand(Connect);
            ConnectSelectedCommand = new RelayCommand(ConnectSelected);
            ConnectAllCommand = new RelayCommand(ConnectAll);
            CaptureCommand = new RelayCommand(Capture, CaptureCanExecute);
            OpenCameraCommand = new RelayCommand(OpenCamera);
            RecordCommand = new RelayCommand(Record);
            StopCommand = new RelayCommand(Stop);
            //Init Collections
            Cameras = new ObservableCollection<CameraModel>();
            Insoles = new ObservableCollection<InsoleModel>();
            //Init listeners
            apiService.ScanReceived += (List<InsoleScan> insolesReceived) =>
            {
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    Insoles.Clear();
                    for (int i = 0; i < insolesReceived.Count; i++)
                    {
                        InsoleScan insole = insolesReceived[i];
                        InsoleModel insoleModel = new(i, insole.name, insole.MAC, this);
                        Insoles.Add(insoleModel);
                    }
                });
            };
            apiService.DeviceConnected += (Device dev) =>
            {
                InsoleModel insole = Insoles.First((InsoleModel insole) => insole.MAC == dev.Id);
                insole.connected = true;
            };
            cameraService.ScanReceived += (List<CameraScan> camerasReceived) =>
            {
                Cameras.Clear();
                foreach(CameraScan cam in camerasReceived)
                {
                    CameraModel camera = new(cam.number, cam.name, cam.fps, cam.resolutions, this);
                    Cameras.Add(camera);
                }
            };
            cameraService.FrameAvailable += (int index, Mat frame) =>
            {
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    CurrentFrame = BitmapSourceConverter.ToBitmapSource(frame);
                });
                saveService.AppendVideo(frame);
            };
            apiService.DataReceived += (byte handler, List<InsoleData> packet) =>
            {
                liveCalculationsService.ProcessPacket(handler, packet);
            };
            liveCalculationsService.ResultReady += 
                (
                    List<Dictionary<Sensor, double>> left, List<Dictionary<Sensor, double>> right,
                    float[] metricLeft, float[] metricRight
                ) =>
            {
                GraphModel.UpdateData(metricLeft, metricRight);
                saveService.AppendCSV(left, right, metricLeft, metricRight);
            };
            Plot = new WpfPlot();
            Plot.Plot.Title("test plot");
            GraphModel = new(Plot);
        }
    }
}
