using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Expanz.ThinRIA.Core;

namespace Expanz.ThinRIA.Controls
{
    /// <summary>
    /// Renders a basic Windows Menu from a Process Map
    /// </summary>
    public class ApplicationNavigationMenu : ItemsControl
    {
        #region Member Variables
        private ObservableCollection<Expanz.ThinRIA.Core.MenuItem> _menuItems = new ObservableCollection<Expanz.ThinRIA.Core.MenuItem>(); 
        #endregion

        #region Constructor
        public ApplicationNavigationMenu() : base()
        {
            this.Loaded += new RoutedEventHandler(ApplicationNavigationMenu_Loaded);
            this.DefaultStyleKey = typeof(ApplicationNavigationMenu);

            ApplicationEx.Instance.ApplicationMenu.CollectionChanged += ApplicationMenu_CollectionChanged;
        }
        #endregion

        #region Control Event Handlers
        private void ApplicationNavigationMenu_Loaded(object sender, RoutedEventArgs e)
        {
            this.ItemsSource = _menuItems;
        }

        private void ApplicationMenu_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Strange behaviour occurs for WPF control when changing items source (as per the previously simple method implementation), 
            // leading to a duplicate menu item being created. Therefore, have had to put this more complex implementation in place.
            if (ApplicationEx.Instance.ApplicationMenu.Count == 1)
            {
                // Since there's only one top level item, show its children instead as the top level items
                //this.ItemsSource = ApplicationEx.Instance.ApplicationMenu[0];
                _menuItems.Clear();
                _menuItems.Add(ApplicationEx.Instance.ApplicationMenu[0]);
            }
            else
            {
                //this.ItemsSource = ApplicationEx.Instance.ApplicationMenu;

                if (_menuItems.Count == 1 || e.Action != NotifyCollectionChangedAction.Add)
                {
                    _menuItems.Clear();

                    foreach (var menuItem in ApplicationEx.Instance.ApplicationMenu)
                        _menuItems.Add(menuItem);
                }
                else
                {
                    foreach (var menuItem in e.NewItems)
                    _menuItems.Add(menuItem as Expanz.ThinRIA.Core.MenuItem);
                }
            }
        } 

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            //if (SelectedMenuItem != null)
            //{
            //    SelectedMenuItem.IsSelected = false;
            //}

            //SelectedMenuItem = 
            //string uri = e.Uri.ToString();
        }
        #endregion

        #region Public Properties
        public int DropdownYOffset { get; set; }

        public Frame ContentFrame
        {
            get { return (Frame)GetValue(ContentFrameProperty); }
            set { SetValue(ContentFrameProperty, value); }
        }

        public static readonly DependencyProperty ContentFrameProperty =
            DependencyProperty.Register("ContentFrame", typeof(Frame), typeof(ApplicationNavigationMenu), new PropertyMetadata(ContentFramePropertyChanged));

        private static void ContentFramePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var navMenu = d as ApplicationNavigationMenu;

            if (e.OldValue != null)
            {
                var oldFrame = e.NewValue as Frame;
                oldFrame.Navigated -= navMenu.ContentFrame_Navigated;
            }

            var frame = e.NewValue as Frame;

            if (frame != null)
            {
                frame.Navigated += navMenu.ContentFrame_Navigated;
            }
        } 
        #endregion
    }
}
