using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using System.ComponentModel;
using System.Threading;
using insoles;

namespace insoles
{
    /// <summary>
    /// Lógica de interacción para SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : Window
    {
        const int totalTimeMs = 2000;
        const int numIterations = 20;
        MainWindow mainWindow;
        public SplashScreen()
        {
            InitializeComponent();
            mainWindow = new MainWindow();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerAsync();
        }
        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 1; i <= numIterations; i++)
            {
                (sender as BackgroundWorker).ReportProgress(i);
                Thread.Sleep(totalTimeMs / numIterations);
            }
        }
        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage * 100 / numIterations;
            if (e.ProgressPercentage == numIterations)
            {
                Close();
                mainWindow.ShowDialog();
            }
        }
    }
}