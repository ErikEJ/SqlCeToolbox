using System;
using System.Windows.Controls;
using System.Windows.Input;
using ErikEJ.SqlCeScripting;
using ErikEJ.SqlCeToolbox.ToolWindows;

namespace ErikEJ.SqlCeToolbox.Commands
{
    public class IndexMenuCommandsHandler : CommandHandlerBase
    {
        public IndexMenuCommandsHandler(ExplorerToolWindow parent)
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
                using (IRepository repository = Helpers.RepositoryHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    var generator = Helpers.DataConnectionHelper.CreateGenerator(repository, menuInfo.DatabaseInfo.DatabaseType);
                    generator.GenerateIndexScript(menuInfo.Name, menuInfo.Description);
                    OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);
                    Helpers.DataConnectionHelper.LogUsage("IndexScriptAsCreate");
                }
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
                using (IRepository repository = Helpers.RepositoryHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    var generator = Helpers.DataConnectionHelper.CreateGenerator(repository, menuInfo.DatabaseInfo.DatabaseType);
                    generator.GenerateIndexDrop(menuInfo.Name, menuInfo.Description);
                    OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);
                    Helpers.DataConnectionHelper.LogUsage("IndexScriptAsDrop");
                }
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void ScriptAsStatistics(object sender, ExecutedRoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null) return;
            var menuInfo = menuItem.CommandParameter as MenuCommandParameters;
            if (menuInfo == null) return;
            try
            {
                using (IRepository repository = Helpers.RepositoryHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    var generator = Helpers.DataConnectionHelper.CreateGenerator(repository, menuInfo.DatabaseInfo.DatabaseType);
                    generator.GenerateIndexStatistics(menuInfo.Name, menuInfo.Description);
                    OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);
                    Helpers.DataConnectionHelper.LogUsage("ColumnScriptAsStatistics");
                }
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
        }
    }
}