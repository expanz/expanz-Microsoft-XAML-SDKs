using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Expanz.ThinRIA.Controls;

namespace Expanz.ThinRIA.Behaviours
{
    public class ShowControl
    {
        public static FrameworkElement GetTargetControl(DependencyObject obj)
        {
            return (FrameworkElement)obj.GetValue(TargetControlProperty);
        }

        public static void SetTargetControl(DependencyObject obj, FrameworkElement value)
        {
            obj.SetValue(TargetControlProperty, value);
        }

        public static readonly DependencyProperty TargetControlProperty =
            DependencyProperty.RegisterAttached("TargetControl", typeof(FrameworkElement), typeof(ShowControl), new PropertyMetadata(OnTargetControlChanged));

        private static void OnTargetControlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Button)
            {
                Button button = d as Button;
                button.Click += new RoutedEventHandler(Button_Click);
            }
            else if (d is DataGridEx)
            {
                var dataGrid = d as DataGridEx;
                dataGrid.ItemDoubleClick += new EventHandler(DataGrid_Click);
            }
        }

        private static void Button_Click(object sender, RoutedEventArgs e)
        {
            HandleShowControl(sender as DependencyObject);
        }

        private static void DataGrid_Click(object sender, EventArgs e)
        {
            HandleShowControl(sender as DependencyObject);
        }
  
        private static void HandleShowControl(DependencyObject sourceControl)
        {
            FrameworkElement control = GetTargetControl(sourceControl);

            if (control != null)
            {
                control.Visibility = Visibility.Visible;

                Control firstControl = RecurseChildren<Control>(control).FirstOrDefault(x => x.GetType() != typeof(LabelEx) && x.GetType() != typeof(ContentControl));

                if (firstControl != null)
                    firstControl.Focus();
            }
        }

        public static IEnumerable<T> RecurseChildren<T>(DependencyObject root) where T : UIElement
        {
            if (root is T)
            {
                yield return root as T;
            }

            if (root != null)
            {
                var count = VisualTreeHelper.GetChildrenCount(root);

                for (var idx = 0; idx < count; idx++)
                {
                    foreach (var child in RecurseChildren<T>(VisualTreeHelper.GetChild(root, idx)))
                    {
                        yield return child;
                    }
                }
            }
        }     
    }
}
