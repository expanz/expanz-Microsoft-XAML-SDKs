using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Xml.Linq;
using Expanz.ThinRIA.Core;
using System.IO;
using System.Xml;
using System.Text;
using Expanz.Extensions.BCL;

namespace Expanz.ThinRIA.Controls
{
    public class DataGridEx : DataGrid, IServerBoundControl, IEditableControl, IRepeatingDataControl
    {
        #region Member Variables
        //internal bool AllowEdits;
        protected ControlHarness _controlHarness;
        protected string _populateQuery;
        protected string _populateMethod;
        protected string _autoPopulate;
        protected bool _init;
        private string _lastKey;
        private int _contextKey;
        private string _contextType;
        private Dictionary<DataGridColumn, ColumnInfo> _columnMatrixKeys = new Dictionary<DataGridColumn, ColumnInfo>();
        #endregion;

        #region Structures
        private struct ColumnInfo
        {
            public string ID;
            public string FieldName;
            public string MatrixKey;
            public string DataType;
        }
        #endregion

        #region Constructor
        public DataGridEx() : base()
        {
#if !WPF
            this.Loaded += new RoutedEventHandler(DataGridEx_Loaded);
#else
            CanUserAddRows = false;
            CanUserDeleteRows = false;
#endif
        } 
        #endregion

        #region Event Handlers
#if WPF
        public override void EndInit()
        {
            base.EndInit();
            InitHarness();
        }
#else
        private void DataGridEx_Loaded(object sender, RoutedEventArgs e)
        {
            InitHarness();
        }
#endif

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            if (SetContextOnSelect)
                SendContextToServer(SetContextOnSelect, SelectAction);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            bool isDoubleClick = MouseDoubleClickCheck.IsDoubleClick(e);

            if (IsReadOnly && SetContextOnDoubleClick && isDoubleClick)
                SendContextToServer(SetContextOnDoubleClick, DoubleClickAction);

            if (isDoubleClick & ItemDoubleClick != null)
                ItemDoubleClick(this, new EventArgs());
        }

