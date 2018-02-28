using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using Xamarin.Forms;

namespace ASFT.Converters
{
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case bool _:
                    return (bool) value;
                case string _:
                    if (int.TryParse((string) value, out int i)) return i > 0;
                    return !string.IsNullOrEmpty((string) value);
                case IEnumerable _:
                    IEnumerable list = value as IEnumerable;
                    return list.Cast<object>().Any();
                case null:
                    return false;
            }

            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}