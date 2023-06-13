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
        private IDatabaseService databaseService;
        private ObservableCollection<Paciente> pacientes;
        public CrearPacienteCommand(IDatabaseService databaseService, 
            ObservableCollection<Paciente> pacientes)
        {
            this.databaseService = databaseService;
            this.pacientes = pacientes;
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
            CrearPacienteForm form = new CrearPacienteForm(databaseService, pacientes);
            form.ShowDialog();
        }
    }
}
