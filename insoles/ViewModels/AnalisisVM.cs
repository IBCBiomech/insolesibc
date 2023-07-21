using insoles.Commands;
using insoles.DataHolders;
using insoles.Model;
using insoles.Services;
using insoles.States;
using insoles.UserControls;
using insoles.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace insoles.ViewModel
{
    public class AnalisisVM : ViewModelBase
    {
        private DatabaseBridge databaseBridge;
        private AnalisisState state;
        private ICodesService codes;
        private IPlantillaService plantilla;
        private IFileExtractorService fileExtractor;
        private IButterflyService butterfly;
        private IPressureMapService pressureMap;
        private IInformesGeneratorService informesGeneratorService;
        public ObtenerPacientesCommand obtenerPacientesCommand { get; set; }
        public CrearPacienteCommand crearPacienteCommand { get; set; }
        public TimelinePlayCommand timelinePlayCommand { get; set; }
        public TimelinePauseCommand timelinePauseCommand { get; set; }
        public TimelineFastBackwardCommand timelineFastBackwardCommand { get; set; }
        public TimelineFastForwardCommand timelineFastForwardCommand { get; set; }
        public ObservableCollection<PacientesTreeView> Pacientes
        {
            get
            {
                return databaseBridge.PacientesTreeView;
            }
        }
        public TimeLine timeLine { get; set; }
        public GRF grf { get; set; }
        public GrafoMariposa grafoMariposa {get; set;}
        public Heatmap heatmap { get; set;}
        public CamaraReplay camaraViewport1 { get; set; }
        public CamaraReplay camaraViewport2 { get; set; }
        public AnalisisVM()
        {
            state = new AnalisisState();
            ((MainWindow)Application.Current.MainWindow).analisisState = state; // Cambiar despues
            timeLine = new TimeLine(state);
            codes = new CodesService();
            plantilla = new PlantillaService(codes);
            fileExtractor = new FileExtractorService();
            grf = new GRF(state);
            butterfly = new ButterflyService(plantilla);
            pressureMap = new PressureMapCentersService(state, plantilla.sensor_map, codes,
                plantilla.CalculateSensorPositionsLeft(), plantilla.CalculateSensorPositionsRight());
            grafoMariposa = new GrafoMariposa(plantilla);
            heatmap = new Heatmap(state);
            informesGeneratorService = new InformesGeneratorService(grf, grafoMariposa, heatmap);
            ((MainWindow)Application.Current.MainWindow).informesGeneratorService = informesGeneratorService;
            camaraViewport1 = new CamaraReplay();
            camaraViewport2 = new CamaraReplay();
            databaseBridge = ((MainWindow)Application.Current.MainWindow).databaseBridge;
            crearPacienteCommand = new CrearPacienteCommand(databaseBridge);
            obtenerPacientesCommand = new ObtenerPacientesCommand(databaseBridge);
            timelinePlayCommand = new TimelinePlayCommand(state, timeLine);
            timelinePauseCommand = new TimelinePauseCommand(state, timeLine);
            timelineFastForwardCommand = new TimelineFastForwardCommand(state, timeLine);
            timelineFastBackwardCommand = new TimelineFastBackwardCommand(state, timeLine);
            timeLine.TimeChanged += (sender, time) =>
            {
                camaraViewport1.time = time;
                camaraViewport2.time = time;
                grf.time = time;
                heatmap.time = time;
                grafoMariposa.time = time;
            };
            state.PropertyChanged += async(object sender, PropertyChangedEventArgs e) =>
            {
                if(e.PropertyName == "test")
                {
                    if(state.test != null)
                    {
                        await Task.Run(async () =>
                        {
                            VariablesData variables = fileExtractor.ExtractVariables(state.test.csv);
                            GraphData data = await fileExtractor.ExtractCSV(state.test.csv);
                            await Application.Current.Dispatcher.BeginInvoke(() => timeLine.ChangeLimits(data.maxTime));
                            FramePressures[] frames;
                            List<Tuple<double, double>> cps_left;
                            List<Tuple<double, double>> cps_right;
                            await butterfly.Calculate(data, out frames, out cps_left, out cps_right);
                            await Task.Run(() => grafoMariposa.framePressures = frames);

                            if (state.test.video1 != null)
                            {
                                await Application.Current.Dispatcher.BeginInvoke(() =>
                                    camaraViewport1.videoPath = Environment.ExpandEnvironmentVariables(state.test.video1));
                            }
                            else
                            {
                                camaraViewport1.video = null;
                            }
                            if (state.test.video2 != null)
                            {
                                await Application.Current.Dispatcher.BeginInvoke(() =>
                                    camaraViewport2.videoPath = Environment.ExpandEnvironmentVariables(state.test.video2));
                            }
                            else
                            {
                                camaraViewport2.video = null;
                            }
                            await grf.Update(data, variables);
                            await heatmap.UpdateLimits(data);
                            await heatmap.CalculateCenters(cps_left, cps_right);
                            var pressureMaps = await pressureMap.CalculateMetrics(data);
                            await Task.Run(() => heatmap.pressure_maps_metrics = pressureMaps);
                            var pressureMapsLive = await pressureMap.CalculateLive(data);
                            await Task.Run(() => heatmap.pressure_maps_live = pressureMapsLive);
                        });
                    }         
                }
            };
        }
    }
}
