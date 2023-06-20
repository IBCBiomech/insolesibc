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
using System.Windows;
using System.Windows.Input;

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
        public ObtenerPacientesCommand obtenerPacientesCommand { get; set; }
        public CrearPacienteCommand crearPacienteCommand { get; set; }
        public TimelinePlayCommand timelinePlayCommand { get; set; }
        public TimelinePauseCommand timelinePauseCommand { get; set; }
        public TimelineFastBackwardCommand timelineFastBackwardCommand { get; set; }
        public TimelineFastForwardCommand timelineFastForwardCommand { get; set; }
        public ObservableCollection<Paciente> Pacientes
        {
            get
            {
                return databaseBridge.Pacientes;
            }
        }
        public TimeLine timeLine { get; set; }
        public GrafoMariposa grafoMariposa {get; set;}
        public AnalisisVM()
        {
            state = new AnalisisState();
            ((MainWindow)Application.Current.MainWindow).analisisState = state; // Cambiar despues
            timeLine = new TimeLine(state);
            codes = new CodesService();
            plantilla = new PlantillaService(codes);
            fileExtractor = new FileExtractorService();
            butterfly = new ButterflyService(plantilla);
            grafoMariposa = new GrafoMariposa();
            databaseBridge = ((MainWindow)Application.Current.MainWindow).databaseBridge;
            crearPacienteCommand = new CrearPacienteCommand(databaseBridge);
            obtenerPacientesCommand = new ObtenerPacientesCommand(databaseBridge);
            timelinePlayCommand = new TimelinePlayCommand(state, timeLine);
            timelinePauseCommand = new TimelinePauseCommand(state, timeLine);
            timelineFastForwardCommand = new TimelineFastForwardCommand(state, timeLine);
            timelineFastBackwardCommand = new TimelineFastBackwardCommand(state, timeLine);
            state.PropertyChanged += (object sender, PropertyChangedEventArgs e) =>
            {
                if(e.PropertyName == "test")
                {
                    if(state.test != null)
                    {
                        GraphData data = fileExtractor.ExtractCSV(state.test.csv);
                        FramePressures[] frames;
                        List<Tuple<double, double>> cps_left;
                        List<Tuple<double, double>> cps_right;
                        butterfly.Calculate(data, out frames, out cps_left, out cps_right);
                        grafoMariposa.DrawData(frames);
                    }         
                }
            };
        }

    }
}
