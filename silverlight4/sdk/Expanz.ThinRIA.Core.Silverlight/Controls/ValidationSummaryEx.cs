using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Expanz.ThinRIA.Core;

namespace Expanz.ThinRIA.Controls
{
    // Never completed, and probably isn't required.
    internal class ValidationSummaryEx : ValidationSummary, IMessageDisplay
    {
        #region Constructor
        public ValidationSummaryEx() : base()
        {
            this.Loaded += new RoutedEventHandler(ValidationSummaryEx_Loaded);
        } 
        #endregion

        #region Event Handlers
        private void ValidationSummaryEx_Loaded(object sender, RoutedEventArgs e)
        {
            RegisterWithParentActivityContainer();
        } 
        #endregion

        #region Public Methods
        /// <summary>
        /// Function to publish message to the message panel.
        /// </summary>
        /// <param name="msg"></param>
        public void PublishMessage(XElement msg)
        {
            string severity = string.Empty;

            if (msg.Attribute(Common.MessageType) != null)
                severity = msg.Attribute(Common.MessageType).Value.ToUpper();

            string message = msg.Value;

            ValidationSummaryItem item = new ValidationSummaryItem(msg.Value);

            //TextBlock MSG = new TextBlock();
            //MSG.Text = message;
            //if (severity == "ERROR")
            //{
            //    MSG.Foreground = new SolidColorBrush(Colors.Red);
            //}
            //else if (severity == "WARNING")
            //{
            //    MSG.Foreground = new SolidColorBrush(Colors.Black);
            //}
            //else
            //{
            //    MSG.Foreground = new SolidColorBrush(Colors.Green);
            //}

            if (msg.Attribute(Common.PopupMessage) != null && Common.boolValue(msg.Attribute(Common.PopupMessage).Value))
                ApplicationEx.Instance.DisplayMessageBox(message, severity);

            this.Errors.Add(item);
        }

        /// <summary>
        /// Function to clear the data from message panel.
        /// </summary>
        public void Clear()
        {
            this.Errors.Clear();
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

            if (parentContainer != null)
                parentContainer.RegisterControl(this);
        } 
        #endregion
    }
}
