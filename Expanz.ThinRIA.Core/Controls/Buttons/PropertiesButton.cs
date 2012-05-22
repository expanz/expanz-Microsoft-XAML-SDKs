using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace Expanz.ThinRIA.Controls
{
    public class PropertiesButton : ButtonEx
    {
        public int ContextId
        {
            get { return (int)GetValue(ContextIdProperty); }
            set { SetValue(ContextIdProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ContextId.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContextIdProperty =
            DependencyProperty.Register("ContextId", typeof(int), typeof(PropertiesButton), new PropertyMetadata(string.Empty));

        public string ContextType
        {
            get { return (string)GetValue(ContextTypeProperty); }
            set { SetValue(ContextTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ContextType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContextTypeProperty =
            DependencyProperty.Register("ContextType", typeof(string), typeof(PropertiesButton), new PropertyMetadata(string.Empty));


        public PropertiesButton()
            : base()
        {
        }

        protected override void ButtonEx_Click(object sender, RoutedEventArgs e)
        {
            var elementList = new XElement[2];

            var contextElement = createRequestElement(Common.Requests.SetContext);
            contextElement.SetAttributeValue(Common.IDAttrib, ContextId.ToString());
            contextElement.SetAttributeValue(Common.Data.RowType, ContextType);
            var actionElement = _controlHarness.MethodElement;

            var method = MethodName ?? "menuActionProperties";

            actionElement.SetAttributeValue(Common.RequestMethodName, method);

            if (!string.IsNullOrEmpty(ModelObject))
            {
                actionElement.SetAttributeValue(Common.contextObject, ModelObject);
            }
            elementList[0] = contextElement;
            elementList[1] = actionElement;
            _controlHarness.SendXml(elementList);
        }

        private XElement createRequestElement(string elementName)
        {
            var returnElement = new XElement(elementName);
            if (!string.IsNullOrEmpty(ModelObject))
            {
                returnElement.SetAttributeValue(Common.contextObject, ModelObject);
            }
            return returnElement;
        }

    }
}
