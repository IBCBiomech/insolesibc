using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using insoles.ToolBar.Enums;

namespace insoles.ToolBar
{
    /// <summary>
    /// Lógica de interacción para ToolBar.xaml
    /// </summary>
    public partial class ToolBar : Page
    {
        private const int ANIMATION_MS = 100;
        private const int INITIAL_ICON_SIZE = 30;
        private const int PRESSED_ICON_SIZE = 25;
        private const int INITIAL_FONT_SIZE = 11;
        private const int PRESSED_FONT_SIZE = 9;

        public ToolBar()
        {
            InitializeComponent();
            DataContext = ((MainWindow)Application.Current.MainWindow).virtualToolBar.properties;
        }
        // Cambia el icono del boton Pause
        public void changePauseState(PauseState pauseState)
        {
            if (pauseState == PauseState.Pause)
            {
                pauseImage.Source = new BitmapImage(new Uri("pack://application:,,,/UI/ToolBar/Icons/Blue/pause-blue-icon.png"));
                pauseText.Text = "Pause";
            }
            else if (pauseState == PauseState.Play)
            {
                pauseImage.Source = new BitmapImage(new Uri("pack://application:,,,/UI/ToolBar/Icons/Blue/play-pause-blue-icon.png"));
                pauseText.Text = "Play";
            }
        }
        // Cambia el icono del boton Record
        public void changeRecordState(RecordState recordState)
        {
            if (recordState == RecordState.RecordStopped)
            {
                recordImage.Source = new BitmapImage(new Uri("pack://application:,,,/UI/ToolBar/Icons/Blue/record-stop-blue-icon.png"));
                recordText.Text = "Record Stopped";
            }
            else if (recordState == RecordState.Recording)
            {
                recordImage.Source = new BitmapImage(new Uri("pack://application:,,,/UI/ToolBar/Icons/record-recording-icon.png"));
                recordText.Text = "Recording...";
            }
        }
    }
}

