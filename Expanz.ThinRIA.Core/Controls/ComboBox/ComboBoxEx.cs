using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Xml.Linq;
using Expanz.Extensions.BCL;
using Expanz.ThinRIA.Core;

namespace Expanz.ThinRIA.Controls
{
    public class ComboBoxEx : ComboBox, IServerBoundControl, IEditableControl, IRepeatingDataControl, IContainerSummaryText
    {
        #region Member Variables
        private Dictionary<string, ComboBoxItemEx> _dictionary;
        protected ControlHarness _controlHarness;
        protected string _populateQuery;
        protected string _populateMethod;
        protected string _autoPopulate;
        protected string _modelObject;
        private string _lastKey; 
        #endregion

        #region Constructor
        public ComboBoxEx() : base()
        {
#if !WPF
            this.Loaded += new RoutedEventHandler(ComboBoxEx_Loaded);
#endif
            this.SelectionChanged += new SelectionChangedEventHandler(ComboBoxEx_SelectionChanged);
        } 
        #endregion

        #region Event Handlers
        private void ComboBoxEx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedItem != null)
            {
                ComboBoxItemEx item = (ComboBoxItemEx)SelectedItem;

                if (_lastKey == null || item.ID != _lastKey)
                {
                    _lastKey = item.ID;

                    if (!publishing)
                        _controlHarness.SendXml(DeltaXml);
                }
            }
        }
        
#if WPF
        public override void EndInit()
        {
            base.EndInit();
            InitHarness();
        }
#else
        private void ComboBoxEx_Loaded(object sender, RoutedEventArgs e)
        {
            InitHarness();
        }
