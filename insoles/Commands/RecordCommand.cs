using insoles.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace insoles.Commands
{
    public class RecordCommand : ICommand
    {
        private ISaveService saveService;
        private ICameraService cameraService;
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public RecordCommand(ISaveService saveService, ICameraService cameraService)
        {
            this.saveService = saveService;
            this.cameraService = cameraService;
        }

        public bool CanExecute(object? parameter)
        {
            return !saveService.recording;
        }

        public void Execute(object? parameter)
        {
            saveService.Start(cameraService.getFps(0), cameraService.getResolution(0), cameraService.getFourcc(0));
        }
    }
}
