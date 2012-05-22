using System.Windows;
//using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using Expanz.ThinRIA.Core;

namespace Expanz.ThinRIA.Controls
{
    public class TopLevelMenuItem1 : ToggleButton //System.Windows.Controls.HyperlinkButton
    {
        public TopLevelMenuItem1() : base()
        {
            
        }

        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(TopLevelMenuItem), new PropertyMetadata(IsActivePropertyChanged));

        private static void IsActivePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
                VisualStateManager.GoToState(d as FrameworkElement, "ActiveLink", true);
            else
                VisualStateManager.GoToState(d as FrameworkElement, "InactiveLink", true);
        }

        protected override void OnClick()
        {
            base.OnClick();

            var menuItemData = this.DataContext as MenuItem;

            if (menuItemData != null)
            {
                if (menuItemData.IsCategory)
                {
                    // Need to create the context menu each time, as otherwise the positioning 
                    // of the context menu for the next time is incorrect (bug).
                    var menuItem = new System.Windows.Controls.ContextMenu();

                    foreach (MenuItem childMenuItemData in menuItemData)
                    {
                        var childMenuItem = new System.Windows.Controls.MenuItem();
                        childMenuItem.Header = childMenuItemData.Title;
                        childMenuItem.DataContext = childMenuItemData;
                        childMenuItem.Click += MenuItem_Click; // TODO: Change to command to avoid memory leak?
                        menuItem.Items.Add(childMenuItem);
                    }

                    menuItem.Closed += Menu_Closed;

                    int dropdownYOffset = GetDropdownYOffset();

#if WPF
                    menuItem.Placement = PlacementMode.Bottom;
                    menuItem.PlacementTarget = this;
                    menuItem.VerticalOffset = dropdownYOffset;
#else
                    GeneralTransform transform = this.TransformToVisual(null);
                    Point topLeftCorner = transform.Transform(new Point());

                    menuItem.HorizontalOffset = topLeftCorner.X + 1;
                    menuItem.VerticalOffset = topLeftCorner.Y + this.ActualHeight - 2 + dropdownYOffset;
#endif

                    menuItem.IsOpen = true;
                }
                else
                {
                    // Launch the activity
                    ApplicationEx.Instance.CreateActivityContainer(menuItemData.ActivityName, menuItemData.ActivityStyle, null, null);            
                }
            }
        }

        private int GetDropdownYOffset()
        {
            int yOffset = 0;

            DependencyObject parent = this;

            while (parent != null && !(parent is ApplicationNavigationMenu))
                parent = VisualTreeHelper.GetParent(parent);

            if (parent != null)
            {
                yOffset = ((ApplicationNavigationMenu)parent).DropdownYOffset;
            }

            return yOffset;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuitem = sender as System.Windows.Controls.MenuItem;
            var menuItemData = menuitem.DataContext as MenuItem;

            if (menuItemData.IsCategory)
            {
                // Not implemented to this level as yet
            }
            else
            {
                ApplicationEx.Instance.CreateActivityContainer(menuItemData.ActivityName, menuItemData.ActivityStyle, null, null);            
            }
        }

        private void Menu_Closed(object sender, RoutedEventArgs e)
        {
            //this.IsChecked = false;

            var menu = sender as System.Windows.Controls.ContextMenu;
            menu.Closed -= Menu_Closed;

            foreach (System.Windows.Controls.MenuItem menuItem in menu.Items)
            {
                //menuItem.Click -= MenuItem_Click; // Can't deregister here, or otherwise the Click event handler won't fire
            }

            //_openMenuButton = null;
        }
    }
}
