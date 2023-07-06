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
        public Informe informeDB;
        public InformeTreeView(Informe informe)
        {
            informeDB = informe;
        }
    }
}
