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
    public class CrearCarpetaInformeCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        private DatabaseBridge databaseBridge;
        private Paciente paciente;
        public CrearCarpetaInformeCommand(DatabaseBridge databaseBridge, Paciente paciente)
        {
            this.databaseBridge = databaseBridge;
            this.paciente = paciente;
        }
        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            ((MainWindow)Application.Current.MainWindow).Dispatcher.BeginInvoke(async () =>
            {
                try
                {
                    await databaseBridge.CrearCarpetaInforme(paciente);
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
