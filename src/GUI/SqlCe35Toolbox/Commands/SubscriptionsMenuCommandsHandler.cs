using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;
using ErikEJ.SqlCeScripting;
using ErikEJ.SqlCeToolbox.Helpers;
using ErikEJ.SqlCeToolbox.ToolWindows;

namespace ErikEJ.SqlCeToolbox.Commands
{
    public class SubscriptionsMenuCommandsHandler
    {
        private readonly ExplorerToolWindow _parentWindow;

        public SubscriptionsMenuCommandsHandler(ExplorerToolWindow parent)
        {
            _parentWindow = parent;
        }

        public void NewSubscription(object sender, ExecutedRoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null) return;

            var menuInfo = menuItem.CommandParameter as MenuCommandParameters;
            if (menuInfo == null) return;
            OpenSubscriptionToolWindow(menuInfo);
        }

        public void DropSubscription(object sender, ExecutedRoutedEventArgs e)
        {
            if (EnvDteHelper.ShowMessageBox("Do you really want to remove replication metadata from the SQL Server Compact database?", Microsoft.VisualStudio.Shell.Interop.OLEMSGBUTTON.OLEMSGBUTTON_YESNO, Microsoft.VisualStudio.Shell.Interop.OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_SECOND, Microsoft.VisualStudio.Shell.Interop.OLEMSGICON.OLEMSGICON_WARNING) == System.Windows.Forms.DialogResult.No)
            {
                return;
            }

            var menuItem = sender as MenuItem;
            if (menuItem == null) return;
            var menuInfo = menuItem.CommandParameter as MenuCommandParameters;
            if (menuInfo == null) return;
            try
            {
                SqlCeReplicationHelper.DropPublication(menuInfo.DatabaseInfo.ConnectionString, menuInfo.Name);
                if (_parentWindow != null && _parentWindow.Content != null)
                {
                    ExplorerControl control = _parentWindow.Content as ExplorerControl;
                    if (control != null) control.BuildDatabaseTree();
                }
                DataConnectionHelper.LogUsage("SubscriptionDrop");
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        private void OpenSubscriptionToolWindow(MenuCommandParameters menuInfo)
        {
            var pkg = _parentWindow.Package as SqlCeToolboxPackage;
            Debug.Assert(pkg != null, "Package property of the Explorere Tool Window should never be null, have you tried to create it manually and not through FindToolWindow()?");

            try
            {
                if (string.IsNullOrWhiteSpace(menuInfo.Name))
                {
                    menuInfo.Name = "Add";
                }
                var subsWindow = pkg.CreateWindow<SubscriptionWindow>(Math.Abs(menuInfo.DatabaseInfo.ConnectionString.GetHashCode() - menuInfo.Name.GetHashCode()));
                var control = subsWindow.Content as SubscriptionControl;
                if (control != null)
                {
                    control.DatabaseInfo = menuInfo.DatabaseInfo;
                    if (menuInfo.MenuItemType == MenuType.Manage)
                    {
                        control.Publication = menuInfo.Name;
                        control.IsNew = false;
                        subsWindow.Caption = menuInfo.Name;
                    }
                    else
                    {
                        control.IsNew = true;
                        subsWindow.Caption = "New Subscription";
                    }
                }
                DataConnectionHelper.LogUsage("SubscriptionManage");
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType);
            }
        }
    }
}