        protected override void OnCellEditEnding(DataGridCellEditEndingEventArgs e)
        {
            base.OnCellEditEnding(e);

            // Pass new value to server
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var elementList = new List<XElement>();

                // Set the context for this row
                SetCurrentContext();
                XElement contextElement = CreateRequestElement(Common.Requests.SetContext);
                contextElement.SetAttributeValue(Common.IDAttrib, _contextKey.ToString());
                contextElement.SetAttributeValue(Common.Data.RowType, _contextType);
                //contextElement.SetAttributeValue(Common.Data.SetIdFromContext, "1");
                SetContextObject(contextElement);
                elementList.Add(contextElement);

                // Now set the new value
                string newValue = null;
                XElement delta = _controlHarness.DeltaElement;

                if (e.EditingElement is TextBox)
                {
                    var textBox = e.EditingElement as TextBox;
                    newValue = textBox.Text;
                }
                else if (e.EditingElement is ComboBox)
                {
#if !WINDOWS_PHONE
                    var comboBox = e.EditingElement as ComboBox;
                    newValue = comboBox.SelectedValue.ToString();
#endif
                }
                else if (e.EditingElement is CheckBox)
                {
                    var checkBox = e.EditingElement as CheckBox;
                    newValue = checkBox.IsChecked.Value ? "1" : "0";
                }

                if (string.IsNullOrEmpty(newValue))
                    delta.SetAttributeValue(Common.ValueIsNull, "1");
                else
                    delta.SetAttributeValue(Common.PublishFieldValue, newValue);

                string fieldName = _columnMatrixKeys[e.Column].FieldName;

                if (fieldName != null)
                    delta.SetAttributeValue(Common.IDAttrib, fieldName);

                string matrixKey = _columnMatrixKeys[e.Column].MatrixKey;

                if (matrixKey != null)
                    delta.SetAttributeValue(Common.Data.MatrixKey, matrixKey);

                elementList.Add(delta);

                _controlHarness.SendXml(elementList.ToArray());
            }
        }
        #endregion

        #region Private Methods
        private void SetContextObject(XElement xml)
        {
            string context = null;

            if (this.ModelObject != null && this.ModelObject.Length > 0)
            {
                context = this.ModelObject;
            }

            if (context == null && _controlHarness.ParentActivity.FixedContext != null && _controlHarness.ParentActivity.FixedContext.Length > 0)
            {
                context = _controlHarness.ParentActivity.FixedContext;
            }

            if (context != null) 
                xml.SetAttributeValue(Common.contextObject, context);
        }

        private XElement CreateRequestElement(string elementName)
        {
            XElement returnElement = new XElement(elementName);

            if (this.ModelObject != null && this.ModelObject.Length > 0)
                returnElement.SetAttributeValue(Common.contextObject, this.ModelObject);

            return returnElement;
        }

        private void SetCurrentContext()
        {
            _contextKey = this.SelectedId;
            _contextType = null;

            if (_contextKey > 0)
            {
                _contextType = SelectedRowData[Common.Data.RowType].ToString();
            }
        }

        /// <summary>
        /// function to send context to server
        /// </summary>
        protected void SendContextToServer(bool setContextID, string action)
        {
            SetCurrentContext();
            //if (myContextType != null && myContextType.Length > 0 && myContextKey > 0)
            //{
            //send a context and menuaction_properties
            var elementList = new List<XElement>();
            XElement contextElement, actionElement;

            contextElement = CreateRequestElement(Common.Requests.SetContext);
            contextElement.SetAttributeValue(Common.IDAttrib, _contextKey.ToString());
            contextElement.SetAttributeValue(Common.Data.RowType, _contextType);

            if (setContextID)
                contextElement.SetAttributeValue(Common.Data.SetIdFromContext, "1");

            SetContextObject(contextElement);
            elementList.Add(contextElement);

            if (IsReadOnly && (action ?? "").ToLower() != Common.None)
            {
                actionElement = CreateRequestElement(Common.Requests.MenuAction);

                if (string.IsNullOrEmpty(action))
                    actionElement.SetAttributeValue(Common.DefaultAction, "1");
                else
                    actionElement.SetAttributeValue(Common.MenuAction, action);

                SetContextObject(actionElement);
                elementList.Add(actionElement);
            }

            _controlHarness.SendXml(elementList.ToArray());
            //}
        }

        private void ConfigureDataGridColumns(XElement data)
        {
            //if (data.GetAttributeValue<bool>("clearColumnDefinitions"))
                this.Columns.Clear();

            XElement ColumnDefintions = data.Element(Common.Data.Columns);

            if (ColumnDefintions == null)
                return;

            this.AutoGenerateColumns = false;

            XElement columnDef = (XElement)ColumnDefintions.FirstNode;

            while (columnDef != null)
            {
                DataGridColumn col = null;
                string cellid = columnDef.Attribute(Common.IDAttrib).Value;
                bool columnIsEditable = columnDef.GetAttributeValue<bool>(Common.Data.ColumnEditable);
                string columnDataType = columnDef.GetAttributeValue(Common.Datatypes.AttribName);

                string field = null; // columnDef.GetAttributeValue(Common.Data.ColumnField);

                if (string.IsNullOrEmpty(field))
                    field = "__CELL" + cellid;

                if (columnDataType == Common.Datatypes.BLOB)
                {
#if SILVERLIGHT
                    const string assemblyName = "Expanz.ThinRIA.Core.Silverlight";
#elif WINDOWS_PHONE
                    const string assemblyName = "Expanz.ThinRIA.Core.WP7";
#elif WPF
                    const string assemblyName = "Expanz.ThinRIA.WPF";
#endif

                    string xaml = @"<DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                                                  xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
                                                  xmlns:ec=""clr-namespace:Expanz.ThinRIA.Controls;assembly=" + assemblyName + @""">
                                        <Grid>
                                            <ec:ImageEx Url=""{Binding [" + field + @"]}"" DisplayAsThumbnail=""true"" Stretch=""None"" Margin=""0,3"" />
                                        </Grid>
                                    </DataTemplate>";

                    DataTemplate imageTemplate = GetDataTemplateFromString(xaml);

                    col = new DataGridTemplateColumn()
                    {
                        CellTemplate = imageTemplate
                    };
                }
                else if (columnIsEditable && columnDef.HasElements) // && UseRadioButtonsForEnums)
                {
                    XElement entry = (XElement)columnDef.FirstNode;
                    var comboBoxItems = new Dictionary<string, string>();

                    while (entry != null)
                    {
                        comboBoxItems.Add(entry.GetAttributeValue(Common.KeyValueValue), entry.GetAttributeValue(Common.text));
                        entry = (XElement)entry.NextNode;
                    }

                    this.Resources.Add(field + "Values", comboBoxItems);

#if WPF
                    col = new DataGridComboBoxColumn()
                    {
                        DisplayMemberPath = "Value",
                        SelectedValuePath = "Key",
                        SelectedValueBinding = new Binding { Path = new PropertyPath("[" + field + "]"), Mode = BindingMode.TwoWay },
                        ItemsSource = comboBoxItems
                    };
#else
                    // TODO: Implement as a combo box!!!
                    Binding binding = new Binding { Path = new PropertyPath("[" + field + "]") };

                    col = new DataGridTextColumn()
                    {
                        IsReadOnly = !columnIsEditable,
                        Binding = binding
                    };
#endif

                    // Won't seem to work as a StaticResource in WPF. Need to test Silverlight
                    // to see if StaticResource works OK there.
//#if WPF
//                    string itemsSource = "{DynamicResource " + field + "Values}";
//#else
//                    string itemsSource = "{Binding Source={StaticResource " + field + @"Values}}";
//#endif

//                    string xaml = @"<DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
//                                                  xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
//                                                  xmlns:ec=""clr-namespace:Expanz.ThinRIA.Controls;assembly=Expanz.ThinRIA.Core.Silverlight"">
//                                        <Grid>
//                                            <ComboBox Name=""SelectValueComboBox"" ItemsSource=""" + itemsSource + @""" DisplayMemberPath=""Value"" SelectedValuePath=""Key"" SelectedValue=""{Binding [" + field + @"]}"" />
//                                        </Grid>
//                                    </DataTemplate>";

//                    DataTemplate comboBoxTemplate = GetDataTemplateFromString(xaml);

//                    col = new DataGridTemplateColumn()
//                    {
//                        CellTemplate = comboBoxTemplate
//                    };

                    //object comboBoxControl = comboBoxTemplate.FindName("SelectValueComboBox", null);
                }
                else if (columnDataType == Common.Datatypes.Boolean)
                {
                    // To confirm this works - data type conversion may be required
                    Binding binding = new Binding { Path = new PropertyPath("[" + field + "]") };

                    col = new DataGridCheckBoxColumn()
                    {
                        IsReadOnly = !columnIsEditable,
                        Binding = binding,
                        CanUserSort = true,
                        SortMemberPath = binding.Path.Path
                    };
                }
                else
                {
                    Binding binding = new Binding { Path = new PropertyPath("[" + field + "]") };

                    col = new DataGridTextColumn()
                    {
                        IsReadOnly = !columnIsEditable,
                        Binding = binding,
                        CanUserSort = true,
                        SortMemberPath = binding.Path.Path
                    };
                }

                int width;

                if (!int.TryParse(columnDef.GetAttributeValue(Common.Data.ColumnWidth), out width))
                    width = 100;
#if WPF
                col.Width = width;
#else
                col.Width = new DataGridLength(width);
#endif
                col.Header = columnDef.Attribute(Common.Data.ColumnLabel).Value;
                this.Columns.Add(col);

                ColumnInfo colInfo;
                colInfo.ID = columnDef.GetAttributeValue(Common.Data.MatrixKey);
                colInfo.FieldName = columnDef.GetAttributeValue(Common.Data.ColumnField);
                colInfo.DataType = columnDef.GetAttributeValue(Common.Datatypes.AttribName);
                colInfo.MatrixKey = columnDef.GetAttributeValue(Common.Data.MatrixKey);
                _columnMatrixKeys.Add(col, colInfo);

                columnDef = (XElement)columnDef.NextNode;
            }
        }

        private DataTemplate GetDataTemplateFromString(string xaml)
        {
#if WPF
            var stream = new MemoryStream(ASCIIEncoding.Default.GetBytes(xaml));
            DataTemplate dataTemplate = (DataTemplate)XamlReader.Load(stream);
            stream.Dispose();
#else
            DataTemplate dataTemplate = (DataTemplate)XamlReader.Load(xaml);
#endif

            return dataTemplate;
        }

        private List<DynamicDataObject> ConvertPublicationToCollection(XElement data)
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
                            string field = null; // columnDef.GetAttributeValue(Common.Data.ColumnField);

                            if (string.IsNullOrEmpty(field))
                                field = "__CELL" + cellid;

                            //string value = rowData.Element("Cell" + cellid).Value;
                            string value = rowData.Elements("Cell").Where(x => x.Attribute(Common.IDAttrib).Value == cellid).FirstOrDefault().Value;

                            // Create property and assign value
                            string propertyName = field.Replace('.', '_');

                            if (columnDef.GetAttributeValue(Common.Datatypes.AttribName) == Common.Datatypes.Boolean)
                                dataObject[propertyName] = Common.boolValue(value);
                            else
                                dataObject[propertyName] = value;

                            columnDef = (XElement)columnDef.NextNode;
                        }

                        // Add to the collection
                        collection.Add(dataObject);
                    }
                    rowNode = rowNode.NextNode as XElement;
                }
            }
            catch { }

            return collection;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Initialize harness class
        /// </summary>
        public void InitHarness()
        {
            _controlHarness = new ControlHarness(this);
        } 

        internal void SelectKey(string key)
        {
            //if (myDictionary != null && myDictionary.ContainsKey(key))
            //{
            //    SelectedItem = myDictionary[key];
            //}
        }

        /// <summary>
        /// Function to publish the xml data to the control
        /// </summary>
        /// <param name="data"></param>
        public virtual void PublishData(XElement data)
        {
            _isPublishing = true;

            XDocument dataDoc = XDocument.Parse(data.ToString());
            var root = dataDoc.Descendants("Rows");

            IEnumerable elements = null;

            if (root.Count() > 0)
            {
                XElement xeleRoot = root.ElementAt(0);
                elements = xeleRoot.Elements("Row");
            }

            ConfigureDataGridColumns(data);

            this.IsReadOnly = !(data.GetAttributeValue<bool>(Common.Data.HasEditableColumns));            
            this.SelectionMode = DataGridSelectionMode.Extended;
            this.SelectedIndex = -1;

            List<DynamicDataObject> collection = ConvertPublicationToCollection(data);
            this.ItemsSource = collection;

            _lastKey = null;
            _isPublishing = false;
        }
        #endregion

        #region Public Properties
        public bool UseRadioButtonsForEnums { get; set; }
        public string RedirectURLInEdit { get; set; }

        public DynamicDataObject SelectedRowData
        {
            get
            {
                return (DynamicDataObject)SelectedItem;
            }
        }

        [Category("expanz")]
        [Description("Sets the current context on the server to the selected item when it's selected.")]
        public bool SetContextOnSelect
        {
            get { return (bool)GetValue(SetContextOnSelectProperty); }
            set { SetValue(SetContextOnSelectProperty, value); }
        }

        public static readonly DependencyProperty SetContextOnSelectProperty =
            DependencyProperty.Register("SetContextOnSelect", typeof(bool), typeof(DataGridEx), new PropertyMetadata(
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
            DependencyProperty.Register("SelectAction", typeof(string), typeof(DataGridEx), null);        


        [Category("expanz")]
        [Description("Sets the current context on the server to the selected item,")]
        public bool SetContextOnDoubleClick
        {
            get { return (bool)GetValue(SetContextOnDoubleClickProperty); }
            set { SetValue(SetContextOnDoubleClickProperty, value); }
        }

        public static readonly DependencyProperty SetContextOnDoubleClickProperty =
            DependencyProperty.Register("SetContextOnDoubleClick", typeof(bool), typeof(DataGridEx), new PropertyMetadata(
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
            DependencyProperty.Register("DoubleClickAction", typeof(string), typeof(DataGridEx), null);


        public string WrapperName
        {
            get { return (string)GetValue(WrapperNameProperty); }
            set { SetValue(WrapperNameProperty, value); }
        }

        public static readonly DependencyProperty WrapperNameProperty =
            DependencyProperty.Register("WrapperName", typeof(string), typeof(DataGridEx), null);
        

        //[Category("expanz")]
        //[Description("Whether the server should be notified when an item is selected / double-clicked.")]
        //public bool SyncContextWithServer
        //{
        //    get { return _syncContextWithServer; }
        //    set { _syncContextWithServer = value; }
        //}

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

        public int SelectedId
        {
            get
            {
                int id = 0;

                if (SelectedItem != null)
                {
                    int.TryParse(SelectedRowData[Common.IDAttrib].ToString(), out id);
                }

                return id;
            }
        }

        public List<int> SelectedKeys
        {
            get
            {
                List<int> keys = new List<int>();
                IEnumerator ie = this.SelectedItems.GetEnumerator();
                //while (ie.MoveNext())
                //{
                //    if (ie.Current is ListBoxItemEx)
                //    {
                //        keys.Add(((ListBoxItemEx)ie.Current).Key);
                //    }
                //}
                return keys;
            }
        }
        #endregion

        #region Interface Implementations
        public string FieldId
        {
            //get { return Common.makeServerName(Name); }
            get { return null; } // string.IsNullOrEmpty(_fieldId) ? Name : _fieldId; }
        }

        public XElement DeltaXml
        {
            get
            {
                XElement delta = _controlHarness.DeltaElement;
                delta.SetAttributeValue(Common.IDAttrib, FieldId);
                SetDeltaText(delta);
                return delta;
            }
        }

        /// <summary>
        /// function to set the visibilty of control
        /// </summary>
        /// <param name="v"></param>
        public void SetVisible(bool visible)
        {
            this.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// function to enable\disable the control
        /// </summary>
        /// <param name="e"></param>
        public void SetEditable(bool e)
        {
            IsEnabled = e;
        }

        /// <summary>
        /// function to reset control values
        /// </summary>
        public void SetNull()
        {
            _lastKey = null;
            _isPublishing = true;
            this.SelectedIndex = -1;
            _isPublishing = false;
        }

        public void SetValue(string text)
        {
            _isPublishing = true;
            //selectKey(text);
            _isPublishing = false;
            _lastKey = null;
        }

        public void SetHint(string hint)
        {
            ToolTipService.SetToolTip(this, hint);
        }

        public void PreDataPublishXml(XElement xml)
        {

        }

        public void PublishXml(XElement xml)
        {
        }

        protected virtual void SetDeltaText(XElement xml)
        {
            if (_lastKey == null || _lastKey.Length == 0)
                xml.SetAttributeValue(Common.ValueIsNull, "1");
            else
                xml.SetAttributeValue(Common.PublishFieldValue, _lastKey);
        }
        #endregion

        #region IDataControl
        protected bool _isPublishing;
        
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
            DependencyProperty.Register("QueryId", typeof(string), typeof(DataGridEx), null);

        [Category("expanz")]
        [Description("Set the name of the Server method to call to get results to bind to.")]
        public string PopulateMethod
        {
            get { return (string)GetValue(PopulateMethodProperty); }
            set { SetValue(PopulateMethodProperty, value); }
        }

        public static readonly DependencyProperty PopulateMethodProperty =
            DependencyProperty.Register("PopulateMethod", typeof(string), typeof(DataGridEx), null);

        [Category("expanz")]
        [Description("Set the Server ModelObject to execute the PopulateMethod on.")]
        public string ModelObject
        {
            get { return (string)GetValue(ModelObjectProperty); }
            set { SetValue(ModelObjectProperty, value); }
        }

        public static readonly DependencyProperty ModelObjectProperty =
            DependencyProperty.Register("ModelObject", typeof(string), typeof(DataGridEx), null);        

        [Category("expanz")]
        [Description("Set to true to auto execute the PopulateMethod or the QueryID.")]
        public string AutoPopulate
        {
            get { return (string)GetValue(AutoPopulateProperty); }
            set { SetValue(AutoPopulateProperty, value); }
        }

        public static readonly DependencyProperty AutoPopulateProperty =
            DependencyProperty.Register("AutoPopulate", typeof(string), typeof(DataGridEx), null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dp"></param>
        public void FillServerRegistrationXml(XElement dp)
        {
        }

        /// <summary>
        /// function to publish data to control
        /// </summary>
        /// <param name="data"></param>
        //public void publishData(XElement data)
        //{
        //    publishing = true;
        //    dgView.publishData(data);
        //    lastKey = null;
        //    dgView.SelectedIndex = -1;
        //    publishing = false;
        //}
        #endregion

        #region Events
        public event EventHandler ItemDoubleClick; 
        #endregion
    }
}
