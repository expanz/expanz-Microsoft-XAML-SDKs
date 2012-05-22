using System.Windows;
using System.Windows.Controls;

namespace Expanz.ThinRIA.Controls
{
    public partial class LoginForm : Control
    {
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            BeginLogin(null);
        }
    }
}
