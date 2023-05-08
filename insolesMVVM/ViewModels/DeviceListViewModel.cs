using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using CommunityToolkit.Mvvm.Messaging;
using insolesMVVM.Messages;
using insolesMVVM.Models;
using OpenCvSharp;
using ReactiveUI;

namespace insolesMVVM.ViewModels
{
	public class DeviceListViewModel : ViewModelBase
	{
		private ObservableCollection<Camera> _cameras;
		public DeviceListViewModel() 
		{
            _cameras = new ObservableCollection<Camera>();

            SourceCameras = new FlatTreeDataGridSource<Camera>(_cameras)
            {
                Columns =
                {
                    new TextColumn<Camera, int>("Number", x => x.Number),
                    new TextColumn<Camera, string>("Name", x => x.Name),
                    new TextColumn<Camera, int?>("Fps", x => x.Fps),
                },
            };

            WeakReferenceMessenger.Default.Register<ScanCamerasMessage>(this, OnScanCameras);
            WeakReferenceMessenger.Default.Register<OpenCameraMessage>(this, OnOpenCamera);
        }
        private void OnScanCameras(object sender, ScanCamerasMessage args)
        {
            Trace.WriteLine("OnScanCameras DeviceListViewModel");
            _cameras.Clear();
            foreach(var camera in args.cameras)
            {
                _cameras.Add(new Camera(camera));
            }
        }
        private void OnOpenCamera(object sender, OpenCameraMessage args)
        {
            Camera? cameraSelected = null;
            foreach(var camera in _cameras)
            {
                if (camera.IsSelected)
                {
                    Trace.Write(camera.Name + " selected");
                    cameraSelected = camera;
                    break;
                }
            }
            if(cameraSelected == null)
            {
                cameraSelected = _cameras[0];
            }
            OpenCameraSelectedMessage message = new(cameraSelected);
            WeakReferenceMessenger.Default.Send(message);
        }
        public FlatTreeDataGridSource<Camera> SourceCameras { get; }
        public IList<object> SelectedCameras { get; set; }
    }
}