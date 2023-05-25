using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mvvm.Messages
{
    public class ConnectInsolesMessage: Message
    {
        public ConnectInsolesMessage(List<string> macs) 
        { 
            this.macs = macs;
        }
        public List<string> macs { get; set;}
    }
}
