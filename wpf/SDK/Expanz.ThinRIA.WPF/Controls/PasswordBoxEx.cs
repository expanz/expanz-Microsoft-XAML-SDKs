using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.Linq;
using Expanz.ThinRIA.Core;

namespace Expanz.ThinRIA.Controls
{
    [Description("expanz Password Input Control")]
    [TemplatePart(Name = TemplatePart_PasswordBoxControl, Type = typeof(PasswordBox))]
    public class PasswordBoxEx : Control, IServerBoundControl, IEditableControl, IFieldErrorMessage, INotifyPropertyChanged, IDataErrorInfo
    {
        #region Constants
        private const string TextBoxLabelProbName = "LabelText";
        private const string IsMandatoryProbName = "IsMandatory";
        private const string InvalidFocusedState = "InvalidFocused";
        private const string InvalidUnfocusedState = "InvalidUnfocused";
        private const string ValidState = "Valid";

        private const string TemplatePart_PasswordBoxControl = "PasswordBoxControl";
        #endregion

        #region Member Variables
        private string _datatype;
        private bool _multiline;
        private ControlHarness _controlHarness;
        private string _fieldId;
        private bool _isValid = true;
        private string _value;
        private string _validationError = null;
        private bool _isSearchField = false;
        private bool _preventSendXml = false;
        private PasswordBox _passwordBoxControl = null;
        #endregion

        #region Constructor
        public PasswordBoxEx()
        {
            Loaded += PasswordBoxEx_Loaded;
            LostFocus += PasswordBoxEx_LostFocus;

            this.DefaultStyleKey = typeof(PasswordBoxEx);

            AutoSelectAllOnGotFocus = true;
        }
        #endregion

        #region Overrides
        public override void OnApplyTemplate()
        {
            _passwordBoxControl = this.GetTemplateChild(TemplatePart_PasswordBoxControl) as PasswordBox;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="TextBoxEx"/> is multiline.
        /// </summary>
        /// <value><c>true</c> if multiline; otherwise, <c>false</c>.</value>
        public bool Multiline
        {
            get { return _multiline; }
            set { _multiline = value; }
        }

        /// <summary>
        /// Gets or sets the datatype of the TextBox
        /// </summary>
        /// <value>The datatype.</value>
        public string Datatype
        {
            get { return _datatype; }
            set { _datatype = value; }
        }

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

        public XElement DeltaXml
        {
            get { return generateDeltaXML(); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is valid.
        /// </summary>
        /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        public bool IsValid
        {
            get
            {
                return _isValid;
            }
            private set
            {
                _isValid = value;

                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("IsValid"));
            }
        }

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

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Description("Default values are set in MetaData on the server")]
        public string LabelText
        {
            get { return (string)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }

        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register(TextBoxLabelProbName, typeof(string), typeof(PasswordBoxEx),
                                        new PropertyMetadata(string.Empty));

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Description("Set either at runtime via business rules or in MetaData on the server")]
        public bool IsMandatory
        {
            get { return (bool)GetValue(IsMandatoryProperty); }
            set { SetValue(IsMandatoryProperty, value); }
        }

        public static readonly DependencyProperty IsMandatoryProperty =
           DependencyProperty.Register(IsMandatoryProbName, typeof(bool), typeof(PasswordBoxEx),
                                       new PropertyMetadata(false));

        public bool HasFocus
        {
            get
            {
#if WPF
                return FocusManager.GetFocusedElement(_passwordBoxControl) == this;
#else
                return FocusManager.GetFocusedElement() == this;
#endif
            }
        }

        public bool AutoSelectAllOnGotFocus { get; set; }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IsFieldValueValid { get; set; }
        #endregion

        #region Event Handlers
        private void PasswordBoxEx_Loaded(object sender, RoutedEventArgs e)
        {
            InitHarness();

            // Bind the Text property to our Value property, enabling us 
            // to show validation errors
//            Binding binding = new Binding("Value");
//            binding.Mode = BindingMode.TwoWay;
//            binding.NotifyOnValidationError = true;
//#if !WINDOWS_PHONE
//            binding.ValidatesOnDataErrors = true;
//#endif
//            binding.Source = this;
//            this.SetBinding(PasswordBox.pa.TextProperty, binding);
        }

        private void PasswordBoxEx_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_passwordBoxControl != null && _passwordBoxControl.Password != Value)
            {
                Value = _passwordBoxControl.Password;
            }
        }

