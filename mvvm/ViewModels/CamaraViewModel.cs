using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using mvvm.Helpers;
using mvvm.Messages;
using mvvm.Services;
using mvvm.Services.Interfaces;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Wpf.Ui.Common.Interfaces;

namespace mvvm.ViewModels
{
    public partial class CamaraViewModel : ObservableObject, INavigationAware
    {
        [ObservableProperty]
        private BitmapSource _currentFrame;

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
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                CurrentFrame = BitmapSourceConverter.ToBitmapSource(
                    App.GetService<ICameraService>().GetInitFrame());
            });
            WeakReferenceMessenger.Default.Register<FrameAvailableMessage>(this, ChangeFrame);
            _isInitialized = true;
        }
        [RelayCommand]
        private void OnClose()
        {
            Trace.WriteLine("close camera");
        }
        private void ChangeFrame(object sender, FrameAvailableMessage message)
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                CurrentFrame = BitmapSourceConverter.ToBitmapSource(message.frame);
            });     
        }
    }
}
