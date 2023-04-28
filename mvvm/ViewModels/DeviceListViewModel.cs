using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using mvvm.Messages;
using mvvm.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Controls;
using Wpf.Ui.Common.Interfaces;

namespace mvvm.ViewModels
{
    public partial class DeviceListViewModel : ObservableObject, INavigationAware
    {

        private bool _isInitialized = false;

        [ObservableProperty]
        private IEnumerable<CameraInfo> _cameras;

        [ObservableProperty]
        private IEnumerable<InsoleInfo> _insoles;

        [ObservableProperty]
        private IEnumerable<object> _itemsSelected;
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
            WeakReferenceMessenger.Default.Register<ScanMessageInsoles>(this, onScanMessageReceived);
            WeakReferenceMessenger.Default.Register<ScanMessageCameras>(this, onScanMessageReceived);
            WeakReferenceMessenger.Default.Register<OpenCameraClickMessage>(this, onOpenCameraClickMessageReceived);

            _isInitialized = true;
        }
        private void onScanMessageReceived(object sender, ScanMessageInsoles args)
        {
            Trace.WriteLine("onCameraScanMessageReceived Insoles");
            var insolesCollection = new List<InsoleInfo>();
            var insolesMessage = args.Insoles;
            foreach(var insole in insolesMessage)
            {
                insolesCollection.Add(new InsoleInfo(insole));
            }
            Insoles = insolesCollection;
        }
        private void onScanMessageReceived(object sender, ScanMessageCameras args)
        {
            Trace.WriteLine("onCameraScanMessageReceived cameras");
            var camerasCollection = new List<CameraInfo>();
            var camerasMessage = args.cameras;
            foreach (var c in camerasMessage)
            {
                camerasCollection.Add(new CameraInfo(
                c
                ));
                Trace.WriteLine(c.number + " " + c.name);
            }
            Cameras = camerasCollection;
        }
        private void onOpenCameraClickMessageReceived(object sender, OpenCameraClickMessage args)
        {
            Trace.WriteLine("onOpenCameraClickMessageReceived");
            int id = -1;
            foreach (var cameraInfo in Cameras)
            {
                if (cameraInfo.IsSelected)
                {
                    id = cameraInfo.number; //Id de la camara
                    break;
                }
            }
            Trace.WriteLine(id);
            OpenCameraMessage message = new OpenCameraMessage(id);
            WeakReferenceMessenger.Default.Send(message);
        }
    }
}
