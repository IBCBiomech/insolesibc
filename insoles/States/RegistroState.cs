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
        private bool _calibratingLeft = false;
        public bool calibratingLeft
        {
            get { return _calibratingLeft; }
            set { _calibratingLeft = value; OnPropertyChanged(); }
        }
        private bool _calibratingRight = false;
        public bool calibratingRight
        {
            get { return _calibratingRight; }
            set { _calibratingRight = value; OnPropertyChanged(); }
        }
        private bool _normalizing = false;
        public bool normalizing { get { return _normalizing; } set { _normalizing = value; OnPropertyChanged(); } }
        private float _lastLeft;
        public float lastLeft { get { return _lastLeft; } set { _lastLeft = value; OnPropertyChanged(); } }
        private float _lastRight;
        public float lastRight { get { return _lastRight; } set { _lastRight = value; OnPropertyChanged(); } }
        public double? timeDiference { get; set; } = null;
        public double? timeDiferenceCamera { get; set; } = null;

        public float? fcLeft { get; set; } = null;
        public float? fcRight { get; set; } = null;
        public List<float> weightsLeft { get; set; } = new List<float>();
        public List<float> weightsRight { get; set; } = new List<float>();
        private float? avgLeft_ { get; set; } = null;
        private float? avgRight_ { get; set; } = null;
        public float? avgLeft {
            get
            {
                return avgLeft_;
            }
            set
            {
                avgLeft_ = value;
                if(value == null)
                {
                    if(fcLeft != null)
                    {
                        fcLeft = null;
                    }
                    if(fcRight != null)
                    {
                        fcRight = null;
                    }
                }
                else
                {
                    if(avgRight != null)
                    {
                        CalculateFCs();
                    }
                }
            } 
        }
        public float? avgRight
        {
            get
            {
                return avgRight_;
            }
            set
            {
                avgRight_ = value;
                if (value == null)
                {
                    if (fcLeft != null)
                    {
                        fcLeft = null;
                    }
                    if (fcRight != null)
                    {
                        fcRight = null;
                    }
                }
                else
                {
                    if (avgLeft != null)
                    {
                        CalculateFCs();
                    }
                }
            }
        }
        private void CalculateFCs()
        {
            if (avgLeft > avgRight)
            {
                fcLeft = 1;
                fcRight = avgLeft / avgRight;
            }
            else
            {
                fcLeft = avgRight / avgLeft;
                fcRight = 1;
            }
            Trace.WriteLine("left " + fcLeft + " right " + fcRight);
        }
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
