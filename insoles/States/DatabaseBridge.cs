using insoles.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace insoles.Services
{
    public class DatabaseBridge
    {
        private IDatabaseService databaseService;
        public ObservableCollection<Paciente> Pacientes { get; set; } = new ObservableCollection<Paciente>();
        public DatabaseBridge()
        {
            databaseService = new SQLiteDatabaseService();
        }
        public async Task LoadPacientes()
        {
            List<Paciente> pacientesDB = await databaseService.GetPacientes();
            await Application.Current.Dispatcher.BeginInvoke(() =>
            {
                Pacientes.Clear();
                foreach (Paciente paciente in pacientesDB)
                {
                    Pacientes.Add(paciente);
                }
            });
        }
        public async Task AddPaciente(Paciente paciente)
        {
            await databaseService.AddPaciente(paciente);
            await LoadPacientes();
        }
        public async Task AddTest(Paciente paciente, Test test)
        {
            await databaseService.AddTest(paciente, test);
            await LoadPacientes();
        }
        public Paciente? GetSelectedPaciente()
        {
            foreach(Paciente paciente in Pacientes)
            {
                if(paciente.IsSelected) return paciente;
            }
            return null;
        }
    }
}
