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

namespace insoles.Graphs
{
    /// <summary>
    /// Lógica de interacción para GraphsCP.xaml
    /// </summary>
    public partial class GraphsCP : Page
    {
        private Foot foot;
        public ModelCPs model { get; private set; }
        public GraphsCP()
        {
            InitializeComponent();
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow.foot == null)
            {
                mainWindow.initialized += (s, e) =>
                {
                    foot = mainWindow.foot;
                    model = new ModelCPs(plotLeftX, plotLeftY, plotRightX, plotRightY);
                };
            }
            else
            {
                foot = mainWindow.foot;
                model = new ModelCPs(plotLeftX, plotLeftY, plotRightX, plotRightY);
            }
            DataContext = this;
        }
        public void DrawData(FramePressures[] data)
        {
            model.DrawData(data);
        }
    }
}
