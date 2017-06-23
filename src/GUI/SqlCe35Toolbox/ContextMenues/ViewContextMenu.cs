using System.Windows.Controls;
using System.Windows.Input;
using ErikEJ.SqlCeToolbox.Commands;
using ErikEJ.SqlCeToolbox.Helpers;
using ErikEJ.SqlCeToolbox.ToolWindows;

namespace ErikEJ.SqlCeToolbox.ContextMenues
{
    public class ViewContextMenu : ContextMenu
    {
        public ViewContextMenu(MenuCommandParameters menuCommandParameters, ExplorerToolWindow parent)
        {
            var tcmd = new ViewMenuCommandsHandler(parent);
            CreateReportDataMenuItem(tcmd, menuCommandParameters);
            AddSqlEditorItem(tcmd, menuCommandParameters);
            Items.Add(new Separator());
            CreateScriptAsCreateMenuItem(tcmd, menuCommandParameters);
            CreateScriptAsDropMenuItem(tcmd, menuCommandParameters);
            CreateScriptAsDropAndCreateMenuItem(tcmd, menuCommandParameters);
            Items.Add(new Separator());
            CreateScriptAsSelectMenuItem(tcmd, menuCommandParameters);
        }

        private void AddSqlEditorItem(ViewMenuCommandsHandler tcmd, MenuCommandParameters menuCommandParameters)
        {
            var showSqlEditorCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                                                                tcmd.SpawnSqlEditorWindow);
            var showSqlEditorMenuItem = new MenuItem
            {
                Header = "New Query",
                Icon = ImageHelper.GetImageFromResource("../resources/NewQuery.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = menuCommandParameters
            };
            showSqlEditorMenuItem.CommandBindings.Add(showSqlEditorCommandBinding);
            Items.Add(showSqlEditorMenuItem);
        }

        private void CreateReportDataMenuItem(ViewMenuCommandsHandler tcmd, MenuCommandParameters menuCommandParameters)
        {
            var scriptCommandBinding = new CommandBinding(TableMenuCommands.TableCommand, tcmd.ReportTableData);
            var scriptMenuItem = new MenuItem
            {
                Header = "View Data as Report",
                Icon = ImageHelper.GetImageFromResource("../resources/Tables_8928.png"),
                Command = TableMenuCommands.TableCommand,
                CommandParameter = menuCommandParameters
            };
            scriptMenuItem.CommandBindings.Add(scriptCommandBinding);
            Items.Add(scriptMenuItem);
        }

        private void CreateScriptAsCreateMenuItem(ViewMenuCommandsHandler tcmd, MenuCommandParameters menuCommandParameters)
        {
            var scriptCommandBinding = new CommandBinding(IndexMenuCommands.IndexCommand, tcmd.ScriptAsCreate);
            var scriptMenuItem = new MenuItem
            {
                Header = "Script as CREATE",
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
                Command = IndexMenuCommands.IndexCommand,
                CommandParameter = menuCommandParameters
            };
            scriptMenuItem.CommandBindings.Add(scriptCommandBinding);
            Items.Add(scriptMenuItem);
        }

        private void CreateScriptAsDropMenuItem(ViewMenuCommandsHandler tcmd, MenuCommandParameters menuCommandParameters)
        {
            var scriptCommandBinding = new CommandBinding(IndexMenuCommands.IndexCommand, tcmd.ScriptAsDrop);
            var scriptMenuItem = new MenuItem
            {
                Header = "Script as DROP",
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
                Command = IndexMenuCommands.IndexCommand,
                CommandParameter = menuCommandParameters
            };
            scriptMenuItem.CommandBindings.Add(scriptCommandBinding);
            Items.Add(scriptMenuItem);
        }

        private void CreateScriptAsDropAndCreateMenuItem(ViewMenuCommandsHandler tcmd, MenuCommandParameters menuCommandParameters)
        {
            var scriptCommandBinding = new CommandBinding(TableMenuCommands.TableCommand, tcmd.ScriptAsDropAndCreate);
            var scriptMenuItem = new MenuItem
            {
                Header = "Script as DROP and CREATE",
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
                Command = TableMenuCommands.TableCommand,
                CommandParameter = menuCommandParameters
            };
            scriptMenuItem.CommandBindings.Add(scriptCommandBinding);
            Items.Add(scriptMenuItem);
        }

        private void CreateScriptAsSelectMenuItem(ViewMenuCommandsHandler tcmd, MenuCommandParameters menuCommandParameters)
        {
            var scriptCommandBinding = new CommandBinding(TableMenuCommands.TableCommand, tcmd.ScriptAsSelect);
            var scriptMenuItem = new MenuItem
            {
                Header = "Script as SELECT",
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
                Command = TableMenuCommands.TableCommand,
                CommandParameter = menuCommandParameters
            };
            scriptMenuItem.CommandBindings.Add(scriptCommandBinding);
            Items.Add(scriptMenuItem);
        }
    }
}