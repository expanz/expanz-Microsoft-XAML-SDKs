using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Expanz.Extensions.BCL;
using Expanz.ThinRIA.Core;
using Microsoft.Phone.Controls;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace Expanz.ThinRIA.Controls
{
    public class ComboBoxEx : ListPicker, IServerBoundControl, IEditableControl, IRepeatingDataControl, IContainerSummaryText
    {
        #region Member Variables
        private ObservableCollection<ComboBoxItemDetails> _items = new ObservableCollection<ComboBoxItemDetails>();
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
            this.Loaded += new RoutedEventHandler(ComboBoxEx_Loaded);
            this.SelectionChanged += new SelectionChangedEventHandler(ComboBoxEx_SelectionChanged);

            this.DisplayMemberPath = "DisplayText";
        }
        #endregion

        #region Event Handlers
        private void ComboBoxEx_Loaded(object sender, RoutedEventArgs e)
        {
            InitHarness();

            // Bind the ItemsSource property to our collection of items
            this.ItemsSource = _items;
        }

        private void ComboBoxEx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedItem != null)
            {
                ComboBoxItemDetails item = (ComboBoxItemDetails)SelectedItem;

                if (_lastKey == null || item.ID != _lastKey)
                {
                    _lastKey = item.ID;

                    if (!publishing)
                        _controlHarness.SendXml(DeltaXml);
                }
            }
        }
        #endregion

        #region Public Properties
        public ControlHarness BindingHarness
        {
            get { return _controlHarness; }
        }

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

        protected string _SummaryProperty;
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

        public string SummaryText
        {
            get
            {
#if WPF
                if (_SummaryProperty == null || _SummaryProperty.Length == 0 || _SummaryProperty == Common.None) return null;
                bool stringKey = false;
                double d=0;
                if (_lastKey != null && !double.TryParse(_lastKey, out d))
                {
                    stringKey = true;
                }
                if (_SummaryProperty.ToLower() == "key" || (_SummaryProperty.ToLower()=="auto" && stringKey))
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
                XElement ColumnDefintions = data.Element(Common.Data.Columns);
                XElement Rows = data.Element(Common.Data.Rows);
                _items.Clear();

                if (Rows == null || ColumnDefintions == null)
                    return;

                string columnToUse = null;

                // Determine which column to display in the list
                if (!string.IsNullOrEmpty(DisplayTextColumn))
                {
                    // Use the column specified by the client
                    XElement displayColumn = ColumnDefintions.Elements(Common.Data.Column).Where(x => x.Attribute(Common.Data.ColumnField).Value == DisplayTextColumn).FirstOrDefault();

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
                if (wantNullEntry)
                {
                    var nullItem = new ComboBoxItemDetails("", NullEntryText, Common.Null);
                    _items.Add(nullItem);
                }

                XNode node = Rows.FirstNode;

                while (node != null)
                {
                    if (node is XElement)
                    {
                        XElement rowData = (XElement)node;
                        string id = rowData.Attribute(Common.IDAttrib).Value;
                        string type = rowData.GetAttributeValue(Common.Data.RowType);
                        string displayText = null;

                        XElement displayColumn = rowData.Elements("Cell").Where(x => x.Attribute("id").Value == columnToUse).FirstOrDefault();

                        if (displayColumn != null)
                        {
                            displayText = displayColumn.Value;

                            if (id.Length == 0)
                                id = displayColumn.Value;
                        }

                        var item = new ComboBoxItemDetails(id, displayText, type);
                        _items.Add(item);
                    }

                    node = node.NextNode;
                }
            }
            finally
            {
                publishing = false;
            }
        }

        internal void selectKey(string key)
        {
            SelectedItem = _items.FirstOrDefault(x => x.ID == key);
        }
        #endregion

        #region Interface imlementations
        public static readonly DependencyProperty IsValidProperty = DependencyProperty.Register(
            "IsValid",
            typeof(bool),
            typeof(ComboBoxEx), new PropertyMetadata(false));

        public bool IsValid
        {
            get { return (bool)GetValue(IsValidProperty); }
            set { SetValue(IsValidProperty, value); }
        }

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

            if (WantNullEntry && Items.Count != 0)
                SelectedIndex = 0;
            else
                SelectedIndex = -1;

            publishing = false;
        }

        public void SetValue(string text)
        {
            publishing = true;
            selectKey(text);
            publishing = false;
        }

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
            DependencyProperty.Register("LabelText", typeof(string), typeof(ComboBoxEx),
                                        new PropertyMetadata(string.Empty));

        public void SetHint(string hint)
        {
        }

        public void PublishXml(XElement xml)
        {
            if (xml.Attribute("valid") != null)
                IsValid = Common.boolValue(xml.Attribute("valid").Value);
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

        #region IRepeatingDataControl
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

        public void PreDataPublishXml(XElement fieldElement)
        {

        }
        #endregion
    }

    public class ComboBoxItemDetails
    {
        #region Constructor
        public ComboBoxItemDetails(string id, string displayText, string type)
        {
            ID = id;
            DisplayText = displayText;
            Type = type;
        } 
        #endregion

        #region Public Properties
        public string ID { get; private set; }
        public string DisplayText { get; private set; }
        public string Type { get; private set; } 
        #endregion
    }
}