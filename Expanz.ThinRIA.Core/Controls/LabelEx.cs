using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Xml.Linq;
using Expanz.Extensions.BCL;
using Expanz.ThinRIA.Core;

namespace Expanz.ThinRIA.Controls
{
    public class LabelEx : Label, IFieldLabel, IFieldErrorMessage, INotifyPropertyChanged, IDataErrorInfo
    {
        #region Member Variables
        private ControlHarness _controlHarness;
        private string _fieldId;
        private string _labelText;
        private string _validationError = null;
        private string _defaultValue = null;
        private Brush _userSetForeground = new SolidColorBrush(SystemColors.ControlTextColor);
        #endregion

        #region Constructor
        public LabelEx() : base()
        {
            //this.VerticalAlignment = VerticalAlignment.Center;
            Loaded += LabelEx_Loaded;
        }
        #endregion

        #region Event Handlers
        private void LabelEx_Loaded(object sender, RoutedEventArgs e)
        {
            InitHarness();

            _defaultValue = (Content == null) ? "" : Content.ToString();
            LabelText = _defaultValue;

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                // Bind the Content property to the LabelText property
                Binding binding = new Binding("LabelText");
                binding.Mode = BindingMode.OneWay;
                binding.NotifyOnValidationError = true;
#if !WINDOWS_PHONE
                binding.ValidatesOnDataErrors = true;
#endif
                binding.Source = this;
                this.SetBinding(LabelEx.ContentProperty, binding);
            }

            _userSetForeground = Foreground;
        }
        #endregion

        #region Public Properties
        [Category("expanz")]
        public string FieldId
        {
            get { return string.IsNullOrEmpty(_fieldId) ? Name : _fieldId; }
            set { _fieldId = value; }
        }

        [Category("expanz")]
        public string LabelText
        {
            get
            {
                return _labelText;
            }
            internal set
            {
                _labelText = value;
                RaiseNotifyLabelTextChanged();
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IsFieldValueValid { get; set; }
        #endregion

        #region Private Methods
        private void InitHarness()
        {
            _controlHarness = new ControlHarness(this);
        }

        private void RaiseNotifyLabelTextChanged()
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("LabelText"));
        }
        #endregion

        #region Public Methods
        public void SetLabel(string label)
        {
            LabelText = label.Replace("\n", "\r\n") + ":";
        }

        public void SetVisible(bool visible)
        {
            Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public void ShowError(XElement xml)
        {
            _validationError = xml.Value;
            RaiseNotifyLabelTextChanged();

            this.Foreground = new SolidColorBrush(Colors.Red);
        }

        public void HideError()
        {
            _validationError = null;
            RaiseNotifyLabelTextChanged();

            this.Foreground = _userSetForeground;
        }

        public void PublishXml(XElement xml)
        {
            bool isRequiredField = !xml.GetAttributeValue<bool>(Common.PublishFieldNullable, true);

            if (isRequiredField)
                this.FontWeight = FontWeights.Bold;
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
