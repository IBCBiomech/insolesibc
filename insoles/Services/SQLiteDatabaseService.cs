using insoles.Database;
using insoles.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.Services
{
    public class SQLiteDatabaseService : IDatabaseService
    {
        public Task AddPaciente(Paciente paciente)
        {
            using (var dbContext = new DBContextSqlLite())
            {
                dbContext.Pacientes.Add(paciente);
                try
                {
                    dbContext.SaveChanges();
                    return Task.CompletedTask;
                }
                catch (DbUpdateException ex)
                {
                    Exception innerException = ex.InnerException;
                    Trace.WriteLine(innerException.Message);
                    throw innerException;
                }
            }
        }
        public Task AddTest(Paciente paciente, Test test)
        {
            Trace.WriteLine("SQLiteDatabaseService AddTest");
            using (var dbContext = new DBContextSqlLite())
            {
                // Retrieve the existing Paciente object from the database
                Paciente? existingPaciente = dbContext.Pacientes.FirstOrDefault(p => p.Id == paciente.Id);

                if (existingPaciente != null)
                {
                    // Modify the Tests property of the existing Paciente object
                    existingPaciente.Tests.Add(test);

                    try
                    {
                        dbContext.SaveChanges();
                        return Task.CompletedTask;
                    }
                    catch (DbUpdateException ex)
                    {
                        Exception innerException = ex.InnerException;
                        Trace.WriteLine(innerException.Message);
                        throw innerException;
                    }
                }
                else
                {
                    return Task.CompletedTask;
                }
            }

        }
        public async Task<List<Paciente>> GetPacientes()
        {
            using (var dbContext = new DBContextSqlLite())
            {
                dbContext.Database.EnsureCreated();
                return await dbContext.Pacientes.Include(p => p.Tests).ToListAsync();
            }
        }
    }
}
