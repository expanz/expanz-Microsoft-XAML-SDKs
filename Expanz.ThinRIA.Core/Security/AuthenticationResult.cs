using System;

namespace Expanz.ThinRIA.Security
{
    internal partial class AuthenticationResult
    {
        public string Token { get; private set; }
        public string ErrorMessage { get; private set; }
        public Exception Error { get; private set; }
        public AuthenticationResult(string token, string errorMessage)
        {
            Token = token;
            ErrorMessage = errorMessage;

        }

        public AuthenticationResult(Exception error) :
            this(null, null)
        {
            Error = error;
        }
    }
}