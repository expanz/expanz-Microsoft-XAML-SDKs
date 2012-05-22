using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Expanz.ThinRIA.Controls;
using Expanz.ThinRIA.Core;

namespace Expanz.ThinRIA.ActivityContainers
{
    public class PageEx : Page, IActivityContainer, INotifyPropertyChanged
    {
        #region Member Variables
        protected ActivityHarness _harness;
        private XElement _xml;
        private string _iconImage;
        protected Dictionary<string, ButtonEx> _dirtyButtons;
        private bool _isInitialised = false;

        //private System.Windows.Threading.DispatcherTimer myDispatcherTimer;
        #endregion

        #region Constructor
        public PageEx() : base()
        {
            ApplicationEx.CreatingContainer = this;

            if (!System.ComponentModel.DesignerProperties.IsInDesignTool)
            {
                this.Loaded += new RoutedEventHandler(PageEx_Loaded);

                if (ApplicationEx.Instance.HasOpenSession)
                {
                    _harness = new ActivityHarness(this);
                    _harness.PropertyChanged += new PropertyChangedEventHandler(ActivityHarness_PropertyChanged);

                    string className = this.GetType().FullName;

                    if (ApplicationEx.Instance.XamlFileDetailsCollection.ContainsKey(className))
                    {
                        XAMLFileDetails fileDetails = ApplicationEx.Instance.XamlFileDetailsCollection[className];
                        _harness.RegisterUnloadedControls(fileDetails.ExpanzControlDetails);
                    }

                    //harness.Initialise(_xml);
                }
            }
        }
        #endregion

        #region Control Event Handlers
        private void PageEx_Loaded(object sender, RoutedEventArgs e)
        {
            if (ApplicationEx.Instance.HasOpenSession)
            {
                if (!_isInitialised)
                    _harness.Initialise(_xml);

                _isInitialised = true;

                // Set focus to initial focus field if one is specified
                if (!string.IsNullOrEmpty(InitialFocusField))
                {
                    Control focusControl = _harness.Controls.FirstOrDefault(x => x.FieldId == InitialFocusField && !(x is LabelEx)) as Control;

                    if (focusControl != null && focusControl.IsEnabled)
                        focusControl.Focus();
                }
            }
            else
            {
                if (ApplicationEx.Instance.ActivityHostFrame != null)
                {
                    if (ApplicationEx.Instance.ActivityHostFrame.CurrentSource.OriginalString.Length != 0)
                    {
                        // Find login/initial page name (needed, since we're passing in a query parameter)
                        Uri loginPageUri = ApplicationEx.Instance.ActivityHostFrame.UriMapper.MapUri(new Uri("", UriKind.Relative));
                        
                        // Go to login page
                        ApplicationEx.Instance.ActivityHostFrame.Navigate(new Uri(loginPageUri.OriginalString + "?" + ApplicationEx.ReturnUrlParameterName + "=" + NavigationService.Source.OriginalString, UriKind.Relative));
                    }
                }
            }
        }

