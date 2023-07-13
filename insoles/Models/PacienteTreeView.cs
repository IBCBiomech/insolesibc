using insoles.Commands;
using insoles.Model;
using insoles.Models;
using insoles.States;
using insoles.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.Model
{
    public class PacienteTreeView : PacientesTreeViewBase
    {
        public Paciente pacienteDB { get; set; }
        public ObservableCollection<MetaFolderTreeView> MetaFolders { get; set; }
        public TestsTreeView Tests { get; set; }
        public InformesTreeView Informes { get; set; }
        public EditarPacienteCommand editarPacienteCommand { get; set; }
        public BorrarPacienteCommand borrarPacienteCommand { get; set; }
        public string Nombre { 
            get
            {
                return pacienteDB.Nombre;
            } 
            set
            {
                pacienteDB.Nombre = value;
            }
        }
        public PacienteTreeView(Paciente paciente, DatabaseBridge databaseBridge) 
        {
            pacienteDB = paciente;
            Tests = new(paciente.Tests, databaseBridge, paciente);
            Informes = new(paciente.Informes, databaseBridge, pacienteDB);
            MetaFolders = new()
            {
                Tests,
                Informes
            };
            editarPacienteCommand = new EditarPacienteCommand(databaseBridge);
            borrarPacienteCommand = new BorrarPacienteCommand(databaseBridge);
        }
        public PacienteTreeView(string nombre, DatabaseBridge databaseBridge) {
            pacienteDB = new Paciente(nombre, null, null, null, null, null, null, null, null);
            MetaFolders = new();
            Tests = new(databaseBridge, pacienteDB);
            Informes = new(databaseBridge, pacienteDB);
            MetaFolders.Add(Tests);
            MetaFolders.Add(Informes);
        }
    }
}
