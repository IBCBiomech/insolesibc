using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.DataHolders
{
    public class VariablesData
    {
        public float fc { get; private set; }
        public VariablesData(Dictionary<string, string> dictionary) 
        {
            if (dictionary.ContainsKey("fc"))
            {
                fc = float.Parse(dictionary["fc"], CultureInfo.InvariantCulture);
            }
            else
            {
                fc = 1;
            }
        }
        public VariablesData()
        {
            fc = 1;
        }
    }
}
