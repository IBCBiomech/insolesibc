using insoles.Model;
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
    public class StopCommand : ICommand
    {
        private RegistroState state;
        private IApiService apiService;
        private ISaveService saveService;
        private DatabaseBridge databaseBridge;
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public StopCommand(RegistroState state, IApiService apiService, ISaveService saveService,
            DatabaseBridge databaseBridge)
        {
            this.state = state;
            this.apiService = apiService;
            this.saveService = saveService;
            this.databaseBridge = databaseBridge;
        }

        public bool CanExecute(object? parameter)
        {
            return state.recording;
        }

        public void Execute(object? parameter)
        {
            apiService.Stop();
            Test test = saveService.Stop();
            state.capturing = false;
            state.recording = false;
            Task.Run(async () =>
            {
                await databaseBridge.AddTest(state.selectedPaciente, test);
            });
        }
    }
}
