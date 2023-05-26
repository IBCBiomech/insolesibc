using insoles.Services;
using insoles.Utilities;
using System.Collections.Generic;
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
        public RegistroVM()
        {
            apiService = new ApiService();
            cameraService = new CameraService();
            ScanCommand = new RelayCommand(Scan);
            ConnectCommand = new RelayCommand(Connect);
            CaptureCommand = new RelayCommand(Capture);
            OpenCameraCommand = new RelayCommand(OpenCamera);
        }
    }
}
