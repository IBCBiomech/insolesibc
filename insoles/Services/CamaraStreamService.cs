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
        public Size resolution { get; private set; }
        private VideoCapture videoCapture;
        private CancellationTokenSource cancellationTokenSourceDisplay;
        private CancellationToken cancellationTokenDisplay;

        public delegate void FrameAvailableEventHandler(int index, Mat frame);
        public event FrameAvailableEventHandler FrameAvailable;
        public CameraStreamService(int index, int fps, System.Drawing.Size resolution,
            CameraService cameraService)
        {
            this.index = index;
            this.fps = fps;
            this.resolution = new Size(resolution.Width, resolution.Height);
            cancellationTokenSourceDisplay = new CancellationTokenSource();
            cancellationTokenDisplay = cancellationTokenSourceDisplay.Token;
            videoCapture = new VideoCapture(index, VideoCaptureAPIs.DSHOW);
            videoCapture.Set(VideoCaptureProperties.Fps, fps);
            videoCapture.Set(VideoCaptureProperties.FrameHeight, resolution.Height);
            videoCapture.Set(VideoCaptureProperties.FrameWidth, resolution.Width);
            Task.Run(() => { DisplayCameraCallback(); });
            this.cameraService = cameraService;
        }
        public CameraStreamService(int index, int fps,
            CameraService cameraService)
        {
            this.index = index;
            this.fps = fps;
            cancellationTokenSourceDisplay = new CancellationTokenSource();
            cancellationTokenDisplay = cancellationTokenSourceDisplay.Token;
            videoCapture = new VideoCapture(index, VideoCaptureAPIs.DSHOW);
            videoCapture.Set(VideoCaptureProperties.Fps, fps);
            this.resolution = new Size(videoCapture.Get(VideoCaptureProperties.FrameWidth),
                videoCapture.Get(VideoCaptureProperties.FrameHeight));
            Task.Run(() => { DisplayCameraCallback(); });
            this.cameraService = cameraService;
        }
        private void SetCodec(VideoCapture videoCapture, string codecName)
        {
            int fourCC = VideoWriter.FourCC(codecName[0], codecName[1], codecName[2], codecName[3]);
            videoCapture.Set(VideoCaptureProperties.CodecPixelFormat, fourCC);
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
                    cameraService.InvokeFrameAvailable(index, GetBlackImage(resolution));
                    return;
                }
                if (videoCapture.Grab())
                {
                    videoCapture.Retrieve(frame);
                    cameraService.InvokeFrameAvailable(index, frame);
                }
            }
        }
        public static Mat GetBlackImage(Size resolution)
        {
            MatType matType = MatType.CV_8UC3;
            Mat frame = new Mat(resolution.Height, resolution.Width, matType);
            return frame;
        }
    }
}
