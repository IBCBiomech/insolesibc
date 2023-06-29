using insoles.Model;
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

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            Paciente paciente = parameter as Paciente;
            ((MainWindow)Application.Current.MainWindow).Dispatcher.BeginInvoke(async () =>
            {
                try
                {
                    await ((MainWindow)Application.Current.MainWindow).databaseBridge.CrearCarpetaInforme(paciente);
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
