using Emgu.CV;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using Emgu.CV.CvEnum;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace insoles.Services
{
    public class CameraStreamService
    {
        private CameraService cameraService;
        public int index { get; private set; }
        public int fps { 
            get {
                return (int)videoCapture.Get(CapProp.Fps);
            } 
        }
        public int fourcc { 
            get
            {
                return (int)videoCapture.Get(CapProp.FourCC);
            } 
        }
        public Size resolution { 
            get 
            { 
                return new Size((int)videoCapture.Get(CapProp.FrameWidth),
                    (int)videoCapture.Get(CapProp.FrameHeight));
            } 
        }
        private VideoCapture videoCapture;
        private CancellationTokenSource cancellationTokenSourceDisplay;
        private CancellationToken cancellationTokenDisplay;

        public delegate void FrameAvailableEventHandler(int index, Mat frame);
        public event FrameAvailableEventHandler FrameAvailable;
        public CameraStreamService(int index, int fps, System.Drawing.Size resolution,
            CameraService cameraService)
        {
            this.index = index;
            //this.resolution = new Size(resolution.Width, resolution.Height);
            cancellationTokenSourceDisplay = new CancellationTokenSource();
            cancellationTokenDisplay = cancellationTokenSourceDisplay.Token;
            videoCapture = new VideoCapture(index, VideoCapture.API.DShow);
            videoCapture.Set(CapProp.Fps, fps);
            videoCapture.Set(CapProp.FrameHeight, resolution.Height);
            videoCapture.Set(CapProp.FrameWidth, resolution.Width);
            videoCapture.Set(CapProp.Autofocus, 39);

            //videoCapture.Set(CapProp.FourCC, VideoWriter.Fourcc('H', '2', '6', '4')); // Con esto no funciona
            //videoCapture.Set(CapProp.FourCC, VideoWriter.Fourcc('D', 'I', 'V', 'X'));

            videoCapture.ImageGrabbed += (sender, args) =>
            {
                Mat frame = new Mat();
                videoCapture.Retrieve(frame);
                cameraService.InvokeFrameAvailable(index, frame);
            };
            videoCapture.Start();
            //Task.Run(() => { DisplayCameraCallback(); });
            this.cameraService = cameraService;
        }
        private void DisplayCameraCallback()
        {
            Mat frame = new Mat();
            while (true)
            {
                if (cancellationTokenDisplay.IsCancellationRequested)
                {
                    videoCapture.Dispose();
                    //cameraService.InvokeFrameAvailable(index, GetBlackImage(resolution));
                    return;
                }
                if (videoCapture.Grab())
                {
                    videoCapture.Retrieve(frame);
                    cameraService.InvokeFrameAvailable(index, frame);
                }
            }
        }
        /*
        public static Mat GetBlackImage(Size resolution)
        {
            MatType matType = MatType.CV_8UC3;
            Mat frame = new Mat(resolution.Height, resolution.Width, matType);
            return frame;
        }
        */
    }
}
