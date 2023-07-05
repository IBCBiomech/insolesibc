using insoles.Model;
using insoles.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.States
{
    public class RegistroState
    {
        public bool paused { get; set; } = false;
        public bool capturing { get; set; } = false;
        public bool recording { get; set; } = false;
        public bool calibrating { get; set; } = false;
        public Paciente? selectedPaciente
        {
            get
            {
                return databaseBridge.GetSelectedPaciente();
            }
        }
        public Paciente fixedPaciente { get; private set; }
        private DatabaseBridge databaseBridge;
        public RegistroState(DatabaseBridge databaseBridge)
        {
            this.databaseBridge = databaseBridge;
        }
        public void fixPaciente()
        {
            if(selectedPaciente == null)
            {
                throw new Exception("No selected paciente");
            }
            fixedPaciente = selectedPaciente;
        }
    }
}
