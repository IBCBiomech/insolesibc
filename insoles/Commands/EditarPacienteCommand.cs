using insoles.Forms;
using insoles.Model;
using insoles.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace insoles.Commands
{
    public class EditarPacienteCommand : ICommand
    {
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
            Paciente paciente = parameter as Paciente;
            DatabaseBridge databaseBridge = ((MainWindow)Application.Current.MainWindow).databaseBridge;
            EditarPacienteForm form = new EditarPacienteForm(paciente, databaseBridge);
            form.ShowDialog();
        }
    }
}
