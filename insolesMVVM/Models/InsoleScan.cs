using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insolesMVVM.Models
{
    public class InsoleScan
    {
        public string name { get; set; }
        public string MAC { get; set; }
        public InsoleScan(string name, string MAC)
        {
            this.name = name;
            this.MAC = MAC;
        }
    }
}
