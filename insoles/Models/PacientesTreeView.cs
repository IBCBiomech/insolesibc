using insoles.Commands;
using insoles.Model;
using insoles.Models;
using insoles.States;
using insoles.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.Model
{
    public class PacientesTreeView : PacientesTreeViewBase
    {
        public ICollection<PacienteTreeView> Pacientes { get; set; }
        public string Nombre { get; set; } = "Pacientes";
        public ObtenerPacientesCommand obtenerPacientesCommand { get; set; }
        public CrearPacienteCommand crearPacienteCommand { get; set; }
        public PacientesTreeView(List<Paciente> pacientes, DatabaseBridge databaseBridge) 
        {
            Pacientes = new ObservableCollection<PacienteTreeView>();
            foreach(Paciente paciente in pacientes)
            {
                Pacientes.Add(new PacienteTreeView(paciente, databaseBridge));
            }
            obtenerPacientesCommand = new(databaseBridge);
            crearPacienteCommand = new(databaseBridge);
        }
        public PacientesTreeView(DatabaseBridge databaseBridge)
        {
            Pacientes = new ObservableCollection<PacienteTreeView>();
            obtenerPacientesCommand = new(databaseBridge);
            crearPacienteCommand = new(databaseBridge);
        }
    }
}
