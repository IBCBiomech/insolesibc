using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mvvm.Messages
{
    public class InsoleMeasuresMessage : Message
    {
        public List<InsoleMeasureData> measures {  get; set; }
        public InsoleMeasuresMessage(List<InsoleMeasureData> measures) 
        {
            this.measures = measures;
        }
    }
}
