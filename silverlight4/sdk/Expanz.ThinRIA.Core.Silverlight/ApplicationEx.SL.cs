using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;
using Expanz.ThinRIA.ActivityContainers;
using Expanz.ThinRIA.Core;
using System.Reflection;

namespace Expanz.ThinRIA
{
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
                if (_activtyHostFrame == null)
                    _activtyHostFrame = FindFrame(Application.Current.RootVisual);

                return _activtyHostFrame;
            }
        } 
        #endregion

        #region Implemented Partial Methods
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

        partial void CreateActivityContainerInternal(string activity, string style, XElement xml, ActivityHarness sourceHarness)
        {
            // If the container is a Page, navigate to it. If it is a ChildWindow, display it.
            string formFullName = FormMappings.GetFormFullName(activity, style);

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
                        throw new Exception(String.Format("Activity '{0}' could not be created. Check your form mapping file. Ensure the namespaces element matches your project's namespace and your activity elements are configured to map Server Activities to local Views.", formFullName));
                    }
                }
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
                throw new Exception(String.Format("Activity '{0}' could not be created. Check your form mapping file. Ensure the namespaces element matches your project's namespace and your activity elements are configured to map Server Activities to local Views.", form));
            }
        }

        partial void ShowActivityContainer(IActivityContainer ac)
        {
            if (ac is ChildWindowEx)
            {
                ChildWindowEx window = ac as ChildWindowEx;
                window.Show();
            }
            else if (ac is PageEx || ac is UserControlEx)
            {
                if (ActivityHostFrame != null)
                {
                    ActivityHostFrame.Content = ac;
                }
            }
            else
            {
                SetRootVisual(ac as Control);
            }
        }

        partial void PublishUIMessageInternal(XElement uiMsgElement)
        {
            var dlg = new ClientMessageWindow(uiMsgElement);

            dlg.Closed += (s, e) =>
                {
                    if (dlg.DialogResult.HasValue && dlg.DialogResult.Value)
                    {
                        XDocument userDoc = dlg.Result.Document;
                        //XElement action = (XElement)ApplicationEx..DocumentElement;
                        XElement[] sendElt = null;
                        XElement child = (XElement)userDoc.FirstNode;

                        if (child.Name == "Request" || child.Name == "Response")
                        {
                            int count = child.Nodes().Count();
                            sendElt = new XElement[count];
                            child = (XElement)child.FirstNode;
                        }
                        else
                        {
                            sendElt = new XElement[userDoc.Nodes().Count()];
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
                    else
                    {
                        //ServerApplicationService.Response.Document.Root.RemoveAll();
                        //ServerApplicationService.PublishResponse();
                    }
                };

            dlg.Show();
        }
        #endregion

        #region Public Methods
        public void SetRootVisual(Control newControl)
        {
            Application.Current.RootVisual = ContentControl;

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
            ChildWindowEx window = ac as ChildWindowEx;
            window.Show();
        } 
        #endregion
    }
}
