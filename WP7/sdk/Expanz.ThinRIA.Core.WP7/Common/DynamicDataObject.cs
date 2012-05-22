using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;

namespace SilverlightApplication1
{
    public class DynamicDataObject : DynamicObject, INotifyPropertyChanged
    {
        private Dictionary<string, object> _data = new Dictionary<string, object>();

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
                OnPropertyChanged("Data");
                OnPropertyChanged(index);
            }
        }

        // This property is only needed to enable two-way binding via the value converter in WP7
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

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return _data.TryGetValue(binder.Name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _data[binder.Name] = value;
            OnPropertyChanged(binder.Name);
            OnPropertyChanged("");
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }

    public class DynamicDataObjectWP7 : INotifyPropertyChanged
    {
        private Dictionary<string, object> _data = new Dictionary<string, object>();

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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
}