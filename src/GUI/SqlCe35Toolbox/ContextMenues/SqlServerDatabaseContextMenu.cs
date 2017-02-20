using System.Windows.Controls;
using System.Windows.Input;
using ErikEJ.SqlCeToolbox.Commands;
using ErikEJ.SqlCeToolbox.Helpers;
using ErikEJ.SqlCeToolbox.ToolWindows;

namespace ErikEJ.SqlCeToolbox.ContextMenues
{
    public class SqlServerDatabaseContextMenu : ContextMenu
    {
        public SqlServerDatabaseContextMenu(DatabaseMenuCommandParameters databaseMenuCommandParameters, ExplorerToolWindow parent)
        {
            var dcmd = new DatabasesMenuCommandsHandler(parent);
            var dbcmd = new DatabaseMenuCommandsHandler(parent);
            var isSqlCe40Installed = DataConnectionHelper.IsV40Installed();
            
            if (SqlCeToolboxPackage.IsVsExtension) Items.Add(BuildScriptDatabaseGraphMenuItem(databaseMenuCommandParameters, dcmd));
            if (SqlCeToolboxPackage.IsVsExtension) Items.Add(new Separator());

            var scriptDatabaseRootMenuItem = new MenuItem
            {
                Header = "Script SQL Server Database",
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
            };

            var toolTip = new ToolTip
            {
                Content = "Generate a SQL Server Compact compatible database script from SQL Server 2005+"
            };

            var scriptDatabaseCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand, dcmd.ScriptServerDatabase);

            scriptDatabaseRootMenuItem.Items.Add(BuildScriptDatabaseSchemaMenuItem(databaseMenuCommandParameters, toolTip, scriptDatabaseCommandBinding));

            scriptDatabaseRootMenuItem.Items.Add(BuildScriptDatabaseDataMenuItem(databaseMenuCommandParameters, toolTip, scriptDatabaseCommandBinding));

            scriptDatabaseRootMenuItem.Items.Add(BuildScriptDatabaseSchemaDataMenuItem(databaseMenuCommandParameters, toolTip, scriptDatabaseCommandBinding));

            scriptDatabaseRootMenuItem.Items.Add(BuildScriptDatabaseSchemaDataSqLiteMenuItem(databaseMenuCommandParameters, toolTip, scriptDatabaseCommandBinding));

            scriptDatabaseRootMenuItem.Items.Add(BuildScriptDatabaseSchemaSqLiteMenuItem(databaseMenuCommandParameters, toolTip, scriptDatabaseCommandBinding));

            scriptDatabaseRootMenuItem.Items.Add(BuildScriptDatabaseSchemaDataBlobMenuItem(databaseMenuCommandParameters, toolTip, scriptDatabaseCommandBinding));

            Items.Add(scriptDatabaseRootMenuItem);
            Items.Add(new Separator());

#if SSMS
#else
            if (SqlCeToolboxPackage.IsVsExtension) Items.Add(BuildEfCoreModelMenuItem(databaseMenuCommandParameters, dbcmd));
            if (SqlCeToolboxPackage.IsVsExtension) Items.Add(new Separator());
#endif
            Items.Add(BuildExportServerMenuItem(databaseMenuCommandParameters, dcmd, isSqlCe40Installed));

            Items.Add(BuildExportServerToLiteMenuItem(databaseMenuCommandParameters, dcmd));
        }

        private MenuItem BuildScriptDatabaseGraphMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            DatabasesMenuCommandsHandler dcmd)
        {
            var scriptGraphCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                dcmd.GenerateServerDgmlFiles);
            var scriptDatabaseGraphMenuItem = new MenuItem
            {
                Header = "Create SQL Server Database Graph (DGML)...",
                Icon = ImageHelper.GetImageFromResource("../resources/Diagram_16XLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
            };
            scriptDatabaseGraphMenuItem.CommandBindings.Add(scriptGraphCommandBinding);
            return scriptDatabaseGraphMenuItem;
        }

        private MenuItem BuildScriptDatabaseSchemaMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            ToolTip toolTip, CommandBinding scriptDatabaseCommandBinding)
        {
            var scriptDatabaseSchemaMenuItem = new MenuItem
            {
                Header = "Script SQL Server Database Schema...",
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
                ToolTip = toolTip,
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = SqlCeScripting.Scope.Schema
            };
            scriptDatabaseSchemaMenuItem.CommandBindings.Add(scriptDatabaseCommandBinding);
            return scriptDatabaseSchemaMenuItem;
        }

        private MenuItem BuildScriptDatabaseDataMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            ToolTip toolTip, CommandBinding scriptDatabaseCommandBinding)
        {
            var scriptDatabaseDataMenuItem = new MenuItem
            {
                Header = "Script SQL Server Database Data...",
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
                ToolTip = toolTip,
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = SqlCeScripting.Scope.DataOnly
            };
            scriptDatabaseDataMenuItem.CommandBindings.Add(scriptDatabaseCommandBinding);
            return scriptDatabaseDataMenuItem;
        }

        private MenuItem BuildScriptDatabaseSchemaDataMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            ToolTip toolTip, CommandBinding scriptDatabaseCommandBinding)
        {
            var scriptDatabaseSchemaDataMenuItem = new MenuItem
            {
                Header = "Script SQL Server Database Schema and Data...",
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
                ToolTip = toolTip,
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = SqlCeScripting.Scope.SchemaData
            };
            scriptDatabaseSchemaDataMenuItem.CommandBindings.Add(scriptDatabaseCommandBinding);
            return scriptDatabaseSchemaDataMenuItem;
        }

        private MenuItem BuildScriptDatabaseSchemaDataSqLiteMenuItem(
            DatabaseMenuCommandParameters databaseMenuCommandParameters, ToolTip toolTip,
            CommandBinding scriptDatabaseCommandBinding)
        {
            var scriptDatabaseSchemaDataSqLiteMenuItem = new MenuItem
            {
                Header = "Script SQL Server Database Schema and Data for SQLite...",
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
                ToolTip = toolTip,
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = SqlCeScripting.Scope.SchemaDataSQLite
            };
            scriptDatabaseSchemaDataSqLiteMenuItem.CommandBindings.Add(scriptDatabaseCommandBinding);
            return scriptDatabaseSchemaDataSqLiteMenuItem;
        }

        private static MenuItem BuildScriptDatabaseSchemaSqLiteMenuItem(
            DatabaseMenuCommandParameters databaseMenuCommandParameters, ToolTip toolTip,
            CommandBinding scriptDatabaseCommandBinding)
        {
            var scriptDatabaseSchemaSqLiteMenuItem = new MenuItem
            {
                Header = "Script SQL Server Database Schema for SQLite...",
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
                ToolTip = toolTip,
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = SqlCeScripting.Scope.SchemaSQLite
            };
            scriptDatabaseSchemaSqLiteMenuItem.CommandBindings.Add(scriptDatabaseCommandBinding);
            return scriptDatabaseSchemaSqLiteMenuItem;
        }

        private MenuItem BuildScriptDatabaseSchemaDataBlobMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            ToolTip toolTip, CommandBinding scriptDatabaseCommandBinding)
        {
            var scriptDatabaseSchemaDataBlobMenuItem = new MenuItem
            {
                Header = "Script SQL Server Database Schema and Data with BLOBs...",
                ToolTip = toolTip,
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = SqlCeScripting.Scope.SchemaDataBlobs
            };
            scriptDatabaseSchemaDataBlobMenuItem.CommandBindings.Add(scriptDatabaseCommandBinding);
            return scriptDatabaseSchemaDataBlobMenuItem;
        }

#if SSMS
#else
        private MenuItem BuildEfCoreModelMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            DatabaseMenuCommandsHandler dbcmd)
        {
            var efCoreModelCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                dbcmd.GenerateEfCoreModelInProject);
            var efCoreModelMenuItem = new MenuItem
            {
                Header = "Add Entity Framework Core Model to current Project (alpha)...",
                Icon = ImageHelper.GetImageFromResource("../resources/Schema_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters
            };
            efCoreModelMenuItem.CommandBindings.Add(efCoreModelCommandBinding);
            return efCoreModelMenuItem;
        }
#endif
        private MenuItem BuildExportServerMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            DatabasesMenuCommandsHandler dcmd, bool isSqlCe40Installed)
        {
            var exportServerCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                dcmd.ExportServerDatabaseTo40);
            var exportServerMenuItem = new MenuItem
            {
                Header = "Export SQL Server to SQL Server Compact 4.0...",
                Icon = ImageHelper.GetImageFromResource("../resources/ExportReportData_10565.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters
            };
            exportServerMenuItem.CommandBindings.Add(exportServerCommandBinding);
            exportServerMenuItem.IsEnabled = (isSqlCe40Installed);
            return exportServerMenuItem;
        }

        private MenuItem BuildExportServerToLiteMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            DatabasesMenuCommandsHandler dcmd)
        {
            var exportServerToLiteCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                dcmd.ExportServerDatabaseToSqlite);
            var exportServerToLiteMenuItem = new MenuItem
            {
                Header = "Export SQL Server to SQLite...",
                Icon = ImageHelper.GetImageFromResource("../resources/ExportReportData_10565.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters
            };
            exportServerToLiteMenuItem.CommandBindings.Add(exportServerToLiteCommandBinding);
            return exportServerToLiteMenuItem;
        }
    }
}