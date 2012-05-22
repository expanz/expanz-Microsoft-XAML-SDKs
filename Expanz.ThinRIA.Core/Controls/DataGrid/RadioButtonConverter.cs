using System.Globalization;
using System.Windows.Data;
using System;

namespace Expanz.ThinRIA.Controls
{
    public class RadioButtonConverter : IValueConverter
    {
        private string myCheckedValue;

        public RadioButtonConverter(string checkedValue)
        {
            myCheckedValue = checkedValue;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString() == myCheckedValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Common.boolValue(value.ToString())) 
                return myCheckedValue;

            return null;
        }
    }
}