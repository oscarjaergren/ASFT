using System;
using System.Globalization;
using Xamarin.Forms;

namespace ASFT.Converters
{
    public class NoVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            VisibilityConverter visibilityConverter = new VisibilityConverter();
            return !(bool) visibilityConverter.Convert(value, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            return null;
        }
    }
}