        private void ActivityHarness_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!DesignerProperties.IsInDesignTool)
            {
                if (e.PropertyName == "IsLoading")
                    IsLoading = _harness.IsLoading;
            }
        }
        #endregion

        #region Public Properties
        protected virtual UIElement NavigationPanel
        {
            get { return null; }
        }

        public ApplicationEx AppHost
        {
            get { return ApplicationEx.Instance; }
        }

        public OptimiseTiming Optimsation
        {
            get { return _harness.Optimisation; }
            set { _harness.Optimisation = value; }
        }

        public string ActivityName
        {
            set { _harness.ActivityName = value; }
            get { return _harness.ActivityName; }
        }

        public string ActivityStyle
        {
            set { _harness.ActivityStyle = value; }
            get { return _harness.ActivityStyle; }
        }

        public string IconImageName
        {
            get { return _iconImage; }
            set { _iconImage = value; }
        }

        public string ImplementationName { get { return this.Name; } }
        public string ActivityStamp { get { return _harness.ActivityStamp; } }
        public bool IsInitialised { get { return _harness.IsInitialised; } }
        public string ActivityStampEx { get { return _harness.ActivityStampEx; } }
        public int DuplicateIndex { get { return _harness._duplicateIndex; } }
        public string FixedContext { get { return _harness.FixedContext; } }
        public int PersistentKey { get { return _harness.PersistentId; } }
        public bool IsPublishing { get { return _harness.Publishing; } }

        public string InitialFocusField { get; set; }

        public bool IsLoading 
        { 
            get { return _harness.IsLoading; }
            internal set
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("IsLoading"));
            }
        }

        public bool IsOKToClose
        {
            get { return true; }
        }

        protected Dictionary<string, ButtonEx> DirtyButtons
        {
            get
            {
                if (_dirtyButtons == null)
                    _dirtyButtons = new Dictionary<string, ButtonEx>();

                return _dirtyButtons;
            }
        }
        #endregion

        #region Public Methods
        public void Initialise(XElement xml)
        {
            this._xml = xml;
        }

        public void InitialiseCopy(ActivityHarness H)
        {
            _harness.CopyFrom(H);
            ApplicationEx.CreatingContainer = null;
            if (NavigationPanel != null) _harness.NavigationPanel = NavigationPanel;
        }

        public void RegisterControl(object control)
        {
            if (_harness != null)
            {
                _harness.RegisterControl(control);

                if (control is ButtonEx && ((ButtonEx)control).IsDirtyButton)
                    RegisterDirtyButton((ButtonEx)control);
            }
        }

        public void Exec(XElement DeltaXml)
        {
            _harness.Exec(DeltaXml);
        }

        public void Exec(XElement[] DeltaXml)
        {
            _harness.Exec(DeltaXml);
        }

        public void AppendDataPublicationsToActivityRequest(XElement request)
        {
            _harness.AppendDataPublicationsToActivityRequest(request);
        }

        public virtual void PublishResponse(XElement publishElt)
        {
            if (ApplicationEx.PickListActivity != null)
            {
                ApplicationEx.PickListActivity = null;

                if (publishElt.Attribute(Common.closeWindow) != null && Common.boolValue(publishElt.Attribute(Common.closeWindow).Value))
                {
                    XElement msgs = publishElt.Element("Messages");

                    if (msgs != null)
                    {
                        XElement msg = (XElement)msgs.FirstNode;

                        while (msg != null)
                        {
                            AppHost.PublishMessage(msg);
                            msg = (XElement)msg.NextNode;
                        }
                    }

                    //this.Close();
                    return;
                }
            }

            _harness.PublishResponse(publishElt);
        }

        //public void ResetWindowTitle(ActivityHarness H)
        //{
        //    //if (!this.wantFixedWindowTitle) this.Title = H.WindowTitle;
        //}

        //public void PopupHelp(string context)
        //{ }

        //public void LaunchHelp(string context)
        //{ }

        //public virtual void CloseOnLogout()
        //{
        //    //this.Close();
        //}

        //public void Focus()
        //{
        //}

        public void RegisterDirtyButton(ButtonEx b)
        {
            if (!DirtyButtons.ContainsKey(b.ModelObject))
                DirtyButtons.Add(b.ModelObject, b);
        }

        public virtual void PublishDirtyChange(string modelObject, bool dirty)
        {
            if (DirtyButtons.ContainsKey(modelObject))
                DirtyButtons[modelObject].IsDirty = dirty;
        }
        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Overrides
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (!_isInitialised && ApplicationEx.Instance.HasOpenSession)
            {
                // When an activity is navigated to, it needs to get its corresponding
                // activity name and style from the form mappings, and then initialise itself
                string className = this.GetType().FullName;
                KeyValuePair<string, FormDefinition> formEntry = ApplicationEx.FormMappings.Where(x => x.Value.FullName == className).FirstOrDefault();

                if (formEntry.Value != null)
                {
                    ActivityName = formEntry.Value.ActivityName;
                    ActivityStyle = formEntry.Value.ActivityStyle;

                    //Initialise(ApplicationEx.Instance.NavigationCacheActivityXML);

                    _xml = ApplicationEx.Instance.NavigationCacheActivityXML;
                    ApplicationEx.Instance.NavigationCacheActivityXML = null; // Temp cache of XML no longer required
                }
            }
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            _harness.ActivityClosed();

            base.OnNavigatedFrom(e);
        }
        #endregion
    }
}
