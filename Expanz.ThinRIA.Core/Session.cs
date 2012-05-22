using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Text;
using System.Windows.Input;
using System.Xml.Linq;
using Expanz.Extensions.BCL;
using Expanz.ThinRIA.ESAPortal;
using Expanz.ThinRIA.Security;
using Expanz.ThinRIA.Core;

namespace Expanz.ThinRIA
{
    public delegate void SessionCreatedDelegate(Session session);

    public partial class Session : INotifyPropertyChanged
    {
        #region Member Variables
        private bool _releasingSession = false;
        private string _sessionToken;
        private ServerApplicationService _serverApplicationService;
        #endregion

        #region Constructor
        public Session(ServerApplicationService serverApplicationService)
        {
            _serverApplicationService = serverApplicationService;
        }
        #endregion

        #region Public Properties
        public string SessionToken
        {
            get { return _sessionToken; }
            internal set
            {
                _sessionToken = value;

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("IsUserAuthenticated"));
                    PropertyChanged(this, new PropertyChangedEventArgs("Logout"));
                }
            }
        }

        public bool IsUserAuthenticated
        {
            get { return !SessionToken.IsNullOrEmpty(); }
        }

        public string UserName { get; internal set; }

        public bool ReleasingSession
        {
            get { return _releasingSession; }
            private set { _releasingSession = value; }
        }

        protected string blobCacheURL;
        public virtual string BLOBCacheURL
        {
            get
            {
                if (blobCacheURL != null)
                    return blobCacheURL;

                int p = _serverApplicationService.RemoteService.Endpoint.Address.Uri.AbsolutePath.LastIndexOf("/");
                return _serverApplicationService.RemoteService.Endpoint.Address.Uri.AbsolutePath.Substring(0, p) + @"blobcache/";
            }
        }
        #endregion

        #region Commands (plus supporting methods)
        public ICommand Logout
        {
            get
            {
                return new DelegateCommand(BeginLogout, CanLogout);
            }
        }

        private bool CanLogout(object param)
        {
            return this.IsUserAuthenticated;
        }

        public void BeginLogout(object param)
        {
            if (CanLogout(null))
                ReleaseSession();
        }
        #endregion

        #region Events / Delegates
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler LoggedOut;
        public event EventHandler<AuthenticationCompletedEventArgs> AuthenticationCompleted;
        public event SessionCreatedDelegate SessionCreated;
        #endregion

        #region Service Calls + Response Handlers
        public void GetSessionInfo()
        {
            XElement sendElt = new XElement(Common.Requests.SessionData);
            _serverApplicationService.SendRequestToServerPortal(sendElt, GetSessionInfoCallCompleted);
        }

        private void GetSessionInfoCallCompleted(bool success, XDocument response)
        {
            if (success)
            {
                if (response.Root.Attribute(Common.windowTitle) != null)
                    UserName = response.Root.Attribute(Common.windowTitle).Value;

                if (response.Root.Attribute("blobCacheURL") != null)
                {
                    blobCacheURL = response.Root.Attribute("blobCacheURL").Value;

                    if (!blobCacheURL.EndsWith(@"/"))
                        blobCacheURL += @"/";
                }

                try
                {
                    // Set the application Menu
                    ApplicationEx.Instance.LoadApplicationMenu(response.Root.Element(Common.MenuNode));
                }
                catch (Exception ex)
                {
                    Logging.LogException(ex);
                }

                ApplicationEx.Instance.LoadInitialActivity();
            }
        }

        /// <summary>
        /// This function is called to release the used session
        /// </summary>
        /// <returns></returns>
        public void ReleaseSession()
        {
            try
            {
                _releasingSession = true;
                IEnumerator<IActivityContainer> e = ApplicationEx.Instance.OpenActivities.Values.GetEnumerator();

                //while (e.MoveNext())
                //{
                //    e.Current.CloseOnLogout();
                //}
            }
            catch (Exception ex)
            {
                Logging.DebugException(ex, "Exception closing forms");
            }

            _releasingSession = false;

            _serverApplicationService.Request.Document.Root.RemoveAll();

            string msg = null;

            try
            {
                if (IsUserAuthenticated)
                {
                    _serverApplicationService.RemoteService.ReleaseSessionCompleted += new EventHandler<ESAPortal.ReleaseSessionCompletedEventArgs>(RemoteService_ReleaseSessionCompleted);
                    _serverApplicationService.RemoteService.ReleaseSessionAsync(SessionToken, msg);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void RemoteService_ReleaseSessionCompleted(object sender, ReleaseSessionCompletedEventArgs e)
        {
            _serverApplicationService.RemoteService.ReleaseSessionCompleted -= RemoteService_ReleaseSessionCompleted;

            ApplicationEx.Instance.HasOpenSession = false;

            SessionReleased(e);
        }

        internal void AuthenticationComplete(AuthenticationResult authResult)
        {
            if (authResult.Error != null)
            {
                if (authResult.Error is WebException)
                {
                    var wex = authResult.Error as WebException;
                    var errorMsg = string.Format("The server returned: {0}", wex.Message);

                    //var s = new StringBuilder("Data" + Environment.NewLine);
                    //IDictionaryEnumerator ie = wex.Data.GetEnumerator();

                    //while (ie.MoveNext())
                    //{
                    //    s.Append(string.Format("{0}={1}{2}", ie.Key, ie.Value, Environment.NewLine));
                    //}

                    //s.Append(string.Format("Status:{0}{1}", wex.Status, Environment.NewLine));
                    //s.Append(wex.ToString());

                    //ApplicationEx.Instance.DisplayMessageBox(s.ToString(), "Debug info");

                    var eArgs = new AuthenticationCompletedEventArgs(authResult.ErrorMessage, false);

                    if (AuthenticationCompleted != null)
                        AuthenticationCompleted(ApplicationEx.Instance.ActiveSession, eArgs);
                }
                else
                {
                    Logging.LogException(authResult.Error);
                }
            }
            else
            {
                // This method isn't called on the right instance of the Session object.  It works first time, but then
                // logging out and logging back in, the method is called again on the original Session object.  Getting
                // the ActiveSession from the ApplicationEx is a workaround for this problem.
                Session session = ApplicationEx.Instance.ActiveSession;

                session.SessionToken = authResult.Token;
                string reference = authResult.Token;

                bool isAuthenticated = !authResult.Token.IsNullOrEmpty();

                if (!isAuthenticated)
                    reference = authResult.ErrorMessage.IsNullOrEmpty() != true ? authResult.ErrorMessage : "Unable to Authenticate";

                var eArgs = new AuthenticationCompletedEventArgs(reference, isAuthenticated);

                if (AuthenticationCompleted != null)
                    AuthenticationCompleted(session, eArgs);

                if (eArgs.IsAuthenticated)
                {
                    if (SessionCreated != null)
                        SessionCreated(session);

                    session.GetSessionInfo();
                }
            }
        }
        #endregion

        #region Partial Method Declarations
        partial void SessionReleased(ReleaseSessionCompletedEventArgs e);
        #endregion
    }
}
