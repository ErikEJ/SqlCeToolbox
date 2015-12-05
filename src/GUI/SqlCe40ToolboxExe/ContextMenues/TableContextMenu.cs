using System.Windows.Controls;
using System.Windows.Input;
using ErikEJ.SqlCeToolbox.Commands;
using ErikEJ.SqlCeToolbox.Helpers;
using ErikEJ.SqlCeToolbox.ToolWindows;

namespace ErikEJ.SqlCeToolbox.ContextMenues
{

    public class TableContextMenu : ContextMenu
    {
        public TableContextMenu(MenuCommandParameters menuCommandParameters, ExplorerControl parent)
        {
            var tcmd = new TableMenuCommandsHandler(parent);
            CreateEditDataMenuItem(tcmd, menuCommandParameters);
            CreateViewReportMenuItem(tcmd, menuCommandParameters);
            Items.Add(new Separator());
            CreateScriptAsCreateMenuItem(tcmd, menuCommandParameters);
            CreateScriptAsDropMenuItem(tcmd, menuCommandParameters);
            CreateScriptAsDropAndCreateMenuItem(tcmd, menuCommandParameters);
            Items.Add(new Separator());
            CreateScriptAsSelectMenuItem(tcmd, menuCommandParameters);
            CreateScriptAsInsertMenuItem(tcmd, menuCommandParameters);
            CreateScriptAsUpdateMenuItem(tcmd, menuCommandParameters);
            CreateScriptAsDeleteMenuItem(tcmd, menuCommandParameters);
            CreateScriptAsDataMenuItem(tcmd, menuCommandParameters);
            Items.Add(new Separator());
            ImportDataMenuItem(tcmd, menuCommandParameters);
            Items.Add(new Separator());
            DescriptionMenuItem(tcmd, menuCommandParameters);
            Items.Add(new Separator());
            RenameMenuItem(tcmd, menuCommandParameters);
        }

        private void CreateScriptAsCreateMenuItem(TableMenuCommandsHandler tcmd, MenuCommandParameters menuCommandParameters)
        {
            var scriptCommandBinding = new CommandBinding(TableMenuCommands.TableCommand, tcmd.ScriptAsCreate);
            var scriptMenuItem = new MenuItem
            {
                Header = "Script as CREATE",
                Icon = ImageHelper.GetImageFromResource("../resources/sp.png"),
                Command = TableMenuCommands.TableCommand,
                CommandParameter = menuCommandParameters
            };
            scriptMenuItem.CommandBindings.Add(scriptCommandBinding);
            Items.Add(scriptMenuItem);
        }

        private void CreateScriptAsDropMenuItem(TableMenuCommandsHandler tcmd, MenuCommandParameters menuCommandParameters)
        {
            var scriptCommandBinding = new CommandBinding(TableMenuCommands.TableCommand, tcmd.ScriptAsDrop);
            var scriptMenuItem = new MenuItem
            {
                Header = "Script as DROP",
                Icon = ImageHelper.GetImageFromResource("../resources/sp.png"),
                Command = TableMenuCommands.TableCommand,
                CommandParameter = menuCommandParameters
            };
            scriptMenuItem.CommandBindings.Add(scriptCommandBinding);
            Items.Add(scriptMenuItem);
        }

        private void CreateScriptAsDropAndCreateMenuItem(TableMenuCommandsHandler tcmd, MenuCommandParameters menuCommandParameters)
        {
            var scriptCommandBinding = new CommandBinding(TableMenuCommands.TableCommand, tcmd.ScriptAsDropAndCreate);
            var scriptMenuItem = new MenuItem
            {
                Header = "Script as DROP and CREATE",
                Icon = ImageHelper.GetImageFromResource("../resources/sp.png"),
                Command = TableMenuCommands.TableCommand,
                CommandParameter = menuCommandParameters
            };
            scriptMenuItem.CommandBindings.Add(scriptCommandBinding);
            Items.Add(scriptMenuItem);
        }

        private void CreateScriptAsSelectMenuItem(TableMenuCommandsHandler tcmd, MenuCommandParameters menuCommandParameters)
        {
            var scriptCommandBinding = new CommandBinding(TableMenuCommands.TableCommand, tcmd.ScriptAsSelect);
            var scriptMenuItem = new MenuItem
            {
                Header = "Script as SELECT",
                Icon = ImageHelper.GetImageFromResource("../resources/sp.png"),
                Command = TableMenuCommands.TableCommand,
                CommandParameter = menuCommandParameters
            };
            scriptMenuItem.CommandBindings.Add(scriptCommandBinding);
            Items.Add(scriptMenuItem);
        }

