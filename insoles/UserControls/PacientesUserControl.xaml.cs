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
            DependencyProperty.Register("Pacientes", typeof(ObservableCollection<PacientesTreeView>), typeof(PacientesUserControl), new PropertyMetadata(null));

        public ObservableCollection<PacientesTreeView> Pacientes
        {
            get { return (ObservableCollection<PacientesTreeView>)GetValue(PacientesProperty); }
            set { SetValue(PacientesProperty, value); }
        }
        public PacientesUserControl()
        {
            InitializeComponent();
        }
    }
}
