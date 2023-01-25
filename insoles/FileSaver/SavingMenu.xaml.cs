using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace insoles.FileSaver
{
    /// <summary>
    /// Lógica de interacción para SavingMenu.xaml
    /// </summary>
    public partial class SavingMenu : Page
    {
        public SavingMenu()
        {
            InitializeComponent();
            route.Text = Config.INITIAL_PATH;
            DataContext = ((MainWindow)Application.Current.MainWindow).virtualToolBar.properties;
            video.IsEnabledChanged += (s, e) =>
            {
                if (!video.IsEnabled)
                {
                    video.IsChecked = false;
                }
            };
        }
        private void changeDirectory(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if(folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                route.Text = folderBrowserDialog.SelectedPath;
            }
        }
    }
}
