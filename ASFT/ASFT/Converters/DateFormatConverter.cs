using System;
using System.Globalization;
using Xamarin.Forms;

namespace ASFT.Converters
{
    public class DateFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "";

            if (DateTime.TryParse(value.ToString(), out DateTime d))
            {
                string format = "dd/MM/yyyy";
                if (parameter != null)
                    format = parameter.ToString();
                return d.ToString(format);
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}