using insoles.Messages;
using OpenCvSharp;
using System.Collections.Generic;

namespace insoles.Services
{
    public interface ICameraService
    {
        public void Scan();
        public void OpenCamera(int index, int fps, System.Drawing.Size resolution);
        public int getFps(int index);
        public Size getResolution(int index);
        public delegate void CameraScanEventHandler(List<CameraScan> data);
        public event CameraScanEventHandler ScanReceived;
        public delegate void FrameAvailableEventHandler(int index, Mat frame);
        public event FrameAvailableEventHandler FrameAvailable;
    }
}
