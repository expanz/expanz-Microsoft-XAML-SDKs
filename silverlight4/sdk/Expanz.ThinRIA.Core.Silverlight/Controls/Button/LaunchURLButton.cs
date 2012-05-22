using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Expanz.ThinRIA.Core;
using System.Xml.Linq;
using Expanz.Extensions.BCL;

namespace Expanz.ThinRIA.Controls
{
    public class LaunchURLButton : Button, IServerBoundControl, IValueDestination
    {
        #region Member Variables
        private ControlHarness _controlHarness;
        private string _contentType = null; 
        #endregion

        #region Constructor
        public LaunchURLButton() : base()
        {
            Loaded += LabelEx_Loaded;
            Click += new RoutedEventHandler(LaunchURLButton_Click);

            Image image = new Image();
            image.Stretch = Stretch.None;
            image.Source = new BitmapImage(new Uri("/Expanz.ThinRIA.Core.Silverlight;component/Images/Go.png", UriKind.RelativeOrAbsolute));

            this.Content = image;
        }
        #endregion

        #region Event Handlers
        private void LabelEx_Loaded(object sender, RoutedEventArgs e)
        {
            InitHarness();
        }

        private void LaunchURLButton_Click(object sender, RoutedEventArgs e)
        {
            // Check to make sure this is not a file path
            if (!Application.Current.IsRunningOutOfBrowser && Application.Current.Host.Source.Scheme == "file")
            {
                ApplicationEx.Instance.DisplayMessageBox("Due to Silverlight's security restrictions, you cannot launch a URL in a browser window when the application is running from a file path. When the application is launched from a URL (as should be the case in production scenarios), this functionality will start to work.", "expanz");
            }
            else
            {
                try
                {
                    string openUrl = URL;

                    if (_contentType == Common.ContentTypes.EmailAddress)
                        openUrl = "mailto:" + openUrl;

                    HyperlinkButtonWrapper hlbw = new HyperlinkButtonWrapper();
                    hlbw.OpenURL(openUrl);
                }
                catch (Exception ex)
                {
                    ApplicationEx.Instance.DisplayMessageBox(ex.Message, "expanz");
                }
            }
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Set the field on the Server ModelObject to bind this controls input values to.
        /// </summary>       
        [Category("expanz")]
        [Description("Set the field on the Server ModelObject to bind this control's input values to.")]
        public string FieldId
        {
            get { return (string)GetValue(FieldIdProperty); }
            set { SetValue(FieldIdProperty, value); }
        }

        public static readonly DependencyProperty FieldIdProperty =
            DependencyProperty.Register("FieldId", typeof(string), typeof(LaunchURLButton), null);

        public string URL { get; private set; }
        #endregion

        #region Private Methods
        private void InitHarness()
        {
            _controlHarness = new ControlHarness(this);
        } 
        #endregion

        #region Public Methods
        public void SetVisible(bool visible)
        {
            Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public void SetNull()
        {
            URL = string.Empty;
        }

        public void SetValue(string text)
        {
            URL = text.Replace("\n", "\r\n");
        }

        public void SetHint(string hint)
        {
            ToolTipService.SetToolTip(this, hint);
        }

        public void PublishXml(XElement xml)
        {
            _contentType = xml.GetAttributeValue(Common.ContentTypes.AttribName);

            // Change the button image to correspond to the type of content for this field
            string buttonImageUri = null;

            switch (_contentType)
            {
                case Common.ContentTypes.URL:
                    buttonImageUri = "/Expanz.ThinRIA.Core.Silverlight;component/Images/Globe.png";
                    break;
                case Common.ContentTypes.EmailAddress:
                    buttonImageUri = "/Expanz.ThinRIA.Core.Silverlight;component/Images/Envelope.png";
                    break;
            }

            if (buttonImageUri != null)
            {
                Image image = new Image();
                image.Stretch = Stretch.None;
                image.Source = new BitmapImage(new Uri(buttonImageUri, UriKind.RelativeOrAbsolute));

                this.Content = image;
            }
        }
        #endregion

        #region Private Classes
        // Copied from http://blog.falafel.com/blogs/jonathantower/10-11-16/Opening_a_URL_in_a_New_Window_from_Code-Behind_in_Silverlight
        private class HyperlinkButtonWrapper : HyperlinkButton
        {
            public void OpenURL(string navigateUri)
            {
                OpenURL(new Uri(navigateUri, UriKind.Absolute));
            }

            public void OpenURL(Uri navigateUri)
            {
                base.NavigateUri = navigateUri;
                base.TargetName = "_blank";
                base.OnClick();
            }
        } 
        #endregion
    }
}
