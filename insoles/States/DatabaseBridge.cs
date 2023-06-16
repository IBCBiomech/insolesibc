using insoles.Model;
using insoles.Services;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace insoles.States
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
            Trace.WriteLine("Load Pacientes");
            try
            {
                List<Paciente> pacientesDB = await databaseService.GetPacientes();
                Trace.WriteLine("Load Pacientes got from DB");
                await Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    Pacientes.Clear();
                    foreach (Paciente paciente in pacientesDB)
                    {
                        Pacientes.Add(paciente);
                    }
                });
            }
            catch (SqliteException ex)
            {
                Trace.WriteLine("Error Message: " + ex.Message);
                Trace.WriteLine("Error Code: " + ex.ErrorCode);
                Trace.WriteLine("Stack Trace: " + ex.StackTrace);
            }
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
        public async Task UpdateTest(Test test)
        {
            await databaseService.UpdateTest(test);
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
