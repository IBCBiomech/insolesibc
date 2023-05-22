using insoles.DeviceList.Enums;
using insoles.ToolBar;
using insoles.ToolBar.Enums;
using MathNet.Numerics.Random;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace insoles.FileSaver
{
    public class FileSaver
    {
        private const int RECORD_CSV_MS = 10;
        private System.Timers.Timer timerCsv;
        private Stopwatch stopwatchCSV;
        private int frameCsv;
        private System.Timers.Timer timerVideo;

        private List<RecordingActive> activeRecordings;
        private List<CamaraViewport.CamaraViewport> camaraViewports;
        private TimeLine.TimeLine timeLine;
        private VirtualToolBar virtualToolBar;
        private VideoWriter? videoWriter;
        private DeviceList.DeviceList deviceList;

        private string? path;
        private string? csvFile;
        private string? videoFile;
        private bool recordCSV;
        private bool recordVideo;
        private StringBuilder? csvData = new StringBuilder();

        public delegate void filesAddedEvent(object sender, List<string> files);
        public event filesAddedEvent filesAdded;
        public FileSaver()
        {
            recordCSV = false;
            recordVideo = false;
            camaraViewports = new List<CamaraViewport.CamaraViewport>();
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.initialized += (sender, args) => finishInit();
        }
        // Para solucionar problemas de dependencias
        private void finishInit()
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            activeRecordings = new List<RecordingActive>();
            List<Frame> camaraViewportFrames = new List<Frame> { mainWindow.camaraViewport1, mainWindow.camaraViewport2 };
            foreach(Frame frame in camaraViewportFrames)
            {
                if (frame.Content == null)
                {
                    frame.Navigated += delegate (object sender, NavigationEventArgs e)
                    {
                        camaraViewports.Add(frame.Content as CamaraViewport.CamaraViewport);
                    };
                }
                else
                {
                    camaraViewports.Add(frame.Content as CamaraViewport.CamaraViewport);
                }
            }
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
            virtualToolBar = mainWindow.virtualToolBar;
            mainWindow.virtualToolBar.saveEvent += onSaveInfo;
            mainWindow.virtualToolBar.stopEvent += onStopRecording;
        }
        // Acciones para terminar de grabar
        private void onStopRecording(object sender)
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            string message = "";
            bool show = false;
            List<string> files = new List<string>();
            if (recordCSV)
            {
                //timerCsv.Stop();
                //timerCsv = null;
                //mainWindow.virtualToolBar.pauseEvent -= onPauseCsv;
                saveCsvFile();
                recordCSV = false;
                message += "Csv grabado en " + csvFile + ".\n";
                show = true;
                files.Add(path + Path.DirectorySeparatorChar + csvFile);
            }
            if (recordVideo)
            {
                foreach(RecordingActive recording in activeRecordings)
                {
                    recording.stopRecording();
                    message += "Video grabado en " + recording.filename + ".\n";
                    files.Add(path + Path.DirectorySeparatorChar + recording.filename);
                }
                show = true;
                recordVideo = false;
                activeRecordings.Clear();
            }
            if (show)
            {
                MessageBox.Show(message, caption: "Info", button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
                filesAdded?.Invoke(this, files);
            }
        }
        private string fileName()
        {
            DateTime now = DateTime.Now;
            string year = now.Year.ToString();
            string month = now.Month.ToString().PadLeft(2, '0');
            string day = now.Day.ToString().PadLeft(2, '0');
            string hour = now.Hour.ToString().PadLeft(2, '0');
            string minute = now.Minute.ToString().PadLeft(2, '0');
            string second = now.Second.ToString().PadLeft(2, '0');
            string milisecond = now.Millisecond.ToString().PadLeft(3, '0');
            string filename = year + month + day + '-' + hour + '-' + minute + '-' + second + '-' + milisecond;
            return filename;
        }
        // inicializa los ficheros para guardar csv y video
        private void initFiles()
        {
            int undefinedIndex = 0;
            string videoFilename(string baseFilename, Position? position)
            {
                switch (position)
                {
                    case Position.Body:
                        return baseFilename + "_body.avi";
                    case Position.Foot:
                        return baseFilename + "_foot.avi";
                    default:
                        return baseFilename + "_undefined"+ (undefinedIndex++) +".avi";
                }
            }
            string baseFilename = fileName();
            if (recordCSV)
            {
                csvFile = baseFilename + ".txt";
                csvData = new StringBuilder();
                csvData.Append(Config.csvHeaderInsoles);
            }
            activeRecordings.Clear();
            if (recordVideo)
            {
                foreach(CamaraViewport.CamaraViewport camaraViewport in camaraViewports)
                {
                    if (camaraViewport.record.IsChecked.Value)
                    {
                        try
                        {
                            string filename = videoFilename(baseFilename, camaraViewport.position);
                            activeRecordings.Add(new RecordingActive(camaraViewport, path, filename));
                        }
                        catch (KeyNotFoundException)
                        {
                            Trace.WriteLine("Camara not asigned");
                        }
                    }
                }
            }
        }

        //Añadde fila en un csv de forma manual

        public void appendCSVManual(string dataline)
        {
           csvData.Append(dataline);
           
        }
        // Guarda el csv
        private async void saveCsvFile()
        {
            string filePath = path + Path.DirectorySeparatorChar + csvFile;
            await File.WriteAllTextAsync(filePath, csvData.ToString());
        }
        // Se llama al seleccionar las opciones de grabacion
        public void onSaveInfo(object sender, SaveArgs args)
        {
            recordVideo = args.video;
            recordCSV = args.csv;
            path = args.directory;
            initFiles();
        }
        // Se llama cuando se cierra la aplicacion. Para guardar lo grabado
        public void onCloseApplication()
        {
            if (recordCSV)
            {
                timerCsv.Stop();
                saveCsvFile();
            }
            if (recordVideo)
            {
                foreach(RecordingActive recording in activeRecordings)
                {
                    recording.onCloseApplication();
                }
            }
        }
        public void saveFakeFile()
        {
            string stringSole(int min = 0, int max = 4095)
            {
                IEnumerable<int> randomPressures()
                {
                    Random random = new Random();
                    return random.NextInt32Sequence(min, max);
                }
                IEnumerable<int> pressures = randomPressures();
                IEnumerator<int> enumerator = pressures.GetEnumerator();
                string result = "";
                for(int i = 0; i < 7; i++)
                {
                    result += enumerator.Current.ToString() + " ";
                    enumerator.MoveNext();
                }
                result += enumerator.Current.ToString();
                return result;
            }
            csvFile = fileName() + ".txt";
            csvData = new StringBuilder();
            csvData.Append(Config.csvHeaderInsoles);
            int n = 1000;
            for(int i = 0; i < n; i++)
            {
                string dataline = "1 " + (i * 0.01f).ToString("F2") + " " +
                            (i).ToString() + " " + stringSole(0, 1000) + " " +
                            stringSole(2000, 3000) + "\n";
                csvData.Append(dataline);
            }
            string filePath = Config.INITIAL_PATH + Path.DirectorySeparatorChar + csvFile;
            File.WriteAllTextAsync(filePath, csvData.ToString());
        }
        class RecordingActive
        {
            private VideoWriter videoWriter;
            private CamaraViewport.CamaraViewport camaraViewport;
            private System.Timers.Timer timerVideo;
            public string filename { get; private set; }
            public RecordingActive(CamaraViewport.CamaraViewport camaraViewport, string path, string filename)
            {
                this.camaraViewport = camaraViewport;
                this.filename = filename;
                videoWriter = new VideoWriter(path + Path.DirectorySeparatorChar + filename, FourCC.DIVX, 
                    camaraViewport.fps, new OpenCvSharp.Size(camaraViewport.resolution.Width, camaraViewport.resolution.Height));
                timerVideo = new System.Timers.Timer();
                timerVideo.Interval = 1000 / camaraViewport.fps;
                timerVideo.Elapsed += (sender, e) => appendVideo();

                VirtualToolBar virtualToolBar = ((MainWindow)Application.Current.MainWindow).virtualToolBar;
                virtualToolBar.pauseEvent += onPauseVideo;

                if (virtualToolBar.pauseState == PauseState.Play)
                {
                    timerVideo.Start();
                }
                else
                {
                    Trace.WriteLine("record paused");
                }
            }
            private void appendVideo()
            {
                Trace.WriteLine("appendVideo");
                if (videoWriter != null)
                {
                    Mat frame = camaraViewport.currentFrame;
                    if (Config.MAT_TYPE == null)
                    {
                        Config.MAT_TYPE = Config.DEFAULT_MAT_TYPE;
                    }
                    if (frame.Type() != Config.MAT_TYPE)
                    {
                        frame.ConvertTo(frame, (MatType)Config.MAT_TYPE);
                    }
                    Mat frameResized = frame.Resize(new OpenCvSharp.Size(camaraViewport.resolution.Width, camaraViewport.resolution.Height));
                    if (videoWriter != null)
                    {
                        videoWriter.Write(frameResized);
                        Trace.WriteLine("write frame");
                    }
                }
                else
                {
                    Trace.WriteLine("video writer null");
                }
            }
            private void onPauseVideo(object sender, PauseState pauseState)
            {
                if (pauseState == PauseState.Pause)
                {
                    timerVideo.Stop();
                }
                else if (pauseState == PauseState.Play)
                {
                    timerVideo.Start();
                }
            }
            public void stopRecording()
            {
                VirtualToolBar virtualToolBar = ((MainWindow)Application.Current.MainWindow).virtualToolBar;
                timerVideo.Stop();
                timerVideo = null;
                virtualToolBar.pauseEvent -= onPauseVideo;
                videoWriter.Release();
                videoWriter = null;
            }
            public void onCloseApplication()
            {
                timerVideo.Stop();
                videoWriter.Dispose();
                videoWriter = null;
            }
        }
    }
}
