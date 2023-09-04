using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.DataHolders
{
    public class FrameDataInsoles : FrameData
    {
        public DataInsole left { get; set; }
        public DataInsole right { get; set; }
        public FrameDataInsoles(string csvLine)
        {
            string[] values = System.Text.RegularExpressions.Regex.Split(csvLine, @"\s+");
            time = getDouble(values, 1);
            frame = getInt(values, 2);
            left = new DataInsole(values, 3);
            right = new DataInsole(values, 11);
        }

        public override void ApplyFC(float fc)
        {
            left.ApplyFC(fc);
            right.ApplyFC(fc);
        }
    }
}
