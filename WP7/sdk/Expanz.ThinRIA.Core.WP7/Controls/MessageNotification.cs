using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Coding4Fun.Phone.Controls;

namespace Expanz.ThinRIA.Controls
{
    /// <summary>
    /// Really just wraps the ToastPromt class for now. Might make it a 
    /// configurable control at some later stage.
    /// </summary>
    internal class MessageNotification
    {
        internal enum SeverityLevels
        {
            Undefined,
            Success,
            Information,
            Warning,
            Error,
            CriticalError,
            OK
        }

        internal static void PublishMessage(XElement msg)
        {
            if (!DesignerProperties.IsInDesignTool)
            {
                if (msg == null)
                {
                    //Clear();
                }
                else
                {
                    SeverityLevels severity = SeverityLevels.Undefined;
                    string severityString = null;

                    if (msg.Attribute(Common.MessageType) != null)
                        severityString = msg.Attribute(Common.MessageType).Value.ToUpper();

                    switch (severityString)
                    {
                        case "ERROR":
                            severity = SeverityLevels.Error;
                            break;
                        case "WARNING":
                            severity = SeverityLevels.Warning;
                            break;
                        case "SUCCESS":
                            severity = SeverityLevels.Success;
                            break;
                        default:
                            severity = SeverityLevels.Information;
                            break;
                    }

                    if (msg.Attribute(Common.PopupMessage) != null && Common.boolValue(msg.Attribute(Common.PopupMessage).Value))
                    {
                        MessageBox.Show(msg.Value, severity.ToString(), MessageBoxButton.OK);
                    }
                    else
                    {
                        MessageBox.Show(msg.Value, severity.ToString(), MessageBoxButton.OK);

                        //var toast = new ToastPrompt
                        //{
                        //    Title = severity.ToString(),
                        //    Message = msg.Value,
                        //    ImageSource = new BitmapImage(new Uri("/Expanz.ThinRIA.Core.WP7;component/Images/" + severity.ToString() + ".png", UriKind.RelativeOrAbsolute)),
                        //};

                        //toast.Show();
                    }
                }
            }
        }
    }
}
