using insoles.Commands;
using insoles.States;
using System;
using System.Windows;

namespace insoles.Forms
{
    /// <summary>
    /// Lógica de interacción para CrearPacienteForm.xaml
    /// </summary>
    public partial class CrearPacienteForm : Window
    {
        public AcceptarCrearPacienteCommand acceptarCommand { get; set; }
        public string nombre { get;set; }
        public string? apellidos { get; set; }
        public DateTime? fechaNacimiento { get; set; }
        public string? lugar { get; set; }
        public float? peso { get; set; }
        public float? altura { get; set; }
        public float? longitudPie { get; set; }
        public int? numeroPie { get; set; }
        public string? profesion { get; set; }
        public CrearPacienteForm(DatabaseBridge databaseBridge)
        {
            InitializeComponent();
            DataContext = this;
            acceptarCommand = new AcceptarCrearPacienteCommand(this, databaseBridge);
        }
    }
}
