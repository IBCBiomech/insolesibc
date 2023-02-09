using insoles.ToolBar;
using System;
using System.Diagnostics;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Image = System.Windows.Controls.Image;

namespace insoles.TimeLine
{
    /// <summary>
    /// Lógica de interacción para TimeLine.xaml
    /// </summary>
    public partial class TimeLine : Page
    {
        private ReplayManager replayManager;
        private RecordManager recordManager;

        private const int UPDATE_TIME_MS = 100;
        public TimeLine()
        {
            InitializeComponent();
            model = new Model(timeLine, time, UPDATE_TIME_MS);
            replayManager = new ReplayManager(this);
            recordManager = new RecordManager(this, UPDATE_TIME_MS);
            // Patron por si no esta inicializado
            try
            {
                finishInit();
            }
            catch
            {
                ((MainWindow)Application.Current.MainWindow).initialized += (s, e) => finishInit();
            }
        }
        private void finishInit()
        {
            ((MainWindow)Application.Current.MainWindow).virtualToolBar.fileOpenEvent += onFileOpen;
        }
        private void onFileOpen(object sender, string? csv, string? video)
        {
            if (csv != null)
            {
                this.csv.Text = Path.GetFileName(csv);
            }
            else
            {
                this.csv.Text = "";
            }
            if(video != null)
            {
                this.video.Text = Path.GetFileName(video);
            }
            else
            {
                this.video.Text = "";
            }
        }
        public void startReplay()
        {
            if (recordManager.active)
            {
                recordManager.deactivate();
            }
            if (!replayManager.active)
            {
                replayManager.activate();
            }
            else
            {
                replayManager.reset();
            }
        }
        public void endReplay()
        {
            if (replayManager.active)
            {
                replayManager.deactivate();
            }
        }

        // Actualiza solo el contador
        public void startRecord()
        {
            if (replayManager.active)
            {
                replayManager.deactivate();
            }
            if (!recordManager.active)
            {
                recordManager.activate();
            }
            else
            {
                recordManager.reset();
            }
        }
        public void endRecord()
        {
            if (recordManager.active)
            {
                recordManager.deactivate();
            }
        }
        public Model model { get; private set; }
        
        public double elapsed { get { return recordManager.elapsed; } }
    }
    public class ReplayManager
    {
        public bool active { private set; get; }
        private const int TICK_MS = 10;
        private System.Timers.Timer timer;
        private Stopwatch stopwatch;
        private bool playing;
        private bool _paused;
        private double deltaTime;

