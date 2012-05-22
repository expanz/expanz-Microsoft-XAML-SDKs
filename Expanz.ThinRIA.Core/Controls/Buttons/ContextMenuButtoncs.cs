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
using Expanz.ThinRIA.Core;
using System.ComponentModel;
using Expanz.Extensions.BCL;


namespace Expanz.ThinRIA.Controls
{
    public partial class ContextMenuButton : ButtonEx, IContextMenuPublisher
    {
        public ContextMenuButton()
        {
        }

        #region Variable

        private string referenceObj;

        [Category("expanz")]
        [Description("Set the ModelObject to call the MethodName property on. This is only required if there is more than one ModelObject in the Activity")]
        public string ReferenceObject
        {
            get { return referenceObj ?? (referenceObj = string.Empty); }
            set { referenceObj = value; }
        }

        #endregion

        #region Helper Functions


        public void PublishContextMenu(XElement menu)
        {
            PublishContextMenuInt(menu);
        }
    
        partial void PublishContextMenuInt(XElement menu);

        protected override XElement CreateRequestElement(string elementName)
        {
            var returnElement = base.CreateRequestElement(elementName);
            if (!string.IsNullOrEmpty(ReferenceObject))
            {
                returnElement.SetAttributeValue(Common.referenceObject, ReferenceObject);
            }
            return returnElement;
        }
        protected override void ComposeMethodXml(XElement xml)
        {
            base.ComposeMethodXml(xml);
            if (!string.IsNullOrEmpty(ReferenceObject))
            {
                xml.SetAttributeValue(Common.referenceObject, ReferenceObject);
            }
        }

        #endregion

        #region Event

        protected override void ButtonEx_Click(object sender, RoutedEventArgs e)
        {
            ActivityHarness.ContextMenuPublisher = this;
            base.ButtonEx_Click(sender, e);
        }


        #endregion
    }
}
