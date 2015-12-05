using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using ErikEJ.SqlCeScripting;
using ErikEJ.SqlCeToolbox.Dialogs;
using ErikEJ.SqlCeToolbox.Helpers;
using ErikEJ.SqlCeToolbox.ToolWindows;

namespace ErikEJ.SqlCeToolbox.Commands
{
    public class ColumnMenuCommandsHandler : CommandHandlerBase
    {
        public ColumnMenuCommandsHandler(ExplorerToolWindow parent)
        {
            ParentWindow = parent;
        }

        public void ModifyColumn(object sender, ExecutedRoutedEventArgs e)
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
                    List<Column> columns = repository.GetAllColumns();
                    var col = columns.SingleOrDefault(c => c.TableName == menuInfo.Description && c.ColumnName == menuInfo.Name);
                    if (col == null)
                    {
                        EnvDTEHelper.ShowError("Could not find the column in the table, has it been dropped?");
                        return;
                    }
                    TableBuilderDialog tbd = new TableBuilderDialog(menuInfo.Description, menuInfo.DatabaseInfo.DatabaseType);
                    tbd.TableColumns = new List<Column> { col };
                    tbd.Mode = 2;
                    if (tbd.ShowModal() == true && tbd.TableColumns.Count == 1)
                    {
                        generator.GenerateColumnAlterScript(tbd.TableColumns[0]);
                        var script = generator.GeneratedScript.ToString();
                        OpenSqlEditorToolWindow(menuInfo, script);
                        Helpers.DataConnectionHelper.LogUsage("TableBuildColumnEdit");
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType);
            }
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
                    List<Column> columns = repository.GetAllColumns();
                    var col = columns.SingleOrDefault(c => c.TableName == menuInfo.Description && c.ColumnName == menuInfo.Name);
                    if (col == null)
                    {
                        EnvDTEHelper.ShowError("Could not find the column in the table, has it been dropped?");
                        return;
                    }
                    else
                    {
                        generator.GenerateColumnAddScript(col);
                    }
                    OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);
                    Helpers.DataConnectionHelper.LogUsage("ColumnScriptAsCreate");
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
                    List<Column> columns = repository.GetAllColumns();
                    var col = columns.SingleOrDefault(c => c.TableName == menuInfo.Description && c.ColumnName == menuInfo.Name);
                    if (col == null)
                    {
                        EnvDTEHelper.ShowError("Could not find the column in the table, has it been dropped?");
                        return;
                    }
                    else
                    {
                        generator.GenerateColumnDropScript(col);
                    }
                    OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);
                    Helpers.DataConnectionHelper.LogUsage("ColumnScriptAsDrop");
                }
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void ScriptAsAlter(object sender, ExecutedRoutedEventArgs e)
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
                    List<Column> columns = repository.GetAllColumns();
                    var col = columns.SingleOrDefault(c => c.TableName == menuInfo.Description && c.ColumnName == menuInfo.Name);
                    if (col == null)
                    {
                        EnvDTEHelper.ShowError("Could not find the column in the table, has it been dropped?");
                        return;
                    }
                    else
                    {
                        generator.GenerateColumnAlterScript(col);
                    }
                    OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);
                    Helpers.DataConnectionHelper.LogUsage("ColumnScriptAsDrop");
                }
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void AddDescription(object sender, ExecutedRoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null) return;
            var menuInfo = menuItem.CommandParameter as MenuCommandParameters;
            if (menuInfo == null) return;
            string name = menuInfo.Description;
            new BaseCommandHandler(ParentWindow).UpdateDescriptions(menuInfo, name);
        }
    }
}