using Avalonia.Controls;
using insolesMVVM.ViewModels;

namespace insolesMVVM.Views
{
    public partial class DeviceListView : UserControl
    {
        public DeviceListView()
        {
            InitializeComponent();
            DataContext = new DeviceListViewModel();
        }
    }
}
