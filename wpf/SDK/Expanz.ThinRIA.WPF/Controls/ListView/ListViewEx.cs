using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Expanz.ThinRIA.Core;
using System.Windows.Data;
using Expanz.Extensions.BCL;

namespace Expanz.ThinRIA.Controls
{
    public class ListViewEx : ListView, IServerBoundControl, IRepeatingDataControl, IContextMenuPublisher
    {
        #region Constructor
        public ListViewEx() : base()
        {
            SelectionChanged += ListboxEx_SelectionChanged;
#if WPF
            this.ContextMenuOpening += new ContextMenuEventHandler(ListViewEx_ContextMenuOpening);
#else
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                this.Loaded += new RoutedEventHandler(ListBoxEx_Loaded);
#endif
        }
#if WPF
        void ListViewEx_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (sender is ListViewEx)
            {
                bool headerClick = false;
                if (e.OriginalSource is FrameworkElement)
                {
                    FrameworkElement src = (FrameworkElement)e.OriginalSource;
                    while (src != null)
                    {
                        if (src.TemplatedParent is GridViewColumnHeader)
                        {
                            src = null;
                            headerClick = true;
                        }
                        else if (src.TemplatedParent is FrameworkElement)
                        {
                            src = (FrameworkElement)src.TemplatedParent;
                        }
                        else src = null;
                    }
                }
                if (headerClick)
                {
                    e.Handled = true;
                }
                else
                {
                    if (((ListViewEx)sender).SelectedItem is Expanz.DynamicDataObject)//fixme
                    {
                        Expanz.DynamicDataObject dynamicObject = (Expanz.DynamicDataObject)((ListViewEx)sender).SelectedItem;
                        int.TryParse(dynamicObject[Common.IDAttrib].ToString(), out myContextKey);
                        myContextType = dynamicObject[Common.Data.RowType].ToString();
                    }
                }
            }
            if (myContextType != null && myContextType.Length > 0 && myContextKey > 0)
            {
                ActivityHarness.ContextMenuPublisher = this;
                var elementList = new XElement[2];

                var contextElement = CreateRequestElement(Common.Requests.SetContext);
                contextElement.SetAttributeValue(Common.IDAttrib, myContextKey);
                contextElement.SetAttributeValue(Common.Data.RowType, myContextType);
                //contextElement.SetAttributeValue(Common.SetIdFromContextAttribute, "1");

                SetContextObject(contextElement);
                elementList[0] = contextElement;
                var xmenu = CreateRequestElement(Common.Requests.ContextMenu);
                elementList[1] = xmenu;
                _controlHarness.SendXml(elementList);
            }
        }
        public void PublishContextMenu(XElement menu)
        {
            ContextMenu m = _controlHarness.createContextMenu(menu, theItem_Click);
            this.ContextMenu = m;
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, (new openContextMenuDelegate(_openContextMenu)));
        }

        private void _openContextMenu()
        {
            this.ContextMenu.IsOpen = true;
        }
        void theItem_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem mi = (System.Windows.Controls.MenuItem)sender;
            XElement actionElement = CreateRequestElement(Common.Requests.MenuAction);
            actionElement.SetAttribute(Common.MenuAction, mi.Tag.ToString());
            SetContextObject(actionElement);
            _controlHarness.SendXml(actionElement);
        }
#endif
        #endregion
        
        #region Member Variables
        protected ControlHarness _controlHarness;
        protected string _populateQuery;
        protected string _populateMethod;
        protected string _autoPopulate;
        protected string _modelObject;
        private string _lastKey;
        protected bool _isPublishing;
        private string _fieldId;
        private string myContextType;
        private int myContextKey;

        private bool _syncContextWithServer = true;

#if WINDOWS_PHONE
        private bool _setContextOnSelect = true;
        private bool _setContextOnDoubleClick = false;
#else
        private bool _setContextOnSelect = false;
        private bool _setContextOnDoubleClick = true;