        private Model model;
        private Button play;
        private Button begin;
        private Button end;
        private Button pause;
        private Image pauseImage;
        private TextBlock csv;
        private TextBlock video;
        public ReplayManager(TimeLine timeLine)
        {
            model = timeLine.model;

            play = timeLine.play;
            begin = timeLine.begin;
            end = timeLine.end;
            pause = timeLine.pause;
            pauseImage = timeLine.pauseImage;
            csv = timeLine.csv;
            video = timeLine.video;
            

            active = false;
            playing = false;
            _paused = false;
            deltaTime = 0;

            stopwatch = new Stopwatch();
        }
        public void activate()
        {
            if (!active)
            {
                active = true;
                csv.Visibility = Visibility.Visible;
                video.Visibility = Visibility.Visible;
                timer = new System.Timers.Timer();
                timer.Interval = TICK_MS;
                timer.Elapsed += tick;
                play.Click += onPlay;
                model.dragEvent += onDrag;
                model.endEvent += onEnd;
                begin.Click += moveToStart;
                end.Click += moveToEnd;
                pause.Click += onPause;
                playing = false;
                model.moveToStart();
            }
        }
        public void deactivate()
        {
            if (active)
            {
                active = false;
                playing = false;
                paused = false;
                csv.Visibility = Visibility.Hidden;
                video.Visibility = Visibility.Hidden;
                timer.Elapsed -= tick;
                timer.Dispose(); 
                play.Click -= onPlay;
                model.dragEvent -= onDrag;
                model.endEvent -= onEnd;
                begin.Click -= moveToStart;
                end.Click -= moveToEnd;
                pause.Click -= onPause;
                stopwatch.Reset();
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, () =>
                {
                    model.moveToStart();
                }); // Si no se hace con el dispatcher hay una especie de race condition con el tick y no se mueve
            }
        }
        public void reset()
        {
            if (active)
            {
                timer.Stop();
                stopwatch.Reset();
                playing = false;
                pauseImage.Source = new BitmapImage(new Uri("pack://application:,,,/UI/ToolBar/Icons/pause-icon.png"));
                paused = false;
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, () =>
                {
                    model.moveToStart();
                }); // Si no se hace con el dispatcher hay una especie de race condition con el tick y no se mueve
            }
        }
        public void tick(object sender, ElapsedEventArgs e)
        {
            TimeSpan time = stopwatch.Elapsed;
            double totalTime = time.TotalSeconds + deltaTime;
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, () =>
            {
                model.setTime(totalTime);
            });
        }
        // Sirve para que el contador concuerde con el timeline
        private void onDrag(object sender, double time)
        {
            deltaTime = time;
            if (stopwatch.IsRunning)
            {
                stopwatch.Restart();
            }
            else
            {
                stopwatch.Reset();
            }
        }
        // Al pulsar el boton play
        public void onPlay(object sender, EventArgs e)
        {
            if (!playing)
            {
                playing = true;
                timer.Start();
                stopwatch.Restart();
            }
        }
        // Mueve la barra al principio
        public void moveToStart(object sender, EventArgs e)
        {
            model.moveToStart();
        }
        // Mueve la barra al final
        public void moveToEnd(object sender, EventArgs e)
        {
            model.moveToEnd();
        }
        public void onEnd(object sender, EventArgs e)
        {
            paused = true;
            /*
            pauseImage.Source = new BitmapImage(new Uri("pack://application:,,,/UI/ToolBar/Icons/play-pause-icon.png"));
            timer.Stop();
            stopwatch.Stop();
            */
        }
        // Se ejecuta al pulsar el boton de pausa
        public void onPause(object sender, EventArgs e){
            if (playing)
            {
                paused = !paused;
                /*
                if (paused)
                {
                    pauseImage.Source = new BitmapImage(new Uri("pack://application:,,,/UI/ToolBar/Icons/play-pause-icon.png"));
                    timer.Stop();
                    stopwatch.Stop();
                }
                else
                {
                    pauseImage.Source = new BitmapImage(new Uri("pack://application:,,,/UI/ToolBar/Icons/pause-icon.png"));
                    timer.Start();
                    stopwatch.Start();
                }
                */
            }
        }
        private bool paused
        {
            get
            {
                return _paused;
            }
            set
            {
                _paused = value;
                play.IsEnabled = !value;
                if (value)
                {
                    pauseImage.Source = new BitmapImage(new Uri("pack://application:,,,/ToolBar/Icons/play-pause-icon.png"));
                    timer.Stop();
                    stopwatch.Stop();
                }
                else
                {
                    pauseImage.Source = new BitmapImage(new Uri("pack://application:,,,/ToolBar/Icons/pause-icon.png"));
                    timer.Start();
                    stopwatch.Start();
                }
            }
        }
    }
    public class RecordManager
    {
        public bool active { get; private set; }
        private int TICK_MS;
        private System.Timers.Timer timer;
        private Stopwatch stopwatch;

        private VirtualToolBar virtualToolBar;
        private Model model;
        private TextBlock time;
        public RecordManager(TimeLine timeLine, int TICK_MS)
        {
            model = timeLine.model;
            time = timeLine.time;
            virtualToolBar = ((MainWindow)Application.Current.MainWindow).virtualToolBar;

            this.TICK_MS = TICK_MS;

            active = false;

            stopwatch = new Stopwatch();
        }
        public void activate()
        {
            if (!active)
            {
                active = true;
                timer = new System.Timers.Timer();
                timer.Interval = TICK_MS;
                timer.Elapsed += tick;
                timer.Start();
                virtualToolBar.stopEvent += onStop;
                stopwatch.Restart();
            }
        }
        public void deactivate()
        {
            if (active)
            {
                active = false;
                timer.Elapsed -= tick;
                virtualToolBar.stopEvent -= onStop;
                timer.Dispose();
                stopwatch.Reset();
            }
        }
        public void reset()
        {
            if (active)
            {
                stopwatch.Restart();
            }
        }
        private void onStop(object sender)
        {
            deactivate();
        }

        public void tick(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, () =>
            {
                time.Text = model.formatTimer(stopwatch.Elapsed);
            });
        }
        public double elapsed { get { return stopwatch.Elapsed.TotalSeconds; } }
    }
}
