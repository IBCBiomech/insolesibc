using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mvvm.Messages
{
    public class CameraScanMessage
    {
        public string name { get; set; }
        public int number { get; set; }
        public CameraScanMessage(int number, string name)
        {
            this.number = number;
            this.name = name;
        }
    }
}
