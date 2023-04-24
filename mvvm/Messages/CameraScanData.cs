using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mvvm.Messages
{
    public class CameraScanData
    {
        public string name { get; set; }
        public int number { get; set; }
        public CameraScanData(int number, string name)
        {
            this.number = number;
            this.name = name;
        }
    }
}
