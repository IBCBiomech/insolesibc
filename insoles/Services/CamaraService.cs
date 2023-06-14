using DirectShowLib;
using insoles.Messages;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AForge.Video.DirectShow;

using FilterCategory = DirectShowLib.FilterCategory;
using System.Drawing;
using Emgu.CV;

namespace insoles.Services
{
    public class CameraService : ICameraService
    {
        private List<CameraStreamService> cameraStreams = new List<CameraStreamService>();

        public int NumCamerasOpened => cameraStreams.Count;

        public event ICameraService.CameraScanEventHandler ScanReceived;

        public event ICameraService.FrameAvailableEventHandler FrameAvailable;
        public Dictionary<int, int> indicesMap = new Dictionary<int, int>();
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
            Dictionary<int, List<System.Drawing.Size>>[] resolutions = await Task.Run(() => cameraResolutions());
            Dictionary<int, int> directshowToAforge = await Task.Run(() => DirectshowAforgeMap());

            List<CameraScan> cameras = new List<CameraScan>();
            for (int i = 0; i < names.Count; i++)
            {
                if (indices.Contains(i))
                {
                    List<int> camFps = fps[directshowToAforge[i]];
                    Dictionary<int, List<System.Drawing.Size>> camResolutions = resolutions[directshowToAforge[i]];
                    cameras.Add(new CameraScan(i, names[i], camFps, camResolutions));
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
            for (int index = 0; index < maxIndex; index++)
            {
                using (VideoCapture capture = new VideoCapture(index))
                {
                    if (capture.IsOpened)
                    {
                        indices.Add(index);
                        capture.Dispose();
                    }
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
        Dictionary<int, List<System.Drawing.Size>>[] cameraResolutions()
        {
            var devices = new FilterInfoCollection(AForge.Video.DirectShow.FilterCategory.VideoInputDevice);
            Dictionary<int, List<System.Drawing.Size>>[] resolutions = new Dictionary<int, List<System.Drawing.Size>>[devices.Count];
            for (int i = 0; i < devices.Count; i++)
            {

                resolutions[i] = new Dictionary<int, List<System.Drawing.Size>>();
                var captureDevice = new VideoCaptureDevice(devices[i].MonikerString);
                foreach (var capability in captureDevice.VideoCapabilities)
                {
                    if (!resolutions[i].ContainsKey(capability.AverageFrameRate))
                    {
                        resolutions[i][capability.AverageFrameRate] = new List<System.Drawing.Size>();
                    }
                    resolutions[i][capability.AverageFrameRate].Add(capability.FrameSize);
                }
            }
            return resolutions;
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
        public void OpenCamera(int index, int fps, System.Drawing.Size resolution)
        {
            cameraStreams.Add(new CameraStreamService(index, fps, resolution, this)); //Al cambiar la resolucion va muy lento
            for(int i = 0; i < ICameraService.MAX_CAMERAS; i++)
            {
                if (!indicesMap.ContainsValue(i))
                {
                    indicesMap[index] = i;
                    break;
                }
            }
        }
        public void InvokeFrameAvailable(int index, Mat frame)
        {
            try
            {
                FrameAvailable?.Invoke(indicesMap[index], frame);
            }
            catch(KeyNotFoundException)
            {
                Trace.WriteLine("InvokeFrameAvailable KeyNotFoundException");
            }
        }

        public int getFps(int index)
        {
            return cameraStreams[index].fps;
        }

        public Size getResolution(int index)
        {
            return cameraStreams[0].resolution;
        }

        public int getFourcc(int index)
        {
            return cameraStreams[0].fourcc;
        }

        public bool CameraOpened(int index)
        {
            foreach(CameraStreamService stream in cameraStreams)
            {
                if (stream.index == index)
                    return true;
            }
            return false;
        }

        public void CloseCamera(int index)
        {
            foreach (CameraStreamService stream in cameraStreams)
            {
                if (stream.index == index)
                {
                    stream.Stop();
                    cameraStreams.Remove(stream);
                    indicesMap.Remove(index);
                    break;
                }             
            }
        }
    }
}
