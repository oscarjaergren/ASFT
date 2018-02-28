﻿using System;
using System.Globalization;
using System.Reflection;
using Xamarin.Forms;

namespace ASFT.Converters
{
    public class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            ImageSource image =
                ImageSource.FromResource(value.ToString(), typeof(ImageConverter).GetTypeInfo().Assembly);
            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}