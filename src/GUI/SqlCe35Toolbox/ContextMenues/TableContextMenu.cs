using System.Windows.Controls;
using System.Windows.Input;
using ErikEJ.SqlCeToolbox.Commands;
using ErikEJ.SqlCeToolbox.Helpers;
using ErikEJ.SqlCeToolbox.ToolWindows;

namespace ErikEJ.SqlCeToolbox.ContextMenues
{
    public class TableContextMenu : ContextMenu
    {
        public TableContextMenu(MenuCommandParameters menuCommandParameters, ExplorerToolWindow parent)
        {
            var isSqlCe = menuCommandParameters.DatabaseInfo.DatabaseType == DatabaseType.SQLCE35
                || menuCommandParameters.DatabaseInfo.DatabaseType == DatabaseType.SQLCE40;

            var tcmd = new TableMenuCommandsHandler(parent);
            //Edit menu
            CreateEditTableDataMenuItem(tcmd, menuCommandParameters);
            ReportDataMenuItem(tcmd, menuCommandParameters);
            AddSqlEditorItem(tcmd, menuCommandParameters);
            Items.Add(new Separator());
            if (isSqlCe)
            {
                AddColumnMenuItem(tcmd, menuCommandParameters);
                AddIndexMenuItem(tcmd, menuCommandParameters);
                AddFkMenuItem(tcmd, menuCommandParameters);
                Items.Add(new Separator());
            }
            CreateScriptAsCreateMenuItem(tcmd, menuCommandParameters);
            CreateScriptAsDropMenuItem(tcmd, menuCommandParameters);
            CreateScriptAsDropAndCreateMenuItem(tcmd, menuCommandParameters);
            Items.Add(new Separator());
            CreateScriptAsSelectMenuItem(tcmd, menuCommandParameters);
            CreateScriptAsInsertMenuItem(tcmd, menuCommandParameters);
            CreateScriptAsUpdateMenuItem(tcmd, menuCommandParameters);
            CreateScriptAsDeleteMenuItem(tcmd, menuCommandParameters);
            CreateScriptAsDataMenuItem(tcmd, menuCommandParameters);
#if SSMS
            if (isSqlCe)
            {
                CreateScriptAsSQLCLRSampleMenuItem(tcmd, menuCommandParameters);
            }
#endif
            Items.Add(new Separator());
            ImportDataMenuItem(tcmd, menuCommandParameters);
            Items.Add(new Separator());
            if (isSqlCe)
            {
                CompareDataMenuItem(tcmd, menuCommandParameters);
                Items.Add(new Separator());
            }
            RenameMenuItem(tcmd, menuCommandParameters);
            if (isSqlCe)
            {
                DescriptionMenuItem(tcmd, menuCommandParameters);
            }
        }

        private void CreateEditTableDataMenuItem(TableMenuCommandsHandler tcmd, MenuCommandParameters menuCommandParameters)
        {
            var editTableCommandBinding = new CommandBinding(TableMenuCommands.TableCommand,
                                                    tcmd.EditTableData);
            var editTableMenuItem = new MenuItem
            {
                Header = string.Format("Edit Top {0} Rows", Properties.Settings.Default.MaxRowsToEdit),
                Icon = ImageHelper.GetImageFromResource("../resources/Editdatasetwithdesigner_8449.png"),
                Command = TableMenuCommands.TableCommand,
                CommandParameter = menuCommandParameters
            };
            editTableMenuItem.CommandBindings.Add(editTableCommandBinding);
            Items.Add(editTableMenuItem);
        }

        private void ReportDataMenuItem(TableMenuCommandsHandler tcmd, MenuCommandParameters menuCommandParameters)
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

        private void AddColumnMenuItem(TableMenuCommandsHandler tcmd, MenuCommandParameters menuCommandParameters)
        {
            var modifyTableCommandBinding = new CommandBinding(TableMenuCommands.TableCommand,
                                                    tcmd.AddColumn);
            var modifyTableMenuItem = new MenuItem
            {
                Header = "Add column... (beta)",
                Icon = ImageHelper.GetImageFromResource("../resources/table_16xLG.png"),
                Command = TableMenuCommands.TableCommand,
                CommandParameter = menuCommandParameters
            };
            modifyTableMenuItem.CommandBindings.Add(modifyTableCommandBinding);
            Items.Add(modifyTableMenuItem);
        }

