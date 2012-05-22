namespace Expanz.ThinRIA
{
    using System;

    using ESAPortal;

    internal class ESAServiceProxy : Disposable
    {
        private static ESAServiceClient proxy;
        private bool handlerRegistered;

        private static ESAServiceProxy _instance;

        private ESAServiceProxy()
        {
            if (ApplicationEx.Instance != null)
                proxy = ApplicationEx.Instance.ServerApplicationService.RemoteService;
        }

        public static ESAServiceProxy Instance
        {
            get
            {
                if (_instance == null)
                {
                    if (_instance == null)
                    {
                        _instance = new ESAServiceProxy();
                    }
                }

                return _instance;
            }
        }

        public void CreateAuthenticationSession(string authXmlMsg, Action<string, string, Exception> authenticationComplete)
        {
            proxy.CreateSessionAsync(authXmlMsg, null);

            if (!handlerRegistered)
            {
                handlerRegistered = true;

                proxy.CreateSessionCompleted += (s, e) =>
                                                          {
                                                              Exception ex = e.Error;
                                                              authenticationComplete(e.Result, e.errorMessage, ex);
                                                          };
            }
        }

        public void CreateList(string xml,Action<string, string, Exception> authenticationComplete)
        {
            proxy.ExecAnonymousAsync(xml,"","");

            proxy.ExecCompleted+=(s,e)=>
            {
                Exception ex = e.Error;
                authenticationComplete(e.Result, e.errors, ex);
            };
        }
    }
}