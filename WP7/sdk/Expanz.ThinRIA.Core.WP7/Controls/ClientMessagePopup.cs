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
using System.Windows.Controls.Primitives;
using System.Xml.Linq;

namespace Expanz.ThinRIA.Controls
{
    public class ClientMessagePopup
    {
        private readonly Popup popup;

        public XDocument Result { get; private set; }
        public ClientMessagePopup(XElement myXml)
        {
            popup = new Popup();
            Result = new XDocument();
            popup.Child = new ClientMessageControl(myXml);
            popup.VerticalAlignment = VerticalAlignment.Center;
            popup.HorizontalAlignment = HorizontalAlignment.Center;
        }

        public void Show()
        {
            popup.IsOpen = true;
        }
    }
}
