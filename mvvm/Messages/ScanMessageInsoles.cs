using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mvvm.Messages
{
    public class ScanMessageInsoles : Message
    {
        public ScanMessageInsoles(List<InsoleScanData> Insoles)
        {
            this.Insoles = Insoles;
        }
        public List<InsoleScanData> Insoles { get; set; }
    }
}
