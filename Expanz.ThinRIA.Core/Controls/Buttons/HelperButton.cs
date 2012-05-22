using System.Windows;
using System.Windows.Controls;

namespace Expanz.ThinRIA.Controls
{
    public class HelperButton : Button
    {
        private const string masterElement = "MasterUIElement";
        public static readonly DependencyProperty MasterUIElementProperty =
            DependencyProperty.Register(masterElement, typeof(UIElement), typeof(HelperButton),
            new PropertyMetadata(new PropertyChangedCallback(OnMasterUIElementChanged)));

        private static void OnMasterUIElementChanged(DependencyObject dependencyObj, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObj is HelperButton && e.NewValue is UIElement)
            {
                //	((HelperButton)o).myCard = (UIElement)e.NewValue;
            }
        }

        public UIElement MasterUIElement
        {
            get { return (UIElement)GetValue(MasterUIElementProperty); }
            set { SetValue(MasterUIElementProperty, value); }
        }
    }
}
