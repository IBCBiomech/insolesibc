using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using insolesMVVM.Messages;
using System.Diagnostics;

namespace insolesMVVM.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel() 
        {
            Current = new DeviceListViewModel();
        }
        public ViewModelBase Current { get;private set; }
    }
}