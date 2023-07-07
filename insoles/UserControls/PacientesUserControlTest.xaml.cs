using insoles.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace insoles.UserControls
{
    /// <summary>
    /// Lógica de interacción para PacientesUserControl.xaml
    /// </summary>
    public partial class PacientesUserControlTest : UserControl
    {
        public ObservableCollection<PacientesTreeView> Pacientes
        {
            get;
            set;
        }
        public PacientesUserControlTest()
        {
            InitializeComponent();
            Pacientes = new();
            PacientesTreeView pacientes = new();
            PacienteTreeView paciente1 = new PacienteTreeView("Carlos");
            PacienteTreeView paciente2 = new PacienteTreeView("Juan");
            pacientes.Pacientes.Add(paciente1);
            pacientes.Pacientes.Add(paciente2);
            Pacientes.Add(pacientes);
            DataContext = this;
        }
    }
}
