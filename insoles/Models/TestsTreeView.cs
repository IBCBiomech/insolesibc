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
    public class TestsTreeView : ModelBase
    {
        public ICollection<TestTreeView> Tests;
        public TestsTreeView(ICollection<Test> tests) 
        {
            Tests = new ObservableCollection<TestTreeView>();
            foreach(Test test in tests)
            {
                Tests.Add(new TestTreeView(test));
            }
        }
    }
}
