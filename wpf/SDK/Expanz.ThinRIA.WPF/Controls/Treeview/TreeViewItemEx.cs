using System.Windows.Controls;

namespace Expanz.ThinRIA.Controls
{
    public class TreeViewItemEx : TreeViewItem
    {
        int myId;
        public int ID { get { return myId; } }
        string myType;
        private TreeViewEx _treeview;
        public string Type { get { return myType; } }
        private bool childrenFetched;
        public bool ChildrenFetched
        {
            get { return childrenFetched; }
            set { childrenFetched = value; }
        }

        public TreeViewItemEx( ): base()
        {
        }

        void TreeViewItemEx_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            _treeview.ContextMenuOpening(this, e);
        }
        public TreeViewItemEx(TreeViewEx tv, int id, string type, string text, string tooltip, bool hasChildren)
            : base()
        {
            _treeview = tv;
            myId = id;
            myType = type;
            Header = text;
            if (tooltip != null && tooltip.Length > 0) ToolTip = tooltip;
            if (!hasChildren)
            {
                this.ContextMenuOpening += new ContextMenuEventHandler(TreeViewItemEx_ContextMenuOpening);
            }
        }
    }
}
