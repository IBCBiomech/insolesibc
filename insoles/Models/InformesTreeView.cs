using insoles.Commands;
using insoles.Model;
using insoles.Models;
using insoles.States;
using insoles.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.Model
{
    public class InformesTreeView : MetaFolderTreeView
    {
        public ICollection<InformeTreeView> Informes {  get; set; }
        public CrearCarpetaInformeCommand crearCarpetaInformeCommand { get; set; }
        public InformesTreeView(ICollection<Informe> informes, DatabaseBridge databaseBridge, Paciente paciente) 
        {
            Informes = new ObservableCollection<InformeTreeView>();
            foreach(Informe informe in informes)
            {
                Informes.Add(new InformeTreeView(informe));
            }
            crearCarpetaInformeCommand = new CrearCarpetaInformeCommand(databaseBridge, paciente);
        }
        public InformesTreeView(DatabaseBridge databaseBridge, Paciente paciente)
        {
            Informes = new ObservableCollection<InformeTreeView>();
            crearCarpetaInformeCommand = new CrearCarpetaInformeCommand(databaseBridge, paciente);
        }
    }
}
