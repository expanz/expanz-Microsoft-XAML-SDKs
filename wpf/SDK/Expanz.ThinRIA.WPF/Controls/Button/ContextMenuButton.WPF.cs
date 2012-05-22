using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Expanz.ThinRIA.Core;
using System.ComponentModel;
using Expanz.Extensions.BCL;
using System.Windows.Controls;
using System.Windows;

namespace Expanz.ThinRIA.Controls
{
    public partial class ContextMenuButton
    {
        partial void PublishContextMenuInt(XElement menu)
        {
            ContextMenu m = this._controlHarness.createContextMenu(menu, theItem_Click);
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
    }
}
