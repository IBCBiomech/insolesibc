using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace mvvm.Converters
{
    // clase para cambiar el formato del atributo battery
    [ValueConversion(typeof(int), typeof(string))]
    public class BatteryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }
            else
            {
                int battery = (int)value;
                return battery.ToString() + "%";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string strValue = value as string;
            if (strValue == string.Empty)
            {
                return null;
            }
            else
            {
                string strNum = strValue.Remove(strValue.IndexOf("%"));
                return int.Parse(strNum);
            }
        }
    }
}
