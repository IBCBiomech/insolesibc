using insoles.Model;
using Microsoft.EntityFrameworkCore;

namespace insoles.Database
{
    public class DBContextSqlLite : DbContext
    {
        public DbSet<Paciente> Pacientes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
            .UseSqlite("Data Source=mydatabase.db");
        }
        public void ApplyMigrations()
        {
            // Apply pending migrations
            Database.Migrate();
        }
    }
}
