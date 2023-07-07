using insoles.Model;
using insoles.Models;
using insoles.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.Model
{
    public class PacienteTreeView : ModelBase
    {
        public Paciente pacienteDB { get; set; }
        public ObservableCollection<MetaFolderTreeView> MetaFolders { get; set; }
        public TestsTreeView Tests { get; set; }
        public InformesTreeView Informes { get; set; }
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
        public PacienteTreeView(Paciente paciente) 
        {
            pacienteDB = paciente;
            Tests = new(paciente.Tests);
            Informes = new(paciente.Informes);
            MetaFolders = new()
            {
                Tests,
                Informes
            };
        }
        public PacienteTreeView(string nombre) {
            pacienteDB = new Paciente(nombre, null, null, null, null, null, null, null, null);
            MetaFolders = new();
            Tests = new();
            Informes = new();
            MetaFolders.Add(Tests);
            MetaFolders.Add(Informes);
        }
    }
}
