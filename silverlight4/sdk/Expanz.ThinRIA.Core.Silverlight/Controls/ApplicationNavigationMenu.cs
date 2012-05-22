using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Expanz.ThinRIA.Core;

namespace Expanz.ThinRIA.Controls
{
    public class ApplicationNavigationMenu : ItemsControl
    {
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
            this.ItemsSource = ApplicationEx.Instance.ApplicationMenu;
        }

        private void ApplicationMenu_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (ApplicationEx.Instance.ApplicationMenu.Count == 1)
            {
                // Since there's only one top level item, show its children instead as the top level items
                this.ItemsSource = ApplicationEx.Instance.ApplicationMenu[0];
            }
            else
            {
                this.ItemsSource = ApplicationEx.Instance.ApplicationMenu;
            }
        } 

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (SelectedMenuItem != null)
            {
                SelectedMenuItem.IsSelected = false;
            }

            //SelectedMenuItem = 
            //string uri = e.Uri.ToString();
        }
        #endregion

        #region Public Properties
        public Expanz.ThinRIA.Core.MenuItem SelectedMenuItem { get; protected set; }
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
