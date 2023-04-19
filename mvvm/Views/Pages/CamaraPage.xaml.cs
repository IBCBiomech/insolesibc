using mvvm.ViewModels;
using Wpf.Ui.Common.Interfaces;

namespace mvvm.Views.Pages
{
    /// <summary>
    /// Lógica de interacción para CamaraPage.xaml
    /// </summary>
    public partial class CamaraPage : INavigableView<ViewModels.CamaraViewModel>
    {
        public CamaraPage(ViewModels.CamaraViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
        }

        public CamaraViewModel ViewModel
        {
            get;
        }
    }
}
