using insoles.Database;
using insoles.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.Services
{
    public interface IDatabaseService
    {
        Task AddPaciente(Paciente paciente);
        Task AddTest(Paciente paciente, Test test);
        Task UpdateTest(Test test);
        Task DeleteTest(Test test);
        Task UpdatePaciente(Paciente paciente);
        Task DeletePaciente(Paciente paciente);
        Task CrearCarpetaTest(Paciente paciente);
        Task CrearCarpetaInforme(Paciente paciente);
        Task UpdateInforme(Informe informe);
        Task GenerarInforme(Informe informe, InformeFile file);
        Task<List<Paciente>> GetPacientes();
    }
}
