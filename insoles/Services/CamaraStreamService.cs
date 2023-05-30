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
        private CameraService cameraService;
        private int index;
        private VideoCapture videoCapture;
        private CancellationTokenSource cancellationTokenSourceDisplay;
        private CancellationToken cancellationTokenDisplay;

        public delegate void FrameAvailableEventHandler(int index, Mat frame);
        public event FrameAvailableEventHandler FrameAvailable;
        public CameraStreamService(int index, CameraService cameraService)
        {
            this.index = index;
            cancellationTokenSourceDisplay = new CancellationTokenSource();
            cancellationTokenDisplay = cancellationTokenSourceDisplay.Token;
            videoCapture = new VideoCapture(index, VideoCaptureAPIs.DSHOW);
            Task.Run(() => { DisplayCameraCallback(); });
            this.cameraService = cameraService;
        }
        private void DisplayCameraCallback()
        {
            Mat frame = new Mat();
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (true)
            {
                if (cancellationTokenDisplay.IsCancellationRequested)
                {
                    videoCapture.Release();
                    videoCapture = null;
                    cameraService.InvokeFrameAvailable(index, GetBlackImage());
                    return;
                }
                if (videoCapture.Grab())
                {
                    videoCapture.Read(frame);
                    cameraService.InvokeFrameAvailable(index, frame);
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
