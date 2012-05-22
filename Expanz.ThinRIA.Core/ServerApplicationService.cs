using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml.Linq;
using Expanz.ThinRIA.Core;
using Expanz.ThinRIA.ESAPortal;
using System.Xml;
using System.Collections.Generic;

namespace Expanz.ThinRIA
{
    public partial class ServerApplicationService
    {
        #region Member Variables
        private bool _dontAutoPublishResponse;
        private ESAServiceClient _remoteService;
        private string _currentSite;
        private XDocument _request;
        private XDocument _response; 
        private string _currentOperation;
        private RequestDetails _activeRequestDetails = null;
        #endregion

        #region Public Properties
        public ESAServiceClient RemoteService
        {
            get
            {
                if (_remoteService == null)
                    InitialiseService();

                return _remoteService;
            }
        }

        public string CurrentSite
        {
            get
            {
                if (_currentSite != null)
                    return _currentSite;

                return ApplicationEx.Instance.PreferredSite;
            }
        }

        public XDocument Request
        {
            get
            {
                if (_request == null)
                    _request = new XDocument(new XElement(Common.NewEmptyXmlDoc));

                return _request;
            }
        }

        public XDocument Response
        {
            get
            {
                if (_response == null)
                    _response = new XDocument(new XElement(Common.NewEmptyXmlDoc));

                return _response;
            }
            set
            {
                _response = value;
            }
        }

        public int CurrentActivityDupIndex { get; set; }
        public bool IsRequestInProgress { get; private set; }
        private Queue<RequestDetails> RequestQueue { get; set; }
        #endregion

        #region Events / Delegates
        public delegate void ResponseCallBackDelegate(bool busy, XDocument response);

        public event MessageDelegate ServerRequest;
        public event MessageDelegate ServerResponse;
        #endregion

        #region Public Methods
        internal void InitialiseService()
        {
            RequestQueue = new Queue<RequestDetails>();

            if (ApplicationEx.Instance.Url != null)
            {
                bool isSecure = GetScheme(ApplicationEx.Instance.Url) == "https://";
                BasicHttpSecurityMode securityMode = (isSecure ? BasicHttpSecurityMode.Transport : BasicHttpSecurityMode.None);

                string url = ApplicationEx.Instance.Url;
                string bindingName = GetBinding(url);
                Binding binding = null;

                if (bindingName.Length==0)
                {
                    bindingName = isSecure ? "binaryssl" : "binary";
                    url += "/" + bindingName;

                    binding = CreateBinaryBinding(isSecure);
                }
                else
                {
                    switch (bindingName)
                    {
                        case "basic":
                        case "basicssl":
                            binding = CreateBasicBinding(securityMode);
                            break;
                        case "binary":
                            binding = CreateBinaryBinding(false);
                            break;
                        case "binaryssl":
                            binding = CreateBinaryBinding(true);
                            break;
                        default:
                            binding = CreateBasicBinding(securityMode);
                            break;
                    }
                }                

                EndpointAddress address = new EndpointAddress(url);

                // Potentially use BasicHttpMessageInspectorBinding from Microsoft.Silverlight.Samples instead?
                //BasicHttpMessageInspectorBinding binding = new BasicHttpMessageInspectorBinding(new AWServiceMessageInspector(), securityMode);

                _remoteService = new ESAServiceClient(binding, address);
                RemoteService.ExecCompleted += new EventHandler<ESAPortal.ExecCompletedEventArgs>(RemoteService_ExecCompleted);
            }
        }

        public void ListAvailableSites(string errorMsg)
        {
            RemoteService.ListAvailableSitesCompleted += new EventHandler<ESAPortal.ListAvailableSitesCompletedEventArgs>(RemoteService_ListAvailableSitesCompleted);
            RemoteService.ListAvailableSitesAsync(errorMsg);
        }

