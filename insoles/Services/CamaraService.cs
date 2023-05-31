﻿using DirectShowLib;
using insoles.Messages;
using OpenCvSharp;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AForge.Video.DirectShow;

using FilterCategory = DirectShowLib.FilterCategory;

namespace insoles.Services
{
    public class CameraService : ICameraService
    {
        private List<CameraStreamService> cameraStreams = new List<CameraStreamService>();

        public event ICameraService.CameraScanEventHandler ScanReceived;

        public event ICameraService.FrameAvailableEventHandler FrameAvailable;
        public CameraService()
        {

        }

        public async void Scan()
        {
            List<string> names = await Task.Run(() => CameraNames());
            foreach (string name in names) { Trace.WriteLine(name); }
            List<int> indices = await Task.Run(() => CameraIndices(names.Count));
            foreach (int index in indices) { Trace.WriteLine(index); }
            List<int>[] fps = await Task.Run(() => cameraFps());
            Dictionary<int, int> directshowToAforge = await Task.Run(() => DirectshowAforgeMap());

            List<CameraScan> cameras = new List<CameraScan>();
            for (int i = 0; i < names.Count; i++)
            {
                if (indices.Contains(i))
                {
                    List<int> camFps = fps[directshowToAforge[i]];
                    cameras.Add(new CameraScan(i, names[i], camFps));
                }
            }
            ScanReceived?.Invoke(cameras);
        }
        List<string> CameraNames()
        {
            List<DsDevice> devices = new List<DsDevice>(DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice));
            List<string> cameraNames = new List<string>();
            foreach (DsDevice device in devices)
            {
                cameraNames.Add(device.Name);
            }
            return cameraNames;
        }
        List<int> CameraIndices(int maxIndex = 10)
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
        List<int>[] cameraFps()
        {
            var devices = new FilterInfoCollection(AForge.Video.DirectShow.FilterCategory.VideoInputDevice);
            List<int>[] cameraFps = new List<int>[devices.Count];
            for (int i = 0; i < devices.Count; i++)
            {

                cameraFps[i] = new List<int>();
                var captureDevice = new VideoCaptureDevice(devices[i].MonikerString);
                foreach (var capability in captureDevice.VideoCapabilities)
                {
                    if (!cameraFps[i].Contains(capability.AverageFrameRate))
                    {
                        cameraFps[i].Add(capability.AverageFrameRate);
                    }
                }
            }
            return cameraFps;
        }
        Dictionary<int, int> DirectshowAforgeMap()
        {
            var aforgeDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            var directshowDevices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

            var deviceMap = new Dictionary<int, int>();

            for (int ai = 0; ai < aforgeDevices.Count; ai++)
            {
                for (int dsi = 0; dsi < directshowDevices.Count(); dsi++)
                {
                    string monikerDs;
                    directshowDevices[dsi].Mon.GetDisplayName(null, null, out monikerDs);
                    if (aforgeDevices[ai].MonikerString == monikerDs)
                    {
                        deviceMap.Add(dsi, ai);
                        break;
                    }
                }
            }
            return deviceMap;
        }
        public void OpenCamera(int index, int fps)
        {
            cameraStreams.Add(new CameraStreamService(index, fps, this));
        }
        public void InvokeFrameAvailable(int index, Mat frame)
        {
            FrameAvailable?.Invoke(index, frame);
        }

        public int getFps(int index)
        {
            return cameraStreams[index].fps;
        }
    }
}