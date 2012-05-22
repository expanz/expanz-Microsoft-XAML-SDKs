using System.Windows;
using System.Windows.Controls;
using Expanz.ThinRIA.Core;

namespace Expanz.ThinRIA.Controls
{
    public class CloseButton : Button
    {
        public CloseButton() : base()
        {
            Click += CloseButton_Click;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
#if SILVERLIGHT
            IActivityContainer activityContainer = ControlHarness.FindParentActivityContainer(this);

            if (activityContainer != null)
            {
                // For now, just support closing child windows, can be expanded later
                if (activityContainer is ChildWindow)
                {
                    // Probably should incorporate a dirty check at some stage, and notify the user if true?
                    var window = activityContainer as ChildWindow;
                    window.Close();
                }
            }
#elif WPF
            IActivityContainer activityContainer = ControlHarness.FindParentActivityContainer(this);

            if (activityContainer != null)
            {
                // For now, just support closing child windows, can be expanded later
                if (activityContainer is Window)
                {
                    // Probably should incorporate a dirty check at some stage, and notify the user if true?
                    var window = activityContainer as Window;
                    window.Close();
                }
            }
#endif
        }
    }
}
