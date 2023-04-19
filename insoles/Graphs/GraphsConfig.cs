using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static insoles.Common.Helpers;

namespace insoles.Graphs
{
    public static class GraphsConfig
    {
        private static Units _selectedDisplayUnitsValue;
        public static Units SelectedDisplayUnitsValue {
            get
            {
                return _selectedDisplayUnitsValue;
            }
            set
            {
                _selectedDisplayUnitsValue = value;
                transformFunc = getTransformFunc(SelectedCSVUnitsValue, value);
            }
        }
        private static AllUnits _selectedCSVUnitsValue;
        public static AllUnits SelectedCSVUnitsValue
        {
            get
            {
                return _selectedCSVUnitsValue;
            }
            set
            {
                _selectedCSVUnitsValue = value;
                transformFunc = getTransformFunc(value, SelectedDisplayUnitsValue);
            }
        }
        public static Func<float, float> getTransformFunc(AllUnits input, Units output)
        {
            switch (input)
            {
                case AllUnits.digital:
                    switch (output)
                    {
                        case Units.mbar:
                            return (digital) => VALUE_mbar(ADC_neg((int)digital));
                        case Units.N:
                            return (digital) => N(VALUE_mbar(ADC_neg((int)digital)));
                    }
                    break;
                case AllUnits.adc_neg:
                    switch (output)
                    {
                        case Units.mbar:
                            return (adc_neg) => VALUE_mbar((int)adc_neg);
                        case Units.N:
                            return (adc_neg) => N(VALUE_mbar((int)adc_neg));
                    }
                    break;
                case AllUnits.mbar:
                    switch (output)
                    {
                        case Units.mbar:
                            return (mbar) => mbar;
                        case Units.N:
                            return (mbar) => N(mbar);
                    }
                    break;
                case AllUnits.N:
                    switch (output)
                    {
                        case Units.mbar:
                            return (N) => N;
                        case Units.N:
                            return (N) => N;
                    }
                    break;
            }
            Trace.WriteLine("no transformation");
            return (value) => value;
        }
        public static Func<float, float> transformFunc;
    }
}
