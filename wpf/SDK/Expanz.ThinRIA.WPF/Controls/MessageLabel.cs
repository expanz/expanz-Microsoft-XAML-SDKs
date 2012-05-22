using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Expanz.ThinRIA.Core;

namespace Expanz.ThinRIA.Controls
{
    /// <summary>
    /// Custom Control to show latest message from the server and an appropriate icon
    /// </summary>
    [TemplateVisualState(Name = "Success", GroupName = StateGroupSeverityStates)]
    [TemplateVisualState(Name = "Information", GroupName = StateGroupSeverityStates)]
    [TemplateVisualState(Name = "Warning", GroupName = StateGroupSeverityStates)]
    [TemplateVisualState(Name = "Error", GroupName = StateGroupSeverityStates)]
    [TemplateVisualState(Name = "CriticalError", GroupName = StateGroupSeverityStates)]
    [TemplateVisualState(Name = "OK", GroupName = StateGroupSeverityStates)]
    public class MessageLabel : ContentControl, IMessageDisplay, INotifyPropertyChanged
    {
        #region Member Variables
        private SeverityLevels _severity = SeverityLevels.Undefined;
        private string _message;
        private ControlHarness _controlHarness;
        #endregion

        #region Constants
        private const string StateGroupSeverityStates = "SeverityStates"; 
        #endregion

        #region Enumerations
        public enum SeverityLevels
        {
            Undefined,
            Success,
            Information,
            Warning,
            Error,
            CriticalError,
            OK
        }
        #endregion

        #region Constructor
        public MessageLabel() : base()
        {
            this.Loaded += new RoutedEventHandler(MessageLabel_Loaded);

            DefaultStyleKey = typeof(MessageLabel);
        } 
        #endregion

        #region Event Handlers
        private void MessageLabel_Loaded(object sender, RoutedEventArgs e)
        {
            Clear();
            _controlHarness = new ControlHarness(this);
        }
        #endregion

        #region Public Properties
        public SeverityLevels Severity
        {
            get { return (SeverityLevels)GetValue(SeverityProperty); }
            set { SetValue(SeverityProperty, value); }
        }

        public static readonly DependencyProperty SeverityProperty =
            DependencyProperty.Register("Severity", typeof(SeverityLevels), typeof(MessageLabel), new PropertyMetadata(SeverityLevels.Undefined));

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(MessageLabel), null);        
        #endregion

        #region Public Methods
        /// <summary>
        /// Publish message to the message panel.
        /// </summary>
        /// <param name="msg"></param>
        public void PublishMessage(XElement msg)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                if (msg == null)
                {
                    Clear();
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

                    Severity = severity;
                    Message = msg.Value;

                    VisualStateManager.GoToState(this, severity.ToString(), true);

                    if (msg.Attribute(Common.PopupMessage) != null && Common.boolValue(msg.Attribute(Common.PopupMessage).Value))
                        MessageBox.Show(Message, severity.ToString(), MessageBoxButton.OK);

                    this.Visibility = string.IsNullOrEmpty(Message) ? Visibility.Collapsed : Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// Clear the data from the message panel.
        /// </summary>
        public void Clear()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                Severity = SeverityLevels.Undefined;
                Message = null;

                this.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged; 
        #endregion
    }
}
