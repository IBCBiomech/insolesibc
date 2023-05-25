using mvvm.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mvvm.Messages
{
    class LiveDataCalculationsMessage : Message
    {
        public Metric metric { get; set; }
        public Units units { get; set; }
        public List<InsoleData> left {  get; set; }
        public List<InsoleData> right { get; set; }
        public float[] leftCalcs {  get; set; }
        public float[] rightCalcs { get; set; }
        public LiveDataCalculationsMessage(Metric metric, Units units, 
            List<InsoleData> left, List<InsoleData> right,
            float[] leftCalcs, float[] rightCalcs) 
        {
            this.metric = metric;
            this.units = units;
            this.left = left;
            this.right = right;
            this.leftCalcs = leftCalcs;
            this.rightCalcs = rightCalcs;
        }
    }
}
