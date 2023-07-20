using insoles.Commands;
using insoles.Models;
using insoles.States;
using insoles.Utilities;
using insoles.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.Model
{
    public class InformeTreeView : PacientesTreeViewBase
    {
        public Informe informeDB { get; set; }
        public GenerarInformeCommand generarInformeCommand { get; set; }
        public ObservableCollection<InformeFileTreeView> Files { get; set; }
        public InformeTreeView(DatabaseBridge databaseBridge, Informe informe)
        {
            informeDB = informe;
            generarInformeCommand = new GenerarInformeCommand(databaseBridge, informeDB);
            Files = new ObservableCollection<InformeFileTreeView>();
            foreach (InformeFile file in informe.Files)
            {
                Files.Add(new InformeFileTreeView(file));
            }
        }
    }
}
