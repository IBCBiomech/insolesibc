using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace insoles.Graphs;

/// <summary>
/// Lógica de interacción para GraphAccelerometer.xaml
/// </summary>
public partial class GraphSumPressures : Page
{
    private const DispatcherPriority UPDATE_PRIORITY = DispatcherPriority.Render;
    private const DispatcherPriority CLEAR_PRIORITY = DispatcherPriority.Render;

    public Model2S model { get; private set; }
    public GraphSumPressures()
    {
        InitializeComponent();
        model = new Model2S(plot, 0, 10000, title: "Pressures", units: "Pascal");
        MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
        DataContext = this;
        //this.plot.Plot.XLabel("Frames");
        //this.plot.Plot.YLabel("m/s\xB2");
    }
    public void initCapture()
    {
        model.initCapture();
    }
    public async void drawData(float[] left, float[] right)
    {
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            model.updateData(left, right);
        });
    }
    /*
    public async void drawData(GraphData data)
    {
        double[] accX = new double[data.length];
        double[] accY = new double[data.length];
        double[] accZ = new double[data.length];
        for (int i = 0; i < data.length; i++)
        {
            accX[i] = ((FrameData1IMU)data[i]).accX;
            accY[i] = ((FrameData1IMU)data[i]).accY;
            accZ[i] = ((FrameData1IMU)data[i]).accZ;
        }
        await Application.Current.Dispatcher.BeginInvoke(UPDATE_PRIORITY, () =>
        {
            model.updateData(accX, accY, accZ);
        });
    }
    */
    public async void onUpdateTimeLine(object sender, int frame)
    {
        await Application.Current.Dispatcher.BeginInvoke(UPDATE_PRIORITY, () =>
        {
            model.updateIndex(frame);
        });
    }
    // Borra el contenido de los graficos
    public async void clearData()
    {
        await Application.Current.Dispatcher.InvokeAsync( () =>
        {
            model.clear();
        });
    }
}
