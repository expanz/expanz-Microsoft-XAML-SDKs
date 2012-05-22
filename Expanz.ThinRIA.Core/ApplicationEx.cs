using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Expanz.ThinRIA.Core;
using Expanz.ThinRIA.Security;
using System.Diagnostics;

namespace Expanz.ThinRIA
{
    /// <summary>
    /// expanz application class.
    /// Runs as a Singleton and manages all communications with the server
    /// </summary>
    // This is the core expanz application class for use across WPF, Silverlight and Windows Phone 7
    public partial class ApplicationEx : INotifyPropertyChanged
    {
        #region Contructors
        public ApplicationEx()
        {
            singletonAppInstance = this;

            Application.Current.Exit += Application_Exit;

            if (!IsDesignMode)
            {
                LoadPreferences();

                FormMappings = new FormMappings();

                ServerApplicationService = new ServerApplicationService();
                ServerApplicationService.ServerRequest += new MessageDelegate(ServerApplicationService_ServerRequest);
                ServerApplicationService.ServerResponse += new MessageDelegate(ServerApplicationService_ServerResponse);
#if WPF
                //EventManager.RegisterClassHandler(typeof(TreeViewItem), TreeViewItem.PreviewMouseRightButtonDownEvent, new RoutedEventHandler(TreeViewItem_PreviewMouseRightButtonDownEvent));
#endif
                InitialiseApplication();
            }
        }

        protected virtual void InitialisePlatform()
        {
        }
        #endregion

        #region Constants
        internal const string ReturnUrlParameterName = "ReturnUrl"; 
        #endregion

        #region Static Properties
        public static IActivityContainer CreatingContainer { get; set; }

        private static readonly object syncObject = new object();
        private static ApplicationEx singletonAppInstance;
        public static string PickListActivity;
        public static XElement PickListXml;

        public static ApplicationEx Instance
        {
            get
            {
                if (singletonAppInstance == null && !IsDesignMode)
                {
                    lock (syncObject)
                    {
                        if (singletonAppInstance == null)
                            singletonAppInstance = new ApplicationEx();
                    }
                }

                return singletonAppInstance;
            }
        }

        internal static FormMappings FormMappings { get; private set; }

        public static bool IsDesignMode
        {
            get
            {
#if WPF
                return DesignerProperties.GetIsInDesignMode(new DependencyObject());
#else
                return DesignerProperties.IsInDesignTool;
#endif
            }
        }
        #endregion

        #region Web Service
        public bool UseProxy
        {
            get;
            set;
        }
        private string proxy;
        public string Proxy
        {
            get { return proxy; }
            set
            {
                if (value.Length >= 7 && value.Substring(0, 7).Equals("http://"))
                    proxy = value.Substring(7);
                else
                    proxy = value;

            }
        }

        public int ProxyPort
        {
            get;
            set;
        }
        public string ProxyUserName
        {
            get;
            set;
        }
        public string ProxyPassword
        {
            get;
            set;
        }


        #endregion

        #region Member Variables
        private Dictionary<string, IActivityContainer> _openActivities = new Dictionary<string, IActivityContainer>();
        //protected bool _saveMessages;
        private int _duplicateActivityCount = 0;
        private bool _hasOpenSession;
        
        internal XAMLFileDetailsCollection XamlFileDetailsCollection = new XAMLFileDetailsCollection();
        #endregion

        #region Delegates / Events
        public event PropertyChangedEventHandler PropertyChanged;

        public delegate void WebServiceCallInform(bool busy, string message);
        private WebServiceCallInform myInformDelegate;

        public WebServiceCallInform WebServiceInformDelegate
        {
            get { return myInformDelegate; }
            set { myInformDelegate = value; }
        }

        public event SessionCreatedDelegate SessionCreated;

        public event MessageDelegate ServerRequest;
        public event MessageDelegate ServerResponse;

        //public delegate void SetCursorDelegate(Cursor c);
        //public SetCursorDelegate SetCursorProperty { get; set; }
        #endregion

