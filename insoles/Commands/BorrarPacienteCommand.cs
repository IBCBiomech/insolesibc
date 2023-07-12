using insoles.Model;
using insoles.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace insoles.Commands
{
    public class BorrarPacienteCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        private DatabaseBridge databaseBridge;
        public BorrarPacienteCommand(DatabaseBridge databaseBridge)
        {
            this.databaseBridge = databaseBridge;
        }
        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            PacienteTreeView paciente = parameter as PacienteTreeView;
            ((MainWindow)Application.Current.MainWindow).Dispatcher.BeginInvoke(async () =>
            {
                try
                {
                    await databaseBridge.DeletePaciente(paciente.pacienteDB);
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e.Message);
                    throw e;
                }
            });
        }
    }
}
