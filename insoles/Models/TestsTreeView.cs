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
    public class TestsTreeView : MetaFolderTreeView
    {
        public ICollection<TestTreeView> Tests { get; set; }
        public CrearCarpetaTestCommand crearCarpetaTestCommand { get; set; }
        public TestsTreeView(ICollection<Test> tests, DatabaseBridge databaseBridge, Paciente paciente) 
        {
            Tests = new ObservableCollection<TestTreeView>();
            foreach(Test test in tests)
            {
                Tests.Add(new TestTreeView(test, databaseBridge, paciente.Peso != null?paciente.Peso.Value:70));
            }
            crearCarpetaTestCommand = new CrearCarpetaTestCommand(databaseBridge, paciente);
        }
        public TestsTreeView(DatabaseBridge databaseBridge, Paciente paciente)
        {
            Tests = new ObservableCollection<TestTreeView>();
            crearCarpetaTestCommand = new CrearCarpetaTestCommand(databaseBridge, paciente);
        }
    }
}
