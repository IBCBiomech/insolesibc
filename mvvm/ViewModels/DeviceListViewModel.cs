using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using mvvm.Messages;
using mvvm.Models;
using System.Collections.Generic;
using System.Diagnostics;
using Wpf.Ui.Common.Interfaces;

namespace mvvm.ViewModels
{
    public partial class DeviceListViewModel : ObservableObject, INavigationAware
    {

        private bool _isInitialized = false;

        [ObservableProperty]
        private IEnumerable<CameraInfo> _cameras;

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
            WeakReferenceMessenger.Default.Register<List<CameraScanMessage>>(this, onCameraScanMessageReceived);

            _isInitialized = true;
        }
        private void onCameraScanMessageReceived(object sender, List<CameraScanMessage> args)
        {
            Trace.WriteLine("onCameraScanMessageReceived");
            var camerasCollection = new List<CameraInfo>();
            foreach (var c in args)
            {
                camerasCollection.Add(new CameraInfo(
                c
                ));
                Trace.WriteLine(c.number + " " + c.name);
            }
            Cameras = camerasCollection;
        }
    }
}
