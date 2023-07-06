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
    public partial class PacientesUserControl : UserControl
    {
        public static readonly DependencyProperty PacientesProperty =
            DependencyProperty.Register("Pacientes", typeof(ObservableCollection<Paciente>), typeof(PacientesUserControl), new PropertyMetadata(null));

        public ObservableCollection<Paciente> Pacientes
        {
            get { return (ObservableCollection<Paciente>)GetValue(PacientesProperty); }
            set { SetValue(PacientesProperty, value); }
        }
        public PacientesUserControl()
        {
            InitializeComponent();
        }
    }
}
