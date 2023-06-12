using insoles.Model;
using insoles.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace insoles.Commands
{
    public class CloseCameraCommand : ICommand
    {
        private ICameraService cameraService;
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public CloseCameraCommand(ICameraService cameraService)
        {
            this.cameraService = cameraService;
        }

        public bool CanExecute(object? parameter)
        {
            CameraModel camera = parameter as CameraModel;
            return parameter != null  // Si no tira NullReferenceException
                && cameraService.CameraOpened(camera.number);
        }

        public void Execute(object? parameter)
        {
            CameraModel camera = parameter as CameraModel;
            cameraService.CloseCamera(camera.number);
        }
    }
}
