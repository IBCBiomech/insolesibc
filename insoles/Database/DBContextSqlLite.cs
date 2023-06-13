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
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Paciente>()
                .Property(p => p.Apellidos)
                .IsRequired(false);

            modelBuilder.Entity<Paciente>()
                .Property(p => p.FechaNacimiento)
                .IsRequired(false);

            modelBuilder.Entity<Paciente>()
                .Property(p => p.Lugar)
                .IsRequired(false);

            modelBuilder.Entity<Paciente>()
                .Property(p => p.Peso)
                .IsRequired(false);

            modelBuilder.Entity<Paciente>()
                .Property(p => p.Altura)
                .IsRequired(false);

            modelBuilder.Entity<Paciente>()
                .Property(p => p.LongitudPie)
                .IsRequired(false);

            modelBuilder.Entity<Paciente>()
                .Property(p => p.NumeroPie)
                .IsRequired(false);

            modelBuilder.Entity<Paciente>()
                .Property(p => p.Profesion)
                .IsRequired(false);
        }
        public void ApplyMigrations()
        {
            // Apply pending migrations
            Database.Migrate();
        }
    }
}
