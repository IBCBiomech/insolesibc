using Avalonia.Controls;
using insolesMVVM.ViewModels;

namespace insolesMVVM.Views
{
    public partial class ToolBar : UserControl
    {
        public ToolBar()
        {
            InitializeComponent();
            DataContext = new ToolBarViewModel();
        }
    }
}
