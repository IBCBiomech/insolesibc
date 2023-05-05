using Avalonia.Controls;
using insolesMVVM.ViewModels;

namespace insolesMVVM.Views
{
    public partial class ToolBarView : UserControl
    {
        public ToolBarView()
        {
            InitializeComponent();
            DataContext = new ToolBarViewModel();
        }
    }
}