        private void AddIndexMenuItem(TableMenuCommandsHandler tcmd, MenuCommandParameters menuCommandParameters)
        {
            var addIndexCommandBinding = new CommandBinding(TableMenuCommands.TableCommand,
                                                    tcmd.AddIndex);
            var addIndexMenuItem = new MenuItem
            {
                Header = "Add index... (beta)",
                Icon = ImageHelper.GetImageFromResource("../resources/table_16xLG.png"),
                Command = TableMenuCommands.TableCommand,
                CommandParameter = menuCommandParameters
            };
            addIndexMenuItem.CommandBindings.Add(addIndexCommandBinding);
            Items.Add(addIndexMenuItem);
        }

        private void AddFkMenuItem(TableMenuCommandsHandler tcmd, MenuCommandParameters menuCommandParameters)
        {
            var addFkCommandBinding = new CommandBinding(TableMenuCommands.TableCommand,
                                                    tcmd.AddForeignKey);
            var addFkMenuItem = new MenuItem
            {
                Header = "Add foreign key... (beta)",
                Icon = ImageHelper.GetImageFromResource("../resources/table_16xLG.png"),
                Command = TableMenuCommands.TableCommand,
                CommandParameter = menuCommandParameters
            };
            addFkMenuItem.CommandBindings.Add(addFkCommandBinding);
            Items.Add(addFkMenuItem);
        }

        private void AddSqlEditorItem(TableMenuCommandsHandler tcmd, MenuCommandParameters menuCommandParameters)
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

        private void CreateScriptAsCreateMenuItem(TableMenuCommandsHandler tcmd, MenuCommandParameters menuCommandParameters)
        {
            var scriptCommandBinding = new CommandBinding(TableMenuCommands.TableCommand, tcmd.ScriptAsCreate);
            var scriptMenuItem = new MenuItem
            {
                Header = "Script as CREATE",
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
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
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
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
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
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
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
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
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
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
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
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
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
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
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
                Command = TableMenuCommands.TableCommand,
                CommandParameter = menuCommandParameters
            };
            scriptMenuItem.CommandBindings.Add(scriptCommandBinding);
            Items.Add(scriptMenuItem);
        }

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedMember.Local
        private void CreateScriptAsSQLCLRSampleMenuItem(TableMenuCommandsHandler tcmd, MenuCommandParameters menuCommandParameters)
        {
            var scriptCommandBinding = new CommandBinding(TableMenuCommands.TableCommand, tcmd.ScriptAsSQLCLRSample);
            var scriptMenuItem = new MenuItem
            {
                Header = "Script as SQLCLR sample (beta)",
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
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
                Header = "Import Data from CSV...",
                Icon = ImageHelper.GetImageFromResource("../resources/TypeDefinition_521.png"),
                Command = TableMenuCommands.TableCommand,
                CommandParameter = menuCommandParameters
            };
            scriptMenuItem.CommandBindings.Add(scriptCommandBinding);
            Items.Add(scriptMenuItem);
        }

        private void CompareDataMenuItem(TableMenuCommandsHandler tcmd, MenuCommandParameters menuCommandParameters)
        {
            var scriptCommandBinding = new CommandBinding(TableMenuCommands.TableCommand, tcmd.GenerateDataDiffScript);
            var scriptMenuItem = new MenuItem
            {
                Header = "Compare Data (beta)...",
                Icon = ImageHelper.GetImageFromResource("../resources/DataCompare_9880.png"),
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
                Header = "Rename...",
                Icon = ImageHelper.GetImageFromResource("../resources/Rename_6779.png"),
                Command = TableMenuCommands.TableCommand,
                CommandParameter = menuCommandParameters
            };
            scriptMenuItem.CommandBindings.Add(scriptCommandBinding);
            Items.Add(scriptMenuItem);
        }

        private void DescriptionMenuItem(TableMenuCommandsHandler tcmd, MenuCommandParameters menuCommandParameters)
        {
            Items.Add(new Separator());

            var addDescriptionCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                                                    tcmd.AddDescription);
            var addDescriptionMenuItem = new MenuItem
            {
                Header = "Edit descriptions...",
                Icon = ImageHelper.GetImageFromResource("../resources/properties_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = menuCommandParameters
            };
            addDescriptionMenuItem.CommandBindings.Add(addDescriptionCommandBinding);
            Items.Add(addDescriptionMenuItem);
        }

    }
}