using insoles.Commands;
using insoles.Model;
using insoles.States;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace insoles.Forms
{
    /// <summary>
    /// Lógica de interacción para EditarPacienteForm.xaml
    /// </summary>
    public partial class EditarPacienteForm : Window, INotifyPropertyChanged
    {
        public AcceptarEditarPacienteCommand acceptarCommand { get; set; }
        private string _nombre;
        public string nombre { get { return _nombre; } set { _nombre = value; OnPropertyChanged(); } }
        private string? _apellidos;
        public string? apellidos { get { return _apellidos; } set { _apellidos = value; OnPropertyChanged(); } }
        private DateTime? _fechaNacimiento;
        public DateTime? fechaNacimiento { get { return _fechaNacimiento; } 
            set { _fechaNacimiento = value; OnPropertyChanged(); } }
        private string? _lugar;
        public string? lugar
        {
            get { return _lugar; } set { _lugar = value; OnPropertyChanged(); }
        }
        private float? _peso;
        public float? peso
        {
            get { return _peso; }
            set { _peso = value; OnPropertyChanged(); }
        }
        private float? _altura;
        public float? altura
        {
            get { return _altura; }
            set { _altura = value; OnPropertyChanged(); }
        }
        private float? _longitudPie;
        public float? longitudPie
        {
            get { return _longitudPie; }
            set { _longitudPie = value; OnPropertyChanged(); }
        }
        private int? _numeroPie;
        public int? numeroPie
        {
            get { return _numeroPie; }
            set { _numeroPie = value; OnPropertyChanged(); }
        }
        private string? _profesion;
        public string? profesion
        {
            get { return _profesion; }
            set { _profesion = value; OnPropertyChanged(); }
        }
        public EditarPacienteForm(Paciente paciente, DatabaseBridge databaseBridge)
        {
            InitializeComponent();
            nombre = paciente.Nombre;
            apellidos = paciente.Apellidos;
            fechaNacimiento = paciente.FechaNacimiento;
            lugar = paciente.Lugar;
            peso = paciente.Peso;
            altura = paciente.Altura;
            longitudPie = paciente.LongitudPie;
            numeroPie = paciente.NumeroPie;
            profesion = paciente.Profesion; 
            DataContext = this;
            acceptarCommand = new AcceptarEditarPacienteCommand(paciente, this, databaseBridge);
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
