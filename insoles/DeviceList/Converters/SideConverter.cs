using insoles.DeviceList.Enums;
using System;
using System.Globalization;
using System.Windows.Data;

namespace insoles.DeviceList.Converters
{
    //Clase para cambiar el formato del atributo connected
    [ValueConversion(typeof(Side), typeof(string))]
    public class SideConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == null)
            {
                return String.Empty;
            }
            else
            {
                Side side = (Side)value;
                switch (side)
                {
                    case Side.Left:
                        return "Left";
                    case Side.Right:
                        return "Right";
                }
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string strValue = value as string;
            switch (strValue)
            {
                case "Left":
                    return Side.Left;
                case "Right":
                    return Side.Right;
            }
            return null;
        }
    }
}
