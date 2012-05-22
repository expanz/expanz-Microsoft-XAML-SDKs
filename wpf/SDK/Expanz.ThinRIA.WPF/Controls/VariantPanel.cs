using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Expanz.Extensions.BCL;
using Expanz.ThinRIA.Core;

namespace Expanz.ThinRIA.Controls
{
    public class VariantPanel : StackPanel, IServerBoundControl, IEditableControl
    {
        #region Member Variables
        private ControlHarness _controlHarness;
        private string _fieldId;
        private TextBoxEx _textBox = null;
        private CheckBoxEx _checkBox = null;
        private ComboBoxEx _comboBox = null;
        private RadioButtonPanel _radioButtonPanel = null;
        #endregion

        #region Constructor
        public VariantPanel() : base()
        {
            this.Loaded += new RoutedEventHandler(VariantPanel_Loaded);
        }
        #endregion

        #region Event Handlers
        private void VariantPanel_Loaded(object sender, RoutedEventArgs e)
        {
            InitHarness();

            if (_textBox == null)
            {
                _textBox = new TextBoxEx();
                _textBox.FieldId = this.FieldId;
                this.Children.Add(_textBox);
            }

            if (_checkBox == null)
            {
                _checkBox = new CheckBoxEx();
                _checkBox.FieldId = this.FieldId;
                this.Children.Add(_checkBox);
            }

            if (_comboBox == null)
            {
                _comboBox = new ComboBoxEx();
                _comboBox.FieldId = this.FieldId;
                this.Children.Add(_comboBox);
            }

            if (_radioButtonPanel == null)
            {
                _radioButtonPanel = new RadioButtonPanel();
                _radioButtonPanel.FieldId = this.FieldId;
                this.Children.Add(_radioButtonPanel);
            }
        }
        #endregion

        #region Private Methods
        private void InitHarness()
        {
            if (_controlHarness == null)
                _controlHarness = new ControlHarness(this);
        }
        #endregion

        #region IServerBoundControl Members
        /// <summary>
        /// Set the field on the Server ModelObject to bind this controls input values to.
        /// </summary>       
        [Category("expanz")]
        [Description("Set the field on the Server ModelObject to bind this controls input values to.")]
        public string FieldId
        {
            get { return string.IsNullOrEmpty(_fieldId) ? Name : _fieldId; }
            set { _fieldId = value; }
        }

        public void SetVisible(bool visible)
        {
            this.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }
        #endregion

        #region IEditableControl Members
        public bool IsValid { get { return true; } }

        public XElement DeltaXml
        {
            get
            {
                XElement delta = _controlHarness.DeltaElement;
                delta.SetAttributeValue(Common.IDAttrib, FieldId);
                //delta.SetAttributeValue(Common.PublishFieldValue, this.Text);
                return delta;
            }
        }

        /// <summary>
        /// function to enable/disable the control
        /// </summary>
        /// <param name="editable"></param>
        public void SetEditable(bool editable)
        {
            this.IsEnabled = editable;
        }

        /// <summary>
        /// function to reset the control variables
        /// </summary>
        public void SetNull()
        {
            
        }

        /// <summary>
        ///  function to set the value to control
        /// </summary>
        /// <param name="text"></param>
        public void SetValue(string text)
        {
            
        }

        public void SetLabel(string label)
        {
            LabelText = label.Replace("\n", "\r\n");
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Description("Default values are set in Metadata on the server")]
        public string LabelText
        {
            get { return (string)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }

        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string), typeof(VariantPanel), new PropertyMetadata(string.Empty));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hint"></param>
        public void SetHint(string hint)
        {
            //this.ToolTip = hint;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="xml"></param>
        public void PublishXml(XElement xml)
        {
            if (xml.Name == "Data")
            {
                _radioButtonPanel.PublishXml(xml);
            }
            else
            {
                string visualType = null;

                if (xml.GetAttribute(Common.VisualType) != null)
                {
                    visualType = xml.GetAttributeValue(Common.VisualType);

                    if (visualType.Length == 0)
                        visualType = Common.None;
                }

                _radioButtonPanel.Visibility = Visibility.Collapsed;
                _checkBox.Visibility = Visibility.Collapsed;
                _textBox.Visibility = Visibility.Collapsed;
                _comboBox.Visibility = Visibility.Collapsed;

                if (visualType == "rb")
                {
                    _radioButtonPanel.Visibility = Visibility.Visible;
                }
                else if (visualType == "cb")
                {
                    _checkBox.Visibility = Visibility.Visible;
                }
                else if (visualType == "txt")
                {
                    _textBox.Visibility = Visibility.Visible;

                    //if (Common.boolValue(xml.GetAttribute("useDropdown")))
                    //{
                    //    textBox.Visible = false;
                    //    comboBox.Visible = true;
                    //    comboBox.publishXml(xml);
                    //}
                    //else
                    //{
                    //    textBox.Visible = true;
                    //    comboBox.Visible = false;
                    //    textBox.publishXml(xml);
                    //}
                }
            }
        }
        #endregion
    }
}
