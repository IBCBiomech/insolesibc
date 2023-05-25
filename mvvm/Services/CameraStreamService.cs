using CommunityToolkit.Mvvm.Messaging;
using mvvm.Messages;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace mvvm.Services
{
    public class CameraStreamService
    {
        private int index;
        private VideoCapture videoCapture;
        private CancellationTokenSource cancellationTokenSourceDisplay;
        private CancellationToken cancellationTokenDisplay;

        Stopwatch debugTimer;
        public CameraStreamService(int index)
        {
            Trace.WriteLine("CameraStreamService constructor " + index);
            this.index = index;
            cancellationTokenSourceDisplay = new CancellationTokenSource();
            cancellationTokenDisplay = cancellationTokenSourceDisplay.Token;
            videoCapture = new VideoCapture(index, VideoCaptureAPIs.DSHOW);
            videoCapture.Set(VideoCaptureProperties.Fps, 120); // Esto no hace nada
            double fps = videoCapture.Get(VideoCaptureProperties.Fps);
            Trace.WriteLine("fps " + fps);   
            debugTimer = new();
            debugTimer.Start();
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
                    FrameAvailableMessage message = new FrameAvailableMessage(index, GetBlackImage());
                    WeakReferenceMessenger.Default.Send(message);
                    return;
                }
                if (videoCapture.Grab())
                {
                    Trace.WriteLine("no empty " + debugTimer.Elapsed.TotalMilliseconds);
                    videoCapture.Read(frame);
                    FrameAvailableMessage message = new FrameAvailableMessage(index, frame);
                    WeakReferenceMessenger.Default.Send(message);
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
