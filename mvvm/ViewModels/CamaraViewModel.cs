using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using mvvm.Messages;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Wpf.Ui.Common.Interfaces;

namespace mvvm.ViewModels
{
    public partial class CamaraViewModel : ObservableObject, INavigationAware
    {
        [ObservableProperty]
        private BitmapSource _currentFrame = BitmapSourceConverter.ToBitmapSource(getBlackImage());

        private VideoCapture videoCapture;
        private CancellationTokenSource cancellationTokenSourceDisplay;
        private CancellationToken cancellationTokenDisplay;

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
            WeakReferenceMessenger.Default.Register<OpenCameraMessage>(this, onOpenCameraMessageReceived);
            _isInitialized = true;
        }
        [RelayCommand]
        private void OnClose()
        {
            Trace.WriteLine("close camera");
        }
        private static Mat getBlackImage()
        {
            MatType matType = MatType.CV_8UC3;
            Mat frame = new Mat(480, 640, matType);
            return frame;
        }
        private async void onOpenCameraMessageReceived(object sender, OpenCameraMessage args)
        {
            Trace.WriteLine("onOpenCameraMessageReceived");
            int index = args.index;
            Trace.WriteLine(index);
            CurrentFrame = BitmapSourceConverter.ToBitmapSource(getBlackImage());

            cancellationTokenSourceDisplay = new CancellationTokenSource();
            cancellationTokenDisplay = cancellationTokenSourceDisplay.Token;
            videoCapture = new VideoCapture(index, VideoCaptureAPIs.DSHOW);
            await Task.Run(() => displayCameraCallback());
        }
        private async Task displayCameraCallback()
        {
            while (true)
            {
                if (cancellationTokenDisplay.IsCancellationRequested)
                {
                    videoCapture.Release();
                    videoCapture = null;
                    Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        CurrentFrame = BitmapSourceConverter.ToBitmapSource(getBlackImage());
                    });
                    return;
                }
                Mat frame = new Mat();
                videoCapture.Read(frame);
                if (!frame.Empty())
                {
                    Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        CurrentFrame = BitmapSourceConverter.ToBitmapSource(frame);
                    });
                }
            }
        }
    }
}
