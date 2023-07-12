using insoles.Model;
using insoles.States;
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
            DatabaseBridge databaseBridge = new DatabaseBridge();
            PacientesTreeView pacientes = new(databaseBridge);
            PacienteTreeView paciente1 = new PacienteTreeView("Carlos", databaseBridge);
            paciente1.Informes.Informes.Add(new InformeTreeView());
            paciente1.Tests.Tests.Add(new TestTreeView());
            PacienteTreeView paciente2 = new PacienteTreeView("Juan", databaseBridge);
            pacientes.Pacientes.Add(paciente1);
            pacientes.Pacientes.Add(paciente2);
            Pacientes.Add(pacientes);
            DataContext = this;
        }
    }
}
