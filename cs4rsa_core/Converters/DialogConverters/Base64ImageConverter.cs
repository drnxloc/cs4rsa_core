﻿using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace cs4rsa_core.Converters.DialogConverters
{
    class Base64ImageConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is string s))
                return null;

            BitmapImage bi = new BitmapImage();
            Regex regex = new Regex(@"^[\w/\:.-]+;base64,");
            s = regex.Replace(s, string.Empty);
            bi.BeginInit();
            bi.StreamSource = new MemoryStream(System.Convert.FromBase64String(s));
            bi.EndInit();

            return bi;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
