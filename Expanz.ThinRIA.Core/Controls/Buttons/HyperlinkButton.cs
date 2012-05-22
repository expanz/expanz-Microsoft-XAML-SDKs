using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Expanz.ThinRIA.Core;

namespace Expanz.ThinRIA.Controls
{
    public class HyperlinkButtonEx : HyperlinkButton, IMethodCaller
    {
        #region Constructor
        public HyperlinkButtonEx() : base()
        {
            Loaded += HyperlinkButtonEx_Loaded;
            Click += HyperlinkButtonEx_Click;
        } 
        #endregion

        #region Event Handlers
        private void HyperlinkButtonEx_Loaded(object sender, RoutedEventArgs e)
        {
            InitHarness();
        }

        protected virtual void HyperlinkButtonEx_Click(object sender, RoutedEventArgs e)
        {
            SetCursorProperty();
            CallButtonClickEvent();
        }
        #endregion

        #region Member Variables
        protected ControlHarness _controlHarness;
        private string _modelObject;
        #endregion

        #region Public Methods
        /// <summary>
        /// This function will create the server call in respond to different button click
        /// </summary>
        public void CallButtonClickEvent()
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

        public void InitHarness()
        {
            _controlHarness = new ControlHarness(this);
        }

        public bool IsDirty
        {
            set {  }
        }
        #endregion

        #region Private Methods
        private XElement CreateRequestElement(string elementName)
        {
            XElement returnElement = new XElement(elementName);

            if (this.ModelObject != null && this.ModelObject.Length > 0)
                returnElement.SetAttributeValue(Common.contextObject, this.ModelObject);

            return returnElement;
        }

        private void SetContextObject(XElement xml)
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
            get { return _modelObject ?? (_modelObject = string.Empty); }
            set { _modelObject = value; }
        }

        public bool DirtyState
        {
            set
            {
                if (value)
                {
                    if (!Content.ToString().EndsWith("*"))
                        Content = Content + "*";
                }
                else
                {
                    if (Content.ToString().EndsWith("*"))
                        Content = Content.ToString().Substring(0, Content.ToString().Length - 1);
                }
            }
        }

        private bool? isDirtyButton;
        public bool IsDirtyButton
        {
            get
            {
                if (isDirtyButton.HasValue) return (bool)isDirtyButton;
                return MethodName.Length >= 4 && MethodName.Substring(0, 4) == "save";
            }
            set
            {
                isDirtyButton = value;
            }
        }
        #endregion
    }
}
