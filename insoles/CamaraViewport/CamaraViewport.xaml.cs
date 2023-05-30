using OpenCvSharp.WpfExtensions;
using OpenCvSharp;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Navigation;
using insoles.DeviceList.TreeClasses;
using insoles.DeviceList.Enums;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AvalonDock.Layout;
using System.Collections.Generic;
using insoles.FileSaver;

namespace insoles.CamaraViewport
{
    /// <summary>
    /// Lógica de interacción para CamaraViewport.xaml
    /// </summary>
// Task version

    public partial class CamaraViewport : Page
    {
        private TimeLine.TimeLine timeLine;
        private VideoCapture videoCapture;

        private CancellationTokenSource cancellationTokenSourceDisplay;
        private CancellationToken cancellationTokenDisplay;
        private Task displayTask;

        public event EventHandler cameraChanged;

        private Mat _currentFrame;

        private DeviceList.DeviceList deviceList;

        private RecordingActive? recording = null;

        public LayoutAnchorable layoutAnchorable { get; set; }
        public int? index { get; private set; } = null;

        public int fps { get; private set; }
        public Mat currentFrame
        {
            get
            {
                lock (_currentFrame)
                {
                    return _currentFrame;
                }
            }
            set
            {
                lock (_currentFrame)
                {
                    _currentFrame = value;
                }
            }
        }
        private string titleFromPosition(Position position)
        {
            switch (position)
            {
                case Position.Foot:
                    return "Foot Cam";
                case Position.Body:
                    return "Body Cam";
                default:
                    return "Unknow position";
            }
        }
        private string getTitle()
        {
            if (index == null)
            {
                return "Uninitialized camera";
            }
            else
            {
                CameraInfo? cameraInfo = deviceList.getCamera(index.Value);
                if(cameraInfo == null)
                {
                    return "Uninitialized camera";
                }
                else
                {
                    Position? position = cameraInfo.position;
                    if(position == null)
                    {
                        return cameraInfo.name;
                    }
                    else
                    {
                        return titleFromPosition(position.Value);
                    }
                }
            }
        }
        public System.Drawing.Size resolution;
        public Position? position 
        { 
            get
            {
                CameraInfo? cameraInfo = deviceList.getCamera(index.Value);
                if (cameraInfo == null)
                {
                    throw new KeyNotFoundException();
                }
                return cameraInfo.position;
            } 
        }

        public CamaraViewport()
        {
            InitializeComponent();
            _currentFrame = getBlackImage(); // Acceder directamente porque no estaba inicializado (Error sino)
            imgViewport.Source = BitmapSourceConverter.ToBitmapSource(currentFrame);
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            if (mainWindow.timeLine.Content == null)
            {
                mainWindow.timeLine.Navigated += delegate (object sender, NavigationEventArgs e)
                {
                    timeLine = mainWindow.timeLine.Content as TimeLine.TimeLine;
                };
            }
            else
            {
                timeLine = mainWindow.timeLine.Content as TimeLine.TimeLine;
            }
            if (mainWindow.deviceList.Content == null)
            {
                mainWindow.deviceList.Navigated += delegate (object sender, NavigationEventArgs e)
                {
                    deviceList = mainWindow.deviceList.Content as DeviceList.DeviceList;
                };
            }
            else
            {
                deviceList = mainWindow.deviceList.Content as DeviceList.DeviceList;
            }
            CameraInfo.positionChanged += (s,e) => { layoutAnchorable.Title = getTitle(); };
        }
        public async void initReplay(string path)
        {
            endCameraTask(); // Dejar de usar la camara
            await Dispatcher.BeginInvoke(() =>
            {
                imgViewport.Visibility = Visibility.Collapsed;
                videoViewport.Visibility = Visibility.Visible;
                videoViewport.Source = new Uri(path);
                videoViewport.LoadedBehavior = MediaState.Pause;
                videoViewport.ScrubbingEnabled = true;
            });
            timeLine.model.timeEvent -= onUpdateTimeLine;
            timeLine.model.timeEvent += onUpdateTimeLine;
        }
        // Deshace el replay
        private void clearReplay()
        {
            if (videoViewport.Source != null)
            {
                imgViewport.Visibility = Visibility.Visible;
                videoViewport.Visibility = Visibility.Collapsed;
                videoViewport.Source = null;
                timeLine.model.timeEvent -= onUpdateTimeLine;
            }
        }
        public void onUpdateTimeLine(object sender, double time)
        {
            videoViewport.Position = TimeSpan.FromSeconds(time);
        }
        // Comprueba si se esta grabano alguna camara
        public bool someCameraOpened()
        {
            return videoCapture != null;
        }
        // Pantalla en negro cuando no se graba
        private Mat getBlackImage()
        {
            MatType matType = (MatType)(Config.MAT_TYPE == null?Config.DEFAULT_MAT_TYPE:Config.MAT_TYPE);
            Mat frame = new Mat(Config.FRAME_HEIGHT, Config.FRAME_WIDTH, matType);
            return frame;
        }
        // Empieza a grabar la camara
        public async void initializeCamara(int index, int fps, System.Drawing.Size resolution)
        {
            this.fps = fps;
            this.resolution = resolution;
            // Quitar la imagen de la grabacion anterior
            this.index = index;
            currentFrame = getBlackImage();
            imgViewport.Source = BitmapSourceConverter.ToBitmapSource(currentFrame);
            
            clearReplay();

            cancellationTokenSourceDisplay = new CancellationTokenSource();
            cancellationTokenDisplay = cancellationTokenSourceDisplay.Token;
            videoCapture = new VideoCapture(index, VideoCaptureAPIs.DSHOW);
            videoCapture.Set(VideoCaptureProperties.Fps, this.fps);
            videoCapture.Set(VideoCaptureProperties.FrameHeight, resolution.Height);
            videoCapture.Set(VideoCaptureProperties.FrameWidth, resolution.Width);
            cameraChanged?.Invoke(this, EventArgs.Empty);
            layoutAnchorable.Title = getTitle();
            await Task.Run(() => displayCameraCallback());
        }
        // Cierra la camara y la ventana
        private void onClose(object sender, RoutedEventArgs e)
        {
            endCameraTask();
        }
        private void endCameraTask()
        {
            if (videoCapture != null)
            {
                cancellationTokenSourceDisplay.Cancel();
            }
        }

        // Actualiza la imagen
        private async Task displayCameraCallback()
        {
            while (true)
            {
                if (cancellationTokenDisplay.IsCancellationRequested)
                {
                    videoCapture.Release();
                    videoCapture = null;
                    currentFrame = getBlackImage();
                    await Dispatcher.BeginInvoke(DispatcherPriority.Normal, () =>
                    {
                        imgViewport.Source = BitmapSourceConverter.ToBitmapSource(getBlackImage());
                    });
                    cameraChanged?.Invoke(this, EventArgs.Empty);
                    return;
                }
                //Mat frame = new Mat();
                videoCapture.Read(currentFrame);
                if (!currentFrame.Empty())
                {
                    //currentFrame = frame;
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, () =>
                    {
                        imgViewport.Source = BitmapSourceConverter.ToBitmapSource(currentFrame);
                    }
                    );
                    if(recording != null)
                    {
                        Task.Run(() => recording.appendVideo(currentFrame));
                    }
                }
            }
        }
        public void setRecording(RecordingActive recording)
        {
            this.recording = recording;
        }
        // Cierra la camara y el video writer al cerrar la aplicacion
        public void onCloseApplication()
        {
            if(videoCapture != null)
            {
                videoCapture.Release();
            }
        }
    }
}
