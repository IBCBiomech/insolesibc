using insoles.Forms;
using insoles.Model;
using insoles.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace insoles.Commands
{
    public class AcceptarEditarPacienteCommand : ICommand
    {
        private Paciente paciente;
        private EditarPacienteForm form;
        private DatabaseBridge databaseBridge;
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public AcceptarEditarPacienteCommand(Paciente paciente,
            EditarPacienteForm form,
            DatabaseBridge databaseBridge)
        {
            this.paciente = paciente;
            this.form = form;
            this.databaseBridge = databaseBridge;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            paciente.Nombre = form.nombre;
            paciente.Apellidos = form.apellidos;
            paciente.FechaNacimiento = form.fechaNacimiento;
            paciente.Lugar = form.lugar;
            paciente.Peso = form.peso;
            paciente.Altura = form.altura;
            paciente.LongitudPie = form.longitudPie;
            paciente.NumeroPie = form.numeroPie;
            paciente.Profesion = form.profesion;
            Task.Run(async () =>
            {
                await databaseBridge.UpdatePaciente(paciente);
            });
            form.Close();
        }
    }
}
