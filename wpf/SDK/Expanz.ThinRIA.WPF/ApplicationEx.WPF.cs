using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml.Linq;
using Expanz.Extensions.BCL;
using Expanz.ThinRIA.ActivityContainers;
using Expanz.ThinRIA.Core;

namespace Expanz.ThinRIA
{
    /// <summary>
    /// WPF implementation of expanz Application singleton
    /// </summary>
    public partial class ApplicationEx
    {
        #region Public Properties
        private UserControl container;
        public UserControl ContentControl
        {
            get
            {
                if (container == null)
                    container = new UserControl();

                return container;
            }
        }

        private Frame _activtyHostFrame;
        public Frame ActivityHostFrame
        {
            get
            {
                if (_activtyHostFrame == null && Application.Current.MainWindow != null)
                    _activtyHostFrame = FindFrame(Application.Current.MainWindow);

                return _activtyHostFrame;
            }
        }
        #endregion

        #region Private Methods
        private void TreeViewItem_PreviewMouseRightButtonDownEvent(object sender, RoutedEventArgs e)
        {
            (sender as TreeViewItem).Background = new SolidColorBrush(Colors.Pink);
        }

        private System.Xml.XmlTextReader UrlAsXmlTextReader(string url)
        {
            System.Xml.XmlTextReader rdr = null;
            //if (UseProxy)
            //{
            //    WebClient client = new WebClient();
            //    client.Proxy = myProxyServer;
            //    Stream rssStream = client.OpenRead(url);
            //    StreamReader textReader = new StreamReader(rssStream);
            //    rdr = new XmlTextReader(textReader);
            //}
            //else
            //{
            rdr = new System.Xml.XmlTextReader(url);
            //}
            return rdr;
        }

