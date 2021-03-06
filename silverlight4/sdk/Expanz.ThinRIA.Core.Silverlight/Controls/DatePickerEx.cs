﻿using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Expanz.Extensions.BCL;
using Expanz.ThinRIA.Core;

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
        public XElement DeltaXml
        {
            get
            {
                XElement delta = _controlHarness.DeltaElement;
                delta.SetAttributeValue(Common.IDAttrib, FieldId);
                delta.SetAttributeValue(Common.PublishFieldValue, this.Text);
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
            this.DisplayDate = DateTime.Now;
            this.Text = string.Empty;
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
                this.DisplayDate = dt;
                this.SelectedDate = dt;
            }
        }

        public void SetHint(string hint)
        {
            ToolTipService.SetToolTip(this, hint);
        }

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
