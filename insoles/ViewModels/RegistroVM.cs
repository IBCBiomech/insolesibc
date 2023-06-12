using insoles.Enums;
using insoles.Messages;
using insoles.Model;
using insoles.Services;
using insoles.Utilities;
using Emgu.CV;
//using Emgu.CV.WPF; //No funciona
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WisewalkSDK;
using insoles.Commands;
using System.Windows.Navigation;

namespace insoles.ViewModel
{
    public class RegistroVM : ViewModelBase
    {
        private IApiService apiService;
        private ICameraService cameraService;
        private ILiveCalculationsService liveCalculationsService;
        private ISaveService saveService;
        public ScanCommand scanCommand { get; set; }
        public ConnectCommand connectCommand { get; set; }
        public CaptureCommand captureCommand { get; set; }
        public OpenCameraCommand openCameraCommand { get; set; }
        public CloseCameraCommand closeCameraCommand { get; set; }
        public RecordCommand recordCommand { get; set; }
        public StopCommand stopCommand { get; set; }
        public ObservableCollection<CameraModel> Cameras { get; set; }
        public ObservableCollection<InsoleModel> Insoles { get; set; }
        private BitmapSource currentFrameTop;
        public BitmapSource CurrentFrameTop
        {
            get 
            { 
                return currentFrameTop;
            }
            set
            {
                currentFrameTop = value;
                OnPropertyChanged();
            }
        }
        private BitmapSource currentFrameBottom;
        public BitmapSource CurrentFrameBottom
        {
            get
            {
                return currentFrameBottom;
            }
            set
            {
                currentFrameBottom = value;
                OnPropertyChanged();
            }
        }
        public WpfPlot Plot { get; set; }
        private GraphSumPressuresLiveModel GraphModel;
        public RegistroVM()
        {
            //currentFrames.Add(CurrentFrameTop); currentFrames.Add(CurrentFrameBottom);
            //Init services
            apiService = new FakeApiService();
            cameraService = new CameraService();
            liveCalculationsService = new LiveCalculationsService();
            saveService = new SaveService();
            //Init commands
            scanCommand = new ScanCommand(apiService, cameraService);
            connectCommand = new ConnectCommand(apiService);
            captureCommand = new CaptureCommand(apiService, () => Insoles);
            openCameraCommand = new OpenCameraCommand(cameraService);
            closeCameraCommand = new CloseCameraCommand(cameraService);
            recordCommand = new RecordCommand(saveService, cameraService);
            stopCommand = new StopCommand(saveService);
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
                if (index == 0)
                {
                    Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        CurrentFrameTop = FormatConversions.ToBitmapSource(frame);
                    });
                    saveService.AppendVideo(frame);
                }
                else if(index == 1)
                {
                    Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        CurrentFrameBottom = FormatConversions.ToBitmapSource(frame);
                    });
                    saveService.AppendVideo(frame);
                }
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
