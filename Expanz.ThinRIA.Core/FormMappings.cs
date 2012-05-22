using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows;
using System.Windows.Resources;
using System.Xml.Linq;
using Expanz.Extensions.BCL;

namespace Expanz.ThinRIA
{
    internal class FormMappings : Dictionary<string, FormDefinition>
    {
        private Dictionary<string, FormNamespace> _formNamespaces;

        public FormDefinition DefaultMapping { get; set; }

        public FormMappings()
        {
            XDocument doc = LoadFormMappings();
            ParseFormMappings(doc);
        }

        private XDocument LoadFormMappings()
        {
            XDocument doc = new XDocument();
            string fileName = FindFormMappingFileName();

            try
            {
                // Try loading it from the XAP file as a resource
                Assembly assembly = Application.Current.GetType().Assembly;
                string assemblyName = assembly.FullName.Substring(0, assembly.FullName.IndexOf(','));
                Uri uri = new Uri(assemblyName + ";component/" + fileName, UriKind.Relative);

                StreamResourceInfo streamInfo = Application.GetResourceStream(uri);
                {
                    doc = XDocument.Load(streamInfo.Stream);
                }
            }
            catch
            {
            }

            if (doc.Root == null)
            {
                // Resource not found. For backwards compatibility only - try loading it as content
                try
                {
                    doc = XDocument.Load(fileName);
                }
                catch { }
            }

            if (doc.Root == null)
            {
                //ApplicationEx.Instance.DisplayMessageBox("There is a problem with the form mappings at this site.  This is a fatal error.", "expanz ThinRIA");
                throw new FormMappingException("Form Mapping file could not be found. Ensure it is in your project, has a .FormMapping extension, and has its Build Action set to Resource");
            }

            return doc;
        }

        private void ParseFormMappings(XDocument doc)
        {
            var ns = doc.Root.Element("Namespaces");

            if (ns != null)
            {
                _formNamespaces = new Dictionary<string, FormNamespace>();

                foreach (XElement n in ns.Elements())
                {
                    if (n is XElement)
                    {
                        FormNamespace fns = new FormNamespace(n);
                        _formNamespaces.Add(fns.ID, fns);
                    }
                }
            }

            // Loading skin - pending

            // Loading form mappings
            try
            {
                var e = doc.Root.Elements("activity");

                foreach (XElement activityElement in e)
                {
                    FormDefinition formDefinition = new FormDefinition(activityElement);
                    this.Add(formDefinition.ID, formDefinition);

                    XAttribute defaultAttribute = activityElement.Attribute("default");

                    if (defaultAttribute != null)
                    {
                        bool defaultValue;
                        Boolean.TryParse(defaultAttribute.Value, out defaultValue);

                        if (defaultValue)
                            DefaultMapping = formDefinition;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new FormMappingException("An error occurred loading the form mappings", ex);
            }
        }

        /// <summary>
        /// Look for a file with a .FormMapping extension in the XAP file. If one isn't found,
        /// return FormMapping.xml just for backwards compatibility reasons.
        /// </summary>
        /// <returns></returns>
        private string FindFormMappingFileName()
        {
            string fileName = "FormMapping.xml"; // Just in case the old style name has been used, for backwards compatibility

            Assembly assembly = Application.Current.GetType().Assembly;

            string[] embeddedResourceNames = assembly.GetManifestResourceNames();
            
            foreach (var resource in embeddedResourceNames)
            {
                ResourceManager rm = new ResourceManager(resource.Replace(".resources", ""), assembly); // All resources have “.resources” in the name – so we need to get rid of it

                Stream dummy = rm.GetStream("app.xaml"); // Seems like some issue here, but without getting any real stream next statement doesn't work....

                ResourceSet rs = rm.GetResourceSet(Thread.CurrentThread.CurrentUICulture, false, true);

                IDictionaryEnumerator enumerator = rs.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    string resourceName = (string)enumerator.Key;

                    if (resourceName.EndsWith(".FormMapping", System.StringComparison.InvariantCultureIgnoreCase))
                    {
                        fileName = resourceName;
                        break;
                    }
                }
            }

            return fileName;
        }
        internal FormDefinition GetFormDefinition(string activity, string style)
        {
            string id = FormDefinition.MakeFormKey(activity, style);
            if (this.ContainsKey(id))
            {
                return this[id];
            }
            return null;
        }
        internal string GetFormFullName(string activity, string style)
        {
            FormDefinition formDefinition = GetFormDefinition(activity, style);
            if (formDefinition == null)
            {
                throw new FormMappingNotFoundException(string.Format("There is no form mapping definition for '{0}:{1}'", activity,style));
            }
            else
            {
                return GetFormFullName(formDefinition);
            }
        }

        internal string GetFormFullName(FormDefinition formDefinition)
        {
            try
            {
                FormNamespace formNamespace = _formNamespaces[formDefinition.NamespaceReference];
                return string.Format("{0}.{1}", formNamespace.Path, formDefinition.Form);
            }
            catch (KeyNotFoundException ex)
            {
                throw new FormMappingNotFoundException(string.Format("There is no namespace definition for '{0}'", formDefinition.NamespaceReference), ex);
            }
        }
    }

