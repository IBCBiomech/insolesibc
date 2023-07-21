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
        public async Task UpdateTest(Test test)
        {
            using (var dbContext = new DBContextSqlLite())
            {
                dbContext.Tests.Update(test);
                await dbContext.SaveChangesAsync();
            }
        }
        public async Task DeleteTest(Test test)
        {
            using (var dbContext = new DBContextSqlLite())
            {
                dbContext.Tests.Remove(test);
                Paciente paciente = test.Paciente;
                paciente.Tests.Remove(test);

                dbContext.Entry(test).State = EntityState.Deleted;

                await dbContext.SaveChangesAsync();
            }
        }
        public async Task UpdatePaciente(Paciente paciente)
        {
            using (var dbContext = new DBContextSqlLite())
            {
                dbContext.Pacientes.Update(paciente);
                await dbContext.SaveChangesAsync();
            }
        }
        public async Task DeletePaciente(Paciente paciente)
        {
            using (var dbContext = new DBContextSqlLite())
            {
                dbContext.Pacientes.Remove(paciente);
                foreach (Test test in paciente.Tests)
                {
                    dbContext.Tests.Remove(test);
                    dbContext.Entry(test).State = EntityState.Deleted;
                }
                dbContext.Entry(paciente).State = EntityState.Deleted;

                await dbContext.SaveChangesAsync();
            }
        }
        public async Task CrearCarpetaTest(Paciente paciente)
        {
            using (var dbContext = new DBContextSqlLite())
            {
                Paciente? existingPaciente = dbContext.Pacientes.FirstOrDefault(p => p.Id == paciente.Id);

                if (existingPaciente != null)
                {
                    existingPaciente.Tests.Add(new Test());
                    try
                    {
                        await dbContext.SaveChangesAsync();
                    }
                    catch (DbUpdateException ex)
                    {
                        Exception innerException = ex.InnerException;
                        Trace.WriteLine(innerException.Message);
                        throw innerException;
                    }
                }
            }
        }
        public async Task CrearCarpetaInforme(Paciente paciente)
        {
            using (var dbContext = new DBContextSqlLite())
            {
                Paciente? existingPaciente = dbContext.Pacientes.FirstOrDefault(p => p.Id == paciente.Id);

                if (existingPaciente != null)
                {
                    existingPaciente.Informes.Add(new Informe());
                    try
                    {
                        await dbContext.SaveChangesAsync();
                    }
                    catch (DbUpdateException ex)
                    {
                        Exception innerException = ex.InnerException;
                        Trace.WriteLine(innerException.Message);
                        throw innerException;
                    }
                }
            }
        }
        public async Task UpdateInforme(Informe informe)
        {
            using (var dbContext = new DBContextSqlLite())
            {
                dbContext.Informes.Update(informe);
                await dbContext.SaveChangesAsync();
            }
        }
        public async Task<List<Paciente>> GetPacientes()
        {
            using (var dbContext = new DBContextSqlLite())
            {
                return await dbContext.Pacientes.Include(p => p.Tests).Include(p => p.Informes)
                    .ThenInclude(i => i.Files).ToListAsync();
            }
        }

        public async Task GenerarInforme(Informe informe, InformeFile file)
        {
            using (var dbContext = new DBContextSqlLite())
            {
                Informe? existingInforme = dbContext.Informes.FirstOrDefault(i => i.Id == informe.Id);

                if (existingInforme != null)
                {
                    existingInforme.Files.Add(file);
                    try
                    {
                        await dbContext.SaveChangesAsync();
                    }
                    catch (DbUpdateException ex)
                    {
                        Exception innerException = ex.InnerException;
                        Trace.WriteLine(innerException.Message);
                        throw innerException;
                    }
                }
            }
        }
    }
}
