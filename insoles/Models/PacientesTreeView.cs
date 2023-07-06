using insoles.Model;
using insoles.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.Model
{
    public class PacientesTreeView : ModelBase
    {
        public ICollection<PacienteTreeView> Pacientes { get; set; }
        public PacientesTreeView(List<Paciente> pacientes) 
        {
            Pacientes = new ObservableCollection<PacienteTreeView>();
            foreach(Paciente paciente in pacientes)
            {
                Pacientes.Add(new PacienteTreeView(paciente));
            }
        }
    }
}
