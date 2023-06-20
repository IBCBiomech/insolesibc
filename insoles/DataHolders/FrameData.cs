using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.DataHolders
{
    public abstract class FrameData
    {
        public double time { get; set; }
        public int frame { get; set; }
        protected double parseDouble(string s)
        {
            string s_point = s.Replace(",", ".");
            double result = double.Parse(s_point, CultureInfo.InvariantCulture);
            return result;
        }
        protected float parseFloat(string s)
        {
            string s_point = s.Replace(",", ".");
            float result = float.Parse(s_point, CultureInfo.InvariantCulture);
            return result;
        }
        protected float getFloat(string[] values, int index)
        {
            if (index < values.Length)
            {
                return parseFloat(values[index]);
            }
            else
            {
                return 0f;
            }
        }
        protected double getDouble(string[] values, int index)
        {
            if (index < values.Length)
            {
                return parseDouble(values[index]);
            }
            else
            {
                return 0.0;
            }
        }
        protected int getInt(string[] values, int index)
        {
            if (index < values.Length)
            {
                return int.Parse(values[index]);
            }
            else
            {
                return 0;
            }
        }
    }
}
