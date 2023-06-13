using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using insoles.Model;

namespace insoles.Model
{
    public class Paciente
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
        }
    }
}
