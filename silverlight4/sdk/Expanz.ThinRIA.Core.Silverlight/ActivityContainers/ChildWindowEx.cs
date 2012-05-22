using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using Expanz.Extensions.BCL;
using Expanz.ThinRIA.Controls;
using Expanz.ThinRIA.Core;

namespace Expanz.ThinRIA.ActivityContainers
{
    public class ChildWindowEx : ChildWindow, IActivityContainer, INotifyPropertyChanged
    {
        #region Member Variables
        protected ActivityHarness _harness;
        private XElement _xml;
        private string _iconImage;
        protected Dictionary<string, ButtonEx> _dirtyButtons;
        private bool _hasHadInitialFocusSet = false;
        #endregion

        #region Constructor
        public ChildWindowEx() : base()
        {
            ApplicationEx.CreatingContainer = this;

            if (ApplicationEx.Instance.HasOpenSession)
            {
                _harness = new ActivityHarness(this);
                _harness.PropertyChanged += new PropertyChangedEventHandler(ActivityHarness_PropertyChanged);

                if (!System.ComponentModel.DesignerProperties.IsInDesignTool)
                {
                    this.Loaded += new RoutedEventHandler(ChildWindowEx_Loaded);
                    this.GotFocus += new RoutedEventHandler(ChildWindowEx_GotFocus);
                    this.Closing += new EventHandler<CancelEventArgs>(ChildWindowEx_Closing);

                    string className = this.GetType().FullName;

                    if (ApplicationEx.Instance.XamlFileDetailsCollection.ContainsKey(className))
                    {
                        XAMLFileDetails fileDetails = ApplicationEx.Instance.XamlFileDetailsCollection[className];
                        _harness.RegisterUnloadedControls(fileDetails.ExpanzControlDetails);
                    }
                }
            }
        }
        #endregion

        #region Event Handlers
        private void ChildWindowEx_Loaded(object sender, RoutedEventArgs e)
        {
            _harness.Initialise(_xml);
        } 

        private void ChildWindowEx_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!_hasHadInitialFocusSet)
            {
                // Set focus to initial focus field if one is specified
                if (!string.IsNullOrEmpty(InitialFocusField))
                {
                    Control focusControl = _harness.Controls.FirstOrDefault(x => x.FieldId == InitialFocusField && !(x is LabelEx)) as Control;

                    if (focusControl != null && focusControl.IsEnabled)
                        focusControl.Focus();
                }

                _hasHadInitialFocusSet = true;
            }
        } 

        private void ChildWindowEx_Closing(object sender, CancelEventArgs e)
        {
            _harness.ActivityClosed();
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

        public bool IsDirty
        {
            get 
            {
                bool isDirty = false;

                foreach (ButtonEx button in DirtyButtons.Values)
                {
                    if (button.IsDirty)
                        isDirty = true;
                }

                return isDirty;
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
                ApplicationEx.PickListActivity = null;

            if (publishElt.Attribute(Common.closeWindow) != null && Common.boolValue(publishElt.Attribute(Common.closeWindow).Value))
            {
                XElement msgs = publishElt.Element("Messages");

                if (msgs != null)
                {
                    XElement msg = (XElement)msgs.FirstNode;

                    while (msg != null)
                    {
                        if (msg.GetAttributeValue("type") != "Info")
                            AppHost.PublishMessage(msg);

                        msg = (XElement)msg.NextNode;
                    }
                }

                this.Close();
                return;
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
        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Escape)
                this.Close();
        } 
        #endregion
    }
}
