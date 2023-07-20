using insoles.Forms;
using insoles.Model;
using insoles.Services;
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
    public class GenerarInformeCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        private Informe informe;
        private DatabaseBridge databaseBridge;
        public GenerarInformeCommand(DatabaseBridge databaseBridge, Informe informe)
        {
            this.databaseBridge = databaseBridge;
            this.informe = informe;
        }
        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            IInformesGeneratorService informesGeneratorService =
                ((MainWindow)Application.Current.MainWindow).informesGeneratorService;
            InformeFile file = new InformeFile(informesGeneratorService.GenerarInforme().Result, DateTime.Now);
            ((MainWindow)Application.Current.MainWindow).Dispatcher.BeginInvoke(async () =>
            {
                try
                {
                    await databaseBridge.GenerarInforme(informe, file);
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
