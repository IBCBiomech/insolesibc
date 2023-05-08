using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using insolesMVVM.Services;
using insolesMVVM.ViewModels;
using insolesMVVM.Views;
using System;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace insolesMVVM
{
    public partial class App : Application
    {
        public ICameraService CameraService { get; set; } = new CameraService();
        public IApiService ApiService { get; set; } = new ApiService();
        public DeviceListViewModel DeviceListViewModel { get; set; }
        public CameraViewportViewModel CameraViewportViewModel { get; set; }
        public MainWindowViewModel MainWindowViewModel { get; set; }
        public DeviceListView DeviceListView { get; set; }
        public CameraViewportView CameraViewportView { get; set; }
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                DeviceListViewModel = new();
                CameraViewportViewModel = new();
                MainWindowViewModel = new MainWindowViewModel { 
                    DeviceListViewModel = DeviceListViewModel ,
                    CameraViewportViewModel = CameraViewportViewModel
                };
                DeviceListView = new DeviceListView {
                    DataContext = DeviceListViewModel 
                };
                CameraViewportView = new CameraViewportView { 
                    DataContext = CameraViewportViewModel
                };
                desktop.MainWindow = new MainWindow { 
                    DataContext = MainWindowViewModel 
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}