using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insolesMVVM.Messages
{
    public class CameraScan
    {
        public string name { get; set; }
        public int number { get; set; }
        public CameraScan(int number, string name)
        {
            this.number = number;
            this.name = name;
        }
    }
}
