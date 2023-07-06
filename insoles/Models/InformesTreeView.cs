using insoles.Model;
using insoles.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.Model
{
    public class InformesTreeView : ModelBase
    {
        public ICollection<InformeTreeView> Informes {  get; set; }
        public InformesTreeView(ICollection<Informe> informes) 
        {
            Informes = new ObservableCollection<InformeTreeView>();
            foreach(Informe informe in informes)
            {
                Informes.Add(new InformeTreeView(informe));
            }
        }
    }
}
