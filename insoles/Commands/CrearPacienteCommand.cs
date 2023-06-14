using insoles.Forms;
using insoles.Model;
using insoles.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace insoles.Commands
{
    public class CrearPacienteCommand : ICommand
    {
        private DatabaseBridge databaseBridge;
        public CrearPacienteCommand(DatabaseBridge databaseBridge)
        {
            this.databaseBridge = databaseBridge;
        }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            CrearPacienteForm form = new CrearPacienteForm(databaseBridge);
            form.ShowDialog();
        }
    }
}
