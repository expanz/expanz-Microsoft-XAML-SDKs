using System.Windows.Controls;
using System.Windows;
using System.Xml.Linq;

namespace Expanz.ThinRIA.Controls
{
    public class RadioButtonColumn : RadioButton
    {
        private int _contextId;
        private string _modelObject;
        private string _rowType;
        private ControlHarness _harness;

        internal ControlHarness Harness
        {
            get
            {
                if (_harness == null)
                {
                    // Use my ListView harness
                    if (DataContext is XElement)
                    {
                        XElement data = (XElement)this.DataContext;

                        if (data.Name == "Row")
                        {
                            int.TryParse(data.Attribute(Common.IDAttrib).Value, out _contextId);
                            _rowType = data.Attribute(Common.Data.RowType).Value;
                        }
                    }
                    //FrameworkElement fe = ControlHarness.BestParent(this);
                    //while (fe != null && !(fe is IDataControl))
                    //{
                    //    fe = ControlHarness.BestParent(fe);
                    //}
                    //if (fe != null)
                    //{
                    //    _harness = (ControlHarness)((IDataControl)fe).BindingHarness;
                    //    _modelObject = ((IDataControl)fe).ModelObject;
                    //}
                }

                return _harness;
            }
        }

        public string Field
        {
            get
            {
                return (string)GetValue(FieldProperty);
            }
            set
            {
                SetValue(FieldProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for Content.  
        public static readonly DependencyProperty FieldProperty =
        DependencyProperty.Register("Field", typeof(string), typeof(RadioButtonColumn), new PropertyMetadata(string.Empty));

        public RadioButtonColumn() : base()
        {
            this.MinWidth = 35;
            this.HorizontalContentAlignment = HorizontalAlignment.Right;
            this.Checked += new RoutedEventHandler(RadioButtonColumn_Checked);
        }

        private void RadioButtonColumn_Checked(object sender, RoutedEventArgs e)
        {
            Harness.SendXml(DeltaXml);
        }

        public XElement[] DeltaXml
        {
            get
            {
                XElement[] ret = new XElement[2];
                XElement context = Harness.ContextElement;
                context.SetAttributeValue(Common.IDAttrib, _contextId.ToString());
                  
                if (_rowType != null && _rowType.Length > 0)
                    context.SetAttributeValue(Common.Data.RowType, _rowType);

                string id = Field;

                if (_modelObject != null && _modelObject.Length > 0)
                {
                    context.SetAttributeValue(Common.contextObject, _modelObject);
                    id = _modelObject + "." + id;
                }

                ret[0] = context;
                XElement delta = Harness.DeltaElement;
                delta.SetAttributeValue(Common.IDAttrib, id);
                delta.SetAttributeValue(Common.PublishFieldValue, "fixme");
                ret[1] = delta;

                return ret;
            }
        }
    }
}