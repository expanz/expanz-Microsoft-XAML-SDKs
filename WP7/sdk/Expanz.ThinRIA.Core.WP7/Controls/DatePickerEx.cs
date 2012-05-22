using System;
using System.ComponentModel;
using System.Windows;
using System.Xml.Linq;
using Expanz.ThinRIA.Core;
using Microsoft.Phone.Controls;

namespace Expanz.ThinRIA.Controls
{
    public class DatePickerEx : DatePicker, IServerBoundControl, IEditableControl
    {
        #region Member Variables
        private ControlHarness _controlHarness; 
        #endregion

        #region Constructor
        public DatePickerEx() : base()
        {
            this.LostFocus += new RoutedEventHandler(DatePicker_LostFocus);
            this.Loaded += new RoutedEventHandler(DatePickerEx_Loaded);
        } 
        #endregion

        #region Event Handlers
        private void DatePickerEx_Loaded(object sender, RoutedEventArgs e)
        {
            InitHarness();
        }

        private void DatePicker_LostFocus(object sender, RoutedEventArgs e)
        {
            _controlHarness.SendXml(DeltaXml);
        } 
        #endregion

        #region Public Methods
        /// <summary>
        /// function to initialize harness
        /// </summary>
        public void InitHarness()
        {
            _controlHarness = new ControlHarness(this);
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
                delta.SetAttributeValue(Common.PublishFieldValue, this.Value);
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
            this.Value = DateTime.Now;
        }

        /// <summary>
        ///  function to set the value to control
        /// </summary>
        /// <param name="text"></param>
        public void SetValue(string text)
        {
            DateTime dt;
            if (DateTime.TryParse(text, out dt))
            {
                this.Value = dt;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="label"></param>
        public void SetLabel(string label)
        {
            LabelText = label.Replace("\n", "\r\n");
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Description("Default values are set in MetaData on the server")]
        public string LabelText
        {
            get { return (string)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }

        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string), typeof(DatePickerEx),
                                        new PropertyMetadata(string.Empty));

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
        }
        #endregion

        #region IServerBoundControl Members
        private string _fieldId;
        
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

        /// <summary>
        /// function to set the visibility of control
        /// </summary>
        /// <param name="visible"></param>
        public void SetVisible(bool visible)
        {
            this.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }
        #endregion
    }
}
