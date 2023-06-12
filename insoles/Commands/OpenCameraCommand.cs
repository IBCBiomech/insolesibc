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
    public class OpenCameraCommand : ICommand
    {
        private ICameraService cameraService;
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public OpenCameraCommand(ICameraService cameraService)
        {
            this.cameraService = cameraService;
        }

        public bool CanExecute(object? parameter)
        {
            CameraModel camera = parameter as CameraModel;
            return parameter != null  // Si no tira NullReferenceException
                && !cameraService.CameraOpened(camera.number)
                && cameraService.NumCamerasOpened < ICameraService.MAX_CAMERAS;
        }

        public void Execute(object? parameter)
        {
            CameraModel camera = parameter as CameraModel;
            cameraService.OpenCamera(camera.number, camera.fps, camera.resolution);
        }
    }
}
