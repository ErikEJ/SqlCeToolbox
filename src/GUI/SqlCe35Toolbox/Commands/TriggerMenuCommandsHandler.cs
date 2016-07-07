using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using ErikEJ.SqlCeToolbox.ToolWindows;
using ErikEJ.SqlCeScripting;

namespace ErikEJ.SqlCeToolbox.Commands
{
    public class TriggerMenuCommandsHandler : CommandHandlerBase
    {
        private readonly string _separator = ";" + Environment.NewLine + "GO" + Environment.NewLine;

        public TriggerMenuCommandsHandler(ExplorerToolWindow parent)
        {
            ParentWindow = parent;
        }

        public void ScriptAsCreate(object sender, ExecutedRoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null) return;
            var menuInfo = menuItem.CommandParameter as MenuCommandParameters;
            if (menuInfo == null) return;
            try
            {
                OpenSqlEditorToolWindow(menuInfo, string.Format("{0}" + _separator, menuInfo.Description));
                Helpers.DataConnectionHelper.LogUsage("TriggerScriptAsCreate");
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void ScriptAsDrop(object sender, ExecutedRoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null) return;
            var menuInfo = menuItem.CommandParameter as MenuCommandParameters;
            if (menuInfo == null) return;
            try
            {
                OpenSqlEditorToolWindow(menuInfo, string.Format("DROP TRIGGER [{0}]" + _separator, menuInfo.Name));
                Helpers.DataConnectionHelper.LogUsage("TriggerScriptAsDrop");
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
        }
    }
}