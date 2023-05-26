using insoles.Services;
using insoles.Utilities;
using System.Collections.Generic;
using System.Windows.Input;

namespace insoles.ViewModel
{
    public class AnalisisVM : ViewModelBase
    {
        private IApiService apiService;
        public ICommand ScanCommand { get; set; }
        public ICommand ConnectCommand { get; set; }
        public ICommand CaptureCommand { get; set; }
        private void Scan(object obj) => apiService.Scan();
        private void Connect(object obj) => apiService.Connect(new List<string> { "AC:DS"});
        private void Capture(object obj) => apiService.Capture();
        public AnalisisVM()
        {
            apiService = new ApiService();
            ScanCommand = new RelayCommand(Scan);
            ConnectCommand = new RelayCommand(Connect);
            CaptureCommand = new RelayCommand(Capture);
        }

    }
}
