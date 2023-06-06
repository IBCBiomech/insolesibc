using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.WpfExtensions;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using Image = System.Windows.Controls.Image;

namespace fpstest
{
    public class Recorder : IDisposable
    {
        private readonly VideoCaptureAPIs _videoCaptureApi = VideoCaptureAPIs.DSHOW;
        private readonly ManualResetEventSlim _threadStopEvent = new ManualResetEventSlim(false);
        private readonly VideoCapture _videoCapture;
        private VideoWriter _videoWriter;

        private Mat _capturedFrame = new Mat();
        private Thread _captureThread;
        private Thread _writerThread;
        private Image _pictureBox;
        //private OutputArray frame; 

        private bool IsVideoCaptureValid => _videoCapture != null && _videoCapture.IsOpened();

        public Recorder(int deviceIndex, int frameWidth, int frameHeight, double fps, Image pictureBox)
        {
            _videoCapture = VideoCapture.FromCamera(deviceIndex, _videoCaptureApi);
            _videoCapture.Open(deviceIndex, _videoCaptureApi);

            _videoCapture.FrameWidth = frameWidth;
            _videoCapture.FrameHeight = frameHeight;
            _videoCapture.Fps = fps;

            _pictureBox = pictureBox;
        }

        /// <inheritdoc /> 
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        ~Recorder()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopRecording();

                _videoCapture?.Release();
                _videoCapture?.Dispose();
            }
        }

        public void StartRecording(string path)
        {
            if (_writerThread != null)
                return;

            if (!IsVideoCaptureValid)
                throw new ThrowVideoCaptureNotReadyException();



            _videoWriter = new VideoWriter(path, FourCC.H265, _videoCapture.Fps, new OpenCvSharp.Size(_videoCapture.FrameWidth, _videoCapture.FrameHeight));

            _threadStopEvent.Reset();

            _captureThread = new Thread(CaptureFrameLoop);
            _captureThread.Start();

            _writerThread = new Thread(AddCameraFrameToRecordingThread);
            _writerThread.Start();
        }

        public void StopRecording()
        {
            _threadStopEvent.Set();

            _writerThread?.Join();

            _writerThread = null;

            _captureThread?.Join();
            _captureThread = null;

            _threadStopEvent.Reset();

            _videoWriter?.Release();
            _videoWriter?.Dispose();
            _videoWriter = null;
        }

        private void CaptureFrameLoop()
        {
            while (!_threadStopEvent.Wait(0))
            {
                _videoCapture.Read(_capturedFrame);
                if (!(_capturedFrame.Empty()))
                {
                    //_pictureBox.Invoke(new Action(() => _pictureBox.Image = BitmapConverter.ToBitmap(_capturedFrame)));
                    Application.Current.Dispatcher.Invoke(() => { _pictureBox.Source = BitmapSourceConverter.ToBitmapSource(_capturedFrame); });
                }
                    // imgViewport.Source = BitmapSourceConverter.ToBitmapSource(currentFrame);
                    // _pictureBox.Image = BitmapConverter.ToBitmap(_capturedFrame);

                    //  BitmapConverter.ToBitmap(_capturedFrame);

                    // _pictureBox.Image = _capturedFrame.ToBitmap();

            }
        }

        private void AddCameraFrameToRecordingThread()
        {
            var waitTimeBetweenFrames = 1_000 / _videoCapture.Fps;
            var lastWrite = DateTime.Now;

            while (!_threadStopEvent.Wait(0))
            {
                if (DateTime.Now.Subtract(lastWrite).TotalMilliseconds < waitTimeBetweenFrames)
                    continue;
                lastWrite = DateTime.Now;
                _videoWriter.Write(_capturedFrame);
            }
        }

        public Bitmap GetFrameBitmap()
        {
            if (!IsVideoCaptureValid)
                throw new ThrowVideoCaptureNotReadyException();

            using (Mat frame = new Mat())
                return !_videoCapture.Read(frame) ? null : frame.ToBitmap();
        }

        internal void TakeSnapshot()
        {
            var result = GetFrameBitmap();
            result.Save($"Snapshot{DateTime.Now.Ticks}.png", ImageFormat.Png);
        }

        public class DeviceNotFoundException : Exception
        {
            public string DeviceName { get; }

            /// <inheritdoc />
            public DeviceNotFoundException()
            {
            }

            /// <inheritdoc />
            public DeviceNotFoundException(string message) : base(message)
            {
            }

            /// <inheritdoc />
            public DeviceNotFoundException(string message, Exception innerException) : base(message, innerException)
            {
            }

            public DeviceNotFoundException(string message, string deviceName) : base(message)
            {
                DeviceName = deviceName;
            }

            public DeviceNotFoundException(string message, Exception innerException, string deviceName) : base(message, innerException)
            {
                DeviceName = deviceName;
            }
        }

        public class VideoCaptureNotReadyException : Exception
        {
            /// <inheritdoc />
            public VideoCaptureNotReadyException()
            {
            }

            /// <inheritdoc />
            public VideoCaptureNotReadyException(string message) : base(message)
            {
            }

            /// <inheritdoc />
            public VideoCaptureNotReadyException(string message, Exception innerException) : base(message, innerException)
            {
            }
        }

        //internal static class ThrowHelper
        //{
        //    public static void ThrowDeviceNotFoundException(string deviceName)
        //    {
        //        var sb = new StringBuilder($"Urządzenie o nazwie '{deviceName}' nie zostało odnalezione. Dostępne są następujące urządzenia: ");
        //        var inputDeviceNames = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice).Select(x => x.Name).ToArray();
        //        sb.Append(string.Join(", ", inputDeviceNames));
        //        throw new DeviceNotFoundException(sb.ToString(), null, deviceName);
        //    }

        //    public static void ThrowVideoCaptureNotReadyException()
        //    {
        //        throw new VideoCaptureNotReadyException();
        //    }
        //}
    }
    [Serializable]
    internal class ThrowVideoCaptureNotReadyException : Exception
    {
        public ThrowVideoCaptureNotReadyException()
        {
        }

        public ThrowVideoCaptureNotReadyException(string? message) : base(message)
        {
        }

        public ThrowVideoCaptureNotReadyException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected ThrowVideoCaptureNotReadyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
