using insoles.Commands;
using insoles.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.Model
{
    public class InformeTreeView : ModelBase
    {
        public Informe informeDB { get; set; }
        public GenerarInformeCommand generarInformeCommand { get; set; }
        public InformeTreeView(Informe informe)
        {
            informeDB = informe;
            generarInformeCommand = new GenerarInformeCommand();
        }
        public InformeTreeView()
        {
            informeDB = new();
            generarInformeCommand = new GenerarInformeCommand();
        }
    }
}
