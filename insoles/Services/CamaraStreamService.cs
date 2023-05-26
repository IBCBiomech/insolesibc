using insoles.Messages;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace insoles.Services
{
    public class CameraStreamService
    {
        private int index;
        private VideoCapture videoCapture;
        private CancellationTokenSource cancellationTokenSourceDisplay;
        private CancellationToken cancellationTokenDisplay;

        public delegate void FrameAvailableEventHandler(Mat frame);
        public event FrameAvailableEventHandler FrameAvailable;
        public CameraStreamService(int index)
        {
            this.index = index;
            cancellationTokenSourceDisplay = new CancellationTokenSource();
            cancellationTokenDisplay = cancellationTokenSourceDisplay.Token;
            videoCapture = new VideoCapture(index, VideoCaptureAPIs.DSHOW);
            Task.Run(() => { DisplayCameraCallback(); });
        }
        private void DisplayCameraCallback()
        {
            Mat frame = new Mat();
            while (true)
            {
                if (cancellationTokenDisplay.IsCancellationRequested)
                {
                    videoCapture.Release();
                    videoCapture = null;
                    FrameAvailable?.Invoke(GetBlackImage());
                    return;
                }
                if (videoCapture.Grab())
                {
                    videoCapture.Read(frame);
                    FrameAvailable?.Invoke(frame);
                }
            }
        }
        public static Mat GetBlackImage()
        {
            MatType matType = MatType.CV_8UC3;
            Mat frame = new Mat(480, 640, matType);
            return frame;
        }
    }
}
