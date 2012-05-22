using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace Expanz
{
    public partial class DynamicDataObject
    {
        public object this[string index]
        {
            get
            {
                object value = null;
                _data.TryGetValue(index, out value);
                return value;
            }
            set
            {
                _data[index] = value;

                OnPropertyChanged("");
            }
        }

        // This property is only needed to enable two-way binding via the value converter in WP7.
        // Not needed in Silverlight - included only to make demo work.
        public object Data
        {
            get { return this; }
            set
            {
                if (value is KeyValuePair<string, object>)
                {
                    var newValue = (KeyValuePair<string, object>)value;
                    this[newValue.Key] = newValue.Value;
                }
            }
        }
    }
}
