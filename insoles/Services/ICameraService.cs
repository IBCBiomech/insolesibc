using insoles.Messages;
using Emgu.CV;
using System.Collections.Generic;
using System.Drawing;

namespace insoles.Services
{
    public interface ICameraService
    {
        public const int MAX_CAMERAS = 2;
        public void Scan();
        public void OpenCamera(int index, int fps, System.Drawing.Size resolution);
        public void CloseCamera(int index);
        public int getFps(int index);
        public bool CameraOpened(int index);
        public int NumCamerasOpened { get; }
        public Size getResolution(int index);
        public int getFourcc(int index);
        public delegate void CameraScanEventHandler(List<CameraScan> data);
        public event CameraScanEventHandler ScanReceived;
        public delegate void FrameAvailableEventHandler(int index, Mat frame);
        public event FrameAvailableEventHandler FrameAvailable;
    }
}
