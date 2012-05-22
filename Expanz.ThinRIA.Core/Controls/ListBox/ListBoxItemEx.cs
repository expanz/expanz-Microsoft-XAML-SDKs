using System.Windows.Controls;

namespace Expanz.ThinRIA.Controls
{
    public class ListBoxItemEx : ListBoxItem
    {
        #region Constructor
        public ListBoxItemEx(string id, string type)
        {
            ID = id;
            Type = type;
        } 
        #endregion

        #region Public Methods
        public string ID { get; private set; }
        public string Type { get; private set; }

        public int Key
        {
            get
            {
                int key;

                if (!int.TryParse(ID, out key))
                    key = -1;

                return key;
            }
        }
        #endregion
    }
}
