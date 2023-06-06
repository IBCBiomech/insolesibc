using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace fpstest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Recorder recorder = null;
        const int index = 0;
        const int width = 1920;
        const int height = 1080;
        const int fps = 15;
        public MainWindow()
        {
            InitializeComponent();
            try
            {
                recorder = new Recorder(index, width, height, fps, viewport);



            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }
        private void button2_Click(object sender, RoutedEventArgs e)
        {

            
            recorder.StopRecording();

        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (recorder is null)
            {
                recorder = new Recorder(index, width, height, fps, viewport);
            }

            recorder.StartRecording($"file{DateTime.Now.Ticks}.mp4");
        }
    }
}
