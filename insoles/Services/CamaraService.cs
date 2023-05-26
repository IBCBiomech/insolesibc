using DirectShowLib;
using insoles.Messages;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WisewalkSDK;

namespace insoles.Services
{
    public class CameraService : ICameraService
    {
        private List<CameraStreamService> cameraStreams = new List<CameraStreamService>();

        public event ICameraService.CameraScanEventHandler ScanReceived;

        public CameraService()
        {

        }
        public async void Scan()
        {
            List<string> names = await Task.Run(() => CameraNames());
            foreach (string name in names) { Trace.WriteLine(name); }
            List<int> indices = await Task.Run(() => CameraIndices(names.Count));
            foreach (int index in indices) { Trace.WriteLine(index); }

            List<CameraScan> cameras = new List<CameraScan>();
            for (int i = 0; i < names.Count; i++)
            {
                if (indices.Contains(i))
                {
                    cameras.Add(new CameraScan(i, names[i]));
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
        public void OpenCamera(int index)
        {
            cameraStreams.Add(new CameraStreamService(index));
        }
    }
}
