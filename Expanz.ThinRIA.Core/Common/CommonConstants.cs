namespace Expanz
{
    using System;

    internal static class Common
    {
        public const string ActivityHandle = "activityHandle";
        public const string ActivityHintField = "hintField";
        public const string ActivityName = "name";
        public const string ActivityNode = "Activity";
        public const string ActivityPersistentId = "ActivityPersistentId";
        public const string ActivityStyle = "style";
        public const string ActivityTitle = "title";
        public const string ActivityTitleField = "titleField";
        public const string appServerURI = "appServerURI";
        public const string AutoGen = "[AUTO]";
        public const string Base64Encoding = "BASE64";
        public const string BLOBCacheFixedPath = "blobs";
        public const string BootstrapUser = "ESA_ADMIN";
        public const string broadcastMsg = "broadcastMsg";
        public const string BypassAKProcessing = "bypassAKProcessing";
        public const string CarriageReturn = "[@CR]";
        public const string clientAction = "clientAction";
        public const string closeWindow = "closeWindow";
        public const string Content = "content";
        public const string contextObject = "contextObject";
        public const string CreateSession = "CreateSession";
        public const string CRLF = "[$CRLF$]";
        public const string Current = "$current$";
        public const string Custom = "$custom";
        public const string CustomContent = "CustomContent";
        public const string Dashboard = "Dashboard";
        public const string DataPublication = "DataPublication";
        public const string Default = "default";
        public const string DefaultAction = "defaultAction";
        public const int DefaultSessionManagerPort = 8180;
        public const int DefaultSiteManagerPort = 8150;
        public const string Deprecated = "deprecated";
        public const string Dirty = "dirty";
        public const string FieldNode = "Field";
        public const string FileActionPrint = "print";
        public const string FileActionSaveAs = "saveAs";
        public const string FileActionView = "view";
        public const string focusField = "focusField";
        public const string Graph = "Graph";
        public const string HostActivityHandle = "HOST";
        public const string IDAttrib = "id";
        public const string IncludeNulls = "~";
        public const string InitialKey = "initialKey";
        public const string itemType = "itemType";
        public const string IsSearchField = "isSearchField";
        public const string keepAudit = "keepAudit";
        public const string KeyValueKey = "key";
        public const string KeyValueLabel = "label";
        public const string KeyValueValue = "value";
        public const string listview = "listview";
        public const string LongDataEncoding = "encoding";
        public const string LongDataValueIndicator = "$longData$";
        public const string MenuAction = "action";
        public const string MENUACTION_TextMatch = "TEXTMATCH";
        public const string MENUACTION_TextMatchAll = "TEXTMATCHALL";
        public const string MenuItem = "MenuItem";
        public const string MenuNode = "Menu";
        public const string MenuSeparator = "Separator";
        public const string MessageNode = "Message";
        public const string MessagesNode = "Messages";
        public const string MessageType = "type";
        public const string ModelObject = "ModelObject";
        public const string Multiline = "multiline";
        public const string MultiSelect = "multiSelect";
        public const string NameAttrib = "name";
        public const string NavigateDirection = "direction";
        public const string NavigateFirst = "first";
        public const string NavigateLast = "last";
        public const string NavigateNext = "next";
        public const string NavigatePrevious = "prev";
        public const string Newline = "[@LF]";
        public const string None = "none";
        public const string Null = "(null)";
        public const string OperationalMode = "operationalMode";
        public const string PersistentId = "PersistentId";
        public const string pickField = "pickField";
        public const string Picklist = "picklist";
        public const string PicklistSelectMethod = "selectMethod";
        public const string PopupMessage = "popup";
        public const string ProcessActivity = "Activity";
        public const string ProcessArea = "ProcessArea";
        public const string ProcessSeparator = "Separator";
        public const string ProcessTitle = "title";
        public const string PublishActivities = "Activities";
        public const string PublishActivity = "Activity";
        public const string PublishAppSite = "AppSite";
        public const string PublishAppSites = "AppSites";
        public const string PublishFieldChecked = "checked";
        public const string PublishFieldDisabled = "disabled";
        public const string PublishFieldHidden = "hidden";
        public const string PublishFieldHint = "hint";
        public const string PublishFieldLabel = "label";
        public const string PublishFieldMasked = "masked";
        public const string PublishFieldMaxLength = "maxLength";
        public const string PublishFieldNullable = "nullable";
        public const string PublishFieldStatic = "static";
        public const string PublishFieldValue = "value";
        public const string referenceObject = "referenceObject";
        public const string RegistryHome = @"Software\expanz\";
        public const string RegistryTCPPort = "ChannelPort";
        public const string RequestFileAction = "action";
        public const string RequestFileActionName = "name";
        public const string RequestMethodName = "name";
        public const string ResultSuccess = "success";
        public const string Root = "[root]";
        public const string RootElementName = "ESA";
        public const string SchemaVersion = "2.0";
        public const string SecureChannel = "secureChannel";
        public const string SelectCount = "selectCount";
        public const string serverMsg = "serverMessage";
        public const string State = "state";
        public const string StringTextCase = "textCase";
        public const string SystemError = "!System Error!";
        public const string text = "text";
        public const string TextCaseInvisible = "invisible";
        public const string TextCaseLower = "lower";
        public const string TextCaseProper = "proper";
        public const string TextCaseUpper = "upper";
        public const string treeview = "treeview";
        public const string TrickleContent = "trickle";
        public const string userName = "userName";
        public const string valueIsApportioned = "valueIsApportioned";
        public const string ValueIsNull = "null";
        public const string VisualType = "visualType";
        public const string windowTitle = "windowTitle";
        public const string backgroundLabel = "backgroundLabel";
        public const string valid = "valid";
        public const string SetIdFromContextAttribute = "SetIdFromContext";        

        public static string NewEmptyXmlDoc
        {
            get { return "ESA"; }
        }

        public static bool boolValue(string s)
        {
            if (s.Length == 0) return false;
            switch (s.ToLower())
            {
                case "0":
                case "n":
                case "no":
                case "false":
                case "(null)":
                    s = bool.FalseString;
                    break;

                case "1":
                case "y":
                case "yes":
                case "true":
                    s = bool.TrueString;
                    break;
                default:
                    s = bool.TrueString;
                    break;
            }

            return Convert.ToBoolean(s);
        }

        public static string boolString(bool? b)
        {
            if (b != null & b == true)
            {
                return bool.TrueString;
            }
            else
            {
                return bool.FalseString;
            }

        }

        public enum Filter
        {
            Like = 1,
            EqualTo = 2,
            GreaterThan = 3,
            LessThan = 4,
            AnyLike = 5,
        }

        public static class Authentication
        {
            public const string CanChangeCompanyRole = "canChangeCompanyRole";
            public const string DomainAttrib = "authDomain";
            public const string ModeAlternate = "ALTERNATE";
            public const string ModeAttrib = "authenticationMode";
            public const string ModeGuest = "GUEST";
            public const string ModePrimary = "PRIMARY";
            public const string PasswordAttrib = "password";
            public const string ProvisioningNode = "Provisioning";
            public const string SiteAttrib = "appSite";
            public const string UserAttrib = "user";
            public const string WorkstationAttrib = "station";
        }

        public class Chunking
        {
            public const string CancelChunk = "CancelChunk";
            public const string chunking = "chunking";
            public const string chunkProgress = "chunkProgress";
            public const string chunkProgressPercentage = "chunkProgressPercentage";
            public const string KeepChunking = "KeepChunking";

        }

        public class ClientType
        {
            public const string Attrib = "clientType";
            public const string FLEX = "FLEX";
            public const string HTML = "HTML";
            public const string Tester = "TESTER";
            public const string WIN32 = "WIN32";
            public const string XAML = "XAML";
        }

        public static class ContextMenu
        {
            public const string EmptyContextDelegate = "emptyContextDelegate";
            public const string ListEmpty = "listEmpty";
            public const string ListItem = "listItem";
            public const string RecursiveList = "recursiveList";
            public const string Tree = "tree";
        }

        public static class Data
        {
            public const string AutoPopulate = "autoPopulate";
            public const string AutoSizeColumns = "autoSizeColumns";
            public const string CanEditColumnField = "canEdit";
            public const string Column = "Column";
            public const string ColumnCount = "count";
            public const string ColumnEditable = "editable";
            public const string ColumnField = "field";
            public const string ColumnLabel = "label";
            public const string ColumnName = "name";
            public const string Columns = "Columns";
            public const string ColumnWidth = "width";
            public const string ContextId = "contextId";
            public const int DefaultColumnWidth = 100;
            public const string displayStyle = "displayStyle";
            public const string HasEditableColumns = "hasEditableColumns";
            public const string hRef = "hRef";
            public const string MatrixKey = "matrixKey";
            public const string Node = "Data";
            public const string PublicationMetadata = "Metadata";
            public const string PublicationMethod = "populateMethod";
            public const string PublicationQuery = "query";
            public const string Publish = "Publish";
            public const string Row = "Row";
            public const string RowCell = "Cell";
            public const string RowCount = "rowCount";
            public const string Rows = "Rows";
            public const string RowType = "Type";
            public const string selected = "selected";
            public const string sortValue = "sortValue";
            public const string Type = "Type";
            public const string Types = "Types";
            public const string SetIdFromContext = "SetIdFromContext";
        }

        public static class Datatypes
        {
            public const string AttribName = "datatype";
            public const string BLOB = "BLOB";
            public const string Boolean = "bool";
            public const string DateTime = "datetime";
            public const string Enum = "Enum";
            public const string None = "none";
            public const string Number = "number";
            public const string SrvBtn = "SrvBtn";
            public const string String = "string";
            public const string Variant = "Variant";
        }

        public static class ContentTypes
        {
            public const string AttribName = "contentType";
            public const string URL = "url";
            public const string EmailAddress = "email";
        }

        public static class Preferences
        {
            public const string Preference = "Preference";
            public const string PublishNode = "PublishPreferences";
            public const string SaveNode = "SavePreferences";
            public const string SystemUse = "SystemUse";
        }

        public static class Requests
        {
            public const string ActivityClose = "Close";
            public const string ActivityCreate = "CreateActivity";
            public const string ActivityRequest = "ActivityRequest";
            public const string ContextMenu = "ContextMenu";
            public const string Dashboard = "Dashboard";
            public const string DragDrop = "DragDrop";
            public const string DrillDown = "DrillDown";
            public const string FieldLastValue = "LastValue";
            public const string FieldMetadata = "Field";
            public const string FieldValueChange = "Delta";
            public const string File = "GetFile";
            public const string GetSessionFields = "GetSessionFields";
            public const string InitFind = "InitFind";
            public const string MenuAction = "MenuAction";
            public const string MethodInvocation = "Method";
            public const string Navigate = "Navigate";
            public const string Notification = "Notification";
            public const string PublishSchema = "PublishSchema";
            public const string ReplayTestScript = "ReplayTestScript";
            public const string SendSessionTestEmail = "SendSessionTestEmail";
            public const string SessionData = "GetSessionData";
            public const string SetContext = "Context";
            public const string SetSessionField = "SetSessionField";
            public const string StartRecording = "StartRecording";
            public const string StopRecording = "StopRecording";
            public const string TreeChildren = "TreeChildren";
            public const string TreeClick = "TreeClick";
            public const string TreeContext = "TreeContext";
        }

        public static class Scripting
        {
            public const string ScriptHeaderNode = "TestScript";
            public const string ScriptPairNode = "ScriptPair";
        }

        public static class UIMessage
        {
            public const string Action = "Action";
            public const string Actions = "Actions";
            public const string label = "label";
            public const string Node = "UIMessage";
            public const string Option = "Option";
            public const string Options = "Options";
            public const string Request = "Request";
            public const string Response = "Response";
            public const string selectedOption = "selectedOption";
            public const string text = "text";
            public const string Title = "title";
            public const string type = "type";
            public const string typeList = "list";
            public const string typeRadioButton = "radioButton";
        }

        public static class Workflow
        {
            public const string Activity = "activity";
            public const string HasWorkflowTrays = "hasWorkflowTrays";
            public const string PopulateMethod = "method";
            public const string PopulateType = "type";
            public const string TrayName = "name";
            public const string WorkflowTray = "NotificationTray";

            public static class AssignTo
            {
                public const string Originator = "_Originator";
                public const string Self = "_Self";
            }
        }
    }
}