        #region Event Handlers
        private void Application_Exit(object sender, EventArgs e)
        {
            if (ActiveSession != null)
                ActiveSession.ReleaseSession();
        }

        private void ServerApplicationService_ServerRequest(string message)
        {
            if (ServerRequest != null)
                ServerRequest(message);
        }

        private void ServerApplicationService_ServerResponse(string message)
        {
            if (ServerResponse != null)
                ServerResponse(message);
        }
        #endregion

        #region Public Properties
        internal ServerApplicationService ServerApplicationService { get; private set; }
        public Session ActiveSession { get; private set; }
        internal ActivityHarness CurrentActivity { get; set; }
        internal XElement NavigationCacheActivityXML { get; set; } // Yucky hack to pass a view some xml to initialise itself. Value is set before the navigation, and picked up once the view has been navigated to.
        
        [TypeConverter(typeof(UriTypeConverter))]
        public Uri LoginViewUri { get; set; }

        private string _serviceUrl;
        public string Url
        {
            get { return _serviceUrl; }
            set
            {
                if (value != null)
                {
                    _serviceUrl = value;

                    if (ServerApplicationService != null)
                        ServerApplicationService.InitialiseService();
                }
            }
        }

        private string _preferredSite;
        public string PreferredSite
        {
            get
            {
                if (_preferredSite == null)
                    _preferredSite = DefaultSite;

                return _preferredSite;
            }
            set { _preferredSite = value; }
        }

        public virtual string DefaultSite
        {
            get { return "EXPANZ"; }
        }

        private MenuCollection _menuCollection;
        public MenuCollection ApplicationMenu
        {
            get
            {
                if (_menuCollection == null)
                    _menuCollection = new MenuCollection();

                return _menuCollection;
            }
        }

        private Dictionary<string, List<string>> myFormOptimisation;
        internal Dictionary<string, List<string>> FormOptimisations
        {
            get
            {
                if (myFormOptimisation == null) 
                    myFormOptimisation = new Dictionary<string, List<string>>();

                return myFormOptimisation;
            }
        }

        private IMessageDisplay homeMessagePanel;
        public IMessageDisplay HomeMessagePanel
        {
            set { homeMessagePanel = value; }
        }

        private ICustomActivityRequestHandler myCustomActivityRequestHandler;
        public ICustomActivityRequestHandler CustomActivityRequestHandler
        {
            set { myCustomActivityRequestHandler = value; }
        }

        private bool isBusy = false;

        public bool IsBusy
        {
            get { return isBusy; }
            set 
            { 
                isBusy = value;

                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("IsBusy"));
            }
        }

