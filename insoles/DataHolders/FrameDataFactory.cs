using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.DataHolders
{
    public abstract class FrameDataFactory
    {
        protected virtual string header { get; }
        public abstract void addLine(string line);
        public abstract GraphData getData();
        public int compareSimilarity(string header)
        {
            int value1 = 0;
            foreach (char c in this.header)
            {
                int tmp = c;
                value1 += c;
            }
            int value2 = 0;
            foreach (char c in header)
            {
                int tmp = c;
                value2 += c;
            }
            int similarity = Math.Abs(value1 - value2);
            return similarity;
        }
    }
}
