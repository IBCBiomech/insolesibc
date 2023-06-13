using insoles.Commands;
using insoles.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    /// Lógica de interacción para CrearPacienteForm.xaml
    /// </summary>
    public partial class CrearPacienteForm : Window
    {
        public AcceptarCrearPacienteCommand acceptarCommand { get; set; }
        public string nombre { get;set; }
        public string apellidos { get; set; }
        public DateTime fechaNacimiento { get; set; }
        public string lugar { get; set; }
        public float peso { get; set; }
        public float altura { get; set; }
        public float longitudPie { get; set; }
        public int numeroPie { get; set; }
        public string profesion { get; set; }
        public CrearPacienteForm(IDatabaseService databaseService)
        {
            InitializeComponent();
            DataContext = this;
            acceptarCommand = new AcceptarCrearPacienteCommand(this, databaseService);
        }
    }
}
