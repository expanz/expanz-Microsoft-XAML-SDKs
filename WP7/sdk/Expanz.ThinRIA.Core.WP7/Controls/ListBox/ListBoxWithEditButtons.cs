using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Expanz.ThinRIA.Controls
{
    public class ListBoxWithEditButtons : Control
    {
        public ListBoxWithEditButtons()
        {
            this.DefaultStyleKey = typeof(ListBoxWithEditButtons);
        }

        [Category("expanz")]
        [Description("Set the Server ModelObject to execute the PopulateMethod on.")]
        public string ModelObject
        {
            get { return (string)GetValue(ModelObjectProperty); }
            set { SetValue(ModelObjectProperty, value); }
        }

        public static readonly DependencyProperty ModelObjectProperty =
            DependencyProperty.Register("ModelObject", typeof(string), typeof(ListBoxWithEditButtons), null);


        [Category("expanz")]
        [Description("Set the name of the Server method to call to get results to bind to.")]
        public string PopulateMethod
        {
            get { return (string)GetValue(PopulateMethodProperty); }
            set { SetValue(PopulateMethodProperty, value); }
        }

        public static readonly DependencyProperty PopulateMethodProperty =
            DependencyProperty.Register("PopulateMethod", typeof(string), typeof(ListBoxWithEditButtons), null);


        [Category("expanz")]
        [Description("Set the ID of the Server Query to execute and bind the results to.")]
        public string QueryId
        {
            get { return (string)GetValue(QueryIdProperty); }
            set { SetValue(QueryIdProperty, value); }
        }

        public static readonly DependencyProperty QueryIdProperty =
            DependencyProperty.Register("QueryId", typeof(string), typeof(ListBoxWithEditButtons), null);


        public FrameworkElement EditControl
        {
            get { return (FrameworkElement)GetValue(EditControlProperty); }
            set { SetValue(EditControlProperty, value); }
        }

        public static readonly DependencyProperty EditControlProperty =
            DependencyProperty.Register("EditControl", typeof(FrameworkElement), typeof(ListBoxWithEditButtons), null);

        [Category("expanz")]
        [Description("Set to true to auto execute the PopulateMethod or the QueryID.")]
        public string AutoPopulate
        {
            get { return (string)GetValue(AutoPopulateProperty); }
            set { SetValue(AutoPopulateProperty, value); }
        }

        public static readonly DependencyProperty AutoPopulateProperty =
            DependencyProperty.Register("AutoPopulate", typeof(string), typeof(ListBoxWithEditButtons), null);

        [Category("expanz")]
        [Description("Sets the current context on the server to the selected item when it's selected.")]
        public bool SetContextOnSelect
        {
            get { return (bool)GetValue(SetContextOnSelectProperty); }
            set { SetValue(SetContextOnSelectProperty, value); }
        }

        public static readonly DependencyProperty SetContextOnSelectProperty =
            DependencyProperty.Register("SetContextOnSelect", typeof(bool), typeof(ListBoxWithEditButtons), null);

        [Category("expanz")]
        [Description("Specifies what action should take place when an item is selected.")]
        public string SelectAction
        {
            get { return (string)GetValue(SelectActionProperty); }
            set { SetValue(SelectActionProperty, value); }
        }

        public static readonly DependencyProperty SelectActionProperty =
            DependencyProperty.Register("SelectAction", typeof(string), typeof(ListBoxWithEditButtons), null);


        [Category("expanz")]
        [Description("Sets the current context on the server to the selected item,")]
        public bool SetContextOnDoubleClick
        {
            get { return (bool)GetValue(SetContextOnDoubleClickProperty); }
            set { SetValue(SetContextOnDoubleClickProperty, value); }
        }

        public static readonly DependencyProperty SetContextOnDoubleClickProperty =
            DependencyProperty.Register("SetContextOnDoubleClick", typeof(bool), typeof(ListBoxWithEditButtons), null);

        [Category("expanz")]
        [Description("Specifies what action should take place when an item is double clicked.")]
        public string DoubleClickAction
        {
            get { return (string)GetValue(DoubleClickActionProperty); }
            set { SetValue(DoubleClickActionProperty, value); }
        }

        public static readonly DependencyProperty DoubleClickActionProperty =
            DependencyProperty.Register("DoubleClickAction", typeof(string), typeof(ListBoxWithEditButtons), null);
        

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(ListBoxWithEditButtons), null);
    }

    /// <summary>
    /// Used just to prevent implicit styles in the consuming view from
    /// affecting the button controls in this control
    /// </summary>
    public class ListBoxEditButton : ButtonEx
    {

    }
}
