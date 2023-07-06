using insoles.Model;
using insoles.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.Model
{
    public class PacienteTreeView : ModelBase
    {
        public Paciente pacienteDB;
        public TestsTreeView Tests { get; set; }
        public InformesTreeView Informes { get; set; }
        public PacienteTreeView(Paciente paciente) 
        {
            pacienteDB = paciente;
            Tests = new TestsTreeView(paciente.Tests);
            Informes = new InformesTreeView(paciente.Informes);
        }
    }
}
