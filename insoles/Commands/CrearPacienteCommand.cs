using insoles.Forms;
using insoles.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace insoles.Commands
{
    public class CrearPacienteCommand : ICommand
    {
        private IDatabaseService databaseService;
        public CrearPacienteCommand(IDatabaseService databaseService)
        {
            this.databaseService = databaseService;
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
            CrearPacienteForm form = new CrearPacienteForm(databaseService);
            form.ShowDialog();
        }
    }
}
