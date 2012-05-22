using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Xml.Linq;
using System.Windows;
using Expanz.Extensions.BCL;

namespace Expanz.ThinRIA
{
    public delegate void openContextMenuDelegate();
    public partial class ControlHarness
    {
        public delegate void MenuItemClick(object sender, RoutedEventArgs e);
        public ContextMenu createContextMenu(XElement menu, MenuItemClick delg)
        {
            ContextMenu m = new ContextMenu();
            string defaultAction = menu.GetAttribute(Common.DefaultAction);
            XElement MI = (XElement)menu.FirstNode;
            while (MI != null)
            {
                m.Items.Add(processMenuElement(MI, defaultAction, delg));
                MI = (XElement)MI.NextNode;
            }
            return m;
        }

        private object processMenuElement(XElement theElement, string theDefault, MenuItemClick delg)
        {
            if (theElement.Name == Common.MenuItem)
            {
                MenuItem theItem = new MenuItem();
                theItem.Header = theElement.GetAttribute("text");
                string action = theElement.GetAttribute(Common.MenuAction);
                theItem.Tag = action;
                if (action == theDefault)
                {
                    theItem.FontWeight = FontWeights.Bold;
                }
                theItem.Click += new RoutedEventHandler(delg);
                /*
                theElement.GetAttribute("clientAction")
                if (Common.boolValue(theElement.GetAttribute("checked")))
                    theItem.Checked = true;
                if (theItem.Action == theDefault)
                    theItem.DefaultItem = true;
                */
                return theItem;
            }
            else if (theElement.Name == Common.ProcessSeparator)
            {
                Separator s = new Separator();
                return s;
            }
            else//sub menu
            {
                MenuItem theItem = new MenuItem();
                theItem.Header = theElement.Attribute("name").Value;
                foreach(XElement child in theElement.DescendantNodes() )
                {
                    theItem.Items.Add(processMenuElement((XElement)child, theDefault, delg));
                }
                return theItem;
            }
        }
    }
}
