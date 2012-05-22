using System.ComponentModel;
using System.Windows;
using Expanz.ThinRIA.Core;
using Microsoft.Phone.Controls;
using System.Collections;

namespace Expanz.ThinRIA.Controls
{
    public class PivotMenu : Pivot
    {
        #region Constructor
        public PivotMenu()
            : base()
        {
            this.DefaultStyleKey = typeof(PivotMenu);

            this.Loaded += new RoutedEventHandler(PivotMenu_Loaded);
        } 
        #endregion

        #region Public Properties
        public DataTemplate MenuItemTemplate
        {
            get { return (DataTemplate)GetValue(MenuItemTemplateProperty); }
            set { SetValue(MenuItemTemplateProperty, value); }
        }

        public static readonly DependencyProperty MenuItemTemplateProperty =
            DependencyProperty.Register("MenuItemTemplate", typeof(DataTemplate), typeof(PivotMenu), null); 
        #endregion

        #region Control Event Handlers
        private void PivotMenu_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.IsInDesignTool)
            {
                // Some design-time data
                var processMenu = new ProcessAreaItem();
                processMenu.Title = "Process Area 1";

                for (int index = 1; index < 6; index++)
                {
                    var activityItem = new ActivityItem();
                    activityItem.Title = "Activity " + index.ToString();
                    processMenu.Add(activityItem);
                }

                MenuCollection menuCollection = new MenuCollection();
                menuCollection.Add(processMenu);

                processMenu = new ProcessAreaItem();
                processMenu.Title = "Process Area 2";

                for (int index = 1; index < 6; index++)
                {
                    var activityItem = new ActivityItem();
                    activityItem.Title = "Activity " + index.ToString();
                    processMenu.Add(activityItem);
                }

                menuCollection.Add(processMenu);
                
                this.ItemsSource = menuCollection;
            }
            else
            {
                MenuCollection menu = ApplicationEx.Instance.ApplicationMenu;

                if (ShouldMakeSingleLevelMenu(menu))
                    menu = MakeSingleLevelMenu(menu);

                this.ItemsSource = menu;
            }
        }

        private bool ShouldMakeSingleLevelMenu(MenuCollection menuCollection)
        {
            // If every category has only 1 item, then merge all the items
            // into a single level menu
            bool useSingleLevelMenu = true;

            foreach (var menuItem in menuCollection)
            {
                if (menuItem.Count > 1 || menuItem[0].Count != 0 || menuItem[0].IsCategory)
                {
                    useSingleLevelMenu = false;
                    break;
                }
            }

            return useSingleLevelMenu;
        }

        private MenuCollection MakeSingleLevelMenu(MenuCollection menuCollection)
        {
            MenuCollection newMenu = new MenuCollection();

            var mainMenuItem = new Expanz.ThinRIA.Core.MenuItem();
            mainMenuItem.Title = "main menu";

            newMenu.Add(mainMenuItem);

            foreach (var menuItem in menuCollection)
            {
                mainMenuItem.Add(menuItem[0]);
            }

            return newMenu;
        }
        #endregion
    }
}
