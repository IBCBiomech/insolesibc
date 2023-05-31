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
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace insoles.ViewModel
{
    public class RegistroVM : ViewModelBase
    {
        public string header { get; set; } = "header";
        private IApiService apiService;
        private ICameraService cameraService;
        private ILiveCalculationsService liveCalculationsService;
        private ISaveService saveService;
        public ICommand ScanCommand { get; set; }
        public ICommand ConnectCommand { get; set; }
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
            apiService.ConnectAll();
        }
        private void Capture(object obj) => apiService.Capture();
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
            apiService = new FakeApiService();
            cameraService = new CameraService();
            liveCalculationsService = new LiveCalculationsService();
            saveService = new SaveService();
            //Init commands
            ScanCommand = new RelayCommand(Scan);
            ConnectCommand = new RelayCommand(Connect);
            CaptureCommand = new RelayCommand(Capture);
            OpenCameraCommand = new RelayCommand(OpenCamera);
            RecordCommand = new RelayCommand(Record);
            StopCommand = new RelayCommand(Stop);
            //Init Collections
            Cameras = new ObservableCollection<CameraModel>();
            Insoles = new ObservableCollection<InsoleModel>();
            //Init listeners
            apiService.ScanReceived += (List<InsoleScan> insolesReceived) =>
            {
                Insoles.Clear();
                for (int i = 0; i < insolesReceived.Count; i++)
                {
                    InsoleScan insole = insolesReceived[i];
                    InsoleModel insoleModel = new(i, insole.name, insole.MAC, this);
                    Insoles.Add(insoleModel);
                }
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
                    Metric metric, Units units,
                    List<InsoleData> left, List<InsoleData> right,
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
