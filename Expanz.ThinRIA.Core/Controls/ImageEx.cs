using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Expanz.ThinRIA.Core;

#if WPF
using Microsoft.Win32;
#endif

namespace Expanz.ThinRIA.Controls
{
    [TemplatePart(Name = TemplatePart_ImageControl, Type = typeof(Image))]
    [TemplatePart(Name = TemplatePart_UploadButton, Type = typeof(Button))]
    [TemplateVisualState(GroupName = VisualStateGroup_CommonStates, Name=VisualState_Normal)]
    [TemplateVisualState(GroupName = VisualStateGroup_CommonStates, Name=VisualState_MouseOver)]
    [TemplateVisualState(GroupName = VisualStateGroup_Actions, Name = VisualState_Inactive)]
    [TemplateVisualState(GroupName = VisualStateGroup_Actions, Name = VisualState_Downloading)]
    [TemplateVisualState(GroupName = VisualStateGroup_Actions, Name = VisualState_Uploading)]
    public class ImageEx : Control, IServerBoundControl, IEditableControl, IMediaControl, INotifyPropertyChanged
    {
        #region Constants
        private const string VisualStateGroup_CommonStates = "CommonStates";
        private const string VisualStateGroup_Actions = "Actions";
        private const string VisualState_Normal = "Normal";
        private const string VisualState_MouseOver = "MouseOver";
        private const string VisualState_Inactive = "Inactive";
        private const string VisualState_Downloading = "Downloading";
        private const string VisualState_Uploading = "Uploading";

        private const string TemplatePart_ImageControl = "ImageControl";
        private const string TemplatePart_UploadButton = "UploadButton"; 
        #endregion

        #region Member Variables
        private ControlHarness _controlHarness;
        private string _fieldId;
        private Image _imageControl = null;
        private bool _displayAsThumbnail = false;
        private Stretch _stretch = Stretch.UniformToFill;
        private bool _isEditable = false; 
        #endregion

        #region Constructor
        public ImageEx()
        {
            Loaded += ImageEx_Loaded;
            DefaultStyleKey = typeof(ImageEx);
        } 
        #endregion

        #region Overrides
        public override void OnApplyTemplate()
        {
            _imageControl = this.GetTemplateChild(TemplatePart_ImageControl) as Image;

            if (_imageControl != null)
            {
                _imageControl.Stretch = _stretch;
                LoadImage(Url);
            }

            Button uploadButton = this.GetTemplateChild(TemplatePart_UploadButton) as Button;

            if (uploadButton != null)
            {
                uploadButton.Click += new RoutedEventHandler(UploadButton_Click);
            }
        }

        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

#if WINDOWS_PHONE
            SelectAndUploadFile();
#endif
        }

        protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            VisualStateManager.GoToState(this, VisualState_MouseOver, false);
        }

        protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            VisualStateManager.GoToState(this, VisualState_Normal, false);
        }
        #endregion

        #region Event Handlers
        private void ImageEx_Loaded(object sender, RoutedEventArgs e)
        {
            InitHarness();
        }

        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            SelectAndUploadFile();
        }

        private void client_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            BitmapImage image = null;

            if (e.Error == null)
            {
                image = new BitmapImage();

#if WPF
                image.BeginInit();
                image.StreamSource = e.Result;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                //image.Freeze();
#else
                image.SetSource(e.Result);
#endif

                _imageControl.Stretch = Stretch;
            }
            else
            {
                image = GetErrorImage();
                _imageControl.Stretch = Stretch.None;
                SetHint(e.Error.Message);
            }

            _imageControl.Source = image;

            VisualStateManager.GoToState(this, VisualState_Inactive, false);
        }

        private BitmapImage GetErrorImage()
        {
            var image = new BitmapImage();
            AssemblyName assemblyName = new AssemblyName(Assembly.GetExecutingAssembly().FullName);

#if WPF
            image.BeginInit();
            image.UriSource = new Uri("pack://application:,,,/" + assemblyName.Name + ";component/Images/CriticalError.png");
            image.EndInit();
#else
            image.UriSource = new Uri("/" + assemblyName.Name + ";component/Images/CriticalError.png", UriKind.Relative);
#endif

            return image;
        }
        #endregion

        #region Public Methods
        public void InitHarness()
        {
            _controlHarness = new ControlHarness(this);
        }

        public void PublishXml(XElement xml)
        {
            VisualStateManager.GoToState(this, VisualState_Inactive, false);

            if (xml.Attribute("encoding") != null && xml.Attribute("encoding").Value == "BASE64")
            {
                //_imageControl.Source = new ImageSource();
                //var base64Decoder:Base64Decoder = new Base64Decoder();
                //base64Decoder.decode(xml.toString());
                //source = base64Decoder.toByteArray();
            }
            else if (xml.Attribute("url") != null && xml.Attribute("url").Value != "")
            {
                Url = xml.Attribute("url").Value;
            }
        }
        #endregion

        #region Public Properties
        [Category("expanz")]
        [Description("Set the field on the Server ModelObject to bind this controls input values to.")]
        public string FieldId
        {
            get { return string.IsNullOrEmpty(_fieldId) ? Name : _fieldId; }
            set { _fieldId = value; }
        }

        public void SetVisible(bool visible)
        {
            Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public XElement DeltaXml
        {
            get { return null; }
        }

        public void SetEditable(bool editable)
        {
            this.IsEnabled = editable;
            IsEditable = editable;
        }

        public bool IsEditable
        {
            get { return _isEditable; }
            set
            {
                _isEditable = value;

                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("IsEditable"));
            }
        }

        public void SetNull()
        {
            //_value = string.Empty;
            RaiseNotifyValueChanged();
        }

        public void SetValue(string text)
        {
            //_value = text.Replace("\n", "\r\n");
            RaiseNotifyValueChanged();
        }

        public void SetHint(string hint)
        {
            ToolTipService.SetToolTip(this, hint);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Description("Default values are set in metadata on the server")]
        public string Url
        {
            get { return (string)GetValue(UrlProperty); }
            set { SetValue(UrlProperty, value); }
        }

        public static readonly DependencyProperty UrlProperty =
            DependencyProperty.Register("Url", typeof(string), typeof(ImageEx),
                                        new PropertyMetadata(string.Empty, UrlPropertyChanged));

        private static void UrlPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ImageEx;
            string url = e.NewValue.ToString();

            control.LoadImage(url);
        }

        public Stretch Stretch
        {
            get { return _stretch; }
            set
            {
                _stretch = value;

                if (_imageControl != null)
                    _imageControl.Stretch = value;
            }
        }

        public bool DisplayAsThumbnail
        {
            get { return _displayAsThumbnail; }
            set { _displayAsThumbnail = value; }
        }

        private void RaiseNotifyValueChanged()
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("Value"));
        }

        public MediaPublishTypes PublishType { get; set; }

        public string DataId
        {
            get { return FieldId ?? Name; }
        }
        #endregion

        #region Private Methods
        private void SelectAndUploadFile()
        {
#if SILVERLIGHT
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Pictures|*.jpeg;*.jpg;*.png";
            bool? fileSelected = dialog.ShowDialog();

            if (fileSelected.HasValue && fileSelected.Value)
            {
                // Load the file into the image control
                using (FileStream fileStream = dialog.File.OpenRead())
                {
                    BitmapImage image = new BitmapImage();
                    image.SetSource(fileStream);
                    _imageControl.Source = image;
                    fileStream.Close();
                }

                // Now upload the file to the server immediately
                using (FileStream fileStream = dialog.File.OpenRead())
                {
                    UploadFile(fileStream);
                }
            }
#elif WPF
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Pictures|*.jpeg;*.jpg;*.png";
            bool? fileSelected = dialog.ShowDialog();

            if (fileSelected.HasValue && fileSelected.Value)
            {
                // Load the file into the image control
                BitmapImage image = new BitmapImage(new Uri(dialog.FileName, UriKind.Absolute));
                _imageControl.Source = image;
                //image.
                //using (FileStream fileStream = dialog.File.OpenRead())
                //{
                //    BitmapImage image = new BitmapImage();
                //    image.SetSource(fileStream);
                //    _imageControl.Source = image;
                //    fileStream.Close();
                //}

                
                // Now upload the file to the server immediately
                using (FileStream stream = new FileStream(dialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    UploadFile(stream);
                }
            }
#elif WINDOWS_PHONE
            var photoChooser = new Microsoft.Phone.Tasks.PhotoChooserTask();
            photoChooser.ShowCamera = true;
            photoChooser.Completed += new EventHandler<Microsoft.Phone.Tasks.PhotoResult>(photoChooser_Completed);
            photoChooser.Show();
#endif
        }

#if WINDOWS_PHONE
        private void photoChooser_Completed(object sender, Microsoft.Phone.Tasks.PhotoResult e)
        {
            if (e.TaskResult == Microsoft.Phone.Tasks.TaskResult.OK)
            {
                // Load the file into the image control
                BitmapImage image = new BitmapImage();
                image.SetSource(e.ChosenPhoto);
                _imageControl.Source = image;

                e.ChosenPhoto.Seek(0, SeekOrigin.Begin);

                // Now upload the file to the server immediately
                UploadFile(e.ChosenPhoto);
            }
        }
#endif

        private void UploadFile(Stream fileStream)
        {
            VisualStateManager.GoToState(this, VisualState_Uploading, false);

            byte[] fileContents = new byte[fileStream.Length];
            fileStream.Read(fileContents, 0, (int)fileStream.Length);
            fileStream.Close();

            string base64Data = Convert.ToBase64String(fileContents);

            XElement deltaElement = new XElement("Delta");
            deltaElement.SetAttributeValue(Common.IDAttrib, FieldId);
            deltaElement.SetAttributeValue(Common.KeyValueValue, Common.LongDataValueIndicator);
            deltaElement.SetAttributeValue(Common.LongDataEncoding, Common.Base64Encoding);
            deltaElement.Value = base64Data;

            _controlHarness.SendXml(deltaElement);
        }

        private void LoadImage(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                if (url.Contains("?"))
                    url = url.Substring(0, url.IndexOf('?'));

                if (url.ToUpper().Contains(".PDF"))
                {
                    //PDF=harness.parentActivityContainer.AppHost.fileURLPrefix + url;
                    //pdfLabel=new Label();
                    //pdfLabel.width=this.width;
                    //pdfLabel.text="Click here to view PDF";
                    //pdfLabel.addEventListener(MouseEvent.CLICK, onDoubleClick);
                    //this.visible=false;
                    //this.parent.addChild(pdfLabel);
                }
                else
                {
                    //if (pdfLabel != null)
                    //{
                    //    Visibility = Visibility.Visible;
                    //    //pdfLabel.parent.removeChild(pdfLabel);
                    //    //pdfLabel = null;
                    //}

                    //if (url.ToUpper().Contains(".TIF"))
                    //{
                    //    url = url.Substring(0, url.LastIndexOf(".")) + ".jpeg";
                    //}

                    //source = _controlHarness.ParentActivity.AppHost.fileURLPrefix + url;
                    SetHint(url);

                    if (DisplayAsThumbnail)
                        url += "TN";

                    // TEMP HACK!!!!!!
                    url += "?NoCache=" + Guid.NewGuid().ToString();

                    if (_imageControl != null)
                    {
                        // Starting loading the image asynchronously
                        try
                        {
                            WebClient client = new WebClient();
                            client.OpenReadCompleted += client_OpenReadCompleted;
                            client.OpenReadAsync(new Uri(url, UriKind.Absolute));

                            VisualStateManager.GoToState(this, VisualState_Downloading, false);
                        }
                        catch (Exception ex)
                        {
                            BitmapImage image = GetErrorImage();
                            _imageControl.Stretch = Stretch.None;
                            SetHint(ex.Message);
                            _imageControl.Source = image;

                            VisualStateManager.GoToState(this, VisualState_Inactive, false);
                        }
                    }
                }
            }
        }  
        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged; 
        #endregion
    }
}
