using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mvvm.Messages
{
    public class ScanInsolesMessage : Message
    {
        public ScanInsolesMessage(List<InsoleScan> Insoles)
        {
            this.Insoles = Insoles;
        }
        public List<InsoleScan> Insoles { get; set; }
    }
}