#endif

        private string _selectAction = string.Empty;
        private string _doubleClickAction = string.Empty;
        
        //private string myContextType = string.Empty;
        //private int myContextKey = 0;

        private Dictionary<string, ListBoxItemEx> _dictionary = null;
        private Dictionary<string, string> myMetadata;
        public bool HasMetaData { get { return myMetadata != null; } }
        public Dictionary<string, string> Metadata { get { return myMetadata; } }

        #endregion

        #region Events
        public event EventHandler ContextChanging; 
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
            set { _setContextOnSelect = value; }
            get { return _setContextOnSelect; }
        }

        [Category("expanz")]
        [Description("Specifies what action should take place when an item is selected.")]
        public string SelectAction
        {
            set { _selectAction = value; }
            get { return _selectAction; }
        }
        
        [Category("expanz")]
        [Description("Sets the current context on the server to the selected item when it's double-clicked.")]
        public bool SetContextOnDoubleClick
        {
            set { _setContextOnDoubleClick = value; }
            get { return _setContextOnDoubleClick; }
        }

        [Category("expanz")]
        [Description("Specifies what action should take place when an item is double clicked.")]
        public string DoubleClickAction
        {
            set { _doubleClickAction = value; }
            get { return _doubleClickAction; }
        }

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
        public GridViewColumnCollection Columns
        {
            get
            {
                if (this.View == null || !(this.View is GridView)) return null;
                return ((GridView)this.View).Columns;
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

            if (ContextChanging != null)
                ContextChanging(this, new EventArgs());
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
                {
                    this.View = new GridView();
                    return;
                }

                var gridView = new GridView();

                XElement columnDef = (XElement)columnDefintions.FirstNode;

                while (columnDef != null)
                {
                    var column = new GridViewColumn();
                    column.Header = columnDef.GetAttributeValue(Common.Data.ColumnLabel);

                    string propertyName = MapColumnToProperty(columnDef);
                    string bindingPath = string.Format(propertyName, "[{0}]");
                    column.DisplayMemberBinding = new Binding(bindingPath);

                    string columnWidthString = columnDef.GetAttributeValue(Common.Data.ColumnWidth);
                    int columnWidth = 100;
                    int.TryParse(columnWidthString, out columnWidth);
                    if (columnWidth < 0) columnWidth = 100;
                    column.Width = columnWidth;

                    gridView.Columns.Add(column);

                    columnDef = (XElement)columnDef.NextNode;
                }

                this.View = gridView;

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
                        XElement cell = (XElement)columnDefintions.FirstNode;

                        while (cell != null)
                        {
                            string cellid = cell.Attribute(Common.IDAttrib).Value;
                            string field = MapColumnToProperty(cell);

                            //string value = rowData.Element("Cell" + cellid).Value;
                            string value = rowData.Elements("Cell").Where(x => x.Attribute(Common.IDAttrib).Value == cellid).FirstOrDefault().Value;

                            // Create property and assign value
                            dataObject[field] = value;

                            cell = (XElement)cell.NextNode;
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
  
        private string MapColumnToProperty(XElement xml)
        {
            string fieldName = xml.GetAttributeValue(Common.Data.ColumnField);

            if (fieldName == null)
                fieldName = "CELL" + xml.GetAttributeValue(Common.IDAttrib);

            string propertyName = fieldName.Replace(".", "_");
            return propertyName;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public string DataId
        {
            get { return SafeName ?? FieldId ?? QueryId ?? PopulateMethod; }
        }

        private string SafeName
        {
            get { return string.IsNullOrEmpty(Name) ? null : Name; }
        }

        [Category("expanz")]
        [Description("Set the ID of the Server Query to execute and bind the results to.")]
        public string QueryId
        {
            get { return _populateQuery; }
            set { _populateQuery = value; }
        }

        [Category("expanz")]
        [Description("Set the name of the Server method to call to get results to bind to.")]
        public string PopulateMethod
        {
            get { return _populateMethod; }
            set { _populateMethod = value; }
        }

        [Category("expanz")]
        [Description("Set the Server ModelObject to execute the PopulateMethod on.")]
        public string ModelObject
        {
            get { return _modelObject; }
            set { _modelObject = value; }
        }

        [Category("expanz")]
        [Description("Set to true to auto execute the PopulateMethod or the QueryID.")]
        public string AutoPopulate
        {
            get { return _autoPopulate; }
            set { _autoPopulate = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void FillServerRegistrationXml(XElement dp)
        {
            //throw new NotImplementedException();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public ControlHarness BindingHarness
        {
            get { return _controlHarness; }
        }
        #endregion

        #region Implementation of IRepeatingDataControl
        public void PreDataPublishXml(XElement fieldElement)
        {

        }

        public void PublishXml(XElement xml)
        {

        } 
        #endregion
    }
}
