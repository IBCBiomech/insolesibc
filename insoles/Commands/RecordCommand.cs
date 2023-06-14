using insoles.Services;
using insoles.States;
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
        private RegistroState state;
        private ISaveService saveService;
        private ICameraService cameraService;
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public RecordCommand(RegistroState state, ISaveService saveService, ICameraService cameraService)
        {
            this.state = state;
            this.saveService = saveService;
            this.cameraService = cameraService;
        }

        public bool CanExecute(object? parameter)
        {
            return state.capturing && !state.recording;
        }

        public void Execute(object? parameter)
        {
            saveService.Start(cameraService);
            state.recording = true;
        }
    }
}
