using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mvvm.Messages
{
    public class ScanMessage : Message
    {
        public ScanMessage(List<CameraScanMessage> cameras) 
        { 
            this.cameras = cameras;
        }
        public List<CameraScanMessage> cameras {  get; set; }
    }
}
