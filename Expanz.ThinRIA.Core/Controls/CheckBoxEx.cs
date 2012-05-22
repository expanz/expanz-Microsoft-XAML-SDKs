using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Expanz.ThinRIA.Core;
using Expanz.Extensions.BCL;

namespace Expanz.ThinRIA.Controls
{
    public class CheckBoxEx : CheckBox, IServerBoundControl, IEditableControl, IFieldLabel, IContainerSummaryText
    {
        #region Constructor
        public CheckBoxEx()
        {
            Checked += CheckboxEx_Checked;
            Unchecked += CheckboxEx_Unchecked;
            Loaded += CheckboxEx_Loaded;
        } 
        #endregion

        #region Member Variables
        private ControlHarness _controlHarness;
        protected bool _isPublishing;
        private string _fieldId;
        #endregion

        #region Public Properties
        [Category("expanz")]
        [Description("Prevents the label from the server being displayed for the CheckBox. Useful when the field has another label source (such as a LabelEx control).")]
        public bool SuppressServerLabel { get; set; } 
        #endregion

        #region Events Handlers
        /// <summary>
        /// Handles the Checked event of the CheckboxEx control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void CheckboxEx_Checked(object sender, RoutedEventArgs e)
        {
            if (!_isPublishing)
                _controlHarness.SendXml(DeltaXml);
        }

        private void CheckboxEx_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_isPublishing)
                _controlHarness.SendXml(DeltaXml);
        }

        /// <summary>
        /// Handles the Loaded event of the CheckboxEx control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void CheckboxEx_Loaded(object sender, RoutedEventArgs e)
        {
            InitHarness();
        }
        #endregion

        #region Public Methods
        public void InitHarness()
        {
            _controlHarness = new ControlHarness(this);
        } 
        #endregion

        #region Private Methods
        /// <summary>
        /// Sets the XML attribute.
        /// </summary>
        /// <param name="xElement">The x element.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        protected virtual void SetXMLAttribute(XElement xElement, string name, object value)
        {
            xElement.SetAttributeValue(name, value);
        }

        /// <summary>
        /// Generates the delta XML Proberty.
        /// </summary>
        /// <returns></returns>
        private XElement GenerateDeltaXML()
        {
            var deltaXElement = _controlHarness.DeltaElement;

            SetXMLAttribute(deltaXElement, Common.IDAttrib, FieldId);
            SetXMLAttribute(deltaXElement, Common.PublishFieldValue, Common.boolString(IsChecked));

            return deltaXElement;
        } 
        #endregion

        #region Implementation of IServerBoundControl
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
            Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }
        #endregion

        #region Implementation of IEditableControl
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public XElement DeltaXml
        {
            get
            {
                return GenerateDeltaXML();
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string LabelText
        {
            get { return Content == null ? null : Content.ToString(); }
        }

        public void SetEditable(bool editable)
        {
            IsEnabled = editable;
        }

        public void SetNull()
        {
            IsChecked = false;
        }

        public void SetValue(string value)
        {
            _isPublishing = true;
            IsChecked = Common.boolValue(value);
            _isPublishing = false;
        }

        public void SetLabel(string labelValue)
        {
            if (!SuppressServerLabel)
                Content = labelValue;
        }

        public void SetHint(string hint)
        {
            
        }

        public void PublishXml(XElement xml)
        {
            
        }
        #endregion

        #region IContainerSummaryText
        protected string _SummaryProperty;

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string SummaryProperty
        {
            get
            {
                return _SummaryProperty;
            }
            set
            {
                _SummaryProperty = value;
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string SummaryText
        {
            get
            {
#if WPF
                if (_SummaryProperty == null || _SummaryProperty.Length == 0 || _SummaryProperty == Common.None) return null;
                if (_SummaryProperty.ToLower() == "auto" || _SummaryProperty.ToLower() == "true")
                {
                    if (IsChecked.HasValue && IsChecked.Value) return this.Content.ToString();
                }
                return null;
#else
                return null;
#endif
            }
        }
        #endregion
    }
}
