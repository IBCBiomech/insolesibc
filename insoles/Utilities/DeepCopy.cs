using insoles.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace insoles.Utilities
{
    public static class DeepCopy
    {
        public static List<Dictionary<Sensor, double>> Clone(List<Dictionary<Sensor, double>> obj)
        {
            List<Dictionary<Sensor, double>> clone = new();
            for(int i = 0; i < obj.Count; i++)
            {
                Dictionary<Sensor, double> originalDict = obj[i];
                var clonedDict = new Dictionary<Sensor, double>(originalDict.Count);

                foreach (var kvp in originalDict)
                {
                    clonedDict.Add(kvp.Key, kvp.Value);
                }

                clone.Add(clonedDict);
            }
            return clone;
        }
    }
}
