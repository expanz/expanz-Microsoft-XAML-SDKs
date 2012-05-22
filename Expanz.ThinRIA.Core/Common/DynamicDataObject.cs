using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Expanz
{
    public partial class DynamicDataObject : INotifyPropertyChanged
    {
        private Dictionary<string, object> _data = new Dictionary<string, object>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
}