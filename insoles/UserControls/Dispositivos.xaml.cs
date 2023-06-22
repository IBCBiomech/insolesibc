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
    /// Lógica de interacción para Dispositivos.xaml
    /// </summary>
    public partial class Dispositivos : UserControl
    {
        public static readonly DependencyProperty InsolesProperty =
            DependencyProperty.Register("Insoles", typeof(ObservableCollection<InsoleModel>), typeof(Dispositivos), new PropertyMetadata(null));
        public ObservableCollection<InsoleModel> Insoles
        {
            get { return (ObservableCollection<InsoleModel>)GetValue(InsolesProperty); }
            set { SetValue(InsolesProperty, value); }
        }
        public static readonly DependencyProperty CamerasProperty =
            DependencyProperty.Register("Cameras", typeof(ObservableCollection<CameraModel>), typeof(Dispositivos), new PropertyMetadata(null));
        public ObservableCollection<CameraModel> Cameras
        {
            get { return (ObservableCollection<CameraModel>)GetValue(InsolesProperty); }
            set { SetValue(InsolesProperty, value); }
        }
        public Dispositivos()
        {
            InitializeComponent();
        }
    }
}
