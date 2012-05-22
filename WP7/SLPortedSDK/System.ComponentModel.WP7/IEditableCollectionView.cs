using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace System.ComponentModel
{
    public interface IEditableCollectionView
    {
        // Methods
        object AddNew();
        void CancelEdit();
        void CancelNew();
        void CommitEdit();
        void CommitNew();
        void EditItem(object item);
        void Remove(object item);
        void RemoveAt(int index);

        // Properties
        bool CanAddNew { get; }
        bool CanCancelEdit { get; }
        bool CanRemove { get; }
        object CurrentAddItem { get; }
        object CurrentEditItem { get; }
        bool IsAddingNew { get; }
        bool IsEditingItem { get; }
        NewItemPlaceholderPosition NewItemPlaceholderPosition { get; set; }
    }


}
