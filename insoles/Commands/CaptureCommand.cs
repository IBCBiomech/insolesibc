using insoles.Model;
using insoles.Services;
using insoles.States;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace insoles.Commands
{
    public class CaptureCommand : ICommand
    {
        private RegistroState state;
        private Func<ObservableCollection<InsoleModel>> getInsoles;
        private IApiService apiService;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public CaptureCommand(RegistroState state, IApiService apiService, Func<ObservableCollection<InsoleModel>> getInsoles)
        {
            this.state = state;
            this.apiService = apiService;
            this.getInsoles = getInsoles;
        }

        public bool CanExecute(object? parameter)
        {
            ObservableCollection<InsoleModel> insoles = getInsoles();
            return insoles.Where((InsoleModel insole) => insole.connected).Count() == 2
                && !state.capturing;
        }

        public void Execute(object? parameter)
        {
            apiService.Capture();
            state.capturing = true;
        }
    }
}
