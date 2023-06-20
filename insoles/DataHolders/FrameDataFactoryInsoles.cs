using Emgu.CV.Ocl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.DataHolders
{
    public class FrameDataFactoryInsoles : FrameDataFactory
    {
        protected override string header { get { return ""; } }
        private List<FrameDataInsoles> data;
        public FrameDataFactoryInsoles()
        {
            data = new List<FrameDataInsoles>();
        }
        public override void addLine(string line)
        {
            data.Add(new FrameDataInsoles(line));
        }
        public override GraphData getData()
        {
            return new GraphData(data.ToArray());
        }
    }
}
