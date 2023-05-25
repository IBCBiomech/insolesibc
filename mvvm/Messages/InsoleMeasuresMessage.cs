using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mvvm.Messages
{
    public class InsoleMeasuresMessage : Message
    {
        public byte handler { get; set; }
        public List<InsoleData> measures {  get; set; }
        public InsoleMeasuresMessage(byte handler, List<InsoleData> measures) 
        {
            this.handler = handler;
            this.measures = measures;
        }
    }
}