        private void RemoteService_ListAvailableSitesCompleted(object sender, ListAvailableSitesCompletedEventArgs e)
        {
            if (e.Error.Message.Length > 0)
                return;
            //string availableSites = e.Result;
        }

        /// <summary>
        /// Create request to service portal.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public void SendRequestToServerPortal(XElement request, ResponseCallBackDelegate responseCallBack)
        {
            XElement[] req = new XElement[1];
            req[0] = request;
            SendRequestToServerPortal(req, responseCallBack);
        }

        /// <summary>
        /// Create call to service portal.
        /// </summary>
        /// <param name="activityWindow"></param>
        /// <param name="requestList"></param>
        /// <returns></returns>
        public void SendRequestToServerPortal(XElement[] requestList, ResponseCallBackDelegate responseCallBack)
        {
            if (!IsRequestInProgress)
            {
                SendRequestToServerPortal(new RequestDetails(requestList, responseCallBack));
            }
            else
            {
                // Add request to queue
                RequestQueue.Enqueue(new RequestDetails(requestList, responseCallBack));
            }
        }

        private void SendRequestToServerPortal(RequestDetails requestDetails)
        {
            ApplicationEx.Instance.IsBusy = true;
            _activeRequestDetails = requestDetails;

            if (string.IsNullOrEmpty(_currentOperation))
                _currentOperation = "Calling Server...";

            if (ApplicationEx.Instance.WebServiceInformDelegate != null)
                ApplicationEx.Instance.WebServiceInformDelegate(true, _currentOperation);

            Request.Document.Root.RemoveAll();

            if (Response != null)
                Response.Document.Root.RemoveAll();

            for (int payloadEntryIndex = 0; payloadEntryIndex < requestDetails.RequestPayload.GetLength(0); payloadEntryIndex++)
            {
                if (requestDetails.RequestPayload[payloadEntryIndex].Document == Request)
                {
                    Request.Document.Root.Add(requestDetails.RequestPayload[payloadEntryIndex]);
                }
                else
                {
                    XElement child = new XElement(requestDetails.RequestPayload[payloadEntryIndex]);
                    Request.Document.Root.Add(child);
                }
            }

            string errorMessage = string.Empty;
            string request = Request.ToString();

            if (ServerRequest != null)
                ServerRequest(request);

            RemoteService.ExecAsync(request, ApplicationEx.Instance.ActiveSession.SessionToken, errorMessage);
            IsRequestInProgress = true;
        }

        /// <summary>
        /// function to call the server to execute the request.
        /// </summary>
        /// <param name="site"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public bool ExecAnnymous(string site, XDocument request)
        {
            string ErrMessage = "";
            //string results = "";

            Response.Document.Root.RemoveAll();

            bool ok = false;

            try
            {
                //results = RemoteService.ExecAnonymous(site, request.OuterXml, ref ErrMessage);
                RemoteService.ExecAnonymousCompleted += new EventHandler<ESAPortal.ExecAnonymousCompletedEventArgs>(RemoteService_ExecAnonymousCompleted);
                RemoteService.ExecAnonymousAsync(site, request.ToString(), ErrMessage);
            }
            catch (Exception exc)
            {
                ApplicationEx.Instance.DisplayMessageBox(exc.Message + "\r\nFailed to process response\r\n" + exc.StackTrace, "Web Service Error");
                ok = false;
            }

            return ok;
        }
        #endregion

        #region Private Methods
        private string GetScheme(string serviceUrl)
        {
            string scheme = "http://";

            int endIndex = serviceUrl.IndexOf("//");

            if (endIndex != -1)
                scheme = serviceUrl.Substring(0, endIndex + 2);

            return scheme.ToLower();
        }

        private string GetBinding(string serviceUrl)
        {
            string binding = "";

            int endIndex = serviceUrl.IndexOf(".svc/", StringComparison.InvariantCulture);

            if (endIndex != -1)
                binding = serviceUrl.Substring(endIndex + 5);

            return binding.ToLower();
        }

