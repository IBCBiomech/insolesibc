﻿using OpenCvSharp.WpfExtensions;
using OpenCvSharp;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Navigation;

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
        }
        public void initReplay(string path)
        {
            endCameraTask(); // Dejar de usar la camara
            imgViewport.Visibility = Visibility.Collapsed;
            videoViewport.Visibility = Visibility.Visible;
            videoViewport.Source = new Uri(path);
            videoViewport.LoadedBehavior = MediaState.Pause;
            videoViewport.ScrubbingEnabled = true;
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
        public void initializeCamara(int index)
        {
            // Quitar la imagen de la grabacion anterior
            currentFrame = getBlackImage();
            imgViewport.Source = BitmapSourceConverter.ToBitmapSource(currentFrame);
            
            clearReplay();

            cancellationTokenSourceDisplay = new CancellationTokenSource();
            cancellationTokenDisplay = cancellationTokenSourceDisplay.Token;
            videoCapture = new VideoCapture(index, VideoCaptureAPIs.DSHOW);
            displayTask = displayCameraCallback();
            cameraChanged?.Invoke(this, EventArgs.Empty);
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
                    await Dispatcher.BeginInvoke(DispatcherPriority.Normal, () =>
                    {
                        imgViewport.Source = BitmapSourceConverter.ToBitmapSource(currentFrame);
                    }
                    );
                }
                await Task.Delay(1000 / Config.VIDEO_FPS);
            }
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