        #region Public Methods
        public void InitHarness()
        {
            _controlHarness = new ControlHarness(this);
        }
        #endregion

        /// <summary>
        /// Sets the Visibility of the Control
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible] . if set to <c>false</c> [Collapsed] .</param>
        public void SetVisible(bool visible)
        {
            Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public void SetEditable(bool editable)
        {
            this.IsEnabled = editable;
        }

        public void SetNull()
        {
            _value = string.Empty;
            RaiseNotifyValueChanged();
        }

        /// <summary>
        /// Sets the value of TextBox
        /// </summary>
        /// <param name="text">The text.</param>
        public void SetValue(string text)
        {
            _value = text.Replace("\n", "\r\n");
            RaiseNotifyValueChanged();
        }

        /// <summary>
        /// if TextboxLabel is not null and is Empty set its value;
        /// </summary>
        /// <param name="labelValue">The label.</param>
        public void SetLabel(string labelValue)
        {
            if (LabelText != null && labelValue != null && LabelText.Length == 0)
                LabelText = labelValue.Replace("_", string.Empty);
        }

        public void SetHint(string hint)
        {
            ToolTipService.SetToolTip(this, hint);
        }

        public void PublishXml(XElement xml)
        {
            IsMandatory = false;

            if (xml.Attribute(Common.PublishFieldMaxLength) != null)
            {
                int maxLenghValue;

                if (int.TryParse(xml.Attribute(Common.PublishFieldMaxLength).Value, out maxLenghValue))
                {
                    _passwordBoxControl.MaxLength = maxLenghValue;
                }
            }

            if (xml.Attribute(Common.backgroundLabel) != null)
            {
                LabelText = xml.Attribute(Common.backgroundLabel).Value;
            }

            //If Manadatory && not valid && is null 
            if (xml.Attribute(Common.valid) != null && !_controlHarness.ParentActivity.IsLoading)
            {
                IsValid = Common.boolValue(xml.Attribute(Common.valid).Value);

                if (IsValid || !IsValid && this.Value == "")
                    HideError();
            }
        }

        public void ShowError(XElement xml)
        {
            _validationError = xml.Value;
            RaiseNotifyValueChanged();
        }

        public void HideError()
        {
            _validationError = null;
            RaiseNotifyValueChanged();
        }

        private void RaiseNotifyValueChanged()
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("Value"));
        }
        #endregion

        #region Private Methods
        ///// <summary>
        ///// function to set text value to xml element
        ///// </summary>
        ///// <param name="xml"></param>
        //protected virtual void setDeltaText(XElement xml)
        //{
        //    xml.SetAttributeValue(Common.PublishFieldValue, Text);
        //}

        /// <summary>
        /// Sets the XML attribute.
        /// </summary>
        /// <param name="xElement">The x element.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        protected virtual void setXMLAttribute(XElement xElement, string name, object value)
        {
            xElement.SetAttributeValue(name, value);
        }

        /// <summary>
        /// Generates the delta XML Proberty.
        /// </summary>
        /// <returns></returns>
        private XElement generateDeltaXML()
        {
            var deltaXElement = _controlHarness.DeltaElement;

            // Set the Id element in the XML with FieldId value which is refere to Name Value in XAML
            //delta.SetAttributeValue(Common.IDAttrib, FieldId);
            setXMLAttribute(deltaXElement, Common.IDAttrib, FieldId);

            // Set the Value element in XML with Text
            //setDeltaText(delta);
            setXMLAttribute(deltaXElement, Common.PublishFieldValue, Value);

            return deltaXElement;
        }
        #endregion

        #region Overrides
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);

            if (AutoSelectAllOnGotFocus)
                _passwordBoxControl.SelectAll();
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
    }
}