        private Binding CreateBasicBinding(BasicHttpSecurityMode securityMode)
        {
            BasicHttpBinding binding = new BasicHttpBinding(securityMode) { MaxBufferSize = 2147483647, MaxReceivedMessageSize = 2147483647 };
#if WPF
            binding.ReaderQuotas.MaxStringContentLength = 2147483647;
#endif
            return binding;
        }

        private Binding CreateBinaryBinding(bool isSecure)
        {
            var binaryElement = new BinaryMessageEncodingBindingElement();
#if WPF
            binaryElement.ReaderQuotas.MaxStringContentLength = 2147483647;
#endif

            BindingElementCollection elements = new BindingElementCollection();
            elements.Add(binaryElement);

            if (isSecure)
                elements.Add(new HttpsTransportBindingElement() { MaxBufferSize = 2147483647, MaxReceivedMessageSize = 2147483647 });
            else
                elements.Add(new HttpTransportBindingElement() { MaxBufferSize = 2147483647, MaxReceivedMessageSize = 2147483647 });

            return new CustomBinding(elements);
        }

        private void RemoteService_ExecCompleted(object sender, ExecCompletedEventArgs e)
        {
            try
            {
                IsRequestInProgress = false;

                RequestDetails currentResponseRequest = _activeRequestDetails;
                _activeRequestDetails = null; // Doing this now instead of at end, as code called by this method might kick off a new request

                if (e.Error != null)
                {
                    if (currentResponseRequest.ResponseCallBack != null)
                        currentResponseRequest.ResponseCallBack(false, null);

                    ApplicationEx.Instance.DisplayMessageBox(e.Error.Message, "Error Message");

                    _chunking = false;
                    RequestQueue.Clear(); // Future requests made invalid due to error, so clear the queue. TODO: Determine whether there is a better strategy for dealing with queued requests after an error.

                    return;
                }

                if (!string.IsNullOrEmpty(e.errors))
                {
                    if (currentResponseRequest.ResponseCallBack != null)
                        currentResponseRequest.ResponseCallBack(false, null);

                    ApplicationEx.Instance.DisplayMessageBox(e.errors, "Server Message");

                    _chunking = false;
                    RequestQueue.Clear(); // Future requests made invalid due to error, so clear the queue. TODO: Determine whether there is a better strategy for dealing with queued requests after an error.

                    return;
                }

                if (e.Result.Length > 0)
                {
                    Response = XDocument.Parse(e.Result);

                    // Write the message to the output window
//                    System.Diagnostics.Debug.WriteLine("RAW RESPONSE:");

//#if WINDOWS_PHONE
//                    int startPos = 0;

//                    string debugString = Response.ToString();

//                    while (startPos < debugString.Length)
//                    {
//                        if (startPos + 700 > e.Result.Length)
//                            System.Diagnostics.Debug.WriteLine(debugString.Substring(startPos));
//                        else
//                            System.Diagnostics.Debug.WriteLine(debugString.Substring(startPos, 700));

//                        startPos += 700;
//                    }
//#else
//                    System.Diagnostics.Debug.WriteLine(Response.ToString());
//#endif
                    
                    if (ServerResponse != null)
                        ServerResponse(Response.ToString());

                    bool lastSuccess = Common.boolValue(Response.Document.Root.Attribute(Common.ResultSuccess).Value);

                    if (Response.Document.Root.Attribute(Common.serverMsg) != null)
                    {
                        string msg = Response.Document.Root.Attribute(Common.serverMsg).Value;

                        if (msg.Length > 0)
                        {
                            ApplicationEx.Instance.DisplayMessageBox(msg, "Server Broadcast Message");
                            Response.Document.Root.Attribute(Common.serverMsg).Remove();
                        }
                    }

                    #region Chunking
                    if (Response.Document.Root.Attribute(Common.Chunking.chunking) != null &&
                        Common.boolValue(Response.Document.Root.Attribute(Common.Chunking.chunking).Value))
                    {
                        this._chunking = true;

                        // Create cancel window
                        _chunkProgressText = Response.Document.Root.Attribute(Common.Chunking.chunkProgress).Value;
                        string progress = Response.Document.Root.Attribute(Common.Chunking.chunkProgressPercentage).Value;
                        _chunkProgressPercentage = 0;

                        if (progress.Length > 0)
                            int.TryParse(progress, out _chunkProgressPercentage);

                        if (_cancelChunkForm == null && !_chunkThreadStarted)
                        {
                            _chunkingCancelled = false;
                            //Thread t = new Thread(new ThreadStart(createLongOperationCancelForm));
                            //t.SetApartmentState( ApartmentState.STA);
                            //t.Priority = ThreadPriority.AboveNormal;
                            //t.Start();
                            _chunkThreadStarted = true;
                        }

                        if (_cancelChunkForm != null)
                        {
                            _cancelChunkForm.Description = _chunkProgressText;
                            _cancelChunkForm.Progress = _chunkProgressPercentage;
                            _chunkThreadStarted = false;
                        }
                    }
                    else if (_chunking)
                    {
                        this._chunking = false;
                        if (_cancelChunkForm != null)
                        {
                            _cancelChunkForm.Close();
                            _cancelChunkForm = null;
                        }
                    }

                    if (_chunking)
                    {
                        Request.Document.Root.RemoveAll();
                        XElement elt;

                        if (_cancelChunkForm != null && this._chunkingCancelled)
                        {
                            elt = new XElement(Common.Chunking.CancelChunk);
                            _cancelChunkForm.Close();
                            _cancelChunkForm = null;
                        }
                        else
                        {
                            elt = new XElement(Common.Chunking.KeepChunking);
                        }

                        Request.Document.Add(elt);
                    }
                }
                #endregion

                if (currentResponseRequest.ResponseCallBack != null)
                    currentResponseRequest.ResponseCallBack(true, Response);

                if (!_chunking)
                {
                    if (!_dontAutoPublishResponse)
                        PublishResponse(new XDocument(Response)); // NOTE: Doing a new in order to get a deep copy, not affected by other actions on it while it's being parsed
                }
            }
            finally
            {
                ApplicationEx.Instance.IsBusy = false;

                // Now that this request has been processed, move to the next entry in the queue
                if (RequestQueue.Count != 0)
                    SendRequestToServerPortal(RequestQueue.Dequeue());
            }
        }