        public bool HasOpenSession
        {
            get { return _hasOpenSession; }
            set
            {
                _hasOpenSession = value;

                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("HasOpenSession"));
            }
        }

        public Dictionary<string, IActivityContainer> OpenActivities
        {
            get { return _openActivities; }
        }
        

        /*
        private TabControl homeTabControl;
        public TabControl HomeTabControl
        {
            set { homeTabControl = value; if (homeTabControl != null) useTabItemIfAvailable = true; }
        } */
        #endregion

        #region Methods
        /// <summary>
        /// Creates session call to the server. It needs client credentials to validate them against the server.
        /// </summary>
        /// <param name="userName">Client username</param>
        /// <param name="password">Client password</param>
        /// <param name="site"></param>
        /// <param name="deviceName"></param>
        public void CreateSession(string userName, string password, string site, string deviceName, EventHandler<AuthenticationCompletedEventArgs> authenticationComplete)
        {
            ActiveSession = new Session(ServerApplicationService);

            var auth = new ExpanzAuthentication(ClientType.Xaml, site, deviceName, AuthenticationMode.Primary);
            auth.Authenticate(userName, password, ActiveSession.AuthenticationComplete);

            if (authenticationComplete != null)
                ActiveSession.AuthenticationCompleted += authenticationComplete;

            ActiveSession.SessionCreated += new SessionCreatedDelegate(ActiveSession_SessionCreated);
        }

        private void ActiveSession_SessionCreated(Session session)
        {
            if (SessionCreated != null)
                SessionCreated(session);

            HasOpenSession = true;
            
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("ActiveSession"));
        }

        internal void PublishMessage(XElement messageElt)
        {
            if (homeMessagePanel != null)
                homeMessagePanel.PublishMessage(messageElt);
        }

        internal void CreateActivityContainer(XElement xml)
        {
            string activity = xml.Attribute(Common.IDAttrib).Value;
            var attr = xml.Attribute(Common.ActivityStyle);
            string style = (attr != null) ? attr.Value : String.Empty;
            CreateActivityContainer(activity, style, xml, null);
        }

        public void CreateActivityContainer(string activity, string style, XElement xml, ActivityHarness sourceHarness)
        {
            CreateActivityContainerInternal(activity, style, xml, sourceHarness);
        }

        internal void LoadInitialActivity()
        {
            if (ActivityHostFrame != null)
            {
                Page currentPage = ActivityHostFrame.Content as Page;

                if (currentPage != null)
                {
                    /*
                    if (currentPage.NavigationContext.QueryString.ContainsKey(ReturnUrlParameterName))
                    {
                        // The user had navigated to a page, but had to log in first. Navigate back to that page.
                        string returnUrl = currentPage.NavigationContext.QueryString[ReturnUrlParameterName];
                        ActivityHostFrame.Navigate(new Uri(returnUrl, UriKind.Relative));
                        return;
                    } */
                }                
            }

            if (FormMappings.DefaultMapping != null)
            {
                if (!string.IsNullOrEmpty(FormMappings.DefaultMapping.ActivityName))
                    CreateActivityContainer(FormMappings.DefaultMapping.ActivityName, FormMappings.DefaultMapping.ActivityStyle, null, null);
                else if (!string.IsNullOrEmpty(FormMappings.DefaultMapping.Form))
                    CreateActivityContainerInternal(FormMappings.DefaultMapping.FullName, null);
                else if (!string.IsNullOrEmpty(FormMappings.DefaultMapping.Path))
                    CreateActivityContainerFromPath(FormMappings.DefaultMapping.Path, null);
            }
        }

        internal void PublishUIMessage(XElement uiMessageElement)
        {
            PublishUIMessageInternal(uiMessageElement);
        }

        public virtual void DisplayMessageBox(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK);
        }

        public void RequestActivity(IActivityContainer container, string activity, string style, int initialKey, ServerApplicationService.ResponseCallBackDelegate callBack)
        {
            var xml = new XElement(Common.Requests.ActivityCreate);
            xml.SetAttributeValue(Common.ActivityName, activity);

            if (style == null)
                style = string.Empty;

            xml.SetAttributeValue(Common.ActivityStyle, style);

            if (initialKey > 0)
                xml.SetAttributeValue(Common.InitialKey, initialKey.ToString());

            container.AppendDataPublicationsToActivityRequest(xml);

            ServerApplicationService.SendRequestToServerPortal(xml, callBack);
        }

        public void RequestActivity(string activity, string style, XElement xml)
        {
            if (myCustomActivityRequestHandler != null)
                myCustomActivityRequestHandler.UserActivityRequest(activity, style, null);
        }

        public void RegisterActivity(IActivityContainer container)
        {
            if (!_openActivities.ContainsKey(container.ActivityStamp))
                _openActivities.Add(container.ActivityStamp, container);
        }

        public int RegisterActivityCopy(IActivityContainer container)
        {
            _duplicateActivityCount++;
            _openActivities.Add(container.ActivityStamp + "/" + _duplicateActivityCount.ToString(), container);
            return _duplicateActivityCount;
        }

        internal void GetSchema(ActivityHarness harness)
        {
            XElement xml = new XElement(Common.Requests.PublishSchema); //Request.CreateElement(Common.Requests.PublishSchema);
            xml.SetAttributeValue(Common.ActivityHandle, harness.ActivityStamp);
            harness.AppendDataPublicationsToActivityRequest(xml);
            //_dontAutoPublishResponse = true;
            ServerApplicationService.SendRequestToServerPortal(xml, null);
            //_dontAutoPublishResponse = false;
        }

        internal void LoadApplicationMenu(XElement xml)
        {
            ApplicationMenu.LoadMenu(xml);
        }
        #endregion

        #region Partial Method Declarations
        partial void InitialiseApplication();
        partial void PublishUIMessageInternal(XElement uiMessageElement);
        partial void CreateActivityContainerInternal(string activity, string style, XElement xml, ActivityHarness sourceHarness);
        partial void CreateActivityContainerInternal(string form, XElement xml);
        partial void CreateActivityContainerFromPath(string path, XElement xml);
        partial void ShowActivityContainer(IActivityContainer ac);
        partial void LoadPreferences(); // Not required for SL4, WP7
        partial void SavePreferences(); // Not required for SL4, WP7
        #endregion
    }
}



