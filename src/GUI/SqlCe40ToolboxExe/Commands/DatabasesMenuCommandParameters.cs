using System.Windows.Controls;
using ErikEJ.SqlCeToolbox.ToolWindows;

namespace ErikEJ.SqlCeToolbox.Commands
{
    public class DatabasesMenuCommandParameters
    {
        public ExplorerControl ExplorerControl { get; set; }
        public TreeViewItem DatabasesTreeViewItem { get; set; }
    }
}