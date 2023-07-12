using insoles.Commands;
using insoles.Model;
using insoles.Models;
using insoles.States;
using insoles.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.Model
{
    public class TestTreeView : ModelBase
    {
        public Test testDB {get;set;}
        public ObservableCollection<TestFileTreeView> Files { get; set; }
        public RenombrarCarpetaTestCommand renombrarCarpetaTestCommand { get; set; }
        public BorrarTestCommand borrarTestCommand { get; set; }
        public CargarTestCommand cargarTestCommand { get; set; }
        public ImportarTestCommand importarTestCommand { get; set; }
        public TestTreeView(Test test, DatabaseBridge databaseBridge) 
        { 
            testDB = test;
            Files = new ObservableCollection<TestFileTreeView>();
            if(test.csv != null)
            {
                Files.Add(new TextTestFileTreeView(test.csv, test.Date.Value, 
                    new RenombrarFicheroCSVTestCommand(databaseBridge, test)));
            }
            if (test.video1 != null)
            {
                Files.Add(new VideoTestFileTreeView(test.video1, test.Date.Value,
                    new RenombrarFicheroVideo1TestCommand(databaseBridge, test)));
            }
            if (test.video2 != null)
            {
                Files.Add(new VideoTestFileTreeView(test.video2, test.Date.Value,
                    new RenombrarFicheroVideo2TestCommand(databaseBridge, test)));
            }
            renombrarCarpetaTestCommand = new RenombrarCarpetaTestCommand(databaseBridge);
            borrarTestCommand = new BorrarTestCommand(databaseBridge);
            cargarTestCommand = new CargarTestCommand(test);
            importarTestCommand = new ImportarTestCommand(test, databaseBridge);
        }
        public TestTreeView() 
        { 
            testDB = new Test();
        }
    }
}
