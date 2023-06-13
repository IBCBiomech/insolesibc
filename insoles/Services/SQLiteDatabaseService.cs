using insoles.Database;
using insoles.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.Services
{
    public class SQLiteDatabaseService : IDatabaseService
    {
        public void AddPaciente(Paciente paciente)
        {
            using (var dbContext = new DBContextSqlLite())
            {
                dbContext.Pacientes.Add(paciente);
                dbContext.SaveChanges();
            }
        }
        public List<Paciente> GetPacientes()
        {
            using (var dbContext = new DBContextSqlLite())
            {
                return dbContext.Pacientes.ToList();
            }
        }
    }
}
