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
    public class ConnectCommand : ICommand
    {
        private IApiService apiService;
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public ConnectCommand(IApiService apiService)
        {
            this.apiService = apiService;
        }

        public bool CanExecute(object? parameter)
        {
            InsoleModel insole = parameter as InsoleModel;
            return parameter != null && !insole.connected; // Si no tira NullReferenceException
        }

        public void Execute(object? parameter)
        {
            InsoleModel insole = parameter as InsoleModel;
            List<string> macs = new List<string> { insole.MAC };
            apiService.Connect(macs);
        }
    }
}
