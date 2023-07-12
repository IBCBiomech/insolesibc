using insoles.Commands;
using insoles.Model;
using insoles.States;
using insoles.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.Model
{
    public class TestTreeView : ModelBase
    {
        public Test testDB {get;set;}
        public RenombrarCarpetaTestCommand renombrarCarpetaTestCommand { get; set; }
        public BorrarTestCommand borrarTestCommand { get; set; }
        public RenombrarFicheroCSVTestCommand renombrarFicheroCSVTestCommand { get; set; }
        public RenombrarFicheroVideo1TestCommand renombrarFicheroVideo1TestCommand { get; set; }
        public RenombrarFicheroVideo2TestCommand renombrarFicheroVideo2TestCommand { get; set; }
        public CargarTestCommand cargarTestCommand { get; set; }
        public ImportarTestCommand importarTestCommand { get; set; }
        public TestTreeView(Test test, DatabaseBridge databaseBridge) 
        { 
            testDB = test;
            renombrarCarpetaTestCommand = new RenombrarCarpetaTestCommand(databaseBridge);
            borrarTestCommand = new BorrarTestCommand(databaseBridge);
            renombrarFicheroCSVTestCommand = new RenombrarFicheroCSVTestCommand(databaseBridge);
            renombrarFicheroVideo1TestCommand = new RenombrarFicheroVideo1TestCommand(databaseBridge);
            renombrarFicheroVideo2TestCommand = new RenombrarFicheroVideo2TestCommand(databaseBridge);
            cargarTestCommand = new CargarTestCommand(test);
            importarTestCommand = new ImportarTestCommand(test, databaseBridge);
        }
        public TestTreeView() 
        { 
            testDB = new Test();
        }
    }
}
