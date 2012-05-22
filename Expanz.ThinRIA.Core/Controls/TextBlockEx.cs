using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Expanz.ThinRIA.Core;

namespace Expanz.ThinRIA.Controls
{
    public class TextBlockEx : ContentControl, IServerBoundControl, IValueDestination
    {
        #region Member Variables
        private ControlHarness _controlHarness;
        private string _fieldId; 
        #endregion

        #region Constructor
        public TextBlockEx() : base()
        {
            //this.VerticalAlignment = VerticalAlignment.Center;
            Loaded += LabelEx_Loaded;
        }
        #endregion

        #region Event Handlers
        private void LabelEx_Loaded(object sender, RoutedEventArgs e)
        {
            InitHarness();
        }
        #endregion

        #region Public Properties
        [Category("expanz")]
        public string FieldId
        {
            get { return string.IsNullOrEmpty(_fieldId) ? Name : _fieldId; }
            set { _fieldId = value; }
        }

        public string Text
        {
            get { return Content.ToString(); }
            set { Content = value; }
        } 
        #endregion

        #region Private Methods
        private void InitHarness()
        {
            _controlHarness = new ControlHarness(this);
        } 
        #endregion

        #region Public Methods
        public void SetVisible(bool visible)
        {
            Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public void SetNull()
        {
            this.Content = string.Empty;
        }

        public void SetValue(string text)
        {
            Content = text.Replace("\n", "\r\n");
        }

        public void SetHint(string hint)
        {
            // ToDo:Set Hind
        }

        public void PublishXml(System.Xml.Linq.XElement xml)
        {
            
        }
        #endregion
    }
}
