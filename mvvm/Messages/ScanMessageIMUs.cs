using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mvvm.Messages
{
    public class ScanMessageIMUs : Message
    {
        public ScanMessageIMUs(List<IMUScanData> cameras)
        {
            this.IMUs = cameras;
        }
        public List<IMUScanData> IMUs { get; set; }
    }
}
