using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Expanz.Extensions.BCL;
using Expanz.ThinRIA.Core;

namespace Expanz.ThinRIA.Controls
{
    public class TreeViewEx : TreeView, IServerBoundControl, IRepeatingDataControl, IContextMenuPublisher
    {
        #region Member Variables
        private int myValue;
		private Dictionary<int, TreeViewItemEx> myItemList;
        protected ControlHarness _controlHarness;
        protected string _populateQuery;
        protected string _populateMethod;
        protected string _autoPopulate;
        protected string _modelObject;
        private string _fieldId;
        private int myContextKey;
        private string myContextType;
        protected bool isSinglePopulate = true;
        #endregion

        #region Constructor
        public TreeViewEx()
            : base()
        {
        }

        public override void EndInit()
        {
            base.EndInit();
            _controlHarness = new ControlHarness(this);
        } 
        #endregion

        #region Public Properties
        protected Dictionary<int, TreeViewItemEx> ItemList
        {
            get { if (myItemList == null) myItemList = new Dictionary<int, TreeViewItemEx>(); return myItemList; }
        }

        internal int Value { set { myValue = value; SelectValue(); } } 
        #endregion

        #region Private Methods
        private void SelectValue()
        {
            if (ItemList.ContainsKey(myValue))
            {
                TreeViewItemEx item = ItemList[myValue];
                item.IsSelected = true;
                if (!item.IsVisible)
                {
                    if (item.Parent is TreeViewItemEx)
                    {
                        TreeViewItemEx p = (TreeViewItemEx)item.Parent;
                        while (p != null)
                        {
                            p.IsExpanded = true;
                            if (p.Parent is TreeViewItemEx)
                            {
                                p = (TreeViewItemEx)p.Parent;
                            }
                            else p = null;
                        }
                    }
                }
            }
            else
            {
                if (SelectedItem != null)
                {
                    TreeViewItemEx item = (TreeViewItemEx)this.SelectedItem;
                    item.IsSelected = false;
                }
            }
        } 

		private TreeViewItemEx ProcessTreeBranch(XElement branch, XNode types, string selectedType, int selectedId, ref TreeViewItemEx selectedNode)
		{
			TreeViewItemEx returnVal=null;
			// Get the text, id, type and hasChildren attributes
			string theText = branch.GetAttribute("text");
			if (isSinglePopulate)
				theText = branch.GetAttribute("value");
			int theId = 0;
			int.TryParse(branch.GetAttribute(Common.IDAttrib),out theId);
			string tooltip = branch.GetAttribute("tooltip");
			string theType = branch.GetAttribute(Common.Data.RowType);

			// Determine if the node is to have children (display a + sign)
			bool hasChildren = false;
            if (isSinglePopulate)
            {
                hasChildren = branch.HasElements;
            }
            else
            {
                hasChildren = Common.boolValue(branch.GetAttribute("hasChildren"));
            }

			// If no children, just add a single node.
			// If there are to be children (but none passed in the XML), create a dummy child.
			// Otherwise, process each child separately and add as a child to this node.
			if (!hasChildren)
			{
				returnVal = new TreeViewItemEx(this, theId, theType, theText, tooltip, false);
			}
			else
			{
				if (!isSinglePopulate && (branch.FirstNode == null || !(branch.FirstNode is XElement)))
				{
					returnVal = new TreeViewItemEx(this, theId, theType, theText, tooltip, false);
				}
				else
				{
					returnVal = new TreeViewItemEx(this, theId, theType, theText, tooltip, true);

					XElement leaf = (XElement)branch.FirstNode;
					while (leaf != null)
					{
						TreeViewItemEx treeBranch = ProcessTreeBranch(leaf, types, selectedType, selectedId, ref selectedNode);
						returnVal.Items.Add(treeBranch);
						ItemList.Add(treeBranch.ID, treeBranch);
						leaf = (XElement)leaf.NextNode;
					}
					returnVal.ChildrenFetched = true;
				}
			}

			// Set the selectedNode if it's a match
			if (theType == selectedType && theId == selectedId)
				selectedNode = returnVal;
			return returnVal;
		}

		internal void ClearSelection()
		{
			if (SelectedItem != null && SelectedItem is TreeViewItem)
			{
				((TreeViewItem)SelectedItem).IsSelected = false;
			}
        }
        #endregion

        #region Context Menu
        private XElement CreateRequestElement(string elementName)
        {
            XElement returnElement = new XElement(elementName);

            if (this.ModelObject != null && this.ModelObject.Length > 0)
                returnElement.SetAttributeValue(Common.contextObject, this.ModelObject);

            return returnElement;
        }
        private void SetContextObject(XElement xml)
        {
            string context = null;

            if (!string.IsNullOrEmpty(this.ModelObject))
                context = this.ModelObject;

            if (context == null && !string.IsNullOrEmpty(_controlHarness.ParentActivity.FixedContext))
                context = _controlHarness.ParentActivity.FixedContext;

            if (context != null)
                xml.SetAttributeValue(Common.contextObject, context);
        }
        public void ContextMenuOpening(TreeViewItemEx sender, ContextMenuEventArgs e)
        {
            ActivityHarness.ContextMenuPublisher = this;
            myContextKey = sender.ID;
            myContextType = sender.Type;
            var elementList = new XElement[2];

            var contextElement = CreateRequestElement(Common.Requests.TreeContext);
            contextElement.SetAttributeValue(Common.IDAttrib, myContextKey);
            contextElement.SetAttributeValue(Common.Data.RowType, myContextType);
            //contextElement.SetAttributeValue(Common.SetIdFromContextAttribute, "1");

            SetContextObject(contextElement);
            elementList[0] = contextElement;
            var xmenu = CreateRequestElement(Common.Requests.ContextMenu);
            xmenu.SetAttribute("Type", "recursiveList");
            elementList[1] = xmenu;
            _controlHarness.SendXml(elementList);
        }
        public void PublishContextMenu(XElement menu)
        {
            ActivityHarness.ContextMenuPublisher = null;
            ContextMenu m = _controlHarness.createContextMenu(menu, theItem_Click);
            this.ContextMenu = m;
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, (new openContextMenuDelegate(_openContextMenu)));
        }

        private void _openContextMenu()
        {
            this.ContextMenu.IsOpen = true;
        }
        void theItem_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem mi = (System.Windows.Controls.MenuItem)sender;
            XElement actionElement = CreateRequestElement(Common.Requests.MenuAction);
            actionElement.SetAttribute(Common.MenuAction, mi.Tag.ToString());
            SetContextObject(actionElement);
            _controlHarness.SendXml(actionElement);
        }
        #endregion

        #region Implementation of IServerBoundControl
        /// <summary>
        /// Set the field on the Server ModelObject to bind this controls input values to.
        /// </summary>       
        [Category("expanz")]
        [Description("Set the field on the Server ModelObject to bind this controls input values to.")]
        public string FieldId
        {
            get { return string.IsNullOrEmpty(_fieldId) ? SafeName : _fieldId; }
            set { _fieldId = value; }
        }

        public void SetVisible(bool visible)
        {
            Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region Implementation of IRepeatingDataControl
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string DataId
        {
            get { return SafeName ?? FieldId ?? QueryId ?? PopulateMethod; }
        }

        private string SafeName
        {
            get { return string.IsNullOrEmpty(Name) ? null : Name; }
        }

        [Category("expanz")]
        [Description("Set the ID of the Server Query to execute and bind the results to.")]
        public string QueryId
        {
            get { return _populateQuery; }
            set { _populateQuery = value; }
        }

        [Category("expanz")]
        [Description("Set the name of the Server method to call to get results to bind to.")]
        public string PopulateMethod
        {
            get { return _populateMethod; }
            set { _populateMethod = value; }
        }

        [Category("expanz")]
        [Description("Set the Server ModelObject to execute the PopulateMethod on.")]
        public string ModelObject
        {
            get { return _modelObject; }
            set { _modelObject = value; }
        }

        [Category("expanz")]
        [Description("Set to true to auto execute the PopulateMethod or the QueryID.")]
        public string AutoPopulate
        {
            get { return _autoPopulate; }
            set { _autoPopulate = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void FillServerRegistrationXml(XElement dp)
        {
            dp.SetAttribute(Common.Data.RowType, "recursiveList");
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public ControlHarness BindingHarness
        {
            get { return _controlHarness; }
        }

        public void PublishData(XElement publishElement)
        {
            this.Items.Clear();
            myItemList = null;
            if (Common.boolValue(publishElement.GetAttribute("clearOnly")))
                return;

            // Get the type and row nodes
            XElement typeNodes = null;
            XElement rowNodes = null;
            XElement childNode = (XElement)publishElement.FirstNode;
            while (childNode != null)
            {
                if (childNode.Name == "Types")
                {
                    typeNodes = childNode;
                }
                else if (childNode.Name == Common.Data.Rows)
                {
                    rowNodes = childNode;
                }
                childNode = (XElement)childNode.NextNode;
            }
            if (rowNodes == null) return;
            // The server may indicate the ID and type of node to select
            string selectedType = ((XElement)rowNodes).GetAttribute("selectedType");
            int selectedId = myValue;
            if (((XElement)rowNodes).GetAttribute("selectedId").Length > 0)
            {
                selectedId = int.Parse(((XElement)rowNodes).GetAttribute("selectedId"));
            }
            TreeViewItemEx selectedNode = null;

            // Add the branches to the tree, setting the selectedNode if it is in the branch
            XElement rowNode = (XElement)rowNodes.FirstNode;
            while (rowNode != null)
            {
                TreeViewItemEx treeBranch = ProcessTreeBranch(rowNode, typeNodes, selectedType, selectedId, ref selectedNode);
                this.Items.Add(treeBranch);
                this.ItemList.Add(treeBranch.ID, treeBranch);
                rowNode = (XElement)rowNode.NextNode;
            }

            // Select the selectedNode, or the first node if selectedNode is null
            try
            {
                if (selectedNode == null && Items.Count > 0)
                {
                    ((TreeViewItemEx)this.Items[0]).IsExpanded = true;
                }
                if (selectedNode != null)
                {
                    selectedNode.IsSelected = true;
                }
            }
            catch (Exception ex) { ApplicationEx.DebugException(ex); }
        }

        public void PreDataPublishXml(XElement fieldElement)
        {

        }

        public void PublishXml(XElement xml)
        {

        }
        #endregion
	}
}
