using insoles.Forms;
using insoles.Model;
using insoles.Services;
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
    public class GenerarInformeCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        private Informe informe;
        public GenerarInformeCommand(Informe informe)
        {
            this.informe = informe;
        }
        public bool CanExecute(object? parameter)
        {
            return informe.path == null;
        }

        public void Execute(object? parameter)
        {
            IInformesGeneratorService informesGeneratorService =
                ((MainWindow)Application.Current.MainWindow).informesGeneratorService;
            informe.path = informesGeneratorService.GenerarInforme().Result;
        }
    }
}
