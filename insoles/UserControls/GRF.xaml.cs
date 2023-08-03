using insoles.DataHolders;
using ScottPlot.Styles;
using ScottPlot;
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
using System.Net;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ScottPlot.Plottable;
using System.Xml.Linq;
using System.Data;
using MathNet.Numerics;
using insoles.States;
using System.Reflection.Metadata;
using Syncfusion.DocIO.DLS;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using stdgraph.Lib;
using MathNet.Numerics.Statistics;

namespace insoles.UserControls
{
    /// <summary>
    /// Lógica de interacción para GRF.xaml
    /// </summary>
    public enum Units { N, Kg}
   
    /// <summary>
    /// Clase para controlar GRF
    /// </summary>
    public partial class GRF : UserControl, INotifyPropertyChanged
    {
        private List<double> xs_temp_left;
        private List<double> ys_temp_left;
        private List<double> xs_temp_right;
        private List<double> ys_temp_right;

        private List<double> xs_left_N;
        private List<double> ys_left_N;
        private List<double> xs_right_N;
        private List<double> ys_right_N;

        private List<double> xs_left_Kg;
        private List<double> ys_left_Kg;
        private List<double> xs_right_Kg;
        private List<double> ys_right_Kg;

        private List<double> xs_left_N_FC;
        private List<double> ys_left_N_FC;
        private List<double> xs_right_N_FC;
        private List<double> ys_right_N_FC;

        private List<double> xs_left_Kg_FC;
        private List<double> ys_left_Kg_FC;
        private List<double> xs_right_Kg_FC;
        private List<double> ys_right_Kg_FC;

        double[] XPoints = new double[2];

