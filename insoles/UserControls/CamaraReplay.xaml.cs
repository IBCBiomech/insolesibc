using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace insoles.UserControls
{
    /// <summary>
    /// Lógica de interacción para CamaraReplay.xaml
    /// </summary>
    public partial class CamaraReplay : UserControl, INotifyPropertyChanged
    {
        public string videoPath { set
            {
                video = new Uri(value);
            } }
        private Uri? _video;
        public Uri? video { 
            get 
            {
                return _video;
            }
            set 
            {
                _video = value;
                NotifyPropertyChanged();
            } 
        }
        public double time { set
            {
                timespan = TimeSpan.FromSeconds(value);
            } 
        }
        public TimeSpan timespan
        {
            set
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => { mediaElement.Position = value; }));
            }
        }
        public CamaraReplay()
        {
            InitializeComponent();
            DataContext = this;
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
