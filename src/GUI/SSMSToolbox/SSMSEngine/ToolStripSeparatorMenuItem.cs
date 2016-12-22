using System.Windows.Forms;
using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;

namespace ErikEJ.SqlCeToolbox.SSMSEngine
{
    // menu item class to create a separator
    internal class ToolStripSeparatorMenuItem : ToolsMenuItemBase, IWinformsMenuHandler
    {
        protected override void Invoke()
        {
        }

        public override object Clone() => new ToolStripSeparatorMenuItem();

        public ToolStripItem[] GetMenuItems()
        {
            return new ToolStripItem[] {new ToolStripSeparator()};
        }
    }
}
