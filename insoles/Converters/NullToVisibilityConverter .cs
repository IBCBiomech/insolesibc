using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace insoles.Converter
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return Visibility.Collapsed; // Hide the element if value is null
            }
            else
            {
                return Visibility.Visible; // Show the element if value is not null
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

}
