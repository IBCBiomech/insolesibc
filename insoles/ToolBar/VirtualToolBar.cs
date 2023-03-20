using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Navigation;
using insoles.ToolBar.Enums;
using insoles.FileSaver;
using insoles.Graphs;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Reflection.PortableExecutable;
using ScottPlot.Plottable;
using System.Text;
using System.Windows.Controls;
using System.Windows.Shapes;
using Path = System.IO.Path;
using OpenCvSharp;
using System.Threading.Tasks;
using System.Threading;

namespace insoles.ToolBar
{
    // Mantiene el estado para la ToolBar y la MenuBar
    public class VirtualToolBar
    {
        public VirtualToolBarProperties properties;
        public PauseState pauseState { get; set; }
        private RecordState _recordState;
        public RecordState recordState { 
            get
            {
                return _recordState;
            } 
            set 
            { 
                _recordState = value;
                recordChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public bool _capturing;
        public bool capturing
        {
            get
            {
                return _capturing;
            }
            set
            {
                _capturing = value;
                captureChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private ToolBar toolBar;
        private CamaraViewport.CamaraViewport camaraViewport;
        private TimeLine.TimeLine timeLine;

        private SavingMenu saveMenu;
        private GraphManager graphManager;

        public delegate void PauseEventHandler(object sender, PauseState args);
        public delegate void StopEventHandler(object sender);
        public event PauseEventHandler pauseEvent;
        // Se laza cuando se empieza o termina de grabar
        public event StopEventHandler stopEvent;
        // Se lanza cuando se configuran los ficheros de grabar
        public event EventHandler<SaveArgs> saveEvent;

        public event EventHandler buttonsEnabledChanged;
        public event EventHandler recordChanged;
        public event EventHandler captureChanged;

        public delegate void FileOpenHandler(object sender, string? csv, string? video);
        public event FileOpenHandler fileOpenEvent;

        public bool buttonsEnabled = false;

        string error = "";

        public VirtualToolBar()
        {
            pauseState = PauseState.Play;
            _recordState = RecordState.RecordStopped;
            properties = new VirtualToolBarProperties(this);
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.initialized += (sender, args) => finishInit();
        }
        // Para solucionar problemas de dependencias
        private void finishInit()
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            
            graphManager = mainWindow.graphManager;
            
            if (mainWindow.toolBar.Content == null)
            {
                mainWindow.toolBar.Navigated += delegate (object sender, NavigationEventArgs e)
                {
                    toolBar = mainWindow.toolBar.Content as ToolBar;
                    if (toolBar.savingMenu.Content == null)
                    {
                        toolBar.savingMenu.Navigated += delegate (object sender, NavigationEventArgs e)
                        {
                            saveMenu = toolBar.savingMenu.Content as SavingMenu;
                        };
                    }
                    else
                    {
                        saveMenu = toolBar.savingMenu.Content as SavingMenu;
                    }
                };
            }
            else
            {
                toolBar = mainWindow.toolBar.Content as ToolBar;
                if (toolBar.savingMenu.Content == null)
                {
                    toolBar.savingMenu.Navigated += delegate (object sender, NavigationEventArgs e)
                    {
                        saveMenu = toolBar.savingMenu.Content as SavingMenu;
                    };
                }
                else
                {
                    saveMenu = toolBar.savingMenu.Content as SavingMenu;
                }
            }
            if(mainWindow.camaraViewport.Content == null)
            {
                mainWindow.camaraViewport.Navigated += delegate (object sender, NavigationEventArgs e)
                {
                    camaraViewport = mainWindow.camaraViewport.Content as CamaraViewport.CamaraViewport;
                };
            }
            else
            {
                camaraViewport = mainWindow.camaraViewport.Content as CamaraViewport.CamaraViewport;
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
        }
        public void onScanClick()
        {
            if (!buttonsEnabled)
            {
                buttonsEnabled = true;
                buttonsEnabledChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public void captureClick()
        {
            capturing = true;
        }
        // Se ejecuta al clicar el boton pause
        public void pauseClick()
        {
            

            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
           

            if (pauseState == PauseState.Play)
            {

                //Para parar el streaming
                //mainWindow.api.StopStream(out error);

                pauseState = PauseState.Pause;
                toolBar.changePauseState(PauseState.Play);
                if(pauseEvent != null)
                {
                    pauseEvent?.Invoke(this, PauseState.Pause);
                }
            }
            else if(pauseState == PauseState.Pause)
            {

                //Para reaunadar el streaming
                //mainWindow.startActiveDevices();

                pauseState = PauseState.Play;
                toolBar.changePauseState(PauseState.Pause);
                if (pauseEvent != null)
                {
                    pauseEvent?.Invoke(this, PauseState.Play);
                }
            }
        }
        // Se ejecuta al clicar record
        public void recordClick()
        {
            if(recordState == RecordState.RecordStopped)
            {
                toolBar.savingMenu.Visibility = Visibility.Visible;
                if (!Directory.Exists(saveMenu.route.Text))
                {
                    saveMenu.route.Text = "";
                    MessageBox.Show("La carpeta " + saveMenu.route.Text + " no existe, selecciona otra", caption:null, button: MessageBoxButton.OK, icon: MessageBoxImage.Warning);
                }
                saveMenu.ok.Click += continueRecord;
            }
        }
        private void continueRecord(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(saveMenu.route.Text))
            {
                MessageBox.Show("La carpeta " + saveMenu.route.Text + " no existe, selecciona otra", caption: null, button: MessageBoxButton.OK, icon: MessageBoxImage.Error);
                return;
            }
            if(saveEvent != null)
            {
                saveEvent?.Invoke(this, new SaveArgs { directory = saveMenu.route.Text, csv = (bool)saveMenu.csv.IsChecked, video = (bool)saveMenu.video.IsChecked });
            }
            saveMenu.ok.Click -= continueRecord;
            toolBar.savingMenu.Visibility = Visibility.Collapsed;
            recordState = RecordState.Recording;
            toolBar.changeRecordState(RecordState.Recording);
            timeLine.startRecord();
        }
        // Se ejecuta al clicar stop
        public void stopClick()
        {
            capturing = false;
            if(pauseState == PauseState.Pause)
            {
                MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;

                //mainWindow.api.StopStream(out error);

                pauseState = PauseState.Play;
                toolBar.changePauseState(PauseState.Pause);
                
            }
            if(recordState == RecordState.Recording)
            {
                recordState = RecordState.RecordStopped;
                toolBar.changeRecordState(RecordState.RecordStopped);
            }
            if(stopEvent != null)
            {
                stopEvent?.Invoke(this);
            }
        }
        public void transformFiles()
        {
            string fileLeft = Config.INITIAL_PATH + Path.DirectorySeparatorChar + "data_L.csv";
            string fileRight = Config.INITIAL_PATH + Path.DirectorySeparatorChar + "data_R.csv";
            const int HALLUX = 0;
            const int TOES = 1;
            const int MET_1 = 2;
            const int MET_3 = 3;
            const int MET_5 = 4;
            const int ARCH = 5;
            const int HEEL_L = 6;
            const int HEEL_R = 7;
            int[] indices = new int[] {ARCH, HALLUX, HEEL_R, HEEL_L, MET_1, MET_3, MET_5, TOES   };
            using(var readerLeft = new StreamReader(fileLeft))
            {
                using (var readerRight = new StreamReader(fileRight))
                {
                    int headerLines = 1;
                    for (int _ = 0; _ < headerLines; _++)
                    {
                        readerLeft.ReadLine();
                        readerRight.ReadLine();
                        int frame = 0;
                        StringBuilder csvData = new StringBuilder();
                        csvData.Append(Config.csvHeaderInsoles);
                        while (!readerLeft.EndOfStream && !readerRight.EndOfStream)
                        {
                            string lineLeft = readerLeft.ReadLine();
                            string lineRight = readerRight.ReadLine();
                            string separator = ";";
                            string[] valuesLeft = lineLeft.Split(separator);
                            string[] valuesRight = lineRight.Split(separator);
                            string dataLine = "1 " + (frame * 0.01f).ToString("F2") + 
                                " " + frame.ToString();
                            for (int i = 0; i < indices.Length; i++)
                            {
                                string s = valuesLeft[indices[i]];
                                int v = int.Parse(s);
                                v = 4095 - v;
                                dataLine += " " + v.ToString();
                            }
                            for (int i = 0; i < indices.Length; i++)
                            {
                                string s = valuesRight[indices[i]];
                                int v = int.Parse(s);
                                v = 4095 - v;
                                dataLine += " " + v.ToString();
                            }
                            dataLine += "\n";
                            csvData.Append(dataLine);
                            frame++;
                        }
                        string filename = "data.csv";
                        string filePath = Config.INITIAL_PATH + Path.DirectorySeparatorChar + filename;
                        File.WriteAllTextAsync(filePath, csvData.ToString());
                    }
                }
            }
        }
        // Abre los ficheros (csv y avi)
        public async void openClick()
        {
            double getVideoDuration(string path)
            {
                using (ShellObject shell = ShellObject.FromParsingName(path))
                {
                    // alternatively: shell.Properties.GetProperty("System.Media.Duration");
                    IShellProperty prop = shell.Properties.System.Media.Duration;
                    ulong time_10_7 = (ulong)prop.ValueAsObject;
                    return time_10_7 / Math.Pow(10, 7);
                }
            } 
            GraphData extractCSV(string filename)
            {
                using (var reader = new StreamReader(filename))
                {
                    int headerLines = Config.csvHeaderInsoles.Split('\n').Length - 1; //Hay un salto de linea al final del header
                    string header = "";
                    for (int _ = 0; _ < headerLines; _++)
                    {
                        header += reader.ReadLine() + "\n";
                    }
                    FrameDataMetaFactory factory = new FrameDataMetaFactory();
                    factory.changeHeader(header);
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        factory.addLine(line);
                    }
                    return factory.getData();
                }
            }
            void setTimeLineLimits(GraphData csvData, string videoPath)
            {
                // Funcion para obtener la longitud del timeLine (se puede cambiar)
                double resultLength(double csvLength, double videoLength)
                {
                    return Math.Max(csvLength, videoLength);
                }
                double csvLength = csvData.maxTime;
                double videoLength = getVideoDuration(videoPath);
                timeLine.model.updateLimits(0, resultLength(csvLength, videoLength));
            }
            // Que hacer cuando esta grabando (por ahora no permitir abrir)
            if(recordState == RecordState.Recording)
            {
                return;
            }
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.InitialDirectory = Config.INITIAL_PATH;
            openFileDialog.Filter = "Allowed Files(*.txt;*.csv;*.avi)|*.txt;*.csv;*.avi";
            if(openFileDialog.ShowDialog() == true)
            {
                string[] files = openFileDialog.FileNames;
                if(files.Length == 2)
                {
                    GraphData csvData;
                    string file1 = files[0];
                    if (Path.GetExtension(file1) == ".avi")
                    {
                        string videoPath = file1;
                        string file2 = files[1];
                        if(Path.GetExtension(file2) == ".csv" || Path.GetExtension(file2) == ".txt")
                        {
                            csvData = await Task.Run(()=>extractCSV(file2));
                            setTimeLineLimits(csvData, videoPath);
                            graphManager.initReplay(csvData);
                            await Task.Run(() => camaraViewport.initReplay(videoPath));
                            fileOpenEvent?.Invoke(this, file2, file1);
                            MessageBox.Show("Ficheros " + file1 + " " + file2 + " cargados.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("El fichero de datos tiene que ser .csv o .txt", "Error de formato", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else if(Path.GetExtension(file1) == ".csv" || Path.GetExtension(file1) == ".txt")
                    {
                        string file2 = files[1];
                        if (Path.GetExtension(file2) == ".avi")
                        {
                            string videoPath = file2;
                            csvData = await Task.Run(() => extractCSV(file1));
                            setTimeLineLimits(csvData, videoPath);
                            graphManager.initReplay(csvData);
                            await Task.Run(() => camaraViewport.initReplay(videoPath));
                            fileOpenEvent?.Invoke(this, file1, file2);
                            MessageBox.Show("Ficheros " + file1 + " " + file2 + " cargados.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("El fichero de video tiene que ser .avi", "Error de formato", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("El fichero de datos tiene que ser .csv o .txt y el fichero de video tiene que ser .avi", "Error de formato", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else if(files.Length == 1)
                {
                    string file = files[0];
                    string extension = Path.GetExtension(file);
                    if(extension == ".avi")
                    {
                        string videoPath = file;
                        timeLine.model.updateLimits(0, getVideoDuration(videoPath));
                        await Task.Run(() => camaraViewport.initReplay(videoPath));
                        fileOpenEvent?.Invoke(this, null, file);
                        MessageBox.Show("Fichero " + file + " cargado.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else if(extension == ".csv" || extension == ".txt")
                    {
                        GraphData csvData = await Task.Run(() => extractCSV(file));
                        timeLine.model.updateLimits(0, csvData.maxTime);
                        graphManager.initReplay(csvData);
                        fileOpenEvent?.Invoke(this, file, null);
                        MessageBox.Show("Fichero " + file + " cargado.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Solo se admiten ficheros .csv, .txt o .avi", "Error de formato", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Selecciona 1 o 2 ficheros", "Error de numero de ficheros", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
