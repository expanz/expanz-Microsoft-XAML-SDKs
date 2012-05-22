using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows;
using System.Windows.Resources;
using System.Xml.Linq;
using Expanz.ThinRIA.Core;
using Expanz.Extensions.BCL;

namespace Expanz.ThinRIA
{
    internal class XAMLFileDetailsCollection : Dictionary<string, XAMLFileDetails>
    {
        internal XAMLFileDetailsCollection()
        {
#if !WPF
            HarvestExpanzControls();
#endif
        }

        private void HarvestExpanzControls()
        {
            if (!ApplicationEx.IsDesignMode)
            {
                Assembly assembly = Application.Current.GetType().Assembly;
                //Assembly assembly = Assembly.GetCallingAssembly();
                string[] embeddedResourceNames = assembly.GetManifestResourceNames();
                string assemblyName = assembly.FullName.Substring(0, assembly.FullName.IndexOf(','));

                foreach (var resource in embeddedResourceNames)
                {
                    ResourceManager rm = new ResourceManager(resource.Replace(".resources", ""), assembly); // All resources have “.resources” in the name – so we need to get rid of it

                    Stream dummy = rm.GetStream("app.xaml"); // Seems like some issue here, but without getting any real stream next statement doesn't work....

                    ResourceSet rs = rm.GetResourceSet(Thread.CurrentThread.CurrentUICulture, false, true);

                    IDictionaryEnumerator enumerator = rs.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        string resourceName = (string)enumerator.Key;

                        if (resourceName.EndsWith(".xaml", System.StringComparison.InvariantCultureIgnoreCase))
                        {
                            try
                            {
                                Uri uri = new Uri(assemblyName + ";component/" + resourceName, UriKind.Relative);
                                StreamResourceInfo streamInfo = Application.GetResourceStream(uri);
                                {
                                    XAMLFileDetails fileDetails = ParseXAMLFile(resourceName, streamInfo.Stream);

                                    if (fileDetails != null)
                                        this.Add(fileDetails.ClassName, fileDetails);
                                }
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                    }
                }
            }
        }

        private XAMLFileDetails ParseXAMLFile(string resourceName, Stream stream)
        {
            XAMLFileDetails fileDetails = null;
            XDocument document = XDocument.Load(stream);
            XNamespace defaultNamespace = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

            bool isResourceDictionary = (document.Root.Name == XName.Get("ResourceDictionary", defaultNamespace.NamespaceName));

            if (!isResourceDictionary)
            {
                fileDetails = new XAMLFileDetails(resourceName, document);
            }

            return fileDetails;
        } 
    }

    internal class XAMLFileDetails
    {
        public string FileName { get; set; }
        public string ClassName { get; set; }
        public List<ExpanzControlDetails> ExpanzControlDetails { get; set; }

        private readonly XNamespace defaultNamespace = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        private readonly XNamespace xNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

#if WINDOWS_PHONE
        private readonly XNamespace ecNamespace = "clr-namespace:Expanz.ThinRIA.Controls;assembly=Expanz.ThinRIA.Core.WP7";
#else
        private readonly XNamespace ecNamespace = "clr-namespace:Expanz.ThinRIA.Controls;assembly=Expanz.ThinRIA.Core.Silverlight";
#endif

        private const string NameAttribute = "Name";
        private const string FieldIdAttribute = "FieldId";
        private const string MethodNameAttribute = "MethodName";
        private const string PopulateMethodAttribute = "PopulateMethod";
        private const string QueryIdAttribute = "QueryId";
        private const string ModelObjectAttribute = "ModelObject";
        private const string AutoPopulateAttribute = "AutoPopulate";
        private const string PublishTypeAttribute = "PublishType";

        public XAMLFileDetails()
        {

        }

        public XAMLFileDetails(string fileName, XDocument document)
        {
            FileName = fileName;
            ParseXAMLFile(document);
        }

