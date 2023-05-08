using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insolesMVVM.Messages
{
    public class InsoleScan
    {
        public string name { get; set; }
        public string address { get; set; }
        public InsoleScan(string name, string address)
        {
            this.name = name;
            this.address = address;
        }
    }
}
