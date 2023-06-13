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
        public ICollection<Test> Tests { get; set; }
        public Paciente(string nombre)
        {
            Nombre = nombre;
            Tests = new ObservableCollection<Test> { new Test(DateTime.Now, "file.csv") };
        }
    }
}
