using CommunityToolkit.Mvvm.ComponentModel;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Wpf.Ui.Common.Interfaces;

namespace mvvm.ViewModels
{
    public partial class GraphSumPressuresViewModel : ObservableObject, INavigationAware
    {
        [ObservableProperty]
        private WpfPlot _plot;

        private bool _isInitialized = false;
        public void OnNavigatedFrom()
        {
            
        }

        public void OnNavigatedTo()
        {
            if (!_isInitialized)
                InitializeViewModel();
        }
        private void InitializeViewModel()
        {

        }
    }
}
