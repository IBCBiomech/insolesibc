using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace insoles.Graphs.Converters
{
    [ValueConversion(typeof(int), typeof(string))]
    public class PreTextConverter : IValueConverter
    {
        public string text
        {
            get;
            set;
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int valueInt = (int)value;
            if (valueInt == int.MinValue)
            {
                return string.Empty;
            }
            else
            {
                return text + valueInt.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string strValue = value as string;
            if (strValue == string.Empty)
            {
                return int.MinValue;
            }
            else
            {
                string num = strValue.Remove(0, text.Length);
                return int.Parse(num);
            }
        }
    }
}
