using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;
using System.Windows.Forms;

namespace SqlCeScripter
{
    // menu item class to create a separator
    internal class ToolStripSeparatorMenuItem : ToolsMenuItemBase, IWinformsMenuHandler
    {
        protected override void Invoke()
        {

        }

        public override object Clone()
        {
            return new ToolStripSeparatorMenuItem();
        }

        public System.Windows.Forms.ToolStripItem[] GetMenuItems()
        {
            return new ToolStripItem[] { new ToolStripSeparator() };
        }

    }
}
