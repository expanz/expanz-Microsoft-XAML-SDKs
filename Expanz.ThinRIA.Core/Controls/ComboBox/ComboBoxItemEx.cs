using System.Windows.Controls;

namespace Expanz.ThinRIA.Controls
{
    public class ComboBoxItemEx : ComboBoxItem
    {
        #region Constructor
        public ComboBoxItemEx(string id, string type)
        {
            ID = id;
            Type = type;
        } 
        #endregion

        #region Public Methods
        public string ID { get; private set; }
        public string Type { get; private set; } 
        #endregion
    }
}
