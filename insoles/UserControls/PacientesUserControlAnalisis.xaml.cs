using insoles.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace insoles.UserControls
{
    /// <summary>
    /// Lógica de interacción para PacientesUserControl.xaml
    /// </summary>
    public partial class PacientesUserControlAnalisis : UserControl
    {
        public static readonly DependencyProperty PacientesProperty =
            DependencyProperty.Register("Pacientes", typeof(ObservableCollection<PacientesTreeView>), typeof(PacientesUserControlAnalisis), new PropertyMetadata(null));

        public ObservableCollection<PacientesTreeView> Pacientes
        {
            get { return (ObservableCollection<PacientesTreeView>)GetValue(PacientesProperty); }
            set { SetValue(PacientesProperty, value); }
        }
        public PacientesUserControlAnalisis()
        {
            InitializeComponent();
        }
    }
}
