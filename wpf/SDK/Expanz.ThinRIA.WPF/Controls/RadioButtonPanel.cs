using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml.Linq;
using Expanz.Extensions.BCL;
using Expanz.ThinRIA.Core;

namespace Expanz.ThinRIA.Controls
{
    // TODO: Make ItemsControl?  Each option must have a group GUID
    public class RadioButtonPanel : ItemsControl, IServerBoundControl, IEditableControl, INotifyPropertyChanged
    {
        #region Member Variables
        private ControlHarness _controlHarness;
        private string _fieldId;
        private List<DynamicDataObject> _radioButtonData;
        #endregion

        #region Constructor
        public RadioButtonPanel() : base()
        {
            this.Loaded += new RoutedEventHandler(RadioButtonPanel_Loaded);
            DefaultStyleKey = typeof(RadioButtonPanel);
            GroupName = Guid.NewGuid();
            SelectedValue = "2"; //Temp
        }
        #endregion

        #region Event Handlers
        private void RadioButtonPanel_Loaded(object sender, RoutedEventArgs e)
        {
            InitHarness();
        }
        #endregion

        #region Private Methods
        private void InitHarness()
        {
            if (_controlHarness == null)
                _controlHarness = new ControlHarness(this);
        }
        #endregion

        #region Public Properties
        //public Guid GroupName { get; private set; }



        public Guid GroupName
        {
            get { return (Guid)GetValue(GroupNameProperty); }
            set { SetValue(GroupNameProperty, value); }
        }

        public static readonly DependencyProperty GroupNameProperty =
            DependencyProperty.Register("GroupName", typeof(Guid), typeof(RadioButtonPanel));



        public string SelectedValue
        {
            get { return (string)GetValue(SelectedValueProperty); }
            set { SetValue(SelectedValueProperty, value); }
        }

        public static readonly DependencyProperty SelectedValueProperty =
            DependencyProperty.Register("SelectedValue", typeof(string), typeof(RadioButtonPanel));

        

        //public List<DynamicDataObject> RadioButtonData
        //{
        //    get { return _radioButtonData; }
        //    private set
        //    {
        //        _radioButtonData = value;

        //        if (PropertyChanged != null)
        //            PropertyChanged(this, new PropertyChangedEventArgs("RadioButtonData"));
        //    }
        //}

        //public List<DynamicDataObject> RadioButtonData
        //{
        //    get { return (List<DynamicDataObject>)GetValue(RadioButtonDataProperty); }
        //    set { SetValue(RadioButtonDataProperty, value); }
        //}

        //public static readonly DependencyProperty RadioButtonDataProperty =
        //    DependencyProperty.Register("RadioButtonData", typeof(List<DynamicDataObject>), typeof(RadioButtonPanel));

        
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
            DependencyProperty.Register("LabelText", typeof(string), typeof(RadioButtonPanel), new PropertyMetadata(string.Empty));

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
            if (xml.Name == Common.Data.Node)
            {
                this.ItemsSource = GetCollectionFromData(xml);
            }
            else if (xml.Name == Common.FieldNode)
            {
                SelectedValue = xml.GetAttributeValue(Common.PublishFieldValue);
            }
        }

        private List<DynamicDataObject> GetCollectionFromData(XElement data)
        {
            List<DynamicDataObject> collection = new List<DynamicDataObject>();
                        
            try
            {
                // Get Definition references
                var columnDefintions = data.Element(Common.Data.Columns);
                var rows = data.Element(Common.Data.Rows);                

                //Server has nulled the data, exit
                if (rows == null || columnDefintions == null)
                    return collection;

                //Process Rows
                var rowNode = rows.FirstNode as XElement;

                while (rowNode != null)
                {
                    if (rowNode is XElement)
                    {
                        var rowData = rowNode;

                        // Create dynamic object instance to build up
                        var dataObject = new DynamicDataObject();
                        dataObject[Common.IDAttrib] = rowNode.Attribute(Common.IDAttrib).Value;
                        dataObject[Common.Data.RowType] = rowNode.Attribute(Common.Data.RowType).Value;

                        // Add dynamic object properties based on cell definition
                        XElement columnDef = (XElement)columnDefintions.FirstNode;

                        while (columnDef != null)
                        {
                            string cellid = columnDef.Attribute(Common.IDAttrib).Value;
                            string field = columnDef.Attribute(Common.Data.ColumnField).Value;

                            if (!string.IsNullOrEmpty(field))
                            {
                                //string value = rowData.Element("Cell" + cellid).Value;
                                string value = rowData.Elements("Cell").Where(x => x.Attribute(Common.IDAttrib).Value == cellid).FirstOrDefault().Value;

                                // Create property and assign value
                                dataObject[field.Replace('.', '_')] = value;
                            }

                            columnDef = (XElement)columnDef.NextNode;
                        }

                        // Add to the collection
                        collection.Add(dataObject);
                    }
                    rowNode = rowNode.NextNode as XElement;
                }
            }
            finally
            {
            }

            return collection;
        }
        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged; 
        #endregion

        internal void UpdateSelectedValue(string newValue)
        {
            SelectedValue = newValue;

            // Send updated value to the server
            XElement delta = _controlHarness.DeltaElement;
            delta.SetAttributeValue(Common.IDAttrib, _fieldId);
            delta.SetAttributeValue(Common.PublishFieldValue, newValue);
            _controlHarness.SendXml(delta);
        }
    }

    internal class RadioButtonPanelItem : RadioButton
    {
        private bool dontUpdateValue = false;

        public RadioButtonPanelItem() : base()
        {
            this.Loaded += new RoutedEventHandler(RadioButtonPanelItem_Loaded);
        }

        private void RadioButtonPanelItem_Loaded(object sender, RoutedEventArgs e)
        {
            if (ParentPanel != null && DataContext != null)
            {
                DynamicDataObject data = (DynamicDataObject)DataContext;

                if (ParentPanel.SelectedValue == data["id"].ToString())
                {
                    dontUpdateValue = true;
                    this.IsChecked = true;
                    dontUpdateValue = false;
                }
            }
        }

        protected override void OnChecked(RoutedEventArgs e)
        {
            base.OnChecked(e);

            if (!dontUpdateValue && ParentPanel != null && DataContext != null)
            {
                DynamicDataObject data = (DynamicDataObject)DataContext;
                ParentPanel.UpdateSelectedValue(data["id"].ToString());
            }
        }

        public RadioButtonPanel ParentPanel
        {
            get { return (RadioButtonPanel)GetValue(ParentPanelProperty); }
            set { SetValue(ParentPanelProperty, value); }
        }

        public static readonly DependencyProperty ParentPanelProperty =
            DependencyProperty.Register("ParentPanel", typeof(RadioButtonPanel), typeof(RadioButtonPanelItem));        
    }
}
