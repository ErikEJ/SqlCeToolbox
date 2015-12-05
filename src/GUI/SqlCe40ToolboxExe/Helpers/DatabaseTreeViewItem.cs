using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace ErikEJ.SqlCeToolbox.Helpers
{
    public class DatabaseTreeViewItem : TreeViewItem
    {
        public string MetaData { get; set; }
        public bool IsRefreshable { get; set; }
        public bool IsRefreshing { get; private set; }

        public override string ToString()
        {
            return MetaData;
        }

        public bool Refresh()
        {
            if (IsRefreshable)
            {
                RefreshThisNode();
                return true;
            }

            return RefreshParentNode();
        }

        private bool RefreshParentNode()
        {
            var parent = Parent as DatabaseTreeViewItem;

            if (parent != null)
                return parent.Refresh();

            return false;
        }

        private void RefreshThisNode()
        {
            try
            {
                IsRefreshing = true;
                IsExpanded = false;
                Items.Clear();
                AddLoadingChildNode(this);
                IsExpanded = true;
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        public void AddLoadingChildNode(TreeViewItem treeViewItem)
        {
            Items.Add("Loading...");
        }
    }
}