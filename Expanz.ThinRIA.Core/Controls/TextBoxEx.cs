using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.Linq;
using Expanz.ThinRIA.Core;

namespace Expanz.ThinRIA.Controls
{
    [Description("expanz Text Input Control")]
    public partial class TextBoxEx : TextBox, IServerBoundControl, IEditableControl, IFieldErrorMessage, INotifyPropertyChanged, IDataErrorInfo, IContainerSummaryText
    {
        #region Constants
        private const string InvalidFocusedState = "InvalidFocused";
        private const string InvalidUnfocusedState = "InvalidUnfocused";
        private const string ValidState = "Valid";
        #endregion

        #region Member Variables
        private string _datatype;
        private ControlHarness _controlHarness;
        private string _value;
        private string _validationError = null;
        private bool _isSearchField = false;
        private bool _preventSendXml = false;
        #endregion

        #region Constructor
        public TextBoxEx()
        {
            Loaded += TextBoxEx_Loaded;

            AutoSelectAllOnGotFocus = true;
        }
        #endregion

        #region Public Properties
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IsFieldValueValid { get; set; }
        
        /// <summary>
        /// Gets or sets the datatype of the TextBox
        /// </summary>
        /// <value>The datatype.</value>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string Datatype
        {
            get { return _datatype; }
            set { _datatype = value; }
        }

        /// <summary>
        /// Set the field on the Server ModelObject to bind this controls input values to.
        /// </summary>       
        [Category("expanz")]
        [Description("Set the field on the Server ModelObject to bind this control's input values to.")]
        public string FieldId
        {
            get { return (string)GetValue(FieldIdProperty); }
            set { SetValue(FieldIdProperty, value); }
        }

        public static readonly DependencyProperty FieldIdProperty =
            DependencyProperty.Register("FieldId", typeof(string), typeof(TextBoxEx), null);

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public XElement DeltaXml
        {
            get { return GenerateDeltaXML(); }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                RaiseNotifyValueChanged();

                if (!_preventSendXml)
                    _controlHarness.SendXml(DeltaXml);
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IsSearchField
        {
            get
            {
                return _isSearchField;
            }
            private set
            {
                _isSearchField = value;

                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("IsSearchField"));
            }
        }
        
        [Browsable(false)]
        public bool HasFocus
        {
            get
            {
#if WPF
                return FocusManager.GetFocusedElement(this.Parent) == this;
#else
                return FocusManager.GetFocusedElement() == this;
#endif
            }
        }

        public bool AutoSelectAllOnGotFocus { get; set; }
        #endregion

        #region Event Handlers
        private void TextBoxEx_Loaded(object sender, RoutedEventArgs e)
        {
            InitHarness();

            // Bind the Text property to our Value property, enabling us 
            // to show validation errors
            Binding binding = new Binding("Value");
            binding.Mode = BindingMode.TwoWay;
            binding.NotifyOnValidationError = true;
#if !WINDOWS_PHONE
            binding.ValidatesOnDataErrors = true;
#endif
            binding.Source = this;
            this.SetBinding(TextBox.TextProperty, binding);
        }
        #endregion

        #region Public Methods
        public void InitHarness()
        {
            _controlHarness = new ControlHarness(this);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetVisible(bool visible)
        {
            Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetEditable(bool editable)
        {
            this.IsEnabled = editable;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetNull()
        {
            _value = string.Empty;
            RaiseNotifyValueChanged();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetValue(string text)
        {
            _value = text.Replace("\n", "\r\n");
            RaiseNotifyValueChanged();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SetHint(string hint)
        {
            ToolTipService.SetToolTip(this, hint);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void PublishXml(XElement xml)
        {
            if (xml.Attribute(Common.PublishFieldMaxLength) != null)
            {
                int maxLengthValue;

                if (int.TryParse(xml.Attribute(Common.PublishFieldMaxLength).Value, out maxLengthValue))
                    MaxLength = maxLengthValue;
            }

            if (xml.Attribute(Common.IsSearchField) != null)
                IsSearchField = (xml.Attribute(Common.IsSearchField).Value == "1");
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void ShowError(XElement xml)
        {
            _validationError = xml.Value;
            RaiseNotifyValueChanged();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void HideError()
        {
            _validationError = null;
            RaiseNotifyValueChanged();
        }
        #endregion

        #region Private Methods
        private XElement GenerateDeltaXML()
        {
            var deltaXElement = _controlHarness.DeltaElement;

            // Set the Id element in the XML with FieldId value which is refere to Name Value in XAML
            deltaXElement.SetAttributeValue(Common.IDAttrib, FieldId);

            // Set the Value element in XML with Text
            deltaXElement.SetAttributeValue(Common.PublishFieldValue, Value);

            return deltaXElement;
        }

        private void RaiseNotifyValueChanged()
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("Value"));
        }
        #endregion

        #region Overrides
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);

            if (AutoSelectAllOnGotFocus)
                this.SelectAll();
        }
        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region IDataErrorInfo
        public string Error
        {
            get { return _validationError; }
        }

        public string this[string columnName]
        {
            get { return _validationError; }
        } 
        #endregion

        #region IContainerSummaryText
        public string SummaryProperty { get; set; }

        public string SummaryText
        {
            get
            {
#if WPF
                if (SummaryProperty == null || SummaryProperty.Length == 0 || SummaryProperty == Common.None || Text == null || Text.Length == 0) 
                    return null;

                int len = Text.Length;

                if (SummaryProperty.ToLower() == "auto")
                {
                    if (len < 13) return Text;
                    int space = Text.IndexOf(' ');
                    if (space < 13)
                    {
                        //is there another space before 12?
                        string t = Text.Substring(space+1);
                        int sp2 = t.IndexOf(' ');
                        if ((sp2 + space) < 17)
                        {
                            return Text.Substring(0, sp2 + space + 1);
                        }
                        return Text.Substring(0, space);
                    }
                    return Text.Substring(0, 12);
                }
#endif

                return null;
            }
        }
        #endregion
    }
}
