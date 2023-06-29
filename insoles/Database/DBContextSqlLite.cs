using insoles.Model;
using Microsoft.EntityFrameworkCore;

namespace insoles.Database
{
    public class DBContextSqlLite : DbContext
    {
        public DbSet<Paciente> Pacientes { get; set; }

        public DbSet<Test> Tests { get; set; }

        public DbSet<Informe> Informes { get; set; }

        public DBContextSqlLite()
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
            .UseSqlite("Data Source=mydatabase.db");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Test>()
                .HasOne(t => t.Paciente)
                .WithMany(p => p.Tests)
                .HasForeignKey(t => t.PacienteId);

            modelBuilder.Entity<Informe>()
                .HasOne(i => i.Paciente)
                .WithMany(p => p.Informes)
                .HasForeignKey(i => i.PacienteId);

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

            modelBuilder.Entity<Test>()
                .Property(t => t.Date)
                .IsRequired(false);

            modelBuilder.Entity<Test>()
                .Property(t => t.csv)
                .IsRequired(false);

            modelBuilder.Entity<Test>()
                .Property(t => t.video1)
                .IsRequired(false);

            modelBuilder.Entity<Test>()
                .Property(t => t.video2)
                .IsRequired(false);
        }
        public void ApplyMigrations()
        {
            // Apply pending migrations
            Database.Migrate();
        }
    }
}
