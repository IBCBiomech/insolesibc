using insoles.Commands;
using insoles.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace insoles.Models
{
    public class InformeFileTreeView : PacientesTreeViewBase
    {
        public InformeFile informeDB { get; set; }
        public CargarInformeCommand cargarInformeCommand { get; set; }
        public InformeFileTreeView(InformeFile file)
        {
            informeDB = file;
            cargarInformeCommand = new CargarInformeCommand(file);
        }
    }
}
