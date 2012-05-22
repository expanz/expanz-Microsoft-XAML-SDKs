using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;

namespace Expanz
{
    public partial class DynamicDataObject : DynamicObject
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
                OnPropertyChanged(index);
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

    }
}