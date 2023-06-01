using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace insoles.Converter
{
    [ValueConversion(typeof(bool), typeof(string))]
    public class BooleanToCircleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool connected = (bool)value;
            if (connected)
            {
                //return "Images/green-circle-icon.png";
                return new Uri("pack://application:,,,/Images/green-circle-icon.png");
            }
            else
            {
                //return "Images/red-circle-icon.png";
                return new Uri("pack://application:,,,/Images/red-circle-icon.png");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
