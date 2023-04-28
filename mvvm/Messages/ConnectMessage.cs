using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mvvm.Messages
{
    public class ConnectMessage: Message
    {
        public ConnectMessage(List<string> macs) 
        { 
            this.macs = macs;
        }
        public List<string> macs { get; set;}
    }
}
