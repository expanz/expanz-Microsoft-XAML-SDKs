using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;
using Expanz.Extensions.BCL;
using Expanz.ThinRIA.Core;
using Microsoft.Phone.Shell;

namespace Expanz.ThinRIA
{
    public partial class ApplicationEx
    {
        #region Public Properties
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

        #region Private Methods
        private static Frame FindFrame(DependencyObject parent)
        {
            if (parent is Frame)
                return parent as Frame; // RootVisual for WP7 *is* a Frame

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
                        buttonConstant = MessageBoxButton.OKCancel;
                }
            }
            catch { }

            return buttonConstant;
        }
        #endregion

        #region Event Handlers
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            PhoneApplicationService.Current.Activated += new EventHandler<ActivatedEventArgs>(Application_Activated);
            PhoneApplicationService.Current.Deactivated += new EventHandler<DeactivatedEventArgs>(Application_Deactivated);
        }

        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            if (PhoneApplicationService.Current.StartupMode == StartupMode.Activate)
            {
                // Returning from a tombstone event - restore application state
                IDictionary<string, object> appState = PhoneApplicationService.Current.State;

                _menuCollection = appState["ApplicationMenu"] as MenuCollection;
                XamlFileDetailsCollection = appState["XamlFileDetailsCollection"] as XAMLFileDetailsCollection;
                FormMappings = appState["FormMappings"] as FormMappings;

                if (appState["SessionToken"] != null)
                {
                    ActiveSession = new Session(ServerApplicationService);
                    HasOpenSession = true;
                }

                if (appState["SessionToken"] != null)
                    ActiveSession.SessionToken = appState["SessionToken"].ToString();

                if (appState["ApplicationMenu"] != null)
                    ActiveSession.UserName = appState["ApplicationMenu"].ToString();
            }
        }

        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            // Application is being tombstoned - save application state
            IDictionary<string, object> appState = PhoneApplicationService.Current.State;

            appState["ApplicationMenu"] = ApplicationMenu;
            appState["XamlFileDetailsCollection"] = XamlFileDetailsCollection;
            appState["FormMappings"] = FormMappings;
            appState["SessionToken"] = HasOpenSession ? ActiveSession.SessionToken : null;
            appState["ApplicationMenu"] = HasOpenSession ? ActiveSession.UserName : null;
        } 
        #endregion

        #region Implemented Partial Methods
        partial void InitialiseApplication()
        {
            Application.Current.Startup += Application_Startup;
        }

        partial void PublishUIMessageInternal(XElement uiMsgElement)
        {
            // NOTE: This implementation isn't ideal, as a custom form should be shown
            // (see Silverlight SDK). There's only support for a limited number of actions,
            // and no support for options. However, for V1.0, this should do.
            string messageText = uiMsgElement.GetAttributeValue(Common.UIMessage.text);
            string messageTitle = uiMsgElement.GetAttributeValue(Common.UIMessage.Title);
            MessageBoxButton messageBoxButton = GetMessageBoxButtonsForUIMessage(uiMsgElement);

            MessageBoxResult result = MessageBox.Show(messageText, messageTitle, messageBoxButton);

            // Because WP7 only has support for OK/Cancel buttons, we have to coerce OK/Cancel to possible Yes/No.
            // TODO: Use the notification box from http://wpassets.codeplex.com/ instead, to get additional
            // customisable buttons
            string resultStringAlt = (result == MessageBoxResult.OK ? "Yes" : "No");

            try
            {
                XElement action = uiMsgElement.Descendants("Action").Where(x => x.GetAttributeValue("label") == result.ToString() || x.GetAttributeValue("label") == resultStringAlt).FirstOrDefault();

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

        partial void CreateActivityContainerInternal(string activity, string style, XElement xml, ActivityHarness sourceHarness)
        {
            string formFullName = FormMappings.GetFormFullName(activity, style);

            CreateActivityContainerInternal(formFullName, xml);
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
                throw new Exception(String.Format("A view for activity '{0}' could not be found. Check your FormMapping.xml. Ensure the namespaces element matches your project's namespace and your activity elements are configured to map Server Activities to local Views.", form));
            }
        }
        #endregion
    }
}
