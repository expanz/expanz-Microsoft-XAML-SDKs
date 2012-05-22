using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Expanz.Extensions.BCL;
using Expanz.ThinRIA.Core;

namespace Expanz.ThinRIA.Controls
{
    public class ListBoxEx : ListBox, IServerBoundControl, IRepeatingDataControl //, IEditableControl
    {
        #region Constructor
        public ListBoxEx() : base()
        {
            SelectionChanged += ListboxEx_SelectionChanged;
#if !WPF
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                this.Loaded += new RoutedEventHandler(ListBoxEx_Loaded);
#endif
        }
        #endregion
        
        #region Member Variables
        protected ControlHarness _controlHarness;
        private string _lastKey;
        protected bool _isPublishing;
        private string _fieldId;

        private bool _syncContextWithServer = true;
        
        //private string myContextType = "";
        //private int myContextKey = 0;

        private Dictionary<string, ListBoxItemEx> _dictionary = null;

        #endregion

        #region Event Handlers
#if WPF
        public override void EndInit()
        {
            base.EndInit();
            InitHarness();
        }
#else
        private void ListBoxEx_Loaded(object sender, RoutedEventArgs e)
        {
            InitHarness();
        }
#endif

        private void ListboxEx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SetContextOnSelect) 
                SendContextToServer(SetContextOnSelect, SelectAction);
        }

        protected override void OnMouseLeftButtonUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if (SetContextOnDoubleClick && MouseDoubleClickCheck.IsDoubleClick(e))
                SendContextToServer(SetContextOnDoubleClick, DoubleClickAction);
        }
        #endregion

        #region Public Properties
        private string myNullEntryText;
        public string NullEntryText
        {
            set { myNullEntryText = value; }
            get { if (myNullEntryText == null) return "(none)"; else return myNullEntryText; }
        }

        private bool wantNullEntry = true;
        public bool WantNullEntry
        {
            set { wantNullEntry = value; }
            get { return wantNullEntry; }
        }

        [Category("expanz")]
        [Description("Sets the current context on the server to the selected item when it's selected.")]
        public bool SetContextOnSelect
        {
            get { return (bool)GetValue(SetContextOnSelectProperty); }
            set { SetValue(SetContextOnSelectProperty, value); }
        }

        public static readonly DependencyProperty SetContextOnSelectProperty =
            DependencyProperty.Register("SetContextOnSelect", typeof(bool), typeof(ListBoxEx), new PropertyMetadata(
#if WINDOWS_PHONE
                true
#else
                false
#endif
            ));

        [Category("expanz")]
        [Description("Specifies what action should take place when an item is selected.")]
        public string SelectAction
        {
            get { return (string)GetValue(SelectActionProperty); }
            set { SetValue(SelectActionProperty, value); }
        }

        public static readonly DependencyProperty SelectActionProperty =
            DependencyProperty.Register("SelectAction", typeof(string), typeof(ListBoxEx), null);


        [Category("expanz")]
        [Description("Sets the current context on the server to the selected item,")]
        public bool SetContextOnDoubleClick
        {
            get { return (bool)GetValue(SetContextOnDoubleClickProperty); }
            set { SetValue(SetContextOnDoubleClickProperty, value); }
        }

        public static readonly DependencyProperty SetContextOnDoubleClickProperty =
            DependencyProperty.Register("SetContextOnDoubleClick", typeof(bool), typeof(ListBoxEx), new PropertyMetadata(
#if WINDOWS_PHONE
                false
#else
                true
#endif
            ));

        [Category("expanz")]
        [Description("Specifies what action should take place when an item is double clicked.")]
        public string DoubleClickAction
        {
            get { return (string)GetValue(DoubleClickActionProperty); }
            set { SetValue(DoubleClickActionProperty, value); }
        }

        public static readonly DependencyProperty DoubleClickActionProperty =
            DependencyProperty.Register("DoubleClickAction", typeof(string), typeof(ListBoxEx), null);


        public string WrapperName
        {
            get { return (string)GetValue(WrapperNameProperty); }
            set { SetValue(WrapperNameProperty, value); }
        }

        public static readonly DependencyProperty WrapperNameProperty =
            DependencyProperty.Register("WrapperName", typeof(string), typeof(ListBoxEx), null);

        //[Category("expanz")]
        //[Description("Whether the server should be notified when an item is selected / double-clicked.")]
        //public bool SyncContextWithServer
        //{
        //    get { return _syncContextWithServer; }
        //    set { _syncContextWithServer = value; }
        //}

        public List<int> SelectedKeys
        {
            get
            {
                return GetSelectedKeys();
            }
        }
        #endregion

        #region Private Methods
        protected virtual void SetDeltaText(XElement xml)
        {
            if (string.IsNullOrEmpty(_lastKey))
                xml.SetAttributeValue(Common.ValueIsNull, "1");
            else
                xml.SetAttributeValue(Common.PublishFieldValue, _lastKey);
        }

        private XElement CreateRequestElement(string elementName)
        {
            XElement returnElement = new XElement(elementName);

            if (this.ModelObject != null && this.ModelObject.Length > 0)
                returnElement.SetAttributeValue(Common.contextObject, this.ModelObject);

            return returnElement;
        }

        private void SetContextObject(XElement xml)
        {
            string context = null;

            if (!string.IsNullOrEmpty(this.ModelObject))
                context = this.ModelObject;

            if (context == null && !string.IsNullOrEmpty(_controlHarness.ParentActivity.FixedContext))
                context = _controlHarness.ParentActivity.FixedContext;

            if (context != null) 
                xml.SetAttributeValue(Common.contextObject, context);
        }

        protected void SendContextToServer(bool setContextID, string action)
        {
            //TODO Clean up or delete
            //setCurrentContext();

            string contextKey = null;
            string myContextType = null;

            if (this.SelectedItem != null)
            {
                DynamicDataObject dynamicObject = this.SelectedItem as DynamicDataObject;
                contextKey = dynamicObject[Common.IDAttrib].ToString();
                myContextType = dynamicObject[Common.Data.RowType].ToString();
            }
            else
            {
                contextKey = "0";
            }

            var elementList = new XElement[2];

            var contextElement = CreateRequestElement(Common.Requests.SetContext);
            contextElement.SetAttributeValue(Common.IDAttrib, contextKey);
            contextElement.SetAttributeValue(Common.Data.RowType, myContextType);

            //if (setContextID)
                contextElement.SetAttributeValue(Common.Data.SetIdFromContext, "1");

            XElement actionElement = CreateRequestElement(Common.Requests.MenuAction);

            if (string.IsNullOrEmpty(action))
                actionElement.SetAttributeValue(Common.DefaultAction, "1");
            else
                actionElement.SetAttributeValue(Common.MenuAction, action);
            
            SetContextObject(contextElement);
            SetContextObject(actionElement);
            elementList[0] = contextElement;
            elementList[1] = actionElement;
            _controlHarness.SendXml(elementList);
        }

        private void SetCurrentContext()
        {
            int id = this.SelectedIndex;

            if (id > 0)
            {
                //Old WPF code
                //XElement xml = XElement.Parse(this.SelectedItem.ToString());
                //string v = xml.Attribute(Common.IDAttrib).Value;
                //int.TryParse(v, out myContextKey);
                //myContextType = xml.Attribute(Common.Data.RowType).Value;

                //DynamicDataObject dynamicObject = this.SelectedItem as DynamicDataObject;               
                //int.TryParse(dynamicObject[Common.IDAttrib].ToString(), out myContextKey);
            }
        }

        internal void SelectKey(string key)
        {
            if (_dictionary != null && _dictionary.ContainsKey(key))
                SelectedItem = _dictionary[key];
        }

        private List<int> GetSelectedKeys()
        {
            var keys = new List<int>();
            IEnumerator enumerator = this.SelectedItems.GetEnumerator();

            while (enumerator.MoveNext())
            {
                if (enumerator.Current is ListBoxItemEx)
                    keys.Add(((ListBoxItemEx)enumerator.Current).Key);
            }

            return keys;
        }

        public void InitHarness()
        {
            _controlHarness = new ControlHarness(this);
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
            get { return string.IsNullOrEmpty(_fieldId) ? SafeName : _fieldId; }
            set { _fieldId = value; }
        }

        public void SetVisible(bool visible)
        {
            Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public void PreDataPublishXml(XElement xml)
        {

        }

        public void PublishXml(XElement xml)
        {

        }
        #endregion

        #region Implementation of IEditableControl
        //public XElement DeltaXml
        //{
        //    get
        //    {
        //        XElement delta = _controlHarness.DeltaElement;
        //        delta.SetAttributeValue(Common.IDAttrib, FieldId);
        //        SetDeltaText(delta);
        //        return delta;
        //    }
        //}

        //public void SetEditable(bool editable)
        //{
        //    IsEnabled = editable;
        //}

        //public void SetNull()
        //{
        //    _lastKey = null;
        //    _isPublishing = true;
        //    SelectedIndex = -1;
        //    _isPublishing = false;
        //}

        //public void SetValue(string text)
        //{
        //    _isPublishing = true;

        //    SelectKey(text);

        //    _isPublishing = false;
        //    _lastKey = null;
        //}

        //public void SetLabel(string label)
        //{
        //    //ToDo:SetLabel
        //}

        //public void SetHint(string hint)
        //{
        //    //ToDo:SetHint
        //}

        //public void PublishXml(XElement xml)
        //{
        //    //ToDo:PublishXML
        //}

        //public bool IsValid
        //{
        //    get { return true; }
        //}
        #endregion

        #region Implementation of IDataControl
        public void PublishData(XElement data)
        {
            List<DynamicDataObject> collection = new List<DynamicDataObject>();

            _isPublishing = true;

            try
            {
                //Clear existing
                collection.Clear();

                // Get Definition references
                var columnDefintions = data.Element(Common.Data.Columns);
                var rows = data.Element(Common.Data.Rows);                

                //Server has nulled the data, exit
                if (rows == null || columnDefintions == null)
                    return;
                
                //Store Column Definitions                
                //XElement columnDef = (XElement)columnDefintions.FirstNode;
                //while (columnDef != null)
                //{
                //    string cellid = columnDef.Attribute(Common.IDAttrib).Value;
                //    string field = columnDef.Attribute(Common.Data.ColumnField).Value;                 
                //    string label = columnDef.Attribute(Common.Data.ColumnLabel).Value;
                //    string datatype = columnDef.Attribute(Common.Data.Type).Value;
                //    string width = columnDef.Attribute(Common.Data.ColumnWidth).Value;
                //    columnDef = (XElement)columnDef.NextNode;
                //}

                //Process Rows
                var rowNode = rows.FirstNode as XElement;

                while (rowNode != null)
                {
                    if (rowNode is XElement)
                    {
                        var rowData = (XElement)rowNode;

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
                //lastKey = null;
                SelectedIndex = -1;
                _isPublishing = false;
            }

            // Set the local data source
            this.ItemsSource = collection;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public string DataId
        {
            get { return SafeWrapperName ?? SafeName ?? FieldId ?? ModelObject ?? QueryId ?? PopulateMethod; }
        }

        private string SafeWrapperName
        {
            get { return string.IsNullOrEmpty(WrapperName) ? null : WrapperName; }
        }

        private string SafeName
        {
            get { return string.IsNullOrEmpty(Name) ? null : Name; }
        }

        [Category("expanz")]
        [Description("Set the ID of the Server Query to execute and bind the results to.")]
        public string QueryId
        {
            get { return (string)GetValue(QueryIdProperty); }
            set { SetValue(QueryIdProperty, value); }
        }

        public static readonly DependencyProperty QueryIdProperty =
            DependencyProperty.Register("QueryId", typeof(string), typeof(ListBoxEx), null);

        [Category("expanz")]
        [Description("Set the name of the Server method to call to get results to bind to.")]
        public string PopulateMethod
        {
            get { return (string)GetValue(PopulateMethodProperty); }
            set { SetValue(PopulateMethodProperty, value); }
        }

        public static readonly DependencyProperty PopulateMethodProperty =
            DependencyProperty.Register("PopulateMethod", typeof(string), typeof(ListBoxEx), null);

        [Category("expanz")]
        [Description("Set the Server ModelObject to execute the PopulateMethod on.")]
        public string ModelObject
        {
            get { return (string)GetValue(ModelObjectProperty); }
            set { SetValue(ModelObjectProperty, value); }
        }

        public static readonly DependencyProperty ModelObjectProperty =
            DependencyProperty.Register("ModelObject", typeof(string), typeof(ListBoxEx), null);

        [Category("expanz")]
        [Description("Set to true to auto execute the PopulateMethod or the QueryID.")]
        public string AutoPopulate
        {
            get { return (string)GetValue(AutoPopulateProperty); }
            set { SetValue(AutoPopulateProperty, value); }
        }

        public static readonly DependencyProperty AutoPopulateProperty =
            DependencyProperty.Register("AutoPopulate", typeof(string), typeof(ListBoxEx), null);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void FillServerRegistrationXml(XElement dp)
        {
            //throw new NotImplementedException();
        }
        #endregion
    }
}
