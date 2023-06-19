using insoles.Commands;
using insoles.Model;
using insoles.Services;
using insoles.States;
using insoles.UserControls;
using insoles.Utilities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace insoles.ViewModel
{
    public class AnalisisVM : ViewModelBase
    {
        private DatabaseBridge databaseBridge;
        private AnalisisState state;
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
        public AnalisisVM()
        {
            state = new AnalisisState();
            ((MainWindow)Application.Current.MainWindow).analisisState = state; // Cambiar despues
            timeLine = new TimeLine(state);
            databaseBridge = ((MainWindow)Application.Current.MainWindow).databaseBridge;
            crearPacienteCommand = new CrearPacienteCommand(databaseBridge);
            obtenerPacientesCommand = new ObtenerPacientesCommand(databaseBridge);
            timelinePlayCommand = new TimelinePlayCommand(state, timeLine);
            timelinePauseCommand = new TimelinePauseCommand(state, timeLine);
            timelineFastForwardCommand = new TimelineFastForwardCommand(state, timeLine);
            timelineFastBackwardCommand = new TimelineFastBackwardCommand(state, timeLine);
        }

    }
}
