using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using insoles.Commands;
using insoles.Model;
using insoles.Utilities;

namespace insoles.Model
{
    public class Paciente : ModelBase
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string Lugar { get; set; }
        public float? Peso { get; set; }
        public float? Altura { get; set; }
        public float? LongitudPie { get; set; }
        public int? NumeroPie { get; set; }
        public string Profesion { get; set; }
        public ICollection<Test> Tests { get; set; }
        [NotMapped]
        private bool isSelected;
        [NotMapped]
        public bool IsSelected { 
            get 
            {
                return isSelected;
            }
            set 
            { 
                isSelected = value;
                OnPropertyChanged();
            } }
        [NotMapped]
        public EditarPacienteCommand editarPacienteCommand { get; set; }
        [NotMapped]
        public BorrarPacienteCommand borrarPacienteCommand { get; set; }
        public Paciente(string nombre, string? apellidos, DateTime? fechaNacimiento,
            string? lugar, float? peso, float? altura, float? longitudPie, int? numeroPie,
            string? profesion)
        {
            Nombre = nombre;
            Apellidos = apellidos;
            FechaNacimiento = fechaNacimiento;
            Lugar = lugar;
            Peso = peso;
            Altura = altura;
            LongitudPie = longitudPie;
            NumeroPie = numeroPie;
            Profesion = profesion;
            Tests = new ObservableCollection<Test>();
            editarPacienteCommand = new EditarPacienteCommand();
            borrarPacienteCommand = new BorrarPacienteCommand();
        }
    }
}
