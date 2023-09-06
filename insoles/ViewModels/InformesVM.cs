using insoles.Commands;
using insoles.Model;
using insoles.States;
using insoles.UserControls;
using insoles.Utilities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace insoles.ViewModel
{
    public class InformesVM : ViewModelBase
    {
        private InformesState _state;

        private DatabaseBridge databaseBridge;
        public ObservableCollection<PacientesTreeView> Pacientes
        {
            get
            {
                return databaseBridge.PacientesTreeView;
            }
        }
        public ObtenerPacientesCommand obtenerPacientesCommand { get; set; }
        public CrearPacienteCommand crearPacienteCommand { get; set; }

        public EditorInformes MostrarInforme { get; set; }
        public InformesVM()
        {

            _state = new InformesState();
            ((MainWindow)Application.Current.MainWindow).informesState = _state;


            databaseBridge = ((MainWindow)Application.Current.MainWindow).databaseBridge;
            crearPacienteCommand = new CrearPacienteCommand(databaseBridge);
            obtenerPacientesCommand = new ObtenerPacientesCommand(databaseBridge);

            MostrarInforme = new EditorInformes();

            _state.PropertyChanged += async (object sender, PropertyChangedEventArgs e) =>
            {

                MostrarInforme.CargarPath(_state.Path);
              

            };
        }  
    }
}
