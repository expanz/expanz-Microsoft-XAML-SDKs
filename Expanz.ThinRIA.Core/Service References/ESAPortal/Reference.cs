﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Expanz.ThinRIA.Core.ESAPortal {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="ESAPortal.IESAService")]
    public interface IESAService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IESAService/AddSite", ReplyAction="http://tempuri.org/IESAService/AddSiteResponse")]
        bool AddSite(string masterPassword, string site, string name, string authMethod, ref string messages);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IESAService/BroadcastMessage", ReplyAction="http://tempuri.org/IESAService/BroadcastMessageResponse")]
        string BroadcastMessage(string masterPassword, string site, string message, ref string errors);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IESAService/ChangeUserPassword", ReplyAction="http://tempuri.org/IESAService/ChangeUserPasswordResponse")]
        bool ChangeUserPassword(string user, string oldPassword, string newPassword, ref string errors);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IESAService/CreateSession", ReplyAction="http://tempuri.org/IESAService/CreateSessionResponse")]
        string CreateSession(string inXml, ref string errorMessage);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IESAService/CreateUser", ReplyAction="http://tempuri.org/IESAService/CreateUserResponse")]
        bool CreateUser(string masterPassword, string user, string description, string userPassword, ref string errors);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IESAService/DeleteUser", ReplyAction="http://tempuri.org/IESAService/DeleteUserResponse")]
        bool DeleteUser(string masterPassword, string user, ref string errors);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IESAService/DisableSessionManager", ReplyAction="http://tempuri.org/IESAService/DisableSessionManagerResponse")]
        bool DisableSessionManager(string masterPassword, string site, string SessionManagerURI, bool disable, ref string errors);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IESAService/Exec", ReplyAction="http://tempuri.org/IESAService/ExecResponse")]
        string Exec(string inXML, ref string sessionHandle, ref string errors);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IESAService/ExecAnonymous", ReplyAction="http://tempuri.org/IESAService/ExecAnonymousResponse")]
        string ExecAnonymous(string site, string inXml, ref string errors);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IESAService/ExecAnonymousWithTestMode", ReplyAction="http://tempuri.org/IESAService/ExecAnonymousWithTestModeResponse")]
        string ExecAnonymousWithTestMode(string site, string inXml, bool testMode, ref string errors);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IESAService/GetSchemaForActivity", ReplyAction="http://tempuri.org/IESAService/GetSchemaForActivityResponse")]
        string GetSchemaForActivity(string site, string activity, ref string errors);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IESAService/ListActivitiesForSite", ReplyAction="http://tempuri.org/IESAService/ListActivitiesForSiteResponse")]
        string ListActivitiesForSite(string site, ref string errors);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IESAService/ListAvailableSites", ReplyAction="http://tempuri.org/IESAService/ListAvailableSitesResponse")]
        string ListAvailableSites(ref string errors);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IESAService/ListSessionManagersForSite", ReplyAction="http://tempuri.org/IESAService/ListSessionManagersForSiteResponse")]
        string ListSessionManagersForSite(string masterPassword, string site, ref string messages);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IESAService/ListSessionsForServer", ReplyAction="http://tempuri.org/IESAService/ListSessionsForServerResponse")]
        string ListSessionsForServer(string site, string serverURI, string password, ref string messages);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IESAService/ListUsers", ReplyAction="http://tempuri.org/IESAService/ListUsersResponse")]
        string ListUsers(string masterPassword, ref string errors);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IESAService/Ping", ReplyAction="http://tempuri.org/IESAService/PingResponse")]
        bool Ping(string sessionHandle);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IESAService/ProcessEDIWithTestMode", ReplyAction="http://tempuri.org/IESAService/ProcessEDIWithTestModeResponse")]
        string ProcessEDIWithTestMode(string EDIAccessKey, string site, string inXml, string xsdURL, bool testMode, ref string errors);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IESAService/ProcessEDI", ReplyAction="http://tempuri.org/IESAService/ProcessEDIResponse")]
        string ProcessEDI(string EDIAccessKey, string site, string inXml, string xsdURL, ref string errors);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IESAService/ReleaseSession", ReplyAction="http://tempuri.org/IESAService/ReleaseSessionResponse")]
        bool ReleaseSession(string sessionHandle, ref string errors);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IESAService/ResetUserPassword", ReplyAction="http://tempuri.org/IESAService/ResetUserPasswordResponse")]
        bool ResetUserPassword(string masterPassword, string user, string password, ref string errors);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IESAService/SetSessionManagerLoggingProfile", ReplyAction="http://tempuri.org/IESAService/SetSessionManagerLoggingProfileResponse")]
        bool SetSessionManagerLoggingProfile(string masterPassword, string site, string SessionManagerURI, bool logDebug, bool logInfo, bool logWarning, ref string errors);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IESAService/SyncronisePersistentSchema", ReplyAction="http://tempuri.org/IESAService/SyncronisePersistentSchemaResponse")]
        bool SyncronisePersistentSchema(string masterPassword, string site, ref string messages, ref string errors);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IESAService/TerminateSession", ReplyAction="http://tempuri.org/IESAService/TerminateSessionResponse")]
        bool TerminateSession(string site, string serverURI, string sessionHandle, string password, ref string messages);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IESAService/Test", ReplyAction="http://tempuri.org/IESAService/TestResponse")]
        string Test(string val);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IESAService/TrickleContent", ReplyAction="http://tempuri.org/IESAService/TrickleContentResponse")]
        int TrickleContent(string key, byte[] bytes, ref string errors);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IESAService/WebServerPing", ReplyAction="http://tempuri.org/IESAService/WebServerPingResponse")]
        bool WebServerPing();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IESAService/SiteManagerPing", ReplyAction="http://tempuri.org/IESAService/SiteManagerPingResponse")]
        bool SiteManagerPing();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IESAService/SetTraceLogging", ReplyAction="http://tempuri.org/IESAService/SetTraceLoggingResponse")]
        bool SetTraceLogging(string masterPassword, string site, bool traceEnabled, ref string errors);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IESAServiceChannel : Expanz.ThinRIA.Core.ESAPortal.IESAService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class ESAServiceClient : System.ServiceModel.ClientBase<Expanz.ThinRIA.Core.ESAPortal.IESAService>, Expanz.ThinRIA.Core.ESAPortal.IESAService {
        
        public ESAServiceClient() {
        }
        
        public ESAServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public ESAServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ESAServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ESAServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public bool AddSite(string masterPassword, string site, string name, string authMethod, ref string messages) {
            return base.Channel.AddSite(masterPassword, site, name, authMethod, ref messages);
        }
        
        public string BroadcastMessage(string masterPassword, string site, string message, ref string errors) {
            return base.Channel.BroadcastMessage(masterPassword, site, message, ref errors);
        }
        
        public bool ChangeUserPassword(string user, string oldPassword, string newPassword, ref string errors) {
            return base.Channel.ChangeUserPassword(user, oldPassword, newPassword, ref errors);
        }
        
        public string CreateSession(string inXml, ref string errorMessage) {
            return base.Channel.CreateSession(inXml, ref errorMessage);
        }
        
        public bool CreateUser(string masterPassword, string user, string description, string userPassword, ref string errors) {
            return base.Channel.CreateUser(masterPassword, user, description, userPassword, ref errors);
        }
        
        public bool DeleteUser(string masterPassword, string user, ref string errors) {
            return base.Channel.DeleteUser(masterPassword, user, ref errors);
        }
        
        public bool DisableSessionManager(string masterPassword, string site, string SessionManagerURI, bool disable, ref string errors) {
            return base.Channel.DisableSessionManager(masterPassword, site, SessionManagerURI, disable, ref errors);
        }
        
        public string Exec(string inXML, ref string sessionHandle, ref string errors) {
            return base.Channel.Exec(inXML, ref sessionHandle, ref errors);
        }
        
        public string ExecAnonymous(string site, string inXml, ref string errors) {
            return base.Channel.ExecAnonymous(site, inXml, ref errors);
        }
        
        public string ExecAnonymousWithTestMode(string site, string inXml, bool testMode, ref string errors) {
            return base.Channel.ExecAnonymousWithTestMode(site, inXml, testMode, ref errors);
        }
        
        public string GetSchemaForActivity(string site, string activity, ref string errors) {
            return base.Channel.GetSchemaForActivity(site, activity, ref errors);
        }
        
        public string ListActivitiesForSite(string site, ref string errors) {
            return base.Channel.ListActivitiesForSite(site, ref errors);
        }
        
        public string ListAvailableSites(ref string errors) {
            return base.Channel.ListAvailableSites(ref errors);
        }
        
        public string ListSessionManagersForSite(string masterPassword, string site, ref string messages) {
            return base.Channel.ListSessionManagersForSite(masterPassword, site, ref messages);
        }
        
        public string ListSessionsForServer(string site, string serverURI, string password, ref string messages) {
            return base.Channel.ListSessionsForServer(site, serverURI, password, ref messages);
        }
        
        public string ListUsers(string masterPassword, ref string errors) {
            return base.Channel.ListUsers(masterPassword, ref errors);
        }
        
        public bool Ping(string sessionHandle) {
            return base.Channel.Ping(sessionHandle);
        }
        
        public string ProcessEDIWithTestMode(string EDIAccessKey, string site, string inXml, string xsdURL, bool testMode, ref string errors) {
            return base.Channel.ProcessEDIWithTestMode(EDIAccessKey, site, inXml, xsdURL, testMode, ref errors);
        }
        
        public string ProcessEDI(string EDIAccessKey, string site, string inXml, string xsdURL, ref string errors) {
            return base.Channel.ProcessEDI(EDIAccessKey, site, inXml, xsdURL, ref errors);
        }
        
        public bool ReleaseSession(string sessionHandle, ref string errors) {
            return base.Channel.ReleaseSession(sessionHandle, ref errors);
        }
        
        public bool ResetUserPassword(string masterPassword, string user, string password, ref string errors) {
            return base.Channel.ResetUserPassword(masterPassword, user, password, ref errors);
        }
        
        public bool SetSessionManagerLoggingProfile(string masterPassword, string site, string SessionManagerURI, bool logDebug, bool logInfo, bool logWarning, ref string errors) {
            return base.Channel.SetSessionManagerLoggingProfile(masterPassword, site, SessionManagerURI, logDebug, logInfo, logWarning, ref errors);
        }
        
        public bool SyncronisePersistentSchema(string masterPassword, string site, ref string messages, ref string errors) {
            return base.Channel.SyncronisePersistentSchema(masterPassword, site, ref messages, ref errors);
        }
        
        public bool TerminateSession(string site, string serverURI, string sessionHandle, string password, ref string messages) {
            return base.Channel.TerminateSession(site, serverURI, sessionHandle, password, ref messages);
        }
        
        public string Test(string val) {
            return base.Channel.Test(val);
        }
        
        public int TrickleContent(string key, byte[] bytes, ref string errors) {
            return base.Channel.TrickleContent(key, bytes, ref errors);
        }
        
        public bool WebServerPing() {
            return base.Channel.WebServerPing();
        }
        
        public bool SiteManagerPing() {
            return base.Channel.SiteManagerPing();
        }
        
        public bool SetTraceLogging(string masterPassword, string site, bool traceEnabled, ref string errors) {
            return base.Channel.SetTraceLogging(masterPassword, site, traceEnabled, ref errors);
        }
    }
}
