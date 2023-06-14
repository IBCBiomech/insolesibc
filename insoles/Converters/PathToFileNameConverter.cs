using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace insoles.Converter
{
    public class PathToFileNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string path = value as string;
            if (!string.IsNullOrEmpty(path))
            {
                return Path.GetFileName(path);
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

}