#endif
        #endregion

        #region Public Properties
        private string _nullEntryText;
        public string NullEntryText
        {
            set { _nullEntryText = value; }
            get 
            { 
                if (_nullEntryText == null) 
                    return "(none)"; 
                else 
                    return _nullEntryText; 
            }
        }

        private bool? _showNullEntry = null;
        public bool? ShowNullEntry
        {
            set { _showNullEntry = value; }
            get { return _showNullEntry; }
        }

        public string SummaryProperty { get; set; }

        public string SummaryText
        {
            get
            {
#if WPF
                if (SummaryProperty == null || SummaryProperty.Length == 0 || SummaryProperty == Common.None) return null;
                bool stringKey = false;
                double d=0;
                if (_lastKey != null && !double.TryParse(_lastKey, out d))
                {
                    stringKey = true;
                }
                if (SummaryProperty.ToLower() == "key" || (SummaryProperty.ToLower()=="auto" && stringKey))
                {
                    return _lastKey;
                }
                if (Text.Length == 0) return null;
                int max = 15;
                if (Text.Length < 16) max = Text.Length;
                return this.Text.Substring(0, max);
#else
                return null;
#endif
            }
        }

        public string DisplayTextColumn { get; set; }
        #endregion

        #region Public Methods
        public void InitHarness()
        {
            _controlHarness = new ControlHarness(this);
        }

        public virtual void PublishData(XElement data)
        {
            publishing = true;
            _lastKey = null;

            try
            {
                string selectedKey = null;

                if (SelectedItem != null && SelectedItem is ComboBoxItemEx)
                    selectedKey = ((ComboBoxItemEx)SelectedItem).ID; // Cache the previously selected key, to reapply after the list is refilled

                XElement ColumnDefintions = data.Element(Common.Data.Columns);
                XElement Rows = data.Element(Common.Data.Rows);
                this.Items.Clear();

                if (Rows == null || ColumnDefintions == null) 
                    return;

                string columnToUse = null;

                // Determine which column to display in the list
                if (!string.IsNullOrEmpty(DisplayTextColumn ?? DisplayMemberPath))
                {
                    // Use the column specified by the client
                    XElement displayColumn = ColumnDefintions.Elements(Common.Data.Column).Where(x => x.Attribute(Common.Data.ColumnField).Value == (DisplayTextColumn ?? DisplayMemberPath)).FirstOrDefault();

                    if (displayColumn != null)
                        columnToUse = displayColumn.GetAttributeValue(Common.IDAttrib);
                }
                else
                {
                    // Use the first *string* column that has a width
                    XNode columnNode = ColumnDefintions.FirstNode;

                    while (columnToUse == null && columnNode != null)
                    {
                        if (columnNode is XElement)
                        {
                            XElement element = (XElement)columnNode;
                            int theWidth = 0;

                            string dataType = element.GetAttributeValue(Common.Datatypes.AttribName);

                            if (element.Attribute(Common.Data.ColumnWidth) != null)
                                int.TryParse(element.Attribute(Common.Data.ColumnWidth).Value, out theWidth);

                            if (theWidth != 0 && dataType == Common.Datatypes.String)
                                columnToUse = element.Attribute(Common.IDAttrib).Value;
                        }

                        columnNode = columnNode.NextNode;
                    }
                }

                // Always add an empty or null entry
                if (_showNullEntry.HasValue && _showNullEntry.Value)
                {
                    ComboBoxItemEx nullItem = new ComboBoxItemEx("", Common.Null);
                    nullItem.Content = NullEntryText;
                    nullItem.Background = this.Background;
                    this.Items.Add(nullItem);
                }

                _dictionary = new Dictionary<string, ComboBoxItemEx>();
                XNode node = Rows.FirstNode;

                while (node != null)
                {
                    if (node is XElement)
                    {
                        XElement rowData = (XElement)node;
                        string id = rowData.Attribute(Common.IDAttrib).Value;
                        string type = rowData.GetAttributeValue(Common.Data.RowType);
                        ComboBoxItemEx item = new ComboBoxItemEx(id, type);
                        XElement displayColumn = rowData.Elements("Cell").Where(x => x.Attribute("id").Value == columnToUse).FirstOrDefault();

                        if (displayColumn != null)
                        {
                            item.Content = displayColumn.Value;

                            if (id.Length == 0)
                                id = displayColumn.Value;
                        }

                        this.Items.Add(item);
                        _dictionary.Add(item.ID.ToString(), item);
                    }

                    node = node.NextNode;
                }

                if (selectedKey != null)
                    SelectKey(selectedKey); // Select the previously selected key
            }
            finally
            {
                publishing = false;
            }
        }
        #endregion

        #region Private Methods
        private void SelectKey(string key)
        {
            if (_dictionary != null && _dictionary.ContainsKey(key))
                SelectedItem = _dictionary[key];
        }  
        #endregion

        #region Interface imlementations
        public void SetVisible(bool visible)
        {
            this.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public void SetEditable(bool e)
        {
            IsEnabled = e;
        }

        public void SetNull()
        {
            _lastKey = null;
            publishing = true;

            if (ShowNullEntry.HasValue && ShowNullEntry.Value && Items.Count != 0)
                SelectedIndex = 0;
            else
                SelectedIndex = -1;

            publishing = false;
        }

        public void SetValue(string text)
        {
            publishing = true;
            SelectKey(text);
            publishing = false;
        }

        public void SetHint(string hint)
        {
            ToolTipService.SetToolTip(this, hint);
        }

        public void PreDataPublishXml(XElement xml)
        {
            if (!ShowNullEntry.HasValue) // Only set if user hasn't specifically specified if they want a null entry
                ShowNullEntry = xml.GetAttributeValue<bool>(Common.PublishFieldNullable, true);
        }

        public void PublishXml(XElement xml)
        {
        }

        private string _fieldId;

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

        protected virtual void SetDeltaText(XElement xml)
        {
            if (_lastKey == null || _lastKey.Length == 0)
                xml.SetAttributeValue(Common.ValueIsNull, "1");
            else
                xml.SetAttributeValue(Common.PublishFieldValue, _lastKey);
        }
        #endregion

        #region IDataControl
        public void FillServerRegistrationXml(XElement dp)
        {
            dp.SetAttribute("queryMode", "single");
        }

        protected bool publishing;

        public string DataId
        {
            get { return SafeName ?? FieldId ?? ModelObject ?? QueryId ?? PopulateMethod; }
            //set { }
        }

        private string SafeName
        {
            get { return string.IsNullOrEmpty(Name) ? null : Name; }
        }

        public string QueryId
        {
            get { return _populateQuery; }
            set { _populateQuery = value; }
        }

        public string PopulateMethod
        {
            get { return _populateMethod; }
            set { _populateMethod = value; }
        }

        public string ModelObject
        {
            get { return _modelObject; }
            set { _modelObject = value; }
        }

        public string AutoPopulate
        {
            get { return _autoPopulate; }
            set { _autoPopulate = value; }
        }
        #endregion
    }
}
