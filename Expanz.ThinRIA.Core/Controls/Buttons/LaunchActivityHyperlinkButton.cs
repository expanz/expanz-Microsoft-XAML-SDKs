using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Expanz.ThinRIA.Controls
{
    [Description("Use to Launch Activity from the client side. Launches the activity and shows the related View. Set the ActivityName and optionaly the ActivityStyle if required.")]
    public class LaunchActivityHyperlinkButton : HyperlinkButton
    {
        #region Constructor
        public LaunchActivityHyperlinkButton()
        {
            Click += LaunchActivityHyperlinkButton_Click;
        }
        #endregion

        #region Public Properties
        [Category("expanz")]
        [Description("Set the Activity Name to be created when clicked.")]
        public string ActivityName
        {
            get { return (string)GetValue(ActivityNameProperty); }
            set { SetValue(ActivityNameProperty, value); }
        }

        public static readonly DependencyProperty ActivityNameProperty =
            DependencyProperty.Register("ActivityName", typeof(string), typeof(LaunchActivityHyperlinkButton), null);

        [Category("expanz")]
        [Description("The style of the Activity to launch. Not always required. Check your form mapping file for valid Activity Names and Activity Styles")]
        public string ActivityStyle
        {
            get { return (string)GetValue(ActivityStyleProperty); }
            set { SetValue(ActivityStyleProperty, value); }
        }

        public static readonly DependencyProperty ActivityStyleProperty =
            DependencyProperty.Register("ActivityStyle", typeof(string), typeof(LaunchActivityHyperlinkButton), null);        
        #endregion

        #region Event Handlers
        private void LaunchActivityHyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            ApplicationEx.Instance.CreateActivityContainer(ActivityName, ActivityStyle, null, null);            
        }
        #endregion
    }
}
