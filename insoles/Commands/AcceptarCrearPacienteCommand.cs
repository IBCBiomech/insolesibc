using insoles.Database;
using insoles.Forms;
using insoles.Model;
using insoles.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace insoles.Commands
{
    public class AcceptarCrearPacienteCommand : ICommand
    {
        private CrearPacienteForm form;
        private DatabaseBridge databaseBridge;
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public AcceptarCrearPacienteCommand(CrearPacienteForm form, 
            DatabaseBridge databaseBridge)
        {
            this.form = form;
            this.databaseBridge = databaseBridge;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            Trace.WriteLine("AcceptarCreatePacienteCommand executed");
            Paciente paciente = new Paciente(form.nombre, form.apellidos, form.fechaNacimiento,
                form.lugar, form.peso, form.altura, form.longitudPie, form.numeroPie, form.profesion);
            databaseBridge.AddPaciente(paciente);
            form.Close();
        }
    }
}
