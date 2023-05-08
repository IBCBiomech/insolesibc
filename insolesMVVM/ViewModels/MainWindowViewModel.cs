using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using insolesMVVM.Messages;
using ReactiveUI;
using System;
using System.Diagnostics;

namespace insolesMVVM.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel() 
        {
            WeakReferenceMessenger.Default.Register<ScanMessage>(this, OnScan);
            WeakReferenceMessenger.Default.Register<OpenCameraMessage>(this, OnOpenCamera);
        }
        private void OnScan(object sender, ScanMessage message)
        {
            Trace.WriteLine("OnScan MainWindowViewModel");
            Current = DeviceListViewModel;
        }
        private void OnOpenCamera(object sender, OpenCameraMessage message)
        {
            Trace.WriteLine("OnOpenCamera MainWindowViewModel");
            Current = CameraViewportViewModel;
        }
        private ViewModelBase current;
        public ViewModelBase Current
        {
            get { return current; }
            set
            {
                this.RaiseAndSetIfChanged(ref current, value);
            }
        }

        public DeviceListViewModel DeviceListViewModel { get; set; }
        public CameraViewportViewModel CameraViewportViewModel { get; set; }
    }
}