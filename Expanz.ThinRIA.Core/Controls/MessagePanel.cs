using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;
using Expanz.ThinRIA.Core;

namespace Expanz.ThinRIA.Controls
{
    /// <summary>
    /// List that can display messages from the server in an activity container
    /// </summary>
    public class MessagePanel : ListBox, IMessageDisplay
    {
        #region Constructor
        public MessagePanel() : base()
        {
            this.Loaded += new RoutedEventHandler(MessagePanel_Loaded);
        } 
        #endregion

        #region Event Handlers
        private void MessagePanel_Loaded(object sender, RoutedEventArgs e)
        {
            RegisterWithParentActivityContainer();
        } 
        #endregion

        #region Public Methods
        /// <summary>
        /// Publish message to the message panel.
        /// </summary>
        /// <param name="msg"></param>
        public void PublishMessage(XElement msg)
        {
            if (msg != null)
            {
                string severity = string.Empty;

                if (msg.Attribute(Common.MessageType) != null)
                    severity = msg.Attribute(Common.MessageType).Value.ToUpper();

                string message = msg.Value;

                TextBlock messageTextBlock = new TextBlock();
                messageTextBlock.Text = message;

                switch (severity)
                {
                    case "ERROR":
                        messageTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                        break;
                    case "WARNING":
                        messageTextBlock.Foreground = new SolidColorBrush(Colors.Black);
                        break;
                    default:
                        messageTextBlock.Foreground = new SolidColorBrush(Colors.Green);
                        break;
                }

                if (msg.Attribute(Common.PopupMessage) != null && Common.boolValue(msg.Attribute(Common.PopupMessage).Value))
                    ApplicationEx.Instance.DisplayMessageBox(message, severity);

                Items.Add(messageTextBlock);
                this.ScrollIntoView(messageTextBlock);
            }
        }

        /// <summary>
        /// Clear the data from the message panel.
        /// </summary>
        public void Clear()
        {
            if (Items != null)
                Items.Clear();
        } 
        #endregion

        #region Private Methods
        /// <summary>
        /// Auto register myself with parent IActivityContainer.
        /// Might need to traverse the display list upwards until found one
        /// </summary>
        private void RegisterWithParentActivityContainer()
        {
            FrameworkElement parent = this.Parent as FrameworkElement;
            IActivityContainer parentContainer = null;

            while (parentContainer == null && parent != null)
            {
                if (parent is IActivityContainer)
                    parentContainer = parent as IActivityContainer;
                else
                    parent = parent.Parent as FrameworkElement;
            }
#if WPF
            if (parentContainer == null)
            {
                parent = this.TemplatedParent as FrameworkElement;
                while (parentContainer == null && parent != null)
                {
                    if (parent is IActivityContainer)
                        parentContainer = parent as IActivityContainer;
                    else
                        parent = parent.Parent as FrameworkElement;
                }
            }
#endif
            if (parentContainer != null)
                parentContainer.RegisterControl(this);
        } 
        #endregion
    }
}