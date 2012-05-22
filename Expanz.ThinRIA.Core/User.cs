namespace Expanz.ThinRIA
{
    public class User
    {
        ///// <summary>
        ///// function to send forgotten password
        ///// </summary>
        ///// <param name="site"></param>
        ///// <param name="userName"></param>
        ///// <param name="emailAddress"></param>
        //public virtual void SendForgottenPassword(string site, string userName, string emailAddress)
        //{
        //    //format a request to the anon session
        //    XDocument request = new XDocument();
        //    request = XDocument.Parse(Common.NewEmptyXmlDoc);
        //    XElement act = new XElement(Common.ActivityNode);
        //    act.SetAttributeValue(Common.IDAttrib, "EXM.Employee");
        //    request.Document.Add(act);
        //    XElement f1 = new XElement(Common.Requests.FieldValueChange);
        //    f1.SetAttributeValue(Common.IDAttrib, "LoginCode");
        //    f1.SetAttributeValue(Common.PublishFieldValue, userName);
        //    act.Add(f1);
        //    XElement f2 = new XElement(Common.Requests.FieldValueChange);
        //    f2.SetAttributeValue(Common.IDAttrib, "EmailAddress");
        //    f2.SetAttributeValue(Common.PublishFieldValue, emailAddress);
        //    act.Add(f2);
        //    XElement m1 = new XElement(Common.Requests.MethodInvocation);
        //    m1.SetAttributeValue(Common.RequestMethodName, "sendForgottenPassword");
        //    act.Add(m1);
        //    bool ok = ExecAnnymous(site, request);
        //    XElement msgs = Response.Document.Root.Element(Common.MessagesNode);
        //    string msg = "";
        //    if (msgs != null)
        //    {
        //        XElement messageElt = (XElement)msgs.FirstNode;
        //        while (messageElt != null)
        //        {
        //            if (msg.Length > 0) msg += System.Environment.NewLine;
        //            msg += messageElt.Value;
        //            messageElt = (XElement)messageElt.NextNode;
        //        }
        //    }
        //    if (ok)
        //        DisplayMessageBox(msg, "Password changed");
        //    else
        //        DisplayMessageBox(msg, "Password NOT changed");
        //}
        //private bool isPasswordChanged { get; set; }
        //public bool ChangePassword(string userName, string oldPassword, string newPassword, ref string msg)
        //{
        //    //RemoteService.CreateSessionAsync(userName, oldPassword, newPassword,  msg);
        //    RemoteService.ChangeUserPasswordAsync(userName, oldPassword, newPassword, msg);
        //    RemoteService.ChangeUserPasswordCompleted += ChangeUserPasswordCompletedEvent;
        //    return isPasswordChanged;
        //    //bool ok = RemoteService.CreateSession(userName, oldPassword, newPassword, ref msg);
        //    //return ok;
        //}
        //private void ChangeUserPasswordCompletedEvent(object sender, ChangeUserPasswordCompletedEventArgs e)
        //{
        //    isPasswordChanged = string.IsNullOrEmpty(e.errors);
        //}
    }
}