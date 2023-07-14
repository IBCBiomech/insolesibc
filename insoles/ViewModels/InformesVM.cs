using insoles.Commands;
using insoles.Model;
using insoles.States;
using insoles.Utilities;
using System.Collections.ObjectModel;
using System.Windows;

namespace insoles.ViewModel
{
    public class InformesVM : ViewModelBase
    {
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
        public InformesVM()
        {
            databaseBridge = ((MainWindow)Application.Current.MainWindow).databaseBridge;
            crearPacienteCommand = new CrearPacienteCommand(databaseBridge);
            obtenerPacientesCommand = new ObtenerPacientesCommand(databaseBridge);
        }
    }
}
