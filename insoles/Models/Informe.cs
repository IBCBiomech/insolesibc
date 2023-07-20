﻿using insoles.Model;
using insoles.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using insoles.Commands;
using System.Collections.ObjectModel;

namespace insoles.Model
{
    public class Informe : ModelBase
    {
        public int Id { get; set; }
        public int PacienteId { get; set; }
        public Paciente Paciente { get; set; }
        [NotMapped]
        private string nombre;
        public string Nombre
        {
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
        public ICollection<InformeFile> Files { get; set; }
        public Informe()
        {
            this.Nombre = "Informe";
            Files = new ObservableCollection<InformeFile>();
        }
    }
}
