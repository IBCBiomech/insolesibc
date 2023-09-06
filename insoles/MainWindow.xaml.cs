using insoles.Services;
using insoles.States;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace insoles
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public DatabaseBridge databaseBridge { get; set; }
        public AnalisisState analisisState { get; set; }

        public InformesState informesState { get; set; }
        public IInformesGeneratorService informesGeneratorService { get; set; }
        public event EventHandler viewChanged;

        private DateTime lastClickTime = DateTime.Now;
        public MainWindow()
        {
            InitializeComponent();
            databaseBridge = new DatabaseBridge();
        }
        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        public void triggerViewChanged()
        {
            viewChanged?.Invoke(this, EventArgs.Empty);
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TimeSpan elapsed = DateTime.Now - lastClickTime;
            if (elapsed.TotalMilliseconds < 300)
            {
                if(WindowState == WindowState.Normal)
                {
                    WindowState = WindowState.Maximized;
                }
                else if(WindowState == WindowState.Maximized) 
                {
                    WindowState = WindowState.Normal;
                }
                lastClickTime = DateTime.Now.AddMilliseconds(-500);
            }
            else
            {
                this.DragMove();
            }

            lastClickTime = DateTime.Now;
            
        }

        /// <summary>
        /// Atención: este método hay que comentarlo si no, no funciona el Dock
        /// Necesario para mover la ventana sin controles
        /// </summary>
        //protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        //{
        //    base.OnMouseLeftButtonDown(e);

        //    // Begin dragging the window
        //    this.DragMove();
        //} 


    }
}
