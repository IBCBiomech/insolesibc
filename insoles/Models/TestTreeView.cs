﻿using insoles.Model;
using insoles.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.Model
{
    public class TestTreeView : ModelBase
    {
        public Test testDB {get;set;}    
        public TestTreeView(Test test) 
        { 
            testDB = test;
        }
        public TestTreeView() 
        { 
            testDB = new Test();
        }
    }
}
