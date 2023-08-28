using insoles.Commands;
using insoles.States;
using System;
using System.Collections.Generic;
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
    /// Lógica de interacción para CalibrarForm.xaml
    /// </summary>
    public partial class CalibrarForm : Window
    {
        public CalibrarStartCommand calibrarCommand { get; set; }
        public CalibrarResetCommand resetCommand { get; set; }
        public RegistroState state { get; set; }
        public CalibrarForm(RegistroState state)
        {
            InitializeComponent();
            this.state = state;
            calibrarCommand = new CalibrarStartCommand(state);
            resetCommand = new CalibrarResetCommand(state);
            DataContext = this;
        }
    }
}
