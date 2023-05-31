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
        public int fps { get; private set; }
        private VideoCapture videoCapture;
        private CancellationTokenSource cancellationTokenSourceDisplay;
        private CancellationToken cancellationTokenDisplay;

        public delegate void FrameAvailableEventHandler(int index, Mat frame);
        public event FrameAvailableEventHandler FrameAvailable;
        public CameraStreamService(int index, int fps, CameraService cameraService)
        {
            this.index = index;
            this.fps = fps;
            cancellationTokenSourceDisplay = new CancellationTokenSource();
            cancellationTokenDisplay = cancellationTokenSourceDisplay.Token;
            videoCapture = new VideoCapture(index, VideoCaptureAPIs.DSHOW);
            videoCapture.Set(VideoCaptureProperties.Fps, fps);
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
