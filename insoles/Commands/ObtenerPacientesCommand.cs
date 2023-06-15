using insoles.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace insoles.Commands
{
    public class ObtenerPacientesCommand : ICommand
    {
        private DatabaseBridge databaseBridge;
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public ObtenerPacientesCommand(DatabaseBridge databaseBridge)
        {
            this.databaseBridge = databaseBridge;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            Task.Run(async() =>
            {
                await databaseBridge.LoadPacientes();
            });
        }
    }
}
