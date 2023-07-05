using insoles.Enums;
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
using System;
using MathNet.Numerics;
using System.Windows.Documents;

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
        public CrearPacienteCommand crearPacienteCommand { get; set; }
        public CalibrarCommand calibrarCommand { get; set; }
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
        private UserControls.Units _selectedUnits;

        public UserControls.Units selectedUnits
        {
            get { return _selectedUnits; }
            set
            {
                _selectedUnits = value;
                OnPropertyChanged();
            }
        }
        public IEnumerable<UserControls.Units> units
        {
            get { return Enum.GetValues(typeof(UserControls.Units)).Cast<UserControls.Units>(); }
        }
        public bool FC { get; set; }
        private List<float> fcs = new List<float>();
        private float? fc = null;
        private const int FRAMES_TO_CALIBRATE = 50;
        private const float DEFAULT_WEIGHT = 70;
        private string _total;
        public string total { get { return _total; } set { _total = value; OnPropertyChanged(); } }
        public RegistroVM()
        {
            //Init Collections
            Cameras = new ObservableCollection<CameraModel>();
            Insoles = new ObservableCollection<InsoleModel>();

            databaseBridge = ((MainWindow)Application.Current.MainWindow).databaseBridge;
            state = new RegistroState(databaseBridge);
            //currentFrames.Add(CurrentFrameTop); currentFrames.Add(CurrentFrameBottom);
            //Init services

           /* apiService = new FakeApiService(state); *///Fake API
           
            apiService = new ApiService(state); // Real API
            cameraService = new CameraService();
            liveCalculationsService = new LiveCalculationsService(Insoles, apiService);
            saveService = new SaveService(state);
            //Init commands
            scanCommand = new ScanCommand(apiService, cameraService);
            connectCommand = new ConnectCommand(apiService);
            disconnectCommand = new DisconnectCommand(apiService);
            captureCommand = new CaptureCommand(state, apiService, () => Insoles);
            openCameraCommand = new OpenCameraCommand(cameraService);
            closeCameraCommand = new CloseCameraCommand(cameraService);
            recordCommand = new RecordCommand(state, saveService, cameraService);
            pauseCommand = new PauseCommand(state, apiService);
            stopCommand = new StopCommand(state, apiService, saveService, databaseBridge);
            crearPacienteCommand = new CrearPacienteCommand(databaseBridge);
            obtenerPacientesCommand = new ObtenerPacientesCommand(databaseBridge);
            calibrarCommand = new CalibrarCommand(state);
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
                if (state.calibrating)
                {
                    float G = 9.80665f;
                    float FNominal = state.selectedPaciente.Peso.GetValueOrDefault(DEFAULT_WEIGHT) * G;
                    for (int i = 0; i < metricLeft.Length; i++)
                    {
                        float FRegistrada = metricLeft[i] + metricRight[i];
                        if (FRegistrada > 0)
                        {
                            float fc = FNominal / FRegistrada;
                            fcs.Add(fc);
                        }
                    }
                    if(fcs.Count >= FRAMES_TO_CALIBRATE)
                    {
                        fc = fcs.Sum() / fcs.Count;
                        fcs = new();
                        state.calibrating = false;
                    }
                }
                if (FC)
                {
                    if (fc != null)
                    {
                        for (int i = 0; i < metricLeft.Length; i++)
                        {
                            metricLeft[i] *= fc.Value;
                            metricRight[i] *= fc.Value;
                            foreach (Sensor sensor in left[i].Keys)
                            {
                                left[i][sensor] *= fc.Value;
                            }
                            foreach (Sensor sensor in right[i].Keys)
                            {
                                right[i][sensor] *= fc.Value;
                            }
                        }
                    }
                }
                if (selectedUnits == UserControls.Units.N)
                {
                    GraphModel.UpdateData(metricLeft, metricRight);
                    float totalN = metricLeft[metricLeft.Length - 1] + metricRight[metricRight.Length - 1];
                    total = totalN.Round(2) + " N";
                }
                else if (selectedUnits == UserControls.Units.Kg)
                {
                    float[] metricLeftKg = new float[metricLeft.Length];
                    float[] metricRightKg = new float[metricRight.Length];
                    for (int i = 0; i < metricLeft.Length; i++)
                    {
                        metricLeftKg[i] = UnitsConversions.Kg_from_N(metricLeft[i]);
                    }
                    for (int i = 0; i < metricRight.Length; i++)
                    {
                        metricRightKg[i] = UnitsConversions.Kg_from_N(metricRight[i]);
                    }
                    GraphModel.UpdateData(metricLeftKg, metricRightKg);
                    float totalKg = metricLeftKg[metricLeft.Length - 1] + metricRightKg[metricRight.Length - 1];
                    total = totalKg.Round(2) + " Kg";
                }
                saveService.AppendCSV(left, right, metricLeft, metricRight);
            };
            Plot = new WpfPlot();
            Plot.Plot.Title("GRF");
            GraphModel = new(Plot);
        }
    }
}
