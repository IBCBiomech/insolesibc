using Avalonia;
using Avalonia.Controls;
using insolesMVVM.ViewModels;

namespace insolesMVVM.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }
    }
}