#region Archive
//protected void processFiles(XElement filesElt)
//{
//    XElement FILE = (XElement)filesElt.FirstChild;
//    while (FILE != null)
//    {
//        string fileName = FILE.GetAttribute(Common.RequestFileActionName);
//        string realFileName = "";
//        if (fileName.Length > 0)
//        {
//            try
//            {
//                // File doesn't exist where we can see it
//                // Get file as a string from the server
//                XElement sendElt = myRequest.CreateElement(Common.Requests.File);
//                sendElt.SetAttribute("fileName", fileName);
//                dontAutoPublishResponse = true;
//                CurrentOperation = "Retrieving file " + fileName + "...";
//                saveMessages = true;
//                SendRequestToServerPortal(sendElt);
//                dontAutoPublishResponse = false;
//                if (ApplicationEx.LastSuccess)
//                {
//                    XElement fileDoc = (XElement)Response.DocumentElement.FirstChild;
//                    realFileName = SaveFileFromString(fileDoc);
//                }
//            }
//            catch (Exception ex1)
//            {
//                ApplicationEx.debugException(ex1, "Failed to get file from the server");
//            }

//            if (realFileName.Length > 0)
//            {
//                string action = FILE.GetAttribute(Common.RequestFileAction);
//                if (action.Length == 0) action = Common.FileActionView;

//                // Go to extension in classes in registry.
//                // From this, determine the file type association.
//                FileInfo fileInfo = new FileInfo(realFileName);
//                RegistryKey typeKey = Registry.ClassesRoot.OpenSubKey(fileInfo.Extension);
//                if (typeKey == null)
//                {
//                    displayMessageBox("Your system doesn't support " + fileInfo.Extension.ToLower() + " files", "Unknwon File Type");
//                    return;
//                }
//                if (action == Common.FileActionView)
//                {
//                    try { System.Diagnostics.Process.Start(realFileName); }
//                    catch (System.ComponentModel.Win32Exception winEx)
//                    {
//                        displayMessageBox("Error opening file :" + realFileName + "\r\n" + winEx.Message, "Error");
//                    }
//                    catch (Exception ex2)
//                    {
//                        displayMessageBox("Failed to open file :" + realFileName + "\r\n :" + ex2.Message, "Error");
//                    }
//                }
//                else if (action == Common.FileActionPrint)
//                {
//                    try
//                    {
//                        string fileType = (string)typeKey.GetValue("");
//                        // From the file type, determine the command to print
//                        string printCommand = (string)Registry.ClassesRoot.OpenSubKey(fileType + "\\shell\\print\\command").GetValue("");

//                        // Replace %1 with the real file name
//                        printCommand = printCommand.Replace("%1", realFileName);

//                        // Need to split up the whole command into exe and parameters.
//                        int n = printCommand.IndexOf("\"");
//                        if (n == 0)
//                            n = printCommand.IndexOf("\"", 1);
//                        else
//                            n = printCommand.IndexOf(" ");

