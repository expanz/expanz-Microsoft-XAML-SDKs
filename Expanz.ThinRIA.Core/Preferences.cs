namespace Expanz.ThinRIA
{
    using System;

    public partial class Preferences
    {
        public Uri EndPoint { get; private set; }
        public string UserName { get; private set; }
        public string Password { get; private set; }
        public string PreferredSite { get; set; }

        public Preferences(string endPointUrl, string userName, string password)
        {
            Check.Argument.IsNotNullOrEmpty(endPointUrl, "endPointUrl");
            Check.Argument.IsNotInvalidWebUrl(endPointUrl, "endPointUrl");
            Check.Argument.IsNotNullOrEmpty(userName, "userName");

            EndPoint = new Uri(endPointUrl);
            UserName = userName;
            Password = password;
        }
    }
}