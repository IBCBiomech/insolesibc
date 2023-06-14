using insoles.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.Services
{
    public class DatabaseBridge
    {
        private IDatabaseService databaseService;
        public ObservableCollection<Paciente> Pacientes { get; set; } = new ObservableCollection<Paciente>();
        public DatabaseBridge()
        {
            databaseService = new SQLiteDatabaseService();
            List<Paciente> pacientesDB = databaseService.GetPacientes();
            foreach (Paciente paciente in pacientesDB)
            {
                Pacientes.Add(paciente);
            }
        }
        public void AddPaciente(Paciente paciente)
        {
            Pacientes.Add(paciente);
            databaseService.AddPaciente(paciente);
        }
        public void AddTest(Paciente paciente, Test test)
        {
            paciente.Tests.Add(test);
            databaseService.AddTest(paciente, test);
        }
    }
}
