using System;
using System.Windows.Controls;
using System.Windows.Input;
using ErikEJ.SqlCeToolbox.Helpers;
using ErikEJ.SqlCeToolbox.ToolWindows;

namespace ErikEJ.SqlCeToolbox.Commands
{
    public class ScopesMenuCommandsHandler
    {
        private readonly ExplorerToolWindow _parentWindow;

        public ScopesMenuCommandsHandler(ExplorerToolWindow parent)
        {
            _parentWindow = parent;
        }

        public void DropScope(object sender, ExecutedRoutedEventArgs e)
        {
            if (EnvDTEHelper.ShowMessageBox("Do you really want to deprovision this scope?", Microsoft.VisualStudio.Shell.Interop.OLEMSGBUTTON.OLEMSGBUTTON_YESNO, Microsoft.VisualStudio.Shell.Interop.OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_SECOND, Microsoft.VisualStudio.Shell.Interop.OLEMSGICON.OLEMSGICON_WARNING) == System.Windows.Forms.DialogResult.No)
            {
                return;
            }

            var menuItem = sender as MenuItem;
            if (menuItem == null) return;
            var menuInfo = menuItem.CommandParameter as MenuCommandParameters;
            if (menuInfo == null) return;
            try
            {
                SyncFxHelper.DeprovisionSqlCeScope(menuInfo.DatabaseInfo.ConnectionString, menuInfo.Name);
                EnvDTEHelper.ShowMessage("Scope deprovisioned");
                if (_parentWindow != null && _parentWindow.Content != null)
                {
                    ExplorerControl control = _parentWindow.Content as ExplorerControl;
                    control.BuildDatabaseTree();
                }
                Helpers.DataConnectionHelper.LogUsage("SyncScopeDrop");
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType);
            }
        }        
    }
}
