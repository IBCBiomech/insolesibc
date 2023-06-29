using insoles.Commands;
using insoles.States;
using insoles.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace insoles.Model
{
    public class Test : ModelBase
    {
        public int Id { get; set; }
        public int PacienteId { get; set; }
        public Paciente Paciente { get; set; }
        public DateTime? Date { get; set; }
        [NotMapped]
        private string nombre;
        public string Nombre {
            get
            {
                return nombre;
            }
            set
            {
                nombre = value;
                OnPropertyChanged();
            }
        }
        public string? csv { get; set; }
        public string? video1 { get; set; }
        public string? video2 { get; set; }
        [NotMapped]
        public RenombrarCarpetaTestCommand renombrarCarpetaTestCommand { get; set; }
        [NotMapped]
        public BorrarTestCommand borrarTestCommand { get; set; }
        [NotMapped]
        public RenombrarFicheroCSVTestCommand renombrarFicheroCSVTestCommand { get; set; }
        [NotMapped]
        public RenombrarFicheroVideo1TestCommand renombrarFicheroVideo1TestCommand { get; set; }
        [NotMapped]
        public RenombrarFicheroVideo2TestCommand renombrarFicheroVideo2TestCommand { get; set; }
        [NotMapped]
        public CargarTestCommand cargarTestCommand { get; set; }
        public Test()
        {
            this.Nombre = "Test";
            renombrarCarpetaTestCommand = new RenombrarCarpetaTestCommand();
            borrarTestCommand = new BorrarTestCommand();
            renombrarFicheroCSVTestCommand = new RenombrarFicheroCSVTestCommand();
            renombrarFicheroVideo1TestCommand = new RenombrarFicheroVideo1TestCommand();
            renombrarFicheroVideo2TestCommand = new RenombrarFicheroVideo2TestCommand();
            cargarTestCommand = new CargarTestCommand();
        }
        public Test(DateTime date, string csv)
        {
            this.Nombre = "Test";
            this.Date = date;
            this.csv = csv;
            renombrarCarpetaTestCommand = new RenombrarCarpetaTestCommand();
            borrarTestCommand = new BorrarTestCommand();
            renombrarFicheroCSVTestCommand = new RenombrarFicheroCSVTestCommand();
            renombrarFicheroVideo1TestCommand = new RenombrarFicheroVideo1TestCommand();
            renombrarFicheroVideo2TestCommand = new RenombrarFicheroVideo2TestCommand();
            cargarTestCommand = new CargarTestCommand();
        }
        public Test(DateTime date, string csv, List<string> videos) : this(date, csv)
        {
            if(videos.Count >= 1)
            {
                this.video1 = videos[0];
            }
            if(videos.Count >= 2)
            {
                this.video2 = videos[1];
            }
        }
    }
}
