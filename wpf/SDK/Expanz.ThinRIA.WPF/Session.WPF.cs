using System;
using Expanz.ThinRIA.ESAPortal;

namespace Expanz.ThinRIA
{
    public partial class Session
    {
        partial void SessionReleased(ReleaseSessionCompletedEventArgs e)
        {
            try
            {
                SessionToken = null;

                ApplicationEx.Instance.ApplicationMenu.Clear();

                // Go back to the starting view of the app
                if (ApplicationEx.Instance.ActivityHostFrame != null && ApplicationEx.Instance.LoginViewUri != null)
                    ApplicationEx.Instance.ActivityHostFrame.Navigate(ApplicationEx.Instance.LoginViewUri);

                if (e.Error != null)
                {
                    ////MessageBox.Show(e.Error.Message);
                    return;
                }

                if (LoggedOut != null)
                    LoggedOut(this, new EventArgs());
            }
            finally
            {
            }
        }
    }
}
