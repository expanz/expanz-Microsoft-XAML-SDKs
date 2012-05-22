using System.Windows.Controls;
using Expanz.ThinRIA.Core;
using System.Windows;
using System.Windows.Media;

namespace Expanz.ThinRIA.Controls
{
    public class MenuList : ListBox
    {
        public MenuList() : base()
        {
            this.DefaultStyleKey = typeof(MenuList);
            this.SelectionChanged += new SelectionChangedEventHandler(MenuList_SelectionChanged);
            
            
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // We can't bind to the parent PivotMenu's MenuItemTemplate property (that we defined)
            // so we'll need to set it manually (if defined on PivotMenu).
            DataTemplate menuItemTemplate = GetItemTemplate();

            if (menuItemTemplate != null)
                this.ItemTemplate = menuItemTemplate;
        }

        private DataTemplate GetItemTemplate()
        {
            DependencyObject parent = this;

            while (parent != null && !(parent is PivotMenu))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            PivotMenu pivotMenu = parent as PivotMenu;

            return (pivotMenu == null) ? null : pivotMenu.MenuItemTemplate;
        }

        private void MenuList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedItem != null)
            {
                MenuItem menuItem = SelectedItem as MenuItem;

                if (menuItem.IsCategory)
                {
                    // TODO: Don't have a drill down capability yet
                }
                else
                {
                    ApplicationEx.Instance.CreateActivityContainer(menuItem.ActivityName, menuItem.ActivityStyle, null, null);
                }

                this.SelectedItem = null;
            }
        }
    }
}
