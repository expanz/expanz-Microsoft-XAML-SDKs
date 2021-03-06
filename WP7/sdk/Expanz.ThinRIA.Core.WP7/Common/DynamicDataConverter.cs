﻿using System;
using System.Windows.Data;
using System.Collections.Generic;

namespace SilverlightApplication1
{
    public class DynamicDataConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var dataObject = value as DynamicDataObject;
            return (value != null) ? dataObject[parameter.ToString()] : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new KeyValuePair<string, object>(parameter.ToString(), value);
        }
    }
}
