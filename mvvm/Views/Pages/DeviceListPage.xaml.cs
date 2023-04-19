using Wpf.Ui.Common.Interfaces;

namespace mvvm.Views.Pages
{
    /// <summary>
    /// Lógica de interacción para DeviceList.xaml
    /// </summary>
    public partial class DeviceListPage : INavigableView<ViewModels.DeviceListViewModel>
    {
        public ViewModels.DeviceListViewModel ViewModel
        {
            get;
        }
        public DeviceListPage(ViewModels.DeviceListViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
        }
    }
}
