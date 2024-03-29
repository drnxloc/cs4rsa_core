﻿using Cs4rsa.Utils;

using System;
using System.Globalization;
using System.Windows.Data;

namespace Cs4rsa.Converters
{
    class DayCvt : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            DayOfWeek dayOfWeek = (DayOfWeek)value;
            return dayOfWeek.ToCs4rsaThu();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