        private void RemoteService_ExecAnonymousCompleted(object sender, ESAPortal.ExecAnonymousCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message.Length > 0)
                ApplicationEx.Instance.DisplayMessageBox(e.Error.Message, "Server Message");

            if (e.Result.Length > 0)
            {
                bool ok = false;
                Response = XDocument.Parse(e.Result);
                ok = Common.boolValue(Response.Document.Root.Attribute(Common.ResultSuccess).Value);

                if (Response.Document.Root.Attribute(Common.serverMsg) != null)
                {
                    string msg = Response.Document.Root.Attribute(Common.serverMsg).Value;

                    if (msg.Length > 0)
                        ApplicationEx.Instance.DisplayMessageBox(msg, "Server Broadcast Message");
                }
            }
        }

        internal void PublishResponse(XDocument responseDocument)
        {
            //TODO: Refactor to fire event to UI
            if (responseDocument.Root.Attribute(Common.serverMsg) != null && !string.IsNullOrEmpty(responseDocument.Root.Attribute(Common.serverMsg).Value))
                ApplicationEx.Instance.DisplayMessageBox(responseDocument.Root.Attribute(Common.serverMsg).Value, "System Broadcast Message");

            //System.Diagnostics.Debug.WriteLine("PUBLISH RESPONSE:\r\n" + responseDocument.ToString());

            XNode child = responseDocument.Root.FirstNode;

            while (child != null)
            {
                if (child is XElement)
                {
                    XElement xml = (XElement)child;

                    switch (xml.Name.ToString())
                    {
                        case Common.ActivityNode:
                            IActivityContainer publishingContainer = null;
                            string activityStamp = xml.Attribute(Common.ActivityHandle).Value;

                            if (CurrentActivityDupIndex > 0)
                            {
                                activityStamp += "/" + CurrentActivityDupIndex.ToString();
                                CurrentActivityDupIndex = 0;
                            }

                            // Look for the activity in myActivities Dictionary
                            if (activityStamp.Length > 0)
                            {
                                if (ApplicationEx.PickListActivity != null && ApplicationEx.PickListActivity == activityStamp)
                                {
                                    ApplicationEx.PickListActivity = null;
                                    ApplicationEx.PickListXml = xml;
                                }
                                else
                                {
                                    if (ApplicationEx.Instance.OpenActivities.ContainsKey(activityStamp))
                                        publishingContainer = ApplicationEx.Instance.OpenActivities[activityStamp];
                                }
                            }

                            if (publishingContainer != null)
                                publishingContainer.PublishResponse(xml);

                            break;

                        case Common.Requests.ActivityRequest:
                            try
                            {
                                ApplicationEx.Instance.CreateActivityContainer(xml);
                            }
                            catch (FormMappingNotFoundException ex)
                            {
                                throw;
                            }
                            catch (Exception ex)
                            {
                                Logging.LogException(ex);
                            }

                            break;

                        case Common.MessagesNode:
                            try
                            {
                                if (xml.FirstNode != null)
                                {
                                    string message = "The following message(s) have been returned from the server:";
                                    string title = null;

                                    XElement messageElt = (XElement)xml.FirstNode;

                                    while (messageElt != null)
                                    {
                                        message += Environment.NewLine + Environment.NewLine;
                                        message += messageElt.Value;

                                        if (message.Length == 0)
                                            message = "An unknown error has been reported by the server.";

                                        title = messageElt.Attribute(Common.MessageType).Value;

                                        messageElt = (XElement)messageElt.NextNode;
                                    }

                                    ApplicationEx.Instance.DisplayMessageBox(message, title ?? "Error");
                                }
                            }
                            catch (Exception ex)
                            {
                                Logging.LogException(ex);
                            }

                            break;

                        case Common.MenuNode:
                            //TODO what is this for?
                            break;
                    }//end-switch
                }

                child = child.NextNode;
            }
        }
        #endregion

