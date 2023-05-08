using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insolesMVVM.Messages
{
    public class ScanInsolesMessage : Message
    {
        public ScanInsolesMessage(List<InsoleScan> insoles) 
        {
            this.insoles = insoles;
        }
        public List<InsoleScan> insoles { get; set; }
    }
}
