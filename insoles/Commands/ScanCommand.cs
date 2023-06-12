using insoles.Services;
using insoles.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace insoles.Commands
{
    public class ScanCommand : ICommand
    {
        private IApiService apiService;
        private ICameraService cameraService;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public ScanCommand(IApiService apiService, ICameraService cameraService)
        {
            this.apiService = apiService;
            this.cameraService = cameraService;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            apiService.Scan();
            cameraService.Scan();
        }
    }
}
