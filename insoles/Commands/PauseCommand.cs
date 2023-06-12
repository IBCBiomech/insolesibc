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
    public class PauseCommand : ICommand
    {
        private RegistroState state;
        private IApiService apiService;
        public PauseCommand(RegistroState state, IApiService apiService) 
        { 
            this.state = state;
            this.apiService = apiService;
        }
        
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter)
        {
            return state.capturing;
        }

        public void Execute(object? parameter)
        {
            if (state.paused)
            {
                apiService.Resume();
                state.paused = false;
            }
            else
            {
                apiService.Pause();
                state.paused = true;
            }
        }
    }
}
