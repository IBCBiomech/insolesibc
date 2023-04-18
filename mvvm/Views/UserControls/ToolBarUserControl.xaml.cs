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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Mvvm.Interfaces;

namespace mvvm.Views.UserControls
{
    /// <summary>
    /// Lógica de interacción para ToolBarUserControl.xaml
    /// </summary>
    public partial class ToolBarUserControl : UserControl
    {
        public ViewModels.ToolBarViewModel ViewModel
        {
            get;
        }
        public ToolBarUserControl(ViewModels.ToolBarViewModel viewModel)
        {
            ViewModel = viewModel;

            InitializeComponent();

            DataContext = this;
        }
    }
}
