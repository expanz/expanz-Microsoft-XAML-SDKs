namespace Expanz.ThinRIA.Security
{
    using System;

    internal partial interface IAuthentication
    {
        /// <summary>
        /// Gets or sets client application type.
        /// </summary>
        ClientType ClientType { get; set; }

        /// <summary>
        /// Gets or sets Site name. Required by the Authentication message.
        /// </summary>
        string Site { get; set; }
        
        /// <summary>
        /// Gets or sets ClientHost. Usually this is the Workstatio name or device name that's
        /// currently hosting the client application communicating with the ESA Web Service
        /// </summary>
        string ClientHost { get; set; }

        /// <summary>
        /// Gets or sets AuthenticationMode
        /// </summary>
        AuthenticationMode Mode { get; set; }

        /// <summary>
        /// When implemented Authenticate given credentials agains the ESA web service.
        /// This method builds the Authentication Xml Message and invokes CreateSession service
        /// operation on ESA web service.
        /// </summary>
        /// <param name="userName">Username to be authenticated</param>
        /// <param name="password">Password for the username</param>
        /// <param name="authenticationComplete">Action delegate to be called when authentication operation is complete</param>
        void Authenticate(string userName, string password, Action<AuthenticationResult> authenticationComplete);
    }

}