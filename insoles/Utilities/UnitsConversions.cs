using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace insoles.Utilities
{
    public static class UnitsConversions
    {
        public static int ADC_neg(int VALUE_digital)
        {
            return 4095 - VALUE_digital;
        }
        public static float VALUE_mbar(int ADC_neg)
        {
            return MathF.Round(0.0006f * (ADC_neg * ADC_neg) + 0.6975f * ADC_neg, 3);
        }
        public static float N(float VALUE_mbar)
        {
            return (VALUE_mbar * 100) * (float)(435 / Math.Pow(10, 6));
        }
        public static float VALUE_mbar_from_N(float N)
        {
            return N / (float)(435 / Math.Pow(10, 6)) / 100;
        }
        public static float VALUE_mbar_from_VALUE_digital(int VALUE_digital)
        {
            return VALUE_mbar(ADC_neg(VALUE_digital));
        }
        public static float N_from_VALUE_digital(int VALUE_digital)
        {
            return N(VALUE_mbar(ADC_neg(VALUE_digital)));
        }
    }
}
