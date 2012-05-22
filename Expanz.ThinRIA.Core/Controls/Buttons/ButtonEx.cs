using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Expanz.ThinRIA.Core;

namespace Expanz.ThinRIA.Controls
{
    [Description("Use to call server methods on Model Objects. Set the ActivityName and optionally the ActivityStyle if required.")]
    [TemplateVisualState(Name = "Clean", GroupName = "DirtyStates")]
    [TemplateVisualState(Name = "Dirty", GroupName = "DirtyStates")]
    public class ButtonEx : Button, IMethodCaller
    {
        #region Constructor
#if WPF
        static ButtonEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ButtonEx), new FrameworkPropertyMetadata(typeof(ButtonEx)));
        }
#endif

        public ButtonEx() : base()
        {
            if (!ApplicationEx.IsDesignMode)
            {
                Loaded += ButtonEx_Loaded;
                Click += ButtonEx_Click;
            }

#if WPF
            //DefaultStyleKey = typeof(ButtonEx);
#endif
        } 
        #endregion
        
        #region Member Variables
        protected ControlHarness _controlHarness;
        private string _modelObject;
        #endregion

        #region Event Handlers
        private void ButtonEx_Loaded(object sender, RoutedEventArgs e)
        {
            InitHarness();
        }

        protected virtual void ButtonEx_Click(object sender, RoutedEventArgs e)
        {
            SetCursorProperty();
            CallButtonMethod();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// This function will create the server call in response to the button click
        /// </summary>
        public void CallButtonMethod()
        {
            if (!string.IsNullOrEmpty(MethodName))
            {
                // Build up method element
                var methodElement = _controlHarness.MethodElement;
                ComposeMethodXml(methodElement);

                //inspect for Datacontext to set server context
                if (this.DataContext != null)
                {
                    DynamicDataObject dynamicObject = this.DataContext as DynamicDataObject;
                    string contextKey = dynamicObject["id"].ToString();
                    string myContextType = dynamicObject["type"].ToString();

                    var elementList = new XElement[2];

                    var contextElement = CreateRequestElement(Common.Requests.SetContext);
                    contextElement.SetAttributeValue(Common.IDAttrib, contextKey);
                    contextElement.SetAttributeValue(Common.Data.RowType, myContextType);
                    contextElement.SetAttributeValue(Common.SetIdFromContextAttribute, "1");

                    SetContextObject(contextElement);
                    elementList[0] = contextElement;
                    elementList[1] = methodElement;
                    _controlHarness.SendXml(elementList);
                }
                else
                {
                    _controlHarness.SendXml(methodElement);
                }
            }       
        }

        public void InitHarness()
        {
            _controlHarness = new ControlHarness(this);
        }
        #endregion

        #region Private Methods
        protected virtual XElement CreateRequestElement(string elementName)
        {
            XElement returnElement = new XElement(elementName);

            if (this.ModelObject != null && this.ModelObject.Length > 0)
                returnElement.SetAttributeValue(Common.contextObject, this.ModelObject);

            return returnElement;
        }

        protected virtual void SetContextObject(XElement xml)
        {
            string context = null;

            if (!string.IsNullOrEmpty(this.ModelObject))
                context = this.ModelObject;

            if (context == null && !string.IsNullOrEmpty(_controlHarness.ParentActivity.FixedContext))
                context = _controlHarness.ParentActivity.FixedContext;

            if (context != null) 
                xml.SetAttributeValue(Common.contextObject, context);
        }

        private void SetCursorProperty()
        {
            //myHarness.AppHost.SetCursorProperty(Cursors.Wait);
        }

        protected virtual void ComposeMethodXml(XElement xml)
        {
            xml.SetAttributeValue(Common.RequestMethodName, MethodName);

            if (ModelObject.Length > 0)
                xml.SetAttributeValue(Common.contextObject, ModelObject);
        }
        #endregion

        #region Implementation of IMethodCaller
        [Category("expanz")]
        [Description("Set the method name of the Model Object to be called.")]
        public string MethodName { get; set; }

        public void SetVisible(bool visible)
        {
            Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public void SetDisabled(bool disable)
        {
            IsEnabled = !disable;
        }

        [Category("expanz")]
        [Description("Set the ModelObject to call the MethodName property on. This is only required if there is more than one ModelObject in the Activity")]
        public string ModelObject
        {
            get { return (string)GetValue(ModelObjectProperty); }
            set { SetValue(ModelObjectProperty, value); }
        }

        public static readonly DependencyProperty ModelObjectProperty =
            DependencyProperty.Register("ModelObject", typeof(string), typeof(ButtonEx), new PropertyMetadata(""));
        
        private bool? isDirtyButton;
        public bool IsDirtyButton
        {
            get
            {
                if (isDirtyButton.HasValue) return (bool)isDirtyButton;
                return MethodName != null && MethodName.ToLower().StartsWith("save");
            }
            set
            {
                isDirtyButton = value;
            }
        }

        public bool IsDirty
        {
            get { return (bool)GetValue(IsDirtyProperty); }
            set { SetValue(IsDirtyProperty, value); }
        }

        public static readonly DependencyProperty IsDirtyProperty =
            DependencyProperty.Register("IsDirty", typeof(bool), typeof(ButtonEx), new PropertyMetadata(IsDirtyPropertyChanged));

        private static void IsDirtyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ButtonEx)d).ChangeDirtyState((bool)e.NewValue);
        }

        private void ChangeDirtyState(bool isDirty)
        {
#if WPF
            // NOTE: Doesn't always work properly for some unknown reason
            if (isDirty)
            {
                bool success = VisualStateManager.GoToState(this, "Dirty", false);
                //Debug.Assert(success);
            }
            else
            {
                bool success = VisualStateManager.GoToState(this, "Clean", false);
                //Debug.Assert(success);
            }
#endif
        }
        #endregion
    }
}
