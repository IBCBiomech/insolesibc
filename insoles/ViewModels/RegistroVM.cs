﻿using insoles.Enums;
using insoles.Messages;
using insoles.Model;
using insoles.Services;
using insoles.Utilities;
using Emgu.CV;
//using Emgu.CV.WPF; //No funciona
using ScottPlot;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using insoles.Commands;
using insoles.States;

namespace insoles.ViewModel
{
    public class RegistroVM : ViewModelBase
    {
        private RegistroState state;
        private IApiService apiService;
        private ICameraService cameraService;
        private ILiveCalculationsService liveCalculationsService;
        private ISaveService saveService;
        private DatabaseBridge databaseBridge;
        public ScanCommand scanCommand { get; set; }
        public ConnectCommand connectCommand { get; set; }
        public DisconnectCommand disconnectCommand { get; set; }
        public CaptureCommand captureCommand { get; set; }
        public OpenCameraCommand openCameraCommand { get; set; }
        public CloseCameraCommand closeCameraCommand { get; set; }
        public RecordCommand recordCommand { get; set; }
        public StopCommand stopCommand { get; set; }
        public PauseCommand pauseCommand { get; set; }
        public ObtenerPacientesCommand obtenerPacientesCommand { get; set; }
        public event RoutedPropertyChangedEventHandler<object> PacientesSelectionChanged;
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
        public ObservableCollection<Paciente> Pacientes
        {
            get
            {
                return databaseBridge.Pacientes;
            }
        }

        private object selectedPaciente;
        public object SelectedPaciente { 
            get 
            {
                return selectedPaciente;
            }
            set 
            { 
                selectedPaciente = value;
                Trace.WriteLine(((Paciente)value).Nombre);
                OnPropertyChanged();
            } 
        }
        public RegistroVM()
        {
            databaseBridge = new DatabaseBridge();
            ((MainWindow)Application.Current.MainWindow).databaseBridge = databaseBridge; // Temporal para acceder desde los commandos
            state = new RegistroState(databaseBridge);
            //currentFrames.Add(CurrentFrameTop); currentFrames.Add(CurrentFrameBottom);
            //Init services
            apiService = new ApiService(state);
            cameraService = new CameraService();
            liveCalculationsService = new LiveCalculationsService();
            saveService = new SaveService(state);
            //Init commands
            scanCommand = new ScanCommand(apiService, cameraService);
            connectCommand = new ConnectCommand(apiService);
            disconnectCommand = new DisconnectCommand(apiService);
            captureCommand = new CaptureCommand(state, apiService, () => Insoles, () => selectedPaciente);
            openCameraCommand = new OpenCameraCommand(cameraService);
            closeCameraCommand = new CloseCameraCommand(cameraService);
            recordCommand = new RecordCommand(state, saveService, cameraService);
            pauseCommand = new PauseCommand(state, apiService);
            stopCommand = new StopCommand(state, apiService, saveService, databaseBridge);
            crearPacienteCommand = new CrearPacienteCommand(databaseBridge);
            obtenerPacientesCommand = new ObtenerPacientesCommand(databaseBridge);
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
                        InsoleModel insoleModel = new(insole.name, insole.MAC, this);
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
            apiService.HeaderInfoReceived += (string mac, string fw, int battery) =>
            {
                InsoleModel insole = Insoles.First((InsoleModel insole) => insole.MAC == mac);
                insole.fw = fw;
                insole.battery = battery;
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
                    saveService.AppendVideo(frame, index);
                }
                else if(index == 1)
                {
                    Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        CurrentFrameBottom = FormatConversions.ToBitmapSource(frame);
                    });
                    saveService.AppendVideo(frame, index);
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
            PacientesSelectionChanged += (object sender, RoutedPropertyChangedEventArgs<object> e) =>
            {
                Trace.WriteLine("selection changed");
            };
            Plot = new WpfPlot();
            Plot.Plot.Title("test plot");
            GraphModel = new(Plot);
        }
    }
}
