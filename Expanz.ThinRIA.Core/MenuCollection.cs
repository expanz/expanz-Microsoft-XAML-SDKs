using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using System.ComponentModel;
using Expanz.Extensions.BCL;

namespace Expanz.ThinRIA.Core
{
    public class MenuCollection : ObservableCollection<MenuItem>
    {
        private MenuItem _selectedItem;

        public void LoadMenu(System.Xml.Linq.XElement data)
        {
            this.Clear();

            foreach (XElement item in data.Elements())
            {
                CreateMenuItem(item, this);
            }
        }

        private void CreateMenuItem(XElement item, ObservableCollection<MenuItem> parentItem)
        {
            MenuItem menuItem = null;

            if (item.Name == "ProcessArea")
            {
                menuItem = new ProcessAreaItem();
                menuItem.Title = item.GetAttributeValue("title");
                parentItem.Add(menuItem);

                foreach (XElement child in item.Elements())
                {
                    CreateMenuItem(child, menuItem);
                }
            }
            else if (item.Name == "Activity")
            {
                menuItem = new ActivityItem();
                menuItem.Title = item.GetAttributeValue("title");
                menuItem.ActivityName = item.GetAttributeValue("name");
                menuItem.ActivityStyle = item.GetAttributeValue("style");
                parentItem.Add(menuItem);
            }
        }

        public MenuItem SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                if (_selectedItem != null)
                    _selectedItem.IsSelected = false;

                _selectedItem = value;

                if (_selectedItem != null)
                    _selectedItem.IsSelected = true;
            }
        }
    }

    public class ProcessAreaItem : MenuItem
    {
        public ProcessAreaItem()
        {
            IsCategory = true;
        }
    }

    public class ActivityItem : MenuItem
    {

    }

    public class MenuItem : ObservableCollection<MenuItem>
    {
        private bool _isSelected = false;

        public string Title { get; set; }
        public string Name { get; set; }
        public string ActivityName { get; set; }
        public string ActivityStyle { get; set; }
        public string Label { get; set; }

        public bool IsSelected 
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged(new PropertyChangedEventArgs("IsSelected"));
            }
        }

        public bool Enabled { get; set; }
        public bool IsCategory { get; protected set; }
        public Image Image { get; set; }

        public ObservableCollection<MenuItem> Items
        {
            get { return this; }
        }
    }
}