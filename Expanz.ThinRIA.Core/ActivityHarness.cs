using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Expanz.Extensions.BCL;
using Expanz.ThinRIA.Controls;
using Expanz.ThinRIA.Core;

namespace Expanz.ThinRIA
{
    public class ActivityHarness : INotifyPropertyChanged
    {
        #region Member Variables
        private IActivityContainer _container;
        private FrameworkElement _visualContainer;
        internal int _duplicateIndex;
        private string _activityStyle;
        private string _activityStamp;
        protected XDocument _schema;
        private List<IServerBoundControl> _controls;
        private Dictionary<string, Dictionary<IEditableControl, string>> _propertyFields;
        private Dictionary<string, IMethodCaller> _buttons;
        private Dictionary<string, IServerBoundControlContainer> _controlContainers;
        private Dictionary<string, IRepeatingDataControl> _dataControls;
        private Dictionary<string, IGraphControl> _graphControls;
        private List<ICustomSchemaPublisher> _customSchemaControls;
        private Dictionary<string, ICustomContentPublisher> _customContentControls;
        private Dictionary<string, IMediaControl> _mediaControls;
        protected List<IContainerWithSummary> _summaryContainers;
        private List<IContainerWithSummary> _dirtyContainers;
        private Dictionary<string, XElement> _controlDataCache = new Dictionary<string,XElement>();
        private bool _isLoading = false;
        #endregion

        #region Constructor
        public ActivityHarness(IActivityContainer container) : base()
        {
            this._container = container;

            if (this._container is FrameworkElement)
                _visualContainer = (FrameworkElement)container;

            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode((DependencyObject)container))
                IsDesignTime = true;
        }
        #endregion

        #region Public Properties
        public UIElement NavigationPanel { get; set; }
        public string ActivityName { get; set; }
        public IMessageDisplay MessagePanel { get; set; }
        public string Key { get; set; }
        public string FixedContext { get; protected set; }
        public bool Publishing { get; private set; }
        public int PersistentId { get; private set; }
        public bool IsActivityOwner { get; set; }
        public static IContextMenuPublisher ContextMenuPublisher { get; set; }
        public bool IsDesignTime { get; private set; }

        public ApplicationEx AppHost
        {
            get { return ApplicationEx.Instance; }
        }

        public string ActivityStyle
        {
            get { return _activityStyle ?? ""; }
            set { _activityStyle = value; }
        }
        
        public string ActivityStamp
        {
            get { return _activityStamp ?? string.Empty; }
            set { _activityStamp = value; }
        }
        
        public XDocument Schema
        {
            get { return _schema ?? new XDocument(); }
            set { _schema = value; }
        }

        public bool IsInitialised
        {
            get { return ActivityStamp != null; }
        }

        public string ActivityStampEx
        {
            get
            {
                if (_duplicateIndex > 0)
                    return _activityStamp + "/" + _duplicateIndex.ToString();
                else
                    return ActivityStamp;
            }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;

                // Notify the activity container to notify any listeners of *its* IsLoading property 
                // that they need to get the new value of the property
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("IsLoading"));
            }
        }

        protected bool HasSummaryContainers
        {
            get { return _summaryContainers != null; }
        }
        #endregion

        #region Control Arrays
        public List<IServerBoundControl> Controls
        {
            get
            {
                if (_controls == null) 
                    _controls = new List<IServerBoundControl>();

                return _controls;
            }
        }

        // Fields bound to properties other than Value (or label/hint/null)
        protected bool HasPropertyFields
        {
            get { return _propertyFields != null; }
        }

        public Dictionary<string, Dictionary<IEditableControl, string>> PropertyFields
        {
            get
            {
                if (_propertyFields == null)
                    _propertyFields = new Dictionary<string, Dictionary<IEditableControl, string>>();

                return _propertyFields;
            }
        }

        public Dictionary<string, IMethodCaller> Buttons
        {
            get
            {
                if (_buttons == null) 
                    _buttons = new Dictionary<string, IMethodCaller>();

                return _buttons;
            }
        }

        public Dictionary<string, IServerBoundControlContainer> ControlContainers
        {
            get
            {
                if (_controlContainers == null) 
                    _controlContainers = new Dictionary<string, IServerBoundControlContainer>();

                return _controlContainers;
            }
        }

        public Dictionary<string, IRepeatingDataControl> DataControls
        {
            get
            {
                if (_dataControls == null) 
                    _dataControls = new Dictionary<string, IRepeatingDataControl>();

                return _dataControls;
            }
        }

        public Dictionary<string, IGraphControl> GraphControls
        {
            get
            {
                if (_graphControls == null) 
                    _graphControls = new Dictionary<string, IGraphControl>();

                return _graphControls;
            }
        }

        public List<ICustomSchemaPublisher> CustomSchemaControls
        {
            get
            {
                if (_customSchemaControls == null) 
                    _customSchemaControls = new List<ICustomSchemaPublisher>();

                return _customSchemaControls;
            }
        }

        public Dictionary<string, ICustomContentPublisher> CustomContentControls
        {
            get
            {
                if (_customContentControls == null) 
                    _customContentControls = new Dictionary<string, ICustomContentPublisher>();

                return _customContentControls;
            }
        }

        public Dictionary<string, IMediaControl> MediaControls
        {
            get
            {
                if (_mediaControls == null)
                    _mediaControls = new Dictionary<string, IMediaControl>();

                return _mediaControls;
            }
        }

        public List<IContainerWithSummary> SummaryContainers
        {
            get
            {
                if (_summaryContainers == null) 
                    _summaryContainers = new List<IContainerWithSummary>();

                return _summaryContainers;
            }
        }
        #endregion

        #region Optimsation
        private OptimiseTiming _optimisation = OptimiseTiming.Auto;
        private bool _optimiseDone = false;
        private List<string> _optimiseElements;

        public OptimiseTiming Optimisation
        {
            get { return _optimisation; }
            set { _optimisation = value; }
        }

        private List<string> OptimiseElements
        {
            get
            {
                if (_optimiseElements == null)
                    _optimiseElements = new List<string>();

                return _optimiseElements;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Initialize the fields
        /// </summary>
        /// <param name="xml"></param>
        public void Initialise(XElement xml)
        {
            if (!ActivityName.IsNullOrEmpty())
                CreateServerActivity(xml);
        }

        public void CreateServerActivity(XElement xml)
        {
            int InitialKey = 0; 
            int temp;

            IsLoading = true;

            if (xml != null && xml.Attribute(Common.KeyValueKey) != null && xml.Attribute(Common.KeyValueKey).Value.Length > 0)
            {                
                if (int.TryParse(xml.Attribute(Common.KeyValueKey).Value, out temp))
                    InitialKey = temp;
            }
            else if (Key!=null && int.TryParse(Key,out temp))
            {
                InitialKey = temp; 
            }

            var app = ApplicationEx.Instance;
            app.RequestActivity(this._container, ActivityName, ActivityStyle, InitialKey, PublishActivity);
        }

        public void CopyFrom(ActivityHarness ah)
        {
            _activityStamp = ah.ActivityStamp;
            IsActivityOwner = ah.IsActivityOwner;

            if (ah.FixedContext != null)
                this.FixedContext = ah.FixedContext;

            ApplicationEx.Instance.GetSchema(this);
            Schema = XDocument.Parse(ApplicationEx.Instance.ServerApplicationService.Response.ToString());
            XElement act = (XElement)Schema.Document.Root.FirstNode;

            PublishResponse(act);

            if (_customSchemaControls != null)
            {
                IEnumerator<ICustomSchemaPublisher> e = CustomSchemaControls.GetEnumerator();

                while (e.MoveNext())
                {
                    e.Current.CustomPublishSchema(act, this.FixedContext);
                }
            }

            this._duplicateIndex = ApplicationEx.Instance.RegisterActivityCopy(this._container);
        }

        /// <summary>
        /// Deinitialize activity members.
        /// </summary>
        public void ActivityClosed()
        {
            if (_activityStamp != null)
            {
                // When closing the form, inform the server that the activity is closing.
                XElement closeElement = new XElement(Common.Requests.ActivityClose);
                closeElement.SetAttribute(Common.ActivityHandle, _activityStamp);

                SendServerMessage(closeElement);

                if (ApplicationEx.Instance.OpenActivities.ContainsKey(_activityStamp))
                    ApplicationEx.Instance.OpenActivities.Remove(_activityStamp);
            }
          
            _schema = null;
            _activityStamp = null;

            if (ApplicationEx.Instance.CurrentActivity == this)
                ApplicationEx.Instance.CurrentActivity = null;
        }
        #endregion

        #region Publish To Server
        private void PublishActivity(bool success, XDocument response)
        {
            if (success)
            {
                IsActivityOwner = true;
                Schema = XDocument.Parse(response.ToString());
                XElement act = (XElement)Schema.Document.Root.FirstNode;

                while (act != null)
                {
                    if (act.Attribute(Common.ActivityHandle) != null)
                        _activityStamp = act.Attribute(Common.ActivityHandle).Value;
                    
                    if (act.Attribute("fixedContext") != null)
                        FixedContext = act.Attribute("fixedContext").Value;

                    //PublishResponse(act); // TEMP CA

                    if (_customSchemaControls != null)
                    {
                        IEnumerator<ICustomSchemaPublisher> e = CustomSchemaControls.GetEnumerator();

                        while (e.MoveNext())
                        {
                            e.Current.CustomPublishSchema(act, this.FixedContext);
                        }
                    }

                    ApplicationEx.Instance.RegisterActivity(this._container);
                    act = (XElement)act.NextNode;
                }
            }
        }

        public void AppendDataPublicationsToActivityRequest(XElement request)
        {
            IEnumerator<IRepeatingDataControl> dataControlEnumerator = DataControls.Values.GetEnumerator();

            while (dataControlEnumerator.MoveNext())
            {
                XElement dp = new XElement(Common.DataPublication); //request.OwnerDocument.CreateElement(Common.DataPublication);
                dp.SetAttributeValue(Common.IDAttrib, dataControlEnumerator.Current.DataId);

                if (!string.IsNullOrEmpty(dataControlEnumerator.Current.PopulateMethod) && dataControlEnumerator.Current.PopulateMethod.ToLower() != Common.None)
                    dp.SetAttributeValue(Common.Data.PublicationMethod, dataControlEnumerator.Current.PopulateMethod);

                if (!string.IsNullOrEmpty(dataControlEnumerator.Current.QueryId))
                    dp.SetAttributeValue(Common.Data.PublicationQuery, dataControlEnumerator.Current.QueryId);

                if (!string.IsNullOrEmpty(dataControlEnumerator.Current.AutoPopulate) && dataControlEnumerator.Current.AutoPopulate.ToLower()!=Common.None)
                    dp.SetAttributeValue(Common.Data.AutoPopulate, dataControlEnumerator.Current.AutoPopulate);

                if (!string.IsNullOrEmpty(dataControlEnumerator.Current.ModelObject))
                    dp.SetAttributeValue(Common.contextObject, dataControlEnumerator.Current.ModelObject);

                dataControlEnumerator.Current.FillServerRegistrationXml(dp);

                //request.AppendChild(dp);
                request.Add(dp);
            }

            IEnumerator<IMediaControl> mediaControlEnumerator = MediaControls.Values.GetEnumerator();

            while (mediaControlEnumerator.MoveNext())
            {
                XElement dp = new XElement(Common.FieldNode); //request.OwnerDocument.CreateElement(Common.DataPublication);
                dp.SetAttributeValue(Common.IDAttrib, mediaControlEnumerator.Current.DataId);

                if (!string.IsNullOrEmpty(mediaControlEnumerator.Current.PublishType.ToString()))
                    dp.SetAttributeValue(Common.Data.Publish, mediaControlEnumerator.Current.PublishType.ToString());

                request.Add(dp);
            }

            if (!_optimiseDone && this.Optimisation == OptimiseTiming.Auto && ApplicationEx.Instance.FormOptimisations.ContainsKey(_container.ImplementationName))
            {
                List<string> al = ApplicationEx.Instance.FormOptimisations[_container.ImplementationName];
                IEnumerator<string> myEnum = al.GetEnumerator();

                while (myEnum.MoveNext())
                {
                    XElement elt = new XElement(Common.Requests.FieldMetadata); //ApplicationEx.Instance.Request.CreateElement(Common.Requests.FieldMetadata);
                    elt.SetAttributeValue(Common.IDAttrib, myEnum.Current);
                    elt.SetAttributeValue(Common.PublishFieldMasked, "1");
                    //request.AppendChild(elt);
                    // comment it because it add extra XML we need to know what's the use of it?????????
                    //request.Add(elt);
                }
                _optimiseDone = true;
            }
        }
        #endregion

        #region Publish Response to Controls
        public virtual void PublishResponse(XElement publishElt)
        {
            if (publishElt == null) 
                return;

            Publishing = true;

            if (publishElt.Attribute(Common.ActivityPersistentId) != null)
            {
                int id;
                int.TryParse(publishElt.Attribute(Common.ActivityPersistentId).Value, out id);
                PersistentId = id;
            }

            PublishMessage(null); // Bit of a hack to notify the message panels (particularly the MessageLabel control) to reset themselves for a new message

            if (publishElt.Attribute(Common.Dirty) != null)
                _container.PublishDirtyChange("", Common.boolValue(publishElt.Attribute(Common.Dirty).Value));

            // Publish Data nodes to controls first so list based items are populated before their value arrives
            PublishResponseDataNodes(publishElt);

            _dirtyContainers = new List<IContainerWithSummary>();

            // Now do the other elements
            PublishResponseNonDataNodes(publishElt);

            PublishResponseSetFocus(publishElt);
            ResponseOptimisations();
            RefreshSummaryContainers();

            Publishing = false;
            IsLoading = false;
        }
  
        private void PublishResponseDataNodes(XElement publishElt)
        {
            IEnumerable<XElement> DataNodes = publishElt.Elements(Common.Data.Node);
            IEnumerator ie = DataNodes.GetEnumerator();

            while (ie.MoveNext())
            {
                try
                {
                    XElement data = (XElement)ie.Current;
                    string id = string.Empty;

                    if (data.Attribute(Common.IDAttrib) != null)
                        id = data.Attribute(Common.IDAttrib).Value;

                    if (id == Common.Picklist)
                    {
                        #if WPF
                          var PL = new PickListWindow(this, data);
                          PL.ShowDialog();
                        #endif
                    }
                    else
                    {
                        bool process = true;

                        if (FixedContext != null)
                        {
                            if (id.StartsWith(FixedContext))
                                id = id.Remove(0, FixedContext.Length + 1);
                        }

                        if (process)
                        {
                            if (DataControls.ContainsKey(id))
                            {
                                if (DataControls[id] is ExpanzControlDetails)
                                {
                                    // Control has not registered itself with the activity yet. Cache the data for when it does.
                                    _controlDataCache[id] = data;
                                }
                                else
                                {
                                    // Sometimes data controls need to know the field info both before and after 
                                    // it has been fed the data. Therefore, find the corresponding field info
                                    // (if a field ID has been assigned), and pass that field info to the control
                                    // to process before the data is published to it.
                                    if (DataControls[id].FieldId != null)
                                    {
                                        // Find the corresponding field information for this control
                                        XElement fieldElement = publishElt.Elements(Common.FieldNode).Where(x => x.Attribute(Common.IDAttrib).Value == DataControls[id].FieldId).FirstOrDefault();

                                        if (fieldElement != null)
                                        {
                                            DataControls[id].PreDataPublishXml(fieldElement);
                                        }
                                    }


                                    DataControls[id].PublishData(data);
                                }
                            }
                            //else if (this.container is ViewModelBase)
                            //{
                            //    var vm = this.container as ViewModelBase;
                            //    vm.PublishData(data);
                            //}
                        }
                    }
                }
                catch (Exception e) 
                {
                    Logging.LogException(e); 
                }
            }
        }

        private void PublishResponseNonDataNodes(XElement publishElt)
        {
            // Now do the other elements
            XElement currentElt = (XElement)publishElt.FirstNode;

            while (currentElt != null)
            {
                try
                {
                    switch (currentElt.Name.ToString())
                    {
                        case Common.FieldNode:
                            PublishResponseField(currentElt);
                            break;

                        case Common.Requests.MethodInvocation:
                            PublishResponseMethodInvocation(currentElt);
                            break;

                        case Common.Data.Node:
                            PublishResponseNode(currentElt);
                            break;

                        case Common.MessagesNode:
                            XElement messageElt = (XElement)currentElt.FirstNode;

                            while (messageElt != null)
                            {
                                PublishMessage(messageElt);
                                messageElt = (XElement)messageElt.NextNode;
                            }

                            break;

                        case Common.UIMessage.Node:
                            AppHost.PublishUIMessage(currentElt);
                            break;

                        case Common.ModelObject:	
                            // Publishes dirty state
                            bool dirty = Common.boolValue(currentElt.Attribute(Common.Dirty).Value);
                            string mo = currentElt.Attribute(Common.IDAttrib).Value;

                            _container.PublishDirtyChange(mo, dirty);

                            break;

                        case Common.Graph:
                            string id = currentElt.Attribute(Common.IDAttrib).Value;

                            if (GraphControls.ContainsKey(id))
                                GraphControls[id].PublishGraph(currentElt);

                            break;

                        case Common.Requests.ContextMenu:
                            if (ContextMenuPublisher == null)
                                ApplicationEx.Instance.DisplayMessageBox("No publisher for context menu", "Error");
                            else
                                ContextMenuPublisher.PublishContextMenu(currentElt);

                            break;

                        case Common.CustomContent:
                            //id = currentElt.Attribute(Common.IDAttrib).Value;

                            //if (CustomContentControls.ContainsKey(id))
                            //    CustomContentControls[id].publishCustomContent(currentElt);

                            break;
                    }
                }
                catch (Exception e)
                {
                    Logging.LogException(e);
                }

                currentElt = (XElement)currentElt.NextNode;
            }
        }

        private void PublishResponseField(XElement publishElement)
        {
            bool process = true;
            bool processed = false;
            string childID = string.Empty;

            if (publishElement.Attribute(Common.IDAttrib) != null)
                childID = publishElement.Attribute(Common.IDAttrib).Value;

            if (FixedContext != null)
            {
                if (childID.StartsWith(FixedContext) && childID != FixedContext)
                {
                    childID = childID.Remove(0, FixedContext.Length + 1);
                }
                else
                {
                    if (childID.IndexOf(".") < 0) 
                        process = false;
                }
            }

            if (process)
            {
                var publishControls = Controls.Where(x => x.FieldId == childID);

                foreach (IServerBoundControl control in publishControls)
                {
                    PublishResponseToControl(publishElement, control);
                    processed = true;
                }
            }

            // Check for registered container
            if (!processed)
            {
                if (childID.IndexOf(".") > 0)
                {
                    int p = childID.IndexOf(".");
                    string prefix = childID.Substring(0, p);

                    if (ControlContainers.ContainsKey(prefix))
                    {
                        ControlContainers[prefix].PublishXml(publishElement);
                        processed = true;
                    }
                }

                if (!processed)
                {
                    if (!this._optimiseDone && (Optimisation == OptimiseTiming.Post || Optimisation == OptimiseTiming.Auto) && !OptimiseElements.Contains(childID))
                        OptimiseElements.Add(childID);
                }
            }
        }
        
        private void PublishResponseNode(XElement publishElement)
        {
            string childID = string.Empty;

            if (publishElement.Attribute(Common.IDAttrib) != null)
                childID = publishElement.Attribute(Common.IDAttrib).Value;

            if (FixedContext != null)
            {
                if (childID.StartsWith(FixedContext) && childID != FixedContext)
                    childID = childID.Remove(0, FixedContext.Length + 1);
            }

            var publishControls = Controls.Where(x => x.FieldId == childID);

            foreach (IServerBoundControl control in publishControls)
            {
                PublishResponseToControl(publishElement, control);
            }
        }
        
        private void PublishResponseMethodInvocation(XElement publishElement)
        {
            string childID = string.Empty;

            if (publishElement.Attribute(Common.IDAttrib) != null)
                childID = publishElement.Attribute(Common.IDAttrib).Value;

            if (FixedContext != null)
            {
                if (childID.StartsWith(FixedContext) && childID != FixedContext)
                    childID = childID.Remove(0, FixedContext.Length + 1);
            }

            if (Buttons.ContainsKey(childID))
            {
                IMethodCaller c = Buttons[childID];

                if (publishElement.Attribute(Common.State) != null)
                {
                    string state = publishElement.Attribute(Common.State).Value;

                    if (state == Common.PublishFieldDisabled)
                    {
                        c.SetDisabled(true);
                    }
                    else if (state == Common.PublishFieldHidden)
                    {
                        c.SetVisible(false);
                    }
                    else
                    {
                        c.SetVisible(true);
                        c.SetDisabled(false);
                    }
                }
            }
        }

        private void PublishResponseSetFocus(XElement publishElt)
        {
            if (publishElt.Attribute(Common.focusField) != null)
            {
                string field = publishElt.Attribute(Common.focusField).Value;

                if (FixedContext != null)
                {
                    if (field.StartsWith(FixedContext))
                        field = field.Remove(0, FixedContext.Length + 1);
                }

                if (Controls.Count(x => x.FieldId == field) != 0)
                {
                    var control = Controls.First(x => x.FieldId == field) as Control;

                    if (control != null)
                        control.Focus();
                }
                else if (DataControls.ContainsKey(field))
                {
                    var control = DataControls[field] as Control;

                    if (control != null)
                        control.Focus();
                }
            }
        }

        private void ResponseOptimisations()
        {
            if (!this._optimiseDone && (Optimisation == OptimiseTiming.Post || Optimisation == OptimiseTiming.Auto) && OptimiseElements.Count > 0)
            {
                XElement[] elts = new XElement[OptimiseElements.Count];
                IEnumerator<String> myEnum = OptimiseElements.GetEnumerator();
                int i = 0;

                while (myEnum.MoveNext())
                {
                    XElement elt = new XElement(Common.Requests.FieldMetadata); //ApplicationEx.Instance.Request.CreateElement(Common.Requests.FieldMetadata);
                    elt.SetAttributeValue(Common.IDAttrib, myEnum.Current);
                    elt.SetAttributeValue(Common.PublishFieldMasked, "1");
                    elts[i] = elt;
                    i++;
                }

                // Need to set optimiseDone to true before sendXml, or infinite loop occurs
                this._optimiseDone = true;

                if (Optimisation == OptimiseTiming.Auto && !ApplicationEx.Instance.FormOptimisations.ContainsKey(_container.ImplementationName))
                    ApplicationEx.Instance.FormOptimisations.Add(_container.ImplementationName, OptimiseElements);

                this.Exec(elts);
            }
        }

        private void RefreshSummaryContainers()
        {
            foreach (IContainerWithSummary cs in _dirtyContainers)
            {
                cs.RefreshSummaryText(true);
            }
        }

        private void PublishResponseToControl(XElement publishElement, IServerBoundControl control)
        {
            try
            {
                PublishResponseToServerControl(publishElement, control);

                control.PublishXml(publishElement);

                if (control is IValueDestination)
                    PublishResponseToValueDestinationControl(control, publishElement);

                if (control is IEditableControl)
                    PublishResponseToEditableControl(control, publishElement);

                if (control is IFieldErrorMessage)
                    PublishResponseToFieldErrorMessageControl(control, publishElement);
                
                if (control is IFieldLabel)
                    PublishResponseToFieldLabelControl(control, publishElement);
            }
            catch (Exception e)
            {
                Logging.LogException(e);
            }
        }
  
        private void PublishResponseToServerControl(XElement publishElement, IServerBoundControl control)
        {
            bool? hidden = null;

            if (publishElement.Attribute(Common.PublishFieldHidden) != null)
            {
                hidden = Common.boolValue(publishElement.Attribute(Common.PublishFieldHidden).Value);
                control.SetVisible(!(bool)hidden);
            }
        }

        private void PublishResponseToValueDestinationControl(IServerBoundControl control, XElement publishElement)
        {
            var valueControl = control as IValueDestination;

            if (publishElement.Attribute(Common.PublishFieldHint) != null)
            {
                string hint = publishElement.Attribute(Common.PublishFieldHint).Value;
                valueControl.SetHint(hint);
            }

            if (publishElement.Attribute(Common.ValueIsNull) != null && Common.boolValue(publishElement.Attribute(Common.ValueIsNull).Value))
            {
                valueControl.SetNull();

                if (control is IContainerSummaryText)
                    CheckDirtySummaryContainer((IContainerSummaryText)control);
            }
            else if (publishElement.Attribute(Common.PublishFieldValue) != null)
            {
                string textVal = publishElement.Attribute(Common.PublishFieldValue).Value;

                if (textVal == Common.LongDataValueIndicator)
                    textVal = publishElement.Value;

                valueControl.SetValue(textVal);

                if (control is IContainerSummaryText)
                    CheckDirtySummaryContainer((IContainerSummaryText)control);
            }
        }

        private void PublishResponseToEditableControl(IServerBoundControl control, XElement publishElement)
        {
            IEditableControl editableControl = (IEditableControl)control;

            if (publishElement.Attribute(Common.PublishFieldDisabled) != null)
                editableControl.SetEditable(!Common.boolValue(publishElement.Attribute(Common.PublishFieldDisabled).Value));

            if (HasPropertyFields)
            {
                string id = publishElement.Attribute(Common.IDAttrib).Value;

                if (FixedContext != null)
                    id = id.Substring(FixedContext.Length + 1);

                if (PropertyFields.ContainsKey(id))
                {
                    IDictionaryEnumerator de = PropertyFields[id].GetEnumerator();

                    while (de.MoveNext())
                    {
                        foreach (XAttribute a in publishElement.Attributes())
                        {
                            if (a.Name == de.Value.ToString())
                            {
                                IEditableControl ec = (IEditableControl)de.Key;
                                ec.SetValue(a.Value);
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void PublishResponseToFieldErrorMessageControl(IServerBoundControl control, XElement publishElement)
        {
            IFieldErrorMessage errorMessageControl = (IFieldErrorMessage)control;

            if (publishElement.Attribute(Common.valid) != null && !IsLoading)
            {
                bool isFieldValueValid = Common.boolValue(publishElement.Attribute(Common.valid).Value);

                errorMessageControl.IsFieldValueValid = isFieldValueValid;

                if (isFieldValueValid)
                    errorMessageControl.HideError();
            }
        }

        private void PublishResponseToFieldLabelControl(IServerBoundControl control, XElement publishElement)
        {
            var labelControl = control as IFieldLabel;
            string label = publishElement.GetAttributeValue(Common.PublishFieldLabel);

            if (label == null && string.IsNullOrEmpty(labelControl.LabelText))
                label = control.FieldId;

            if (label != null)
                labelControl.SetLabel(label);

            //if (publishElement.Attribute(Common.PublishFieldLabel) != null)
            //{
            //    //check for a field with this id + _label
            //    string l = control.FieldId.Replace(".", "_") + "_label";
            //    /**/
            //    object o = _visualContainer.FindName(l);
            //    if (o != null)
            //    {
            //        UIElement fieldUI = (UIElement)control;
            //        FrameworkElement labelUI = (FrameworkElement)o;

            //        if (o is LabelEx)
            //            ((LabelEx)o).Target = fieldUI as FrameworkElement;

            //        if (o is IFieldLabel)
            //        {
            //            IFieldLabel L = (IFieldLabel)o;
            //            L.SetLabel(label);
            //        }

            //        //regardless of whether its a 'managed' label, bind its visibility to field
            //        Binding b = new Binding();
            //        b.Source = fieldUI;
            //        b.Path = new PropertyPath("Visibility");
            //        labelUI.SetBinding(FrameworkElement.VisibilityProperty, b);
            //    }
            //}
        }

        private void CheckDirtySummaryContainer(IContainerSummaryText c)
        {
            if (HasSummaryContainers)
            {
                foreach (IContainerWithSummary sc in SummaryContainers)
                {
                    if (sc.SummaryChildren != null && sc.SummaryChildren.Contains(c))
                    {
                        if (!_dirtyContainers.Contains(sc))
                            _dirtyContainers.Add(sc);
                    }
                }
            }
        }

        /// <summary>
        /// Publish message to activity's message panel.
        /// </summary>
        /// <param name="messageElt"></param>
        public void PublishMessage(XElement messageElt)
        {
            if (messageElt != null)
            {
                if (messageElt.Attribute("source") != null)
                {
                    string sourceFieldId = messageElt.Attribute("source").Value;

                    var publishControls = Controls.Where(x => x.FieldId == sourceFieldId);

                    foreach (IServerBoundControl control in publishControls)
                    {
                        if (control is IFieldErrorMessage)
                            ((IFieldErrorMessage)control).ShowError(messageElt);
                    }
                }

                // Add messsage to Activity message panel
                if (MessagePanel != null)
                {
                    MessagePanel.PublishMessage(messageElt);
                }
                else
                {
#if WINDOWS_PHONE
                    MessageNotification.PublishMessage(messageElt);
#else
                    Console.WriteLine(messageElt.Value);
                    ApplicationEx.Instance.DisplayMessageBox(messageElt.Value, "Error");
#endif
                }
            }
            else if (MessagePanel != null)
            {
                MessagePanel.Clear();
            }
        }
        #endregion

        #region Control Registration
        /// <summary>
        /// This is kind of a hack to work around the issue in Silverlight where you don't know when
        /// all the controls have been loaded in a page (so you can *then* make a call to the server).
        /// All the XAML pages are parsed when the application is loaded to extract the expanz controls,
        /// and here we'll register them *before* they are actually loaded so that requests can be made
        /// to the server, and then these entries will be updated with the actual control references
        /// when the controls are actually loaded.
        /// </summary>
        internal void RegisterUnloadedControls(List<ExpanzControlDetails> expanzControls)
        {
            var controlsToRegister = expanzControls.Where(x => x.IsDataControl);

            foreach (ExpanzControlDetails controlDetails in controlsToRegister)
            {
                if (controlDetails.DataId != null && !DataControls.ContainsKey(controlDetails.DataId))
                    DataControls.Add(controlDetails.DataId, controlDetails); // Add temporary dummy entry
            }
            
            controlsToRegister = expanzControls.Where(x => x.IsMediaControl);

            foreach (ExpanzControlDetails controlDetails in controlsToRegister)
            {
                if (controlDetails.DataId != null && !DataControls.ContainsKey(controlDetails.DataId))
                    MediaControls.Add(controlDetails.DataId, controlDetails); // Add temporary dummy entry
            }
        }

        public void RegisterControl(object control)
        {
            if (control is IServerBoundControlContainer)
                RegisterControlContainer((IServerBoundControlContainer)control);
            
            if (control is IServerBoundControl)
                RegisterServerBoundControl((IServerBoundControl)control);

            if (control is IRepeatingDataControl)
                RegisterDataControl((IRepeatingDataControl)control);
            
            if (control is IGraphControl)
                RegisterGraphControl((IGraphControl)control);
            
            if (control is ICustomContentPublisher)
                RegisterCustomControl((ICustomContentPublisher)control);
            
            if (control is ICustomSchemaPublisher)
                RegisterCustomSchemaPublisher((ICustomSchemaPublisher)control);
            
            if (control is IMethodCaller)
                RegisterServerBoundButton((IMethodCaller)control);
            
            if (control is IMessageDisplay)
                RegisterActivityMessagePanel((IMessageDisplay)control);
            
            if (control is IMediaControl)
                RegisterMediaControl((IMediaControl)control);
        }

        private void RegisterServerBoundControl(IServerBoundControl control)
        {
            string id = control.FieldId;

            if (id != null)
            {
                // Check for attribute suffix, field_@DayOfWeek or field_@label
                if (control is IEditableControl && id.IndexOf(".@") > 0)
                {
                    int p = id.IndexOf(".@");

                    if (p + 2 < id.Length)
                    {
                        string property = id.Substring(p + 2);

                        id = id.Substring(0, p);

                        if (!PropertyFields.ContainsKey(id))
                            PropertyFields.Add(id, new Dictionary<IEditableControl, string>());

                        PropertyFields[id].Add((IEditableControl)control, property);
                    }
                }

                Controls.Add(control);
            }
        }

        private void RegisterServerBoundButton(IMethodCaller control)
        {
            if (!string.IsNullOrEmpty(control.MethodName))
                Buttons[control.MethodName] = control;
        }

        private void RegisterActivityMessagePanel(IMessageDisplay messagePanel)
        {
            this.MessagePanel = messagePanel;
        }

        private void RegisterControlContainer(IServerBoundControlContainer control)
        {
            if (!string.IsNullOrEmpty(control.FieldId))
                ControlContainers[control.FieldId] = control;
        }

        private void RegisterDataControl(IRepeatingDataControl control)
        {
            if (!string.IsNullOrEmpty(control.DataId))
            {
                DataControls[control.DataId] = control;

                // If this control was late in registering itself (data has already been
                // received from the server) then publish the data from the cache to it.
                if (_controlDataCache.ContainsKey(control.DataId))
                {
                    control.PublishData(_controlDataCache[control.DataId]);
                    _controlDataCache.Remove(control.DataId);
                }
            }
        }

        private void RegisterGraphControl(IGraphControl control)
        {
            if (!string.IsNullOrEmpty(control.GraphId))
                GraphControls[control.GraphId] = control;
        }

        private void RegisterCustomControl(ICustomContentPublisher control)
        {
            if (!string.IsNullOrEmpty(control.ContentId))
                CustomContentControls[control.ContentId] = control;
        }

        private void RegisterCustomSchemaPublisher(ICustomSchemaPublisher control)
        {
            CustomSchemaControls.Add(control);
        }

        private void RegisterMediaControl(IMediaControl control)
        {
            if (!string.IsNullOrEmpty(control.DataId))
                MediaControls[control.DataId] = control;
        }

        public void RegisterSummaryContainer(IContainerWithSummary container)
        {
            SummaryContainers.Add(container);
        }
        #endregion

        #region Server Communication
        /// <summary>
        /// function to instantiate server execution call
        /// </summary>
        /// <param name="DeltaXml"></param>
        public void Exec(XElement DeltaXml)
        {
            Exec(new XElement[] { DeltaXml });
        }

        /// <summary>
        /// function to instantiate server execution call
        /// </summary>
        /// <param name="DeltaXml"></param>
        public void Exec(XElement[] DeltaXml)
        {
            if (_activityStamp.IsNullOrEmpty())
                ApplicationEx.Instance.DisplayMessageBox("This Activity does not have a handle and will fail.", "Error");

            XElement activityElement = new XElement(Common.ActivityNode); //DeltaXml[0].OwnerDocument.CreateElement(Common.ActivityNode);
            activityElement.SetAttributeValue(Common.ActivityHandle, _activityStamp);

            int i = 0;

            while (i < DeltaXml.Length)
            {
                activityElement.Add(DeltaXml[i]);
                i++;
            }

            SendServerMessage(activityElement);
        }

        private void SendServerMessage(XElement payload)
        {
            var app = ApplicationEx.Instance;
            app.ServerApplicationService.CurrentActivityDupIndex = _duplicateIndex;
            app.CurrentActivity = this;
            app.ServerApplicationService.SendRequestToServerPortal(payload, null);
        }
        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged; 
        #endregion
    }
}
