﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace insoles.Graphs;

/// <summary>
/// Lógica de interacción para GraphAccelerometer.xaml
/// </summary>
public partial class GraphSumPressures : Page
{
    public enum Metric { Avg, Sum }
    private const DispatcherPriority UPDATE_PRIORITY = DispatcherPriority.Render;
    private const DispatcherPriority CLEAR_PRIORITY = DispatcherPriority.Render;
    public Metric metricSelected { get; private set; }

    public Model2S model { get; private set; }

    private GraphManager graphManager;
    public GraphSumPressures()
    {
        InitializeComponent();
        model = new Model2S(plot, -100, 2000);
        MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
        DataContext = this;
        metric.SelectionChanged += (s, e) =>
        {
            selectionChanged();
        };
        metric.SelectedIndex = 0;
        if(mainWindow.graphManager != null)
        {
            graphManager = mainWindow.graphManager;
            mbar.IsChecked = true;
        }
        else
        {
            mainWindow.initialized += (s, e) =>
            {
                graphManager = mainWindow.graphManager;
                mbar.IsChecked = true;
            };
        }
        metric.SelectedIndex = 1;
        //this.plot.Plot.XLabel("Frames");
        //this.plot.Plot.YLabel("m/s\xB2");
    }
    private void selectionChanged()
    {
        string selected = metric.SelectedValue.ToString();
        if(selected == "avg")
        {
            metricSelected = Metric.Avg;
        }
        else if(selected == "sum")
        {
            metricSelected = Metric.Sum;
        }
        else
        {
            throw new System.Exception("Error selectionChanged() GraphSumPressures");
        }
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
    public async void drawData(GraphData data)
    {
        double[] left = new double[data.length];
        double[] right = new double[data.length];
        for (int i = 0; i < data.length; i++)
        {
            FrameDataInsoles data_i = (FrameDataInsoles)data[i];
            left[i] = data_i.left.totalPressure;
            right[i] = data_i.right.totalPressure;
            if(metricSelected == Metric.Avg)
            {
                left[i] /= Config.NUM_SENSORS;
                right[i] /= Config.NUM_SENSORS;
            }
        }
        await Application.Current.Dispatcher.BeginInvoke(() =>
        {
            model.updateData(left, right);
        });
    }
    public async void onUpdateTimeLine(object sender, int frame)
    {
        await Application.Current.Dispatcher.BeginInvoke(() =>
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

    private void mbar_Checked(object sender, RoutedEventArgs e)
    {
        graphManager.unit = Common.Helpers.Units.mbar;
        model.plot.Plot.YLabel("mbar");
        model.plot.Refresh();
        if (N.IsChecked.Value)
        {
            N.IsChecked = false;
        }
    }

    private void N_Checked(object sender, RoutedEventArgs e)
    {
        graphManager.unit = Common.Helpers.Units.N;
        model.plot.Plot.YLabel("N");
        model.plot.Refresh();
        if (mbar.IsChecked.Value)
        {
            mbar.IsChecked = false;
        }
    }

    private void mbar_Unchecked(object sender, RoutedEventArgs e)
    {
        if (!N.IsChecked.Value)
        {
            N.IsChecked = true;
        }
    }

    private void N_Unchecked(object sender, RoutedEventArgs e)
    {
        if (!mbar.IsChecked.Value)
        {
            mbar.IsChecked = true;
        }
    }
}