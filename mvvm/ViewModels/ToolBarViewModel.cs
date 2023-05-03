﻿using CommunityToolkit.Mvvm.ComponentModel;
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
            Trace.WriteLine("scan");
            async void scanCameras()
            {
                // Devuelve el nombre de todas las camaras conectadas
                List<string> cameraNames()
                {
                    List<DsDevice> devices = new List<DsDevice>(DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice));
                    List<string> cameraNames = new List<string>();
                    foreach (DsDevice device in devices)
                    {
                        cameraNames.Add(device.Name);
                    }
                    return cameraNames;
                }
                // Devuelve una lista de indice OpenCV de las camaras disponibles
                List<int> cameraIndices(int maxIndex = 10)
                {
                    List<int> indices = new List<int>();
                    VideoCapture capture = new VideoCapture();
                    for (int index = 0; index < maxIndex; index++)
                    {
                        capture.Open(index, VideoCaptureAPIs.DSHOW);
                        if (capture.IsOpened())
                        {
                            indices.Add(index);
                            capture.Release();
                        }
                    }
                    return indices;
                }
                List<string> names = await Task.Run(() => cameraNames());
                foreach (string name in names) { Trace.WriteLine(name); }
                List<int> indices = await Task.Run(() => cameraIndices(names.Count));
                foreach (int index in indices) { Trace.WriteLine(index); }

                List<CameraScanData> cameras = new List<CameraScanData>();
                for (int i = 0; i < names.Count; i++)
                {
                    if (indices.Contains(i))
                    {
                        cameras.Add(new CameraScanData(i, names[i]));
                    }
                }
                ScanMessageCameras message = new ScanMessageCameras(cameras);
                WeakReferenceMessenger.Default.Send(message);
            }
            App.GetService<IApiService>().Scan();
            Task.Run(() => scanCameras());
        }
        [RelayCommand]
        private void OnConnect()
        {
            Trace.WriteLine("connect");
            ConnectClickMessage message = new ConnectClickMessage();
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
    }
}