using System.Xml.Linq;
using Expanz.ThinRIA.Controls;
using System.Collections.Generic;

namespace Expanz.ThinRIA.Core
{
	public enum OptimiseTiming { None, Pre, Post, Auto }

	public delegate void NotifyOnClose(IActivityContainer childContainer);

	public interface IActivityDescription
	{
        string ActivityName { get; set; }
        string ActivityStyle { get; set; }
		string IconImageName { get; }
	}

	public interface IActivityContainer : IActivityDescription
	{
        ApplicationEx AppHost { get; }
		void Initialise(XElement xml);
		void InitialiseCopy(ActivityHarness activityHarness);
		bool IsInitialised { get; }
		string ActivityStamp { get; }
		string ActivityStampEx { get; }
		int DuplicateIndex { get; }
		string FixedContext { get; }
		int PersistentKey { get; }
		void RegisterControl(object control);
        void RegisterDirtyButton(ButtonEx button);
#if WPF
        void RegisterSummaryContainer(IContainerWithSummary container);
#endif
		void Exec(XElement DeltaXml);
		void Exec(XElement[] DeltaXml);
		void AppendDataPublicationsToActivityRequest(XElement xml);        
		void PublishResponse(XElement xml);
		bool IsPublishing { get; }
        bool IsLoading { get; }
		void PublishDirtyChange(string modelObject, bool dirty);
        //void ResetWindowTitle(ActivityHarness harness);
		//IMessagePanel MessagePanel { set; }
        //void CloseOnLogout();
        //void Focus();
		OptimiseTiming Optimsation { get; set; }
		string ImplementationName { get; }
		bool IsOKToClose { get; }
	}

	/// <summary>
	/// Generic interface for any 'Managed' control
	/// </summary>
	public interface IServerBoundControl
	{
		string FieldId { get; }
        void SetVisible(bool visible);
        void PublishXml(XElement xml);
	}

    public interface IServerBoundButton
    {
        string MethodName { get; }
        void SetVisible(bool visible);
        void SetDisabled(bool disable);
    }

	public interface IServerBoundControlContainer : IServerBoundControl
	{
		
	}

	/// <summary>
	/// Generic Interface for any control hosting repeating data
	/// </summary>
    public interface IRepeatingDataControl : IServerBoundControl
	{
		void PublishData(XElement xml);
		string DataId { get; }
		string QueryId { get; }
		string PopulateMethod { get; }
		string ModelObject { get; }
		string AutoPopulate { get; }
		void FillServerRegistrationXml(XElement xml);
        void PreDataPublishXml(XElement fieldElement); // Used for much the same purpose as the PublishXml method, but called *before* the PublishData method is called (instead of after as PublishXml is)
    }

    public interface IValueDestination : IServerBoundControl
    {
        void SetNull();
        void SetValue(string text);
        void SetHint(string hint);
    }

    public interface IFieldLabel : IServerBoundControl
    {
        void SetLabel(string label);
        string LabelText { get; }
    }

	/// <summary>
	/// An editable control (typeable or clickable)
	/// </summary>
    public interface IEditableControl : IServerBoundControl, IValueDestination
	{
		XElement DeltaXml { get; }
		void SetEditable(bool editable);
	}

    /// <summary>
    /// Display targeted error message 'near' to the approriate field
    /// </summary>
    public interface IFieldErrorMessage
    {
        void ShowError(XElement xml);
        void HideError();
        bool IsFieldValueValid { set; }
    }

    public interface IMethodCaller
    {
        string MethodName { get; }
        void SetVisible(bool visible);
        void SetDisabled(bool disable);
        string ModelObject { get; }
        bool IsDirty { set; }
        bool IsDirtyButton { get; }
    }

    /// <summary>
    /// interface to provide short summary text for a collapsed container
    /// </summary>
    public interface IContainerSummaryText
    {
        /// <summary>
        /// the property to use for summary text; varies by control
        /// </summary>
        string SummaryProperty { get; set; }
        /// <summary>
        /// the actual summary text; may be null if no property specified
        /// </summary>
        string SummaryText { get; }
    }

    /// <summary>
    /// a visual container that displays a summary when collapsed (children hidden)
    /// </summary>
    public interface IContainerWithSummary
    {
        List<IContainerSummaryText> SummaryChildren { get; }
        void RefreshSummaryText(bool childChanged);
    }

	/// <summary>
	/// a control bound to repeating tuples (x,y) (x,y,z) etc published by the server
	/// </summary>
	public interface IGraphControl
	{
		void PublishGraph(XElement data);
		string GraphId { get; }
	}

    public interface IMessageDisplay
    {
        void PublishMessage(XElement data);
        void Clear();
    }

    public interface IMenuRenderer
    {
        void LoadMenu(XElement data);
    }

	public interface ICustomContentPublisher
	{
		void PublishCustomContent(XElement content);
		string ContentId { get; }
	}

	public interface ICustomSchemaPublisher
	{
		void CustomPublishSchema(XElement schema, string fixedContext);
	}

	public interface IContextMenuPublisher
	{
		void PublishContextMenu(XElement menu);
	}

	/// <summary>
	/// Common model popup message handling
	/// </summary>
	public interface IMessageBox
	{
		void Show(string body, string caption);
		void Hide();
	}

	public interface IProgressStatusWindow
	{
		int Progress { set; }
		string Description { set; }
		void Close();
	}

	public interface IBarcodeDestination
	{
		void SetBarcode(string s);
	}

	public interface IImageDestination
	{
		void SetImage(System.Windows.Media.ImageSource img);
	}

	public interface ICustomActivityRequestHandler
	{
		void UserActivityRequest(string activity, string style, IActivityDescriptionEx ad);
	}

	public interface IDashboardPublisher
	{
		void PublishDashboard(XElement dashboard);
	}

	public interface IActivityDescriptionEx : IActivityDescription
	{
		string MenuTitle { get; }
		string WindowTitle { get; }
	}

	public interface INavigationPanel
	{
	}

    public interface IModelControl
    {
        string ModelTypeFullName { get; set; }
    }

    public interface IMediaControl : IServerBoundControl
    {
        MediaPublishTypes PublishType { get; set; }
        string DataId { get; }
    }

    public enum MediaPublishTypes
    {
        Url,
        Base64
    }
}