        private void CreateScriptAsInsertMenuItem(TableMenuCommandsHandler tcmd, MenuCommandParameters menuCommandParameters)
        {
            var scriptCommandBinding = new CommandBinding(TableMenuCommands.TableCommand, tcmd.ScriptAsInsert);
            var scriptMenuItem = new MenuItem
            {
                Header = "Script as INSERT",
                Icon = ImageHelper.GetImageFromResource("../resources/sp.png"),
                Command = TableMenuCommands.TableCommand,
                CommandParameter = menuCommandParameters
            };
            scriptMenuItem.CommandBindings.Add(scriptCommandBinding);
            Items.Add(scriptMenuItem);
        }
        private void CreateScriptAsUpdateMenuItem(TableMenuCommandsHandler tcmd, MenuCommandParameters menuCommandParameters)
        {
            var scriptCommandBinding = new CommandBinding(TableMenuCommands.TableCommand, tcmd.ScriptAsUpdate);
            var scriptMenuItem = new MenuItem
            {
                Header = "Script as UPDATE",
                Icon = ImageHelper.GetImageFromResource("../resources/sp.png"),
                Command = TableMenuCommands.TableCommand,
                CommandParameter = menuCommandParameters
            };
            scriptMenuItem.CommandBindings.Add(scriptCommandBinding);
            Items.Add(scriptMenuItem);
        }
        private void CreateScriptAsDeleteMenuItem(TableMenuCommandsHandler tcmd, MenuCommandParameters menuCommandParameters)
        {
            var scriptCommandBinding = new CommandBinding(TableMenuCommands.TableCommand, tcmd.ScriptAsDelete);
            var scriptMenuItem = new MenuItem
            {
                Header = "Script as DELETE",
                Icon = ImageHelper.GetImageFromResource("../resources/sp.png"),
                Command = TableMenuCommands.TableCommand,
                CommandParameter = menuCommandParameters
            };
            scriptMenuItem.CommandBindings.Add(scriptCommandBinding);
            Items.Add(scriptMenuItem);
        }

        private void CreateScriptAsDataMenuItem(TableMenuCommandsHandler tcmd, MenuCommandParameters menuCommandParameters)
        {
            var scriptCommandBinding = new CommandBinding(TableMenuCommands.TableCommand, tcmd.ScriptAsData);
            var scriptMenuItem = new MenuItem
            {
                Header = "Script as Data (INSERTs)",
                Icon = ImageHelper.GetImageFromResource("../resources/sp.png"),
                Command = TableMenuCommands.TableCommand,
                CommandParameter = menuCommandParameters
            };
            scriptMenuItem.CommandBindings.Add(scriptCommandBinding);
            Items.Add(scriptMenuItem);
        }

        private void ImportDataMenuItem(TableMenuCommandsHandler tcmd, MenuCommandParameters menuCommandParameters)
        {
            var scriptCommandBinding = new CommandBinding(TableMenuCommands.TableCommand, tcmd.ImportData);
            var scriptMenuItem = new MenuItem
            {
                Header = "Import Data from CSV",
                Icon = ImageHelper.GetImageFromResource("../resources/sp.png"),
                Command = TableMenuCommands.TableCommand,
                CommandParameter = menuCommandParameters
            };
            scriptMenuItem.CommandBindings.Add(scriptCommandBinding);
            Items.Add(scriptMenuItem);
        }

        private void RenameMenuItem(TableMenuCommandsHandler tcmd, MenuCommandParameters menuCommandParameters)
        {
            var scriptCommandBinding = new CommandBinding(TableMenuCommands.TableCommand, tcmd.Rename);
            var scriptMenuItem = new MenuItem
            {
                Header = "Rename",
                Icon = ImageHelper.GetImageFromResource("../resources/sp.png"),
                Command = TableMenuCommands.TableCommand,
                CommandParameter = menuCommandParameters
            };
            scriptMenuItem.CommandBindings.Add(scriptCommandBinding);
            Items.Add(scriptMenuItem);
        }

        private void CreateEditDataMenuItem(TableMenuCommandsHandler tcmd, MenuCommandParameters menuCommandParameters)
        {
            var editTableCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                                                    tcmd.SpawnDataEditorWindow);
            var editTableMenuItem = new MenuItem
            {
                Header = string.Format("Edit Top {0} Rows", Properties.Settings.Default.MaxRowsToEdit),
                Icon = ImageHelper.GetImageFromResource("../resources/sqlEditor.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = menuCommandParameters
            };
            editTableMenuItem.CommandBindings.Add(editTableCommandBinding);
            Items.Add(editTableMenuItem);
        }

        private void CreateViewReportMenuItem(TableMenuCommandsHandler tcmd, MenuCommandParameters menuCommandParameters)
        {
            var viewReportCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                                                    tcmd.SpawnReportViewerWindow);
            var reportMenuItem = new MenuItem
            {
                Header = "View Data as Report",
                Icon = ImageHelper.GetImageFromResource("../resources/sqlEditor.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = menuCommandParameters
            };
            reportMenuItem.CommandBindings.Add(viewReportCommandBinding);
            Items.Add(reportMenuItem);
        }

        private void DescriptionMenuItem(TableMenuCommandsHandler tcmd, MenuCommandParameters menuCommandParameters)
        {
            var addDescriptionCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                                                    tcmd.AddDescription);
            var addDescriptionMenuItem = new MenuItem
            {
                Header = "Edit description",
                Icon = ImageHelper.GetImageFromResource("../resources/propes.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = menuCommandParameters
            };
            addDescriptionMenuItem.CommandBindings.Add(addDescriptionCommandBinding);
            Items.Add(addDescriptionMenuItem);
        }

    }
}