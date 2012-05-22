using System;
using System.Xml.Linq;

using Expanz.Extensions.BCL;

namespace Expanz.ThinRIA.Security
{
    internal partial class ExpanzAuthentication : IAuthentication
    {
        private const string CreateSessionXmlMessage = @"<ESA><CreateSession user='' password='' clientType='' appSite='' authDomain='' station='' schemaVersion='2.0' clientVersion='1.2'/></ESA>";

        private readonly XDocument CreateSessionXDoc = XDocument.Parse(CreateSessionXmlMessage);

        public ClientType ClientType { get; set; }
        public string Site { get; set; }
        public string ClientHost { get; set; }
        public AuthenticationMode Mode { get; set; }

        public ExpanzAuthentication(ClientType clientType, string site)
        {
            ClientType = clientType;
            Site = site;
        }

        public ExpanzAuthentication(ClientType clientType, string site, string workstation, AuthenticationMode authMode) :
            this(clientType, site)
        {
            ClientHost = workstation;
            Mode = authMode;
        }

        public void Authenticate(string userName, string password, Action<AuthenticationResult> authenticationComplete)
        {
           string authXmlMsg = BuildAuthenticationMessage(userName, password);
           using (var proxy = ESAServiceProxy.Instance)
           {
                proxy.CreateAuthenticationSession(authXmlMsg, (token, errorMsg, ex) =>
                                                                  {
                                                                      if (ex != null)
                                                                      {
                                                                          authenticationComplete(
                                                                              new AuthenticationResult(ex));
                                                                      }
                                                                      var authResult = !errorMsg.IsNullOrEmpty()
                                                                                                            ? new AuthenticationResult(null,
                                                                                                                                       errorMsg)
                                                                                                            : new AuthenticationResult(
                                                                                                                  token, null);

                                                                      authenticationComplete(authResult);
                                                                  });
            }
        }

        private string BuildAuthenticationMessage(string userName, string password)
        {
            var createSessionNode = CreateSessionXDoc.Root.FirstNode as XElement;
            createSessionNode.SetAttributeValue("user", userName);
            createSessionNode.SetAttributeValue("password", password);
            createSessionNode.SetAttributeValue("clientType", ClientType.ToString().ToUpper());
            createSessionNode.SetAttributeValue("appSite", Site);
            createSessionNode.SetAttributeValue("station", ClientHost);
            createSessionNode.SetAttributeValue("authMode", Mode.ToString().ToUpper());

            return CreateSessionXDoc.ToString();
        }
    }
}