    internal class FormDefinition
    {
        public string ID { get; set; }
        public string NamespaceReference { get; set; }
        public string ActivityName { get; set; }
        public string ActivityStyle { get; set; }
        public string Form { get; set; }
        public string Path { get; set; }
        public string TabItem { get; set; }
        public string Skin { get; set; }

        //public FormDefinition(string form, string ns)
        //{
        //    this.Form = form;
        //    this.Namespace = ns;
        //}

        public FormDefinition(XElement definition)
        {
            if (definition != null)
            {
                ActivityName = definition.GetAttributeValue("name");
                ActivityStyle = definition.GetAttributeValue("style");

                ID = MakeFormKey(ActivityName, ActivityStyle);

                Form = definition.GetAttributeValue("form");
                Path = definition.GetAttributeValue("path");
                TabItem = definition.GetAttributeValue("tabItem");
                NamespaceReference = definition.GetAttributeValue("namespace");
                Skin = definition.GetAttributeValue("skin");
            }
        }

        /// <summary>
        /// Creating form string on the basis of string.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        internal static string MakeFormKey(string name, string style)
        {
            return string.Format("{0}({1})", name, style);
            // return name + "(" + style + ")";
        }
        /// <summary>
        /// this form is a compiled resource
        /// </summary>
        public bool IsLocal
        {
            get { return !string.IsNullOrEmpty(NamespaceReference); }
        }

        /// <summary>
        /// this form is available as a window/form
        /// </summary>
        public bool IsAvailableAsForm
        {
            get { return !string.IsNullOrEmpty(Form); }
        }

        /// <summary>
        /// this form is available as a tab page
        /// </summary>
        public bool IsAvailableAsTabItem
        {
            get { return !string.IsNullOrEmpty(TabItem); }
        }
        /// <summary>
        /// fully qualified (eg namespace, assembly) name
        /// </summary>
        public string FullName
        {
            get { return ApplicationEx.FormMappings.GetFormFullName(this); }
        }
    }

    internal class FormNamespace
    {
        public string ID { get; set; }
        public string Path { get; set; }
        public string Assembly { get; set; }

        public FormNamespace(string id, string path, string assembly)
        {
            ID = id;
            Path = path;
            Assembly = assembly;
        }

        public FormNamespace(XElement definition)
        {
            ID = definition.GetAttributeValue("id");
            Path = definition.GetAttributeValue("path");
            Assembly = definition.GetAttributeValue("assembly");
        }
    }

    public class FormMappingException : Exception
    {
        public FormMappingException(string message) : base(message)
        {

        }

        public FormMappingException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }

    public class FormMappingNotFoundException : FormMappingException
    {
        public FormMappingNotFoundException(string message) : base(message)
        {

        }

        public FormMappingNotFoundException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}