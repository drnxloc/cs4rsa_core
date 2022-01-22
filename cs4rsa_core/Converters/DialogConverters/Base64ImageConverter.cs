﻿using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace cs4rsa_core.Converters.DialogConverters
{
    class Base64ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is not string s)
            {
                return null;
            }
            try
            {
                BitmapImage bi = new();
                Regex regex = new(@"^[\w/\:.-]+;base64,");
                s = regex.Replace(s, string.Empty);
                bi.BeginInit();
                bi.StreamSource = new MemoryStream(System.Convert.FromBase64String(s));
                bi.EndInit();
                return bi;
            }
#pragma warning disable CS0168 // The variable 'e' is declared but never used
            catch (NotSupportedException e)
            {
#pragma warning restore CS0168 // The variable 'e' is declared but never used
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
