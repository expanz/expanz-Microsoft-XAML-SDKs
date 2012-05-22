using System.Windows;
using System.Windows.Controls;

namespace Expanz.ThinRIA.Behaviours
{
    public class HideControl
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
            DependencyProperty.RegisterAttached("TargetControl", typeof(FrameworkElement), typeof(HideControl), new PropertyMetadata(OnTargetControlChanged));

        private static void OnTargetControlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Button)
            {
                Button button = d as Button;
                button.Click += new RoutedEventHandler(button_Click);
            }
        }

        private static void button_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement control = GetTargetControl(sender as DependencyObject);

            if (control != null)
                control.Visibility = Visibility.Collapsed;
        }
    }
}
