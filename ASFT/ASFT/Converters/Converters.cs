using System;
using System.Globalization;
using Xamarin.Forms;

namespace ASFT.Converters
{
    public class StatusTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int numValue = (int)value;
            string x = numValue.ToString();
            return x;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            int.TryParse(value as string, out int numVal);
            return numVal;
        }
    }

    public class DateTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            DateTime dt = (DateTime)value;
            return dt.ToString(CultureInfo.CurrentUICulture);
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return new DateTime();
        }
    }
    public class ActiveOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? 1.0 : 0.5;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class SelectedItemEventArgsToSelectedItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var eventArgs = value as SelectedItemChangedEventArgs;
            if (eventArgs.SelectedItem != null) 
            {
            return eventArgs.SelectedItem;

            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
