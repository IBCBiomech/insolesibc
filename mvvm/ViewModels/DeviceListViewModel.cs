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
            WeakReferenceMessenger.Default.Register<ScanInsolesMessage>
                (this, onScanMessageReceived);
            WeakReferenceMessenger.Default.Register<ScanCamerasMessage>
                (this, onScanMessageReceived);
            WeakReferenceMessenger.Default.Register<DeviceConnectedMessage>
                (this, onDeviceConnectedMessageReceived);
            WeakReferenceMessenger.Default.Register<OpenCameraClickMessage>
                (this, onOpenCameraClickMessageReceived);
            WeakReferenceMessenger.Default.Register<ConnectClickMessage>
                (this, onConnectClickMessageReceived);

            _isInitialized = true;
        }
        private void onScanMessageReceived(object sender, ScanInsolesMessage args)
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
        private void onScanMessageReceived(object sender, ScanCamerasMessage args)
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
                    id = cameraInfo.Number; //Id de la camara
                    break;
                }
            }
            Trace.WriteLine(id);
            OpenCameraSelectedMessage message = new OpenCameraSelectedMessage(id);
            WeakReferenceMessenger.Default.Send(message);
        }
        private void onConnectClickMessageReceived(object sender, ConnectClickMessage args)
        {
            Trace.WriteLine("onOpenCameraClickMessageReceived");
            List<string> macs = new List<string>();
            foreach (var insole in Insoles)
            {
                if (insole.IsSelected)
                {
                    macs.Add(insole.Address);
                }
            }
            ConnectInsolesMessage message = new ConnectInsolesMessage(macs);
            WeakReferenceMessenger.Default.Send(message);
        }
        private void onDeviceConnectedMessageReceived(object sender, DeviceConnectedMessage args)
        {
            Trace.WriteLine("onDeviceConnectedMessageReceived from DeviceListViewModel");
            string mac = args.device.Id;
            foreach(var insole in Insoles)
            {
                if (insole.Address.Equals(mac))
                {
                    Trace.WriteLine("onDeviceConnectedMessageReceived insole found");
                    insole.Connected = true;
                    break;
                }
            }
        }
    }
}