        private static Frame FindFrame(DependencyObject parent)
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is Frame)
                {
                    Frame frame = (Frame)child;
                    return frame;
                }
                else
                {
                    Frame frame = FindFrame(child);

                    if (frame != null)
                        return frame;
                }
            }

            return null;
        }

        private MessageBoxButton GetMessageBoxButtonsForUIMessage(XElement uiMsgElement)
        {
            MessageBoxButton buttonConstant = MessageBoxButton.OK;

            try
            {
                // Start by getting the requested buttons, and map to a message box buttons constant
                var buttonLabels = uiMsgElement.Descendants("Action").Select(x => x.GetAttributeValue("label")).ToArray();

                if (buttonLabels.Count() == 2)
                {
                    if (buttonLabels[0].Equals("OK", StringComparison.InvariantCultureIgnoreCase) && buttonLabels[1].Equals("Cancel", StringComparison.InvariantCultureIgnoreCase))
                        buttonConstant = MessageBoxButton.OKCancel;
                    else if (buttonLabels[0].Equals("Yes", StringComparison.InvariantCultureIgnoreCase) && buttonLabels[1].Equals("No", StringComparison.InvariantCultureIgnoreCase))
                        buttonConstant = MessageBoxButton.YesNo;
                }
                else if (buttonLabels.Count() == 3)
                {
                    if (buttonLabels[0].Equals("Yes", StringComparison.InvariantCultureIgnoreCase) && buttonLabels[1].Equals("No", StringComparison.InvariantCultureIgnoreCase) && buttonLabels[2].Equals("Cancel", StringComparison.InvariantCultureIgnoreCase))
                        buttonConstant = MessageBoxButton.YesNoCancel;
                }
            }
            catch { }

            return buttonConstant;
        }
        #endregion

        #region Implemented Partial Methods
        internal object OpenNamedWindow(FormDefinition formDef, XElement xml, ActivityHarness sourceHarness)
        {
            string url = FormsURL + formDef.Path;
            object o = null;

            try
            {
                System.Xml.XmlTextReader rdr = UrlAsXmlTextReader(url);
                o = XamlReader.Load(rdr);
            }
            catch (Exception ex)
            {
                ApplicationEx.DebugException(ex, url);
                return null;
            }

            if (o is WindowEx)
            {
                WindowEx w = (WindowEx)o;
                if (sourceHarness == null)
                {
                    w.Initialise(xml);
                }
                else
                {
                    w.InitialiseCopy(sourceHarness);
                }
                w.Show();
            }            
            else if (o is TabItem)
            {
                TabItem ti = (TabItem)o;
                if (ti is TabItemEx)
                {
                    TabItemEx ati = (TabItemEx)ti;
                    ati.IsDynamic = true;
                    if (sourceHarness == null)
                    {
                        ati.Initialise(xml);
                    }
                    else
                    {
                        ati.InitialiseCopy(sourceHarness);
                        ati.Harness.IsActivityOwner = false;
                    }
                }
                //homeTabControl.Items.Add(ti);
                //homeTabControl.SelectedItem = ti;
            }

            return o;
        }

        internal string FormsURL
        {
            get
            {

                int p = Url.LastIndexOf("/");
                string path = Url.Substring(0, p + 1) + @"Forms/";
                return path;
            }
        }

        partial void CreateActivityContainerInternal(string activity, string style, XElement xml, ActivityHarness sourceHarness)
        {
            // If the container is a Page, navigate to it. If it is a ChildWindow, display it.
            FormDefinition fd = FormMappings.GetFormDefinition(activity, style);

            if (fd == null)
            {
                DisplayMessageBox("No mapping found for " + activity + " (" + style + ")", "Create Activity Error");
                return;
            }

            if (fd.IsLocal)
            {
                string formFullName = FormMappings.GetFormFullName(fd);

                Assembly assembly = Assembly.GetCallingAssembly();
                //var wex = assembly.CreateInstance(formFullName) as IActivityContainer;
                Type type = assembly.GetType(formFullName);

                if (type == null)
                {
                    assembly = Application.Current.GetType().Assembly;
                    //wex = assembly.CreateInstance(formFullName) as IActivityContainer;
                    type = assembly.GetType(formFullName);
                }

                if (type != null)
                {
                    if (type.IsSubclassOf(typeof(Page)) && XamlFileDetailsCollection.ContainsKey(formFullName))
                    {
                        // This is a Page, so navigate to the view
                        NavigationCacheActivityXML = xml;
                        XAMLFileDetails fileDetails = XamlFileDetailsCollection[formFullName];

                        string xamlFileName = fileDetails.FileName;
                        ActivityHostFrame.Navigate(new Uri("/" + xamlFileName, UriKind.Relative));
                    }
                    else
                    {
                        var view = assembly.CreateInstance(formFullName) as IActivityContainer;

                        if (view != null)
                        {
                            view.ActivityName = activity;
                            view.ActivityStyle = style;
                            view.Initialise(xml);

                            ShowActivityContainer(view);
                        }
                        else
                        {
                            throw new Exception(String.Format("Activity '{0}' could not be created. Check your FormMapping.xml. Ensure the namespaces element matches your project's namespace and your activity elements are configured to map Server Activities to local Views.", formFullName));
                        }
                    }
                }
            }
            else//load 'loose'
            {
                object o = OpenNamedWindow(fd, xml, sourceHarness);
            }
        }

        partial void CreateActivityContainerInternal(string form, XElement xml)
        {
            if (XamlFileDetailsCollection.ContainsKey(form))
            {
                NavigationCacheActivityXML = xml;
                XAMLFileDetails fileDetails = XamlFileDetailsCollection[form];

                string xamlFileName = fileDetails.FileName;
                ActivityHostFrame.Navigate(new Uri("/" + xamlFileName, UriKind.Relative));
            }
            else
            {
                throw new Exception(String.Format("Activity '{0}' could not be created. Check your FormMapping.xml. Ensure the namespaces element matches your project's namespace and your activity elements are configured to map Server Activities to local Views.", form));
            }
        }

        partial void CreateActivityContainerFromPath(string path, XElement xml)
        {
                NavigationCacheActivityXML = xml;
                ActivityHostFrame.Navigate(new Uri(path, UriKind.Relative));
        }

        partial void ShowActivityContainer(IActivityContainer ac)
        {
            if (ac is WindowEx)
            {
                WindowEx window = ac as WindowEx;
                window.Show();
            }
                /*
            else if (ac is PageEx || ac is UserControlEx)
            {
                if (ActivityHostFrame != null)
                {
                    ActivityHostFrame.Content = ac;
                }
            } */
            else
            {
                SetRootVisual(ac as Control);
            }
        }

        partial void PublishUIMessageInternal(XElement uiMsgElement)
        {
            // NOTE: This implementation isn't ideal, as a custom form should be shown
            // (see Silverlight SDK). There's only support for a limited number of actions,
            // and no support for options. However, for V1.0, this should do.
            string messageText = uiMsgElement.GetAttributeValue(Common.UIMessage.text);
            string messageTitle = uiMsgElement.GetAttributeValue(Common.UIMessage.Title);
            MessageBoxButton messageBoxButton = GetMessageBoxButtonsForUIMessage(uiMsgElement);
            MessageBoxImage messageBoxImage = (messageBoxButton == MessageBoxButton.OK ? MessageBoxImage.Information : MessageBoxImage.Question);

            MessageBoxResult result = MessageBox.Show(messageText, messageTitle, messageBoxButton, messageBoxImage);

            try
            {
                XElement action = uiMsgElement.Descendants("Action").Where(x => x.GetAttributeValue("label") == result.ToString()).FirstOrDefault();

                if (action != null)
                {
                    XElement[] sendElt = null;
                    XElement child = (XElement)action.FirstNode;

                    if (child != null)
                    {
                        if (child.Name == "Request" || child.Name == "Response")
                        {
                            int count = child.Nodes().Count();
                            sendElt = new XElement[count];
                            child = (XElement)child.FirstNode;
                        }
                        else
                        {
                            sendElt = new XElement[action.Nodes().Count()];
                        }

                        int i = 0;

                        while (child != null)
                        {
                            sendElt[i] = child;
                            child = (XElement)child.NextNode;
                            i++;
                        }

                        this.CurrentActivity.Exec(sendElt);
                    }
                }
                else
                {
                    // Should do something here, but hopefully it should never get here anyway.
                }
            }
            catch { }
        }

        public static DependencyObject GetParentOfType(DependencyObject child, Type t)
        {
            DependencyObject DO = null;
            DependencyObject parent = VisualTreeHelper.GetParent(child);

            while (parent != null && DO == null)
            {
                if (parent.GetType().IsSubclassOf(t))
                    DO = parent;
                else
                    parent = VisualTreeHelper.GetParent(parent);
            }
            return DO;
        }
        #endregion

        #region Public Methods
        public void SetRootVisual(Control newControl)
        {
            //Application.Current.RootVisual = ContentControl;

            if (newControl != null)
            {
                //ContentControl.Height = newControl.Height;
                //ContentControl.Width = newControl.Width;
                //ContentControl.Background = newControl.Background;
                ContentControl.Content = newControl;
            }
        }

        public void SetRootVisual(IActivityContainer ac)
        {
            SetRootVisual(ac as Control);
        }

        protected internal void ShowChildWindow(IActivityContainer ac)
        {
            WindowEx window = ac as WindowEx;
            window.Show();
        }

        public static void DebugException(Exception e)
        {
            MessageBox.Show(ExceptionMessage(e) + System.Environment.NewLine + e.StackTrace, e.Message);
        }

        public static void DebugException(Exception e, string s)
        {
            MessageBox.Show(ExceptionMessage(e) + System.Environment.NewLine + e.StackTrace, s);
        }

        public static string ExceptionMessage(Exception e)
        {
            string ret = e.Message;
            if (e.InnerException != null)
            {
                ret += System.Environment.NewLine + "Inner Exception:" + System.Environment.NewLine + e.InnerException.Message;
            }
            return ret;
        }
        #endregion

        #region Local resources, temp files
        private List<string> myTempFiles;
        internal List<string> TempFiles { get { if (myTempFiles == null) myTempFiles = new List<string>(); return myTempFiles; } }
        #endregion
    }
}
