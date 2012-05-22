using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using System.Windows.Data;

namespace Expanz.ThinRIA.Controls
{
    public partial class TextBoxEx : TextBox
    {
        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (IsSearchField && e.Key == Key.F3)
            {
                _preventSendXml = true;
                BindingExpression expression = this.GetBindingExpression(TextBox.TextProperty);
                expression.UpdateSource();
                _preventSendXml = false;

                SendXmlAndTextMatch(Keyboard.Modifiers == ModifierKeys.Shift);
            }
        }

        public void SendXmlAndTextMatch(bool matchAll)
        {
            XElement[] elementList = new XElement[2];

            elementList[0] = DeltaXml;
            elementList[0].SetAttributeValue(Common.BypassAKProcessing, "1");

            elementList[1] = new XElement(Common.Requests.MenuAction);

            if (matchAll)
                elementList[1].SetAttributeValue(Common.MenuAction, Common.MENUACTION_TextMatchAll);
            else
                elementList[1].SetAttributeValue(Common.MenuAction, Common.MENUACTION_TextMatch);
            
            if (this.FieldId.IndexOf(".") >= 0)
                elementList[1].SetAttributeValue(Common.referenceObject, FieldId.Substring(0, FieldId.LastIndexOf(".")));

            _controlHarness.SendXml(elementList);
        }
    }
}