        /// <summary>
        /// Parses a XAML file, and extracts the details of all the expanz controls that it uses. This
        /// is then used by the activity harness to work out what data it requires from the server.
        /// </summary>
        public void ParseXAMLFile(XDocument document)
        {
            ExpanzControlDetails = new List<ExpanzControlDetails>();

            XAttribute className = document.Root.Attribute(XName.Get("Class", xNamespace.NamespaceName));
            ClassName = className.Value;

            var filteredControls = document.Root.DescendantNodes().OfType<XElement>().Where(x => x.Name.Namespace == ecNamespace);

            foreach (XElement control in filteredControls)
            {
                var controlDetails = new ExpanzControlDetails();
                controlDetails.ControlType = control.Name;

                controlDetails.Name = control.GetAttributeValue(NameAttribute) ?? control.GetAttributeValue(XName.Get(NameAttribute, xNamespace.NamespaceName));
                controlDetails.FieldId = control.GetAttributeValue(FieldIdAttribute);
                controlDetails.MethodName = control.GetAttributeValue(MethodNameAttribute);
                controlDetails.PopulateMethod = control.GetAttributeValue(PopulateMethodAttribute);
                controlDetails.QueryId = control.GetAttributeValue(QueryIdAttribute);
                controlDetails.ModelObject = control.GetAttributeValue(ModelObjectAttribute);
                controlDetails.AutoPopulate = control.GetAttributeValue(AutoPopulateAttribute);

                if (!string.IsNullOrEmpty(control.GetAttributeValue(PublishTypeAttribute)))
                    controlDetails.PublishType = (MediaPublishTypes)Enum.Parse(typeof(MediaPublishTypes), control.GetAttributeValue(PublishTypeAttribute), true);

                ExpanzControlDetails.Add(controlDetails);
            }
        }
    }

    internal class ExpanzControlDetails : IRepeatingDataControl, IMediaControl
    {
        public XName ControlType { get; set; }
        public string Name { get; set; }
        public string FieldId { get; set; }
        public string MethodName { get; set; }
        public string PopulateMethod { get; set; }
        public string QueryId { get; set; }
        public string ModelObject { get; set; }
        public string AutoPopulate { get; set; }
        public MediaPublishTypes PublishType { get; set; }
        //public string ActivityName { get; set; }
        //public string ActivityStyle { get; set; }

        public string DataId
        {
            get 
            {
                string dataId = null;

                if (IsDataControl)
                {
                    dataId = Name ?? FieldId ?? ModelObject ?? QueryId ?? PopulateMethod;
                }
                else if (IsMediaControl)
                {
                    dataId = FieldId ?? Name;
                }
                else
                {
                    throw new System.NotImplementedException(); // TODO
                }

                return dataId;
            }
        }

        public bool IsServerBoundControl
        {
            get 
            {
                var implementingTypes = new string[] { "DataGridEx", "ListBoxEx", "ComboBoxEx", "TextBoxEx", "DatePicker", "CheckBoxEx", "LabelEx", "TextBlockEx", "DataGridWithEditButtons", "ListBoxWithEditButtons", "LaunchURLButton", "HyperlinkTextBox" };
                return implementingTypes.Contains(ControlType.LocalName); 
            }
        }

        public bool IsDataControl
        {
            get
            {
                var implementingTypes = new string[] { "DataGridEx", "ListBoxEx", "ComboBoxEx", "DataGridWithEditButtons", "ListBoxWithEditButtons" };
                return implementingTypes.Contains(ControlType.LocalName);
            }
        }

        public bool IsMediaControl
        {
            get
            {
                var implementingTypes = new string[] { "ImageEx" };
                return implementingTypes.Contains(ControlType.LocalName);
            }
        }

        public bool IsEditableControl
        {
            get
            {
                var implementingTypes = new string[] { "DataGridEx", "ListBoxEx", "ComboBoxEx", "DatePicker", "TextBoxEx", "CheckBoxEx", "DataGridWithEditButtons", "ListBoxWithEditButtons" };
                return implementingTypes.Contains(ControlType.LocalName);
            }
        }

        //public bool IsMethodCaller
        //{
        //    get { return new string[] { "ButtonEx", "HyperlinkButtonEx" }.Contains(ControlType.LocalName); }
        //}

        //public bool IsMessagePanel
        //{
        //    get { return new string[] { "MessagePanel", "ValidationSummaryEx" }.Contains(ControlType.LocalName); }
        //}

        //public bool IsFieldLabel
        //{
        //    get { return new string[] { "LabelEx" }.Contains(ControlType.LocalName); }
        //}

        //public bool IsServerBoundControlContainer
        //{
        //    get { return new string[] { "" }.Contains(ControlType.LocalName); }
        //}

        //public bool IsGraphControl
        //{
        //    get { return new string[] { "" }.Contains(ControlType.LocalName); }
        //}

        //public bool IsCustomContentPublisher
        //{
        //    get { return new string[] { "" }.Contains(ControlType.LocalName); }
        //}

        //public bool IsCustomSchemaPublisher
        //{
        //    get { return new string[] { "" }.Contains(ControlType.LocalName); }
        //}

        public void PublishData(XElement xml)
        {
            
        }

        public void PreDataPublishXml(XElement xml)
        {

        }

        public void PublishXml(XElement xml)
        {
            
        }

        public void FillServerRegistrationXml(XElement xml)
        {
            
        }

        public void SetVisible(bool visible)
        {
            
        }
    }
}