        #region Chunking Stuff
        private bool _chunking = false;
        protected bool _chunkingCancelled;
        private bool _chunkThreadStarted;
        private int _chunkProgressPercentage;
        private string _chunkProgressText;
        internal IProgressStatusWindow _cancelChunkForm;

        public int ChunkProgressPercentage
        {
            get
            {
                return _chunkProgressPercentage;
            }
        }

        public string ChunkProgressText
        {
            get
            {
                return _chunkProgressText;
            }
        }

        public void CancelChunking()
        {
            _chunkingCancelled = true;
        }
        #endregion

        #region Private Classes
        private class RequestDetails
        {
            public XElement[] RequestPayload { get; set; }
            public ImportanceLevels Importance { get; set; }
            public ResponseCallBackDelegate ResponseCallBack { get; set; }

            public enum ImportanceLevels
            {
                Vital,
                Important,
                Unimportant
            }

            public RequestDetails(XElement[] requestPayload)
            {
                RequestPayload = requestPayload;
            }

            public RequestDetails(XElement[] requestPayload, ResponseCallBackDelegate responseCallBack)
            {
                RequestPayload = requestPayload;
                ResponseCallBack = responseCallBack;
            }

            public RequestDetails(XElement[] requestPayload, ResponseCallBackDelegate responseCallBack, ImportanceLevels importance)
            {
                RequestPayload = requestPayload;
                ResponseCallBack = responseCallBack;
                Importance = importance;
            }
        } 
        #endregion
    }

    public delegate void MessageDelegate(string message);
}