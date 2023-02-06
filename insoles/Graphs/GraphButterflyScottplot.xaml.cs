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
    /// Lógica de interacción para GraphButterflyScottplot.xaml
    /// </summary>
    public partial class GraphButterflyScottplot : Page
    {
        private Foot foot;
        public ModelButterfly model { get; private set; }
        public GraphButterflyScottplot()
        {
            InitializeComponent();
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow.foot == null)
            {
                mainWindow.initialized += (s, e) =>
                {
                    foot = mainWindow.foot;
                    model = new ModelButterfly(plot, foot);
                };
            }
            else
            {
                foot = mainWindow.foot;
                model = new ModelButterfly(plot, foot);
            }
            DataContext = this;
        }
        public void DrawData(FramePressures[] data)
        {
            List<double> x = new List<double>();
            List<double> y = new List<double>();
            int index = 0;
            while (data[index].totalCenter == null)
            {
                index++;
            }
            Tuple<double, double> firstPoint = data[index].totalCenter;
            x.Add(firstPoint.Item1);
            y.Add(firstPoint.Item2);
            for (int i = index + 1; i < data.Length; i++)
            {
                if (data[i].totalCenter != null)
                {
                    Tuple<double, double> point = data[i].totalCenter;
                    x.Add(point.Item1);
                    y.Add(point.Item2);
                }
            }
            model.DrawData(x, y);
        }
    }
}
