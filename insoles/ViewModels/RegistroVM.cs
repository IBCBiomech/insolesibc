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
using static WisewalkSDK.Wisewalk;
using insoles.States;
using insoles.Database;
using System.Threading.Tasks;

namespace insoles.ViewModel
{
    public class RegistroVM : ViewModelBase
    {
        private RegistroState state;
        private IApiService apiService;
        private ICameraService cameraService;
        private ILiveCalculationsService liveCalculationsService;
        private ISaveService saveService;
        private IDatabaseService databaseService;
        public ScanCommand scanCommand { get; set; }
        public ConnectCommand connectCommand { get; set; }
        public DisconnectCommand disconnectCommand { get; set; }
        public CaptureCommand captureCommand { get; set; }
        public OpenCameraCommand openCameraCommand { get; set; }
        public CloseCameraCommand closeCameraCommand { get; set; }
        public RecordCommand recordCommand { get; set; }
        public StopCommand stopCommand { get; set; }
        public PauseCommand pauseCommand { get; set; }
        public CrearPacienteCommand crearPacienteCommand { get; set; }
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
        public ObservableCollection<Paciente> Pacientes { get; set; }
        public RegistroVM()
        {
            state = new RegistroState();
            //currentFrames.Add(CurrentFrameTop); currentFrames.Add(CurrentFrameBottom);
            //Init services
            apiService = new FakeApiService(state);
            cameraService = new CameraService();
            liveCalculationsService = new LiveCalculationsService();
            saveService = new SaveService(state);
            databaseService = new SQLiteDatabaseService();
            //Init commands
            scanCommand = new ScanCommand(apiService, cameraService);
            connectCommand = new ConnectCommand(apiService);
            disconnectCommand = new DisconnectCommand(apiService);
            captureCommand = new CaptureCommand(state, apiService, () => Insoles);
            openCameraCommand = new OpenCameraCommand(cameraService);
            closeCameraCommand = new CloseCameraCommand(cameraService);
            recordCommand = new RecordCommand(state, saveService, cameraService);
            pauseCommand = new PauseCommand(state, apiService);
            stopCommand = new StopCommand(state, apiService, saveService);
            crearPacienteCommand = new CrearPacienteCommand(databaseService);
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
            apiService.DeviceConnected += (string mac) =>
            {
                InsoleModel insole = Insoles.First((InsoleModel insole) => insole.MAC == mac);
                insole.connected = true;
            };
            apiService.DeviceDisconnected += (string mac) =>
            {
                Trace.WriteLine("RegistroVM apiService.DeviceDisconnected");
                InsoleModel insole = Insoles.First((InsoleModel insole) => insole.MAC == mac);
                insole.connected = false;
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
            Pacientes = new ObservableCollection<Paciente>(databaseService.GetPacientes());
        }
    }
}
