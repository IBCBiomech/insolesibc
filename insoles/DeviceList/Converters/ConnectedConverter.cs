using System.Globalization;
using System;
using System.Windows.Data;
using System.Diagnostics;

namespace insoles.DeviceList.Converters
{
    //Clase para cambiar el formato del atributo connected
    [ValueConversion(typeof(bool), typeof(string))]
    public class ConnectedConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool connected = (bool)value;
            if (connected)
            {
                return new Uri("pack://application:,,,/UI/DeviceList/Icons/green-circle-icon.png");
            }
            else
            {
                return new Uri("pack://application:,,,/UI/DeviceList/Icons/red-circle-icon.png");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Uri uriValue = value as Uri;
            string path = uriValue.AbsolutePath;
            return path.Contains("green-circle-icon.png"); //Nombre del fichero de boton verde
        }
    }
}