        private Units _selectedUnits;
        private VLine rangeVlabel;
        private List<VLine> listOfVlabels = new List<VLine>();
        private ScatterPlot leftInsolePlot;
        private ScatterPlot leftInsoleErrorPlot;
        private ScatterPlot rightInsolePlot;
        NormalStats Ns;
        public Units selectedUnits
        {
            get { return _selectedUnits; }
            set
            {
                _selectedUnits = value;
                NotifyPropertyChanged();
                if(value == Units.N)
                {
                    if(fc)
                        setN_FC();
                    else
                        setN();
                    plot.Plot.Clear();
                    render();
                }
                else if(value == Units.Kg)
                {
                    if(fc)
                        setKg_FC();
                    else
                        setKg();
                    plot.Plot.Clear();
                    render();
                }
            }
        }
        private bool _fc;
        public bool fc
        {
            get
            {
                return _fc;
            }
            set
            {
                _fc = value;
                if (value)
                {
                    if(selectedUnits == Units.N)
                    {
                        setN_FC();
                    }
                    else if(selectedUnits == Units.Kg)
                    {
                        setKg_FC();
                    }
                }
                else
                {
                    if (selectedUnits == Units.N)
                    {
                        setN();
                    }
                    else if (selectedUnits == Units.Kg)
                    {
                        setKg();
                    }
                }
                plot.Plot.Clear();
                render();
                NotifyPropertyChanged();
            }
        }
        public IEnumerable<Units> units
        {
            get { return Enum.GetValues(typeof(Units)).Cast<Units>(); }
        }
        private const double MIN_REFRESH = 0.1;
        private double lastTime = 0;
        public double time
        {
            set
            {
                if(timeLine != null)
                    timeLine.X = value;
                if (Math.Abs(value - lastTime) > MIN_REFRESH && xs_temp_left != null)
                {
                    lastTime = value;
                    double closest = FindClosest(xs_temp_left, value);
                    int indexClosest = xs_temp_left.IndexOf(closest);
                    leftPlot.Label = "Left = " + ys_temp_left[indexClosest].ToString("0.##");
                    rightPlot.Label = "Right = " + ys_temp_right[indexClosest].ToString("0.##");
                    total = (ys_temp_left[indexClosest] + ys_temp_right[indexClosest]).Round(2).ToString() +
                        selectedUnits.ToString();
                }
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    plot.Refresh();
                }));
            }
        }
        private string _total;
        public string total
        {
            get { return _total; }
            set { _total = value; NotifyPropertyChanged(); }
        }
        private VLine timeLine;
        private ScatterPlot leftPlot;
        private ScatterPlot rightPlot;

        private AnalisisState state;

        HSpan hspan;

        double[] ys_left_array;

        Dictionary<int, double> toes_off = new Dictionary<int, double>();
        Dictionary<int, double> heel_strikes = new Dictionary<int, double>();

        Dictionary<int, List<double>> curves = new Dictionary<int, List<double>>();

        // Inicializa eventos
        public GRF(AnalisisState state)
        {
            InitializeComponent();
            DataContext = this;
            this.state = state;
            plot.MouseDown += PlotControl_MouseDown;
            plot.MouseMove += Plot_MouseMove;

            plot.Plot.Title("Raw Signal");
            plot.Plot.Style(ScottPlot.Style.Seaborn);
            plot.Plot.Palette = ScottPlot.Palette.Amber;

            rangePlot.Plot.Title("Range Selected");
            rangePlot.Plot.Style(ScottPlot.Style.Seaborn);
            rangePlot.Plot.Palette = ScottPlot.Palette.Amber;

            normPlot.Plot.Title("Normalization Plot");
            normPlot.Plot.Style(ScottPlot.Style.Seaborn);
            normPlot.Plot.Palette = ScottPlot.Palette.Amber;


            plot.Render();
            rangePlot.Render();

            Ns = new NormalStats();
        }

        // Imprime en el Xlabel las coordenadas XY (temporalmente)
        private void Plot_MouseMove(object sender, MouseEventArgs e)
        {
            (double x, double y) = plot.GetMouseCoordinates();
            plot.Plot.XLabel($"X: {x:F2}, Y: {y:F2}");
        }

        // Desactiva el efecto por defecto de left click para que no se dibujen 2 lineas
        private void PlotControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                e.Handled = true;
        }
        private void setN()
        {
            xs_temp_left = xs_left_N;
            ys_temp_left = ys_left_N;
            xs_temp_right = xs_right_N;
            ys_temp_right = ys_right_N;
        }
        private void setKg()
        {
            xs_temp_left = xs_left_Kg;
            ys_temp_left = ys_left_Kg;
            xs_temp_right = xs_right_Kg;
            ys_temp_right = ys_right_Kg;
        }
        private void setN_FC()
        {
            xs_temp_left = xs_left_N_FC;
            ys_temp_left = ys_left_N_FC;
            xs_temp_right = xs_right_N_FC;
            ys_temp_right = ys_right_N_FC;
        }
        private void setKg_FC()
        {
            xs_temp_left = xs_left_Kg_FC;
            ys_temp_left = ys_left_Kg_FC;
            xs_temp_right = xs_right_Kg_FC;
            ys_temp_right = ys_right_Kg_FC;
        }
        public Task Update(GraphData data, VariablesData variables)
        {
            float G = 9.80665f;
            float peso = state.test.Paciente.Peso.GetValueOrDefault(70);
            float FNominal = peso * G;

            plot.Plot.Clear();
            xs_left_N = new();
            ys_left_N = new();
            xs_right_N = new();
            ys_right_N = new();

            xs_left_Kg = new();
            ys_left_Kg = new();
            xs_right_Kg = new();
            ys_right_Kg = new();

            xs_left_N_FC = new();
            ys_left_N_FC = new();
            xs_right_N_FC = new();
            ys_right_N_FC = new();

            xs_left_Kg_FC = new();
            ys_left_Kg_FC = new();
            xs_right_Kg_FC = new();
            ys_right_Kg_FC = new();

            foreach (var frame in data.frames)
            {
                FrameDataInsoles frameInsoles = (FrameDataInsoles)frame;
                DataInsole left = frameInsoles.left;
                DataInsole right = frameInsoles.right;

                xs_left_N.Add(frameInsoles.time);
                xs_right_N.Add(frameInsoles.time);
                ys_left_N.Add(left.totalPressure);
                ys_right_N.Add(right.totalPressure);

                xs_left_Kg.Add(frameInsoles.time);
                xs_right_Kg.Add(frameInsoles.time);
                ys_left_Kg.Add(left.totalPressure / 9.8);
                ys_right_Kg.Add(right.totalPressure / 9.8);

                xs_left_N_FC.Add(frameInsoles.time);
                xs_right_N_FC.Add(frameInsoles.time);
                ys_left_N_FC.Add(left.totalPressure * variables.fc);
                ys_right_N_FC.Add(right.totalPressure * variables.fc);

                xs_left_Kg_FC.Add(frameInsoles.time);
                xs_right_Kg_FC.Add(frameInsoles.time);
                ys_left_Kg_FC.Add(left.totalPressure / 9.8 * variables.fc);
                ys_right_Kg_FC.Add(right.totalPressure / 9.8 * variables.fc);
            }
            if (selectedUnits == Units.N)
            {
                if (fc)
                    setN_FC();
                else
                    setN();
            }
            else if (selectedUnits == Units.Kg)
            {
                if (fc)
                    setKg_FC();
                else
                    setKg();
            }

            render();

            return Task.CompletedTask;
        }

        //Este método habría que refactorizar el nombre, se confunde con plot.render()
        private void render()
        {
            if (ys_temp_left == null)
                return;
            double[] dts_left = StdDevCalculation(ys_temp_left);
            double[] dts_right = StdDevCalculation(ys_temp_right);


            plot.MouseDoubleClick += new MouseButtonEventHandler(MouseTracking);

            plot.Plot.Palette = ScottPlot.Palette.Amber;
            plot.Plot.Style(ScottPlot.Style.Seaborn);

            double[] xs_left = xs_temp_left.ToArray();
            double[] ys_left = ys_temp_left.ToArray();
            leftPlot = plot.Plot.AddScatterLines(xs_left, ys_left, System.Drawing.Color.Red, 2, label: "left");
            //plot.Plot.AddFillError(xs_left, ys_left, dts_left, System.Drawing.Color.FromArgb(50, System.Drawing.Color.IndianRed));

            double[] xs_right = xs_temp_right.ToArray();
            double[] ys_right = ys_temp_right.ToArray();
            rightPlot = plot.Plot.AddScatterLines(xs_right, ys_right, System.Drawing.Color.Green, 2, label: "right");
            //plot.Plot.AddFillError(xs_right, ys_right, dts_right, System.Drawing.Color.FromArgb(50, System.Drawing.Color.SkyBlue));

            plot.Plot.Legend();

            timeLine = plot.Plot.AddVerticalLine(0, color: System.Drawing.Color.DeepSkyBlue);

            Application.Current.Dispatcher.Invoke(new Action(() => plot.Render()));
            
        }

        // Este método es el que añade las barras verticales
        private void MouseTracking(object sender, MouseButtonEventArgs e)
        {
            //if ( XPoints.Count >= 0 && XPoints.Count <2)
            //{
            //    (double x, double y) = plot.GetMouseCoordinates();
            //    rangeVlabel = plot.Plot.AddVerticalLine(x, color: System.Drawing.Color.IndianRed, style: ScottPlot.LineStyle.Solid);
            //    rangeVlabel.PositionLabel = true;
            //    rangeVlabel.DragEnabled = true;
            //    plot.Render();
            //    listOfVlabels.Add(rangeVlabel);
            //    XPoints.Add(x);
            //}
            //else
            //{
            //    MessageBox.Show("No se puede hacer rango con más de dos puntos. Elimina la STDDEV actual","Info",MessageBoxButton.OK, MessageBoxImage.Warning);
            //}
       

        }

        // Cálculo de la desviación estándar de cada punto
        private double[] StdDevPointCalculation(List<double> ys_temp)
        {
            double media = ys_temp.Average();

            // Calcular la desviación típica para cada punto
            List<double> desviacionesTipicas = new List<double>();
            foreach (double valor in ys_temp)
            {
                // Calcular la diferencia entre el valor y la media
                double diferencia = valor - media;

                // Elevar al cuadrado la diferencia
                double diferenciaCuadrada = Math.Pow(diferencia, 2);

                //El sqrt para que el gráfico no se vaya con valores grandes
                double diferenciaSqrt = Math.Sqrt(diferenciaCuadrada);

                double std = Math.Min(diferenciaSqrt, valor);
                // Agregar la diferencia cuadrada a la lista
                desviacionesTipicas.Add(std);
            }

            double[] dts = desviacionesTipicas.ToArray();

            return dts;
        }
        // Cálculo de la desviación estándar
        private double[] StdDevCalculation(List<double> ys_temp)
        {
            double avg = ys_temp.Average();
            double sum = ys_temp.Select(x => (avg - x) * (avg - x)).Sum();

            double std = Math.Sqrt(sum / ys_temp.Count);
            return Enumerable.Repeat(std, ys_temp.Count).ToArray();
        }
        private void StdDevButton1_Click(object sender, RoutedEventArgs e)
        {
            //double FirstClosest = FindClosest(xs_temp_left, XPoints[0]);
            //double LastClosest = FindClosest(xs_temp_left, XPoints[1]);

            //int indexFirstClosest = xs_temp_left.IndexOf(FirstClosest);
            //int indexLastClosest = xs_temp_left.IndexOf(LastClosest);

            //List<double> listXleft = xs_temp_left.GetRange(indexFirstClosest, (indexLastClosest - indexFirstClosest));
            //List<double> listYleft = ys_temp_left.GetRange(indexFirstClosest, (indexLastClosest - indexFirstClosest));

            //double[] dataXleft = listXleft.ToArray();
            //double[] dataYleft = listYleft.ToArray();

            //double[] stddevleft = StdDevCalculation(listYleft);
            //leftInsolePlot = rangePlot.Plot.AddScatterLines(dataXleft, dataYleft, System.Drawing.Color.DarkOrange, 2);
            //rangePlot.Plot.AddFillError(dataXleft, dataYleft, stddevleft, System.Drawing.Color.FromArgb(50, System.Drawing.Color.Green));



            //List<double> listXright = xs_temp_right.GetRange(indexFirstClosest, (indexLastClosest - indexFirstClosest));
            //List<double> listYright = ys_temp_right.GetRange(indexFirstClosest, (indexLastClosest - indexFirstClosest));

            //double[] dataXright = listXright.ToArray();
            //double[] dataYright = listYright.ToArray();

            //double[] stddevright = StdDevCalculation(listYright);
            //rangePlot.Plot.AddScatterLines(dataXright, dataYright, System.Drawing.Color.DarkBlue, 2);
            //rangePlot.Plot.AddFillError(dataXright, dataYright, stddevright, System.Drawing.Color.FromArgb(50, System.Drawing.Color.SkyBlue));

            //rangePlot.Render();

            hspan = plot.Plot.AddHorizontalSpan(10, 30);

            hspan.BorderColor = System.Drawing.Color.Red;
            hspan.BorderLineStyle = ScottPlot.LineStyle.Dash;
            hspan.BorderLineWidth = 2;
            hspan.HatchColor = System.Drawing.Color.FromArgb(100, System.Drawing.Color.Blue);
            hspan.HatchStyle = ScottPlot.Drawing.HatchStyle.None;
            hspan.Label = "Customized vSpan";
            hspan.DragEnabled = true;

            hspan.Edge1Dragged += (s, e) =>
            {
                XPoints[0] = e;
                Trace.WriteLine($"X1: {e.ToString("F2")}");
            };
            hspan.Edge2Dragged += (s, e) =>
            {
                XPoints[1] = e;
                Trace.WriteLine($"X2: {e.ToString("F2")}");

            };

            plot.Render();
            
        }
        
        
        // Función que sí utilizamos para obtener el número más cercano de una lista
        private double FindClosest(List<double> data, double value)
        {
            if (value <= data[0])
            {
                return data[0];
            }
            for (var i = 0; i < data.Count - 1; i++)
            {
                if (data[i] <= value && value <= data[i + 1])
                {
                    return data[i];
                }

            }

            return data.Last();
        }

        // Este Código pertenece a una función que retonra el número más próximo de una lista. 
        // Est una segunda opción que no utilizamos por ahora
        private static double? FindNearestValue(IEnumerable<double> arr, double d)
        {
            var minDist = double.MaxValue;
            double? nearestValue = null;

            foreach (var x in arr)
            {
                var dist = Math.Abs(x - d);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearestValue = x;
                }
            }
            return nearestValue;
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void ClearGraphButton_Click(object sender, RoutedEventArgs e)
        {
            
            //grid.Children.Remove(rangePlot);
          
            //foreach (var el in listOfVlabels)
            //{
            //    plot.Plot.Remove(el);

            //}
            //listOfVlabels.Clear();
            //XPoints.Clear();
            //plot.Render();
            //rangePlot.Render();
        }

        

        private void RangeButton_Click(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine($"X1: {XPoints[0]:F2} X2: {XPoints[1]:F2}");
            int start = xs_left_N_FC.IndexOf(Math.Round(XPoints[0], 2));
            int end = xs_left_N_FC.IndexOf(Math.Round(XPoints[1], 2));

            List<double> xs_left = xs_left_N_FC.GetRange(0, end - start);
            List<double> ys_left = ys_left_N_FC.GetRange(start, end - start);

            ys_left_array = Ns.Butterworth(ys_left.ToArray(), 0.01, 6.0);

            List<double> xs_right = xs_right_N_FC.GetRange(0, end - start);
            List<double> ys_right = ys_right_N_FC.GetRange(start, end - start);

            double[] ys_right_array = Ns.Butterworth(ys_right.ToArray(), 0.01, 6.0);

            rangePlot.Plot.AddScatterLines(xs_left.ToArray(), ys_left_array,color: System.Drawing.Color.Red);
            rangePlot.Plot.AddScatterLines(xs_right.ToArray(), ys_right_array.ToArray(),color: System.Drawing.Color.Green);
            rangePlot.Render();

        }

        private void NormalizationButton_Click(object sender, RoutedEventArgs e)
        {
            // Añadir los choques (heel_strikes) y la fase de salida (toes_off)
            // con un threshold de 10

            for (int i = 0; i < ys_left_array.Length - 1; i++)
            {
                if (ys_left_array[i] < 10 && ys_left_array[i + 1] > 10)
                {
                    heel_strikes.Add(i + 1, ys_left_array[i + 1]);

                }

                if (ys_left_array[i] > 10 && ys_left_array[i + 1] < 10)
                {
                    toes_off.Add(i, ys_left_array[i]);
                }


            }

            // pintamos lineas verticales en el gráfico para los choques

            foreach (KeyValuePair<int, double> item in heel_strikes)
            {

                double el = xs_left_N_FC[item.Key];

                var vline = rangePlot.plt.AddVerticalLine(Math.Round(el, 2), color: System.Drawing.Color.Blue);

                vline.PositionLabel = true;

                rangePlot.Render();

            }

            // pintamos las líneas para las salidas
            foreach (KeyValuePair<int, double> item in toes_off)
            {

                double el = xs_left_N_FC[item.Key];

                var vline = rangePlot.plt.AddVerticalLine(Math.Round(el, 2), color: System.Drawing.Color.Yellow);

                vline.PositionLabel = true;

                rangePlot.Render();

            }

            // Sacar las curvas
            for (var i = 0; i < toes_off.Count; i++)
            {
                int init = heel_strikes.ElementAt(i).Key;
                int end = toes_off.ElementAt(i).Key;
                curves.Add(init, ys_left_array[init..end].ToList());

            }

            List<List<double>> curvasInterpoladas = new List<List<double>>();
            List<List<double>> tiemposInterpolados = new List<List<double>>();

            for (var i = 0; i < curves.Count(); i++)
            {
                double[] second_curve = Ns.linspace(0, 99, curves.ElementAt(i).Value.Count);

                (double[] xs, double[] ys) = stdgraph.Lib.CubicInterpol.InterpolateXY(second_curve, curves.ElementAt(i).Value.ToArray(), 100);
                curvasInterpoladas.Add(ys.ToList());
                tiemposInterpolados.Add(xs.ToList());
            }

            List<double> curvaMedia = new List<double>();
            List<double> curvaSt = new List<double>();
            List<double> curvaTime = new List<double>();

            for (int colIndex = 0; colIndex < 100; colIndex++)
            {
                List<double> colAvg = new List<double>();
                List<double> colTime = new List<double>();

                foreach (List<double> item in curvasInterpoladas)
                {
                    if (item.Count > colIndex)
                    {
                        colAvg.Add(item[colIndex]);

                    }
                }

                foreach (List<double> item in tiemposInterpolados)
                {

                    if (item.Count > colIndex)
                    {
                        colTime.Add(item[colIndex]);
                    }

                }

                curvaMedia.Add(colAvg.Average());
                curvaSt.Add(colAvg.StandardDeviation());
                curvaTime.Add(colTime.Average());
            }

            normPlot.Plot.AddScatterLines(curvaTime.ToArray(), curvaMedia.ToArray(), System.Drawing.Color.Blue, lineWidth: 3, label: "Average");
            normPlot.Plot.AddFillError(curvaTime.ToArray(), curvaMedia.ToArray(), curvaSt.ToArray(), System.Drawing.Color.FromArgb(50, System.Drawing.Color.Blue));

            normPlot.Render();


        }


        // Movido a InformesGeneratorService
        /*
        private void ClearGraphButton_Copy_Click(object sender, RoutedEventArgs e)
        {
            rangePlot.Plot.SaveFig("range.png");
            plot.Plot.SaveFig("GRF.png");

            // Creating a new document.
            WordDocument document = new WordDocument();
            //Adding a new section to the document.
            WSection section = document.AddSection() as WSection;
            //Set Margin of the section
            section.PageSetup.Margins.All = 72;
            //Set page size of the section
            section.PageSetup.PageSize = new System.Drawing.SizeF(612, 792);



            IWParagraph paragraph = section.HeadersFooters.Header.AddParagraph();

            paragraph.ParagraphFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Left;
            WTextRange textRange = paragraph.AppendText("InnerFEET Pressure Register Tool") as WTextRange;
            textRange.CharacterFormat.FontSize = 12f;
            textRange.CharacterFormat.FontName = "Calibri";

            

            //Appends paragraph.
            paragraph = section.AddParagraph();
            paragraph.ParagraphFormat.FirstLineIndent = 36;
            paragraph.BreakCharacterFormat.FontSize = 12f;
            textRange = paragraph.AppendText("A continuación se muestra un informe con el Gráfico de GRF:") as WTextRange;
            textRange.CharacterFormat.FontSize = 12f;

            // Gets the image stream.
            IWPicture picture = paragraph.AppendPicture(new System.Drawing.Bitmap(@"GRF.png")) as WPicture;


            textRange = paragraph.AppendText("A continuación se muestra un informe con el Gráfico de STDDEV:") as WTextRange;
            IWPicture picture2 = paragraph.AppendPicture(new System.Drawing.Bitmap(@"Range.png")) as WPicture;


            document.Save("Sample.docx");


        }
        */
    }
}
