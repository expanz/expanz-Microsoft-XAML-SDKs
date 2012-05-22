using System;
using System.Windows;

namespace Expanz.ThinRIA
{
    public partial class AuthenticationCompletedEventArgs : EventArgs
    {
        public string Reference { get; private set; }
        public bool IsAuthenticated { get; private set; }
        public AuthenticationCompletedEventArgs(string message, bool authenticated)
        {
            Reference = message;
            IsAuthenticated = authenticated;
        }
    }
}