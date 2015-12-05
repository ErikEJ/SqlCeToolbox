using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ErikEJ.SqlCeToolbox.ToolWindows;
using ErikEJ.SqlCeScripting;
using FabTab;

namespace ErikEJ.SqlCeToolbox.Commands
{
    public class SubscriptionsMenuCommandsHandler
    {

        private ExplorerControl _parent;

        public SubscriptionsMenuCommandsHandler(ExplorerControl parent)
        {
            _parent = parent;
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
            if (MessageBox.Show("Do you really want to remove replication metadata from the SQL Server Compact database?", "SQL Server Compact Toolbox", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No, MessageBoxOptions.None) == MessageBoxResult.No)
            {
                return;
            }

            var menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                var menuInfo = menuItem.CommandParameter as MenuCommandParameters;

                if (menuInfo != null)
                {
                    try
                    {
                        SqlCeReplicationHelper.DropPublication(menuInfo.Connectionstring, menuInfo.Name);
                        if (_parent != null )
                        {
                            _parent.BuildDatabaseTree();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(Helpers.DataConnectionHelper.ShowErrors(ex));
                    }
                }
            }
        }

        private void OpenSubscriptionToolWindow(MenuCommandParameters menuInfo)
        {
            SubscriptionControl control = new SubscriptionControl();

            if (string.IsNullOrWhiteSpace(menuInfo.Name))
            {
                menuInfo.Name = "Add";
            }
            if (menuInfo.MenuItemType == MenuCommandParameters.MenuType.Manage)
            {
                control.Publication = menuInfo.Name;
                control.IsNew = false;
                menuInfo.Caption = menuInfo.Name;
            }
            else
            {
                control.IsNew = true;
                menuInfo.Caption = "New Subscription";
            }
            control.Database = menuInfo.Connectionstring;
            FabTabItem tab = new FabTabItem();
            tab.Content = control;
            tab.Header = menuInfo.Caption;
            _parent.FabTab.Items.Add(tab);
            _parent.FabTab.SelectedIndex = _parent.FabTab.Items.Count - 1; 
            return;
        }
    }

}
