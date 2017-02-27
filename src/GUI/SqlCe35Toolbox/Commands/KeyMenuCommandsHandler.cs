using System;
using System.Windows.Controls;
using System.Windows.Input;
using ErikEJ.SqlCeToolbox.ToolWindows;
using ErikEJ.SqlCeScripting;

namespace ErikEJ.SqlCeToolbox.Commands
{
    public class KeyMenuCommandsHandler :CommandHandlerBase
    {
        public KeyMenuCommandsHandler(ExplorerToolWindow parent)
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
                using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    var generator = Helpers.DataConnectionHelper.CreateGenerator(repository, menuInfo.DatabaseInfo.DatabaseType);
                    if (menuInfo.MenuItemType == MenuType.Fk)
                    { 
                        generator.GenerateForeignKey(menuInfo.Name, menuInfo.Description);
                    }
                    if (menuInfo.MenuItemType == MenuType.Pk)
                    {
                        generator.GeneratePrimaryKeys(menuInfo.Name);
                    }
                    OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);
                    Helpers.DataConnectionHelper.LogUsage("KeyScriptAsCreate");
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
                using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    var generator = Helpers.DataConnectionHelper.CreateGenerator(repository, menuInfo.DatabaseInfo.DatabaseType);
                    if (menuInfo.MenuItemType == MenuType.Fk)
                    {
                        generator.GenerateForeignKeyDrop(menuInfo.Name, menuInfo.Description);
                    }
                    if (menuInfo.MenuItemType == MenuType.Pk)
                    {
                        var pk = new PrimaryKey { KeyName = menuInfo.Description };
                        generator.GeneratePrimaryKeyDrop(pk, menuInfo.Name);
                    }
                    OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);
                    Helpers.DataConnectionHelper.LogUsage("KeyScriptAsDrop");
                }
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
        }
    }
}