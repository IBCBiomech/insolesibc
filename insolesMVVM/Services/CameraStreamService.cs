using CommunityToolkit.Mvvm.Messaging;
using insolesMVVM.Messages;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace insolesMVVM.Services
{
    public class CameraStreamService
    {
        private int index;
        private VideoCapture videoCapture;
        private CancellationTokenSource cancellationTokenSourceDisplay;
        private CancellationToken cancellationTokenDisplay;
        public CameraStreamService(int index) 
        {
            this.index = index;
            cancellationTokenSourceDisplay = new CancellationTokenSource();
            cancellationTokenDisplay = cancellationTokenSourceDisplay.Token;
            videoCapture = new VideoCapture(index);
            Task.Run(() => { DisplayCameraCallback(); });
        }
        private async Task DisplayCameraCallback()
        {
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
                Mat frame = new Mat();
                videoCapture.Read(frame);
                if (!frame.Empty())
                {
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
