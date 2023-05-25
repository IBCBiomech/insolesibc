using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DirectShowLib;
using mvvm.Messages;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wpf.Ui.Common.Interfaces;
using Wpf.Ui.Dpi;

using WisewalkSDK;
using static WisewalkSDK.Protocol_v3;
using System.Text.RegularExpressions;
using mvvm.Services.Interfaces;
using ScottPlot.Renderable;

namespace mvvm.ViewModels
{
    public partial class ToolBarViewModel : ObservableObject
    {
        private bool _isInitialized = false;

        public ToolBarViewModel()
        {
            if (!_isInitialized)
                InitializeViewModel();
        }
        private void InitializeViewModel()
        {
            _isInitialized = true;
        }
        [RelayCommand]
        private void OnScan()
        {
            Trace.WriteLine("Scan");
            WeakReferenceMessenger.Default.Send(new ScanMessage());
        }
        [RelayCommand]
        private void OnConnect()
        {
            Trace.WriteLine("connect");
            ConnectClickMessage message = new ConnectClickMessage();
            WeakReferenceMessenger.Default.Send(message);
        }
        [RelayCommand]
        private void OnCapture()
        {
            Trace.WriteLine("capture");
            CaptureMessage message = new CaptureMessage();
            WeakReferenceMessenger.Default.Send(message);
        }
        [RelayCommand]
        private void OnOpenCamera()
        {
            Trace.WriteLine("openCamera");
            OpenCameraClickMessage message = new OpenCameraClickMessage();
            WeakReferenceMessenger.Default.Send(message);
        }
        [RelayCommand]
        private void OnRecord()
        {
            Trace.WriteLine("record");
            App.GetService<ISaveService>().Start();
        }
        [RelayCommand]
        private void OnStop()
        {
            Trace.WriteLine("stop");
            StopMessage message = new StopMessage();
            WeakReferenceMessenger.Default.Send(message);
        }
        [RelayCommand]
        private void OnStart()
        {
            Trace.WriteLine("start");
            CaptureMessage message = new CaptureMessage();
            WeakReferenceMessenger.Default.Send(message);
        }
    }
}