//                        if (n == -1)
//                            System.Diagnostics.Process.Start(printCommand);
//                        else
//                            System.Diagnostics.Process.Start(printCommand.Substring(0, n + 1), printCommand.Substring(n + 2));
//                    }
//                    catch (Exception ex3)
//                    {
//                        displayMessageBox("Failed to print file :\"" + realFileName + "\"\r\nMessage :" + ex3.Message, "Error");
//                    }
//                }
//                else if (action == Common.FileActionSaveAs)
//                {
//                    try
//                    {
//                        // First determine filename
//                        SaveFileDialog dialog = new SaveFileDialog();
//                        dialog.OverwritePrompt = true;

//                        // Get extension of real file name
//                        string extension = Path.GetExtension(realFileName);

//                        // Set filter
//                        if (extension.Length == 0)
//                            dialog.Filter = "All Files (*.*)|*.*";
//                        else
//                            dialog.Filter = extension.Substring(1).ToUpper() +
//                                " Files (*" + extension + ")|*" +
//                                extension + "|All Files (*.*)|*.*";
//                        dialog.FilterIndex = 1;

//                        string type = FILE.GetAttribute(Common.Data.RowType);
//                        bool? ok = dialog.ShowDialog();
//                        if (ok.HasValue && (bool)ok)
//                        {
//                            // If the file exists, have to make sure it's not read only.
//                            // Otherwise errors occur when copying.
//                            if (File.Exists(dialog.FileName))
//                            {
//                                File.SetAttributes(dialog.FileName, FileAttributes.Normal);
//                            }
//                            File.Copy(realFileName, dialog.FileName, true);
//                        }
//                    }
//                    catch (Exception ex4)
//                    {
//                        ApplicationEx.debugException(ex4, "Failed to save file :[" + realFileName + "] to local drive.");
//                    }
//                }
//                else
//                    throw new Exception("Unsupported file action: " + action);
//            }
//        }

//        FILE = (XElement)FILE.NextSibling;
//    }
//}


//protected virtual void createLongOperationCancelForm()
//{
//    cancelChunkForm = (IProgressStatusWindow)new LongOperationCancelDialog(this);
//    if (cancelChunkForm is Window)
//    {
//        Window w = (Window)cancelChunkForm;
//        w.Show();
//        Dispatcher.Run();
//        w.InvalidateVisual();
//    }
//    cancelChunkForm.Description = chunkProgressText;
//    cancelChunkForm.Progress = ChunkProgressPercentage;
//    chunkThreadStarted = false;
//}



//public void ListAvailableSites(string errorMsg)
//{
//    //string availableSites = "";
//    //try
//    //{
//    RemoteService.ListAvailableSitesCompleted += new EventHandler<ESAPortal.ListAvailableSitesCompletedEventArgs>(RemoteService_ListAvailableSitesCompleted);
//    RemoteService.ListAvailableSitesAsync(errorMsg);
//    //}
//    //catch (System.IO.IOException e)
//    //{
//    //    StringBuilder message = new StringBuilder();
//    //    message.Append("It appears that the web service at " + RemoteService.Url + " is incompatible with this client.");
//    //    message.Append(System.Environment.NewLine + "The most likely cause is uncompressed responses from the web server.");
//    //    message.Append(System.Environment.NewLine + System.Environment.NewLine + "Please change your preferences to connect to a compatible web server.");
//    //    message.Append(System.Environment.NewLine + System.Environment.NewLine + e.Message);
//    //    errorMsg = message.ToString();
//    //}
//    //catch (System.Net.WebException e)
//    //{
//    //    string message = "The web server " + RemoteService.Url + " is not contactable at this time.";
//    //    message += System.Environment.NewLine + e.Message;
//    //    message += System.Environment.NewLine + "Check for internet connectivity and local firewall settings";
//    //    errorMsg = message;
//    //}
//    //catch (Exception e)
//    //{
//    //    string message = "The web server at " + RemoteService.Url + " is not contactable at this time.";
//    //    message += System.Environment.NewLine + e.Message;
//    //    message += System.Environment.NewLine + "Check for internet connectivity and local firewall settings";
//    //    errorMsg = message;
//    //}

//    //return availableSites;
//} 
#endregion