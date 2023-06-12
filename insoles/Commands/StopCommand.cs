using insoles.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace insoles.Commands
{
    public class StopCommand : ICommand
    {
        private ISaveService saveService;
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public StopCommand(ISaveService saveService)
        {
            this.saveService = saveService;
        }

        public bool CanExecute(object? parameter)
        {
            return saveService.recording;
        }

        public void Execute(object? parameter)
        {
            saveService.Stop();
        }
    }
}
