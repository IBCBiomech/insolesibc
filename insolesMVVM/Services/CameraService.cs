using CommunityToolkit.Mvvm.Messaging;
using DirectShowLib;
using insolesMVVM.Messages;
using OpenCvSharp;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace insolesMVVM.Services
{
    public class CameraService : ICameraService
    {
        private List<CameraStreamService> cameraStreams = new List<CameraStreamService>();
        public CameraService() 
        {
            WeakReferenceMessenger.Default.Register<ScanMessage>(this, Scan);
            WeakReferenceMessenger.Default.Register<OpenCameraSelectedMessage>(this, OpenCamera);
        }
        public async void Scan(object sender, ScanMessage args)
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
            ScanCamerasMessage message = new(cameras);
            WeakReferenceMessenger.Default.Send(message);
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
        private void OpenCamera(object sender, OpenCameraSelectedMessage args)
        {
            cameraStreams.Add(new CameraStreamService(args.camera.Number));
        }
        public Mat GetInitFrame()
        {
            return CameraStreamService.GetBlackImage();
        }
    }
}
