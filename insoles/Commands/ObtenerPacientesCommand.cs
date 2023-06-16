using insoles.States;
using System;
using System.Threading.Tasks;
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
