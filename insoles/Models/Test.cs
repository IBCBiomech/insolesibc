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

        public Test()
        {
            this.Nombre = "Test";
        }
        public Test(DateTime date, string csv) : this()
        {
            this.Date = date;
            this.csv = csv;
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
