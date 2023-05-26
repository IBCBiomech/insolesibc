using insoles.Messages;
using insoles.Services;
using insoles.Utilities;
using System.Collections.Generic;
using System.Data;
using System.Windows.Input;

namespace insoles.ViewModel
{
    public class RegistroVM : ViewModelBase
    {
        private IApiService apiService;
        private ICameraService cameraService;
        public ICommand ScanCommand { get; set; }
        public ICommand ConnectCommand { get; set; }
        public ICommand CaptureCommand { get; set; }
        public ICommand OpenCameraCommand { get; set; }

        private void Scan(object obj)
        {
            apiService.Scan();
            cameraService.Scan();
        }
        private void Connect(object obj) => apiService.Connect(new List<string> { "AC:DS" });
        private void Capture(object obj) => apiService.Capture();
        private void OpenCamera(object obj) => cameraService.OpenCamera(0);
        private DataTable insoles;
        private DataTable cameras;
        public DataTable Insoles
        {
            get { return insoles; }
            set
            {
                insoles = value;
                OnPropertyChanged();
            }
        }
        public DataTable Cameras
        {
            get { return cameras; }
            set
            {
                cameras = value;
                OnPropertyChanged();
            }
        }
        private double[] dataLeft;
        private double[] dataRight;
        public RegistroVM()
        {
            //Init services
            apiService = new ApiService();
            cameraService = new CameraService();
            //Init commands
            ScanCommand = new RelayCommand(Scan);
            ConnectCommand = new RelayCommand(Connect);
            CaptureCommand = new RelayCommand(Capture);
            OpenCameraCommand = new RelayCommand(OpenCamera);
            //Init tables columns
            insoles = new DataTable("Insoles");
            insoles.Columns.Add("Id", typeof(int));
            insoles.Columns.Add("Name", typeof(string));
            insoles.Columns.Add("MAC", typeof(string));
            cameras = new DataTable("Cameras");
            cameras.Columns.Add("Id", typeof(int));
            cameras.Columns.Add("Name", typeof(string));
            //Init listeners
            apiService.ScanReceived += (List<InsoleScan> insolesReceived) =>
            {
                Insoles.Clear();
                for(int i = 0; i < insolesReceived.Count; i++)
                {
                    InsoleScan insole = insolesReceived[i];
                    Insoles.Rows.Add(i, insole.name, insole.MAC);
                }
            };
            cameraService.ScanReceived += (List<CameraScan> camerasReceived) =>
            {
                Cameras.Clear();
                for (int i = 0; i < camerasReceived.Count; i++)
                {
                    CameraScan camera = camerasReceived[i];
                    Cameras.Rows.Add(i, camera.name);
                }
            };
        }
    }
}
