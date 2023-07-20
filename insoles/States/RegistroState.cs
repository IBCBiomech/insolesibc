using insoles.Model;
using insoles.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace insoles.States
{
    public class RegistroState : INotifyPropertyChanged
    {
        private bool _paused = false;
        public bool paused { get { return _paused; } set { _paused = value; OnPropertyChanged(); } }
        private bool _capturing = false;
        public bool capturing { get { return _capturing; } set { _capturing = value; OnPropertyChanged(); } }
        private bool _recording = false;
        public bool recording { get { return _recording; } set { _recording = value; OnPropertyChanged(); } }
        private bool _calibrating = false;
        public bool calibrating { get { return _calibrating; } set { _calibrating = value; OnPropertyChanged(); } }
        public double? timeDiference { get; set; } = null;
        public double? timeDiferenceCamera { get; set; } = null;
        public int? firstIndex { get; set; } = null;
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
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
