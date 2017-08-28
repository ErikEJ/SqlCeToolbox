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
            var itemBuilder = new DatabaseContextMenuItems();
            var dcmd = new SqlServerDatabaseMenuCommandsHandler(parent);
            var dbcmd = new DatabaseMenuCommandsHandler(parent);
            var isSqlCe40Installed = DataConnectionHelper.IsV40Installed();
            if (databaseMenuCommandParameters.DatabaseInfo.DatabaseType != DatabaseType.SQLServer)
                return;
            
            if (SqlCeToolboxPackage.IsVsExtension) Items.Add(BuildScriptServerDatabaseGraphMenuItem(databaseMenuCommandParameters, dcmd));
            if (SqlCeToolboxPackage.IsVsExtension) Items.Add(new Separator());

            var scriptDatabaseRootMenuItem = new MenuItem
            {
                Header = "Script Database",
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
            };

            var toolTip = new ToolTip
            {
                Content = "Generate a SQL Server Compact compatible script from SQL Server"
            };

            var toolTipSqlite = new ToolTip
            {
                Content = "Generate a SQLite compatible script from SQL Server"
            };

            // Database scripting items
            var scriptDatabaseCommandBinding = 
                new CommandBinding(DatabaseMenuCommands.DatabaseCommand, dcmd.ScriptServerDatabase);

            scriptDatabaseRootMenuItem.Items.Add(itemBuilder.BuildScriptDatabaseSchemaMenuItem(databaseMenuCommandParameters, toolTip, scriptDatabaseCommandBinding));

            scriptDatabaseRootMenuItem.Items.Add(itemBuilder.BuildScriptDatabaseDataMenuItem(databaseMenuCommandParameters, toolTip, scriptDatabaseCommandBinding));

            scriptDatabaseRootMenuItem.Items.Add(itemBuilder.BuildScriptDatabaseSchemaDataMenuItem(databaseMenuCommandParameters, toolTip, scriptDatabaseCommandBinding));

            scriptDatabaseRootMenuItem.Items.Add(itemBuilder.BuildScriptDatabaseSchemaDataSqLiteMenuItem(databaseMenuCommandParameters, toolTipSqlite, scriptDatabaseCommandBinding));

            scriptDatabaseRootMenuItem.Items.Add(BuildScriptDatabaseSchemaSqLiteMenuItem(databaseMenuCommandParameters, toolTipSqlite, scriptDatabaseCommandBinding));

            scriptDatabaseRootMenuItem.Items.Add(itemBuilder.BuildScriptDatabaseSchemaDataBlobMenuItem(databaseMenuCommandParameters, toolTip, scriptDatabaseCommandBinding));

            Items.Add(scriptDatabaseRootMenuItem);
            Items.Add(new Separator());
#if SSMS
#else
            if (SqlCeToolboxPackage.VsSupportsEfCore()) Items.Add(itemBuilder.BuildEfCoreModelMenuItem(databaseMenuCommandParameters, dbcmd));
#endif
#if VS2010
#else
            if (SqlCeToolboxPackage.VsSupportsEf6()) Items.Add(BuildScriptEfPocoDacPacMenuItem(databaseMenuCommandParameters, dcmd));
#endif
            if (SqlCeToolboxPackage.IsVsExtension) Items.Add(new Separator());
            Items.Add(BuildExportServerMenuItem(databaseMenuCommandParameters, dcmd, isSqlCe40Installed));

            Items.Add(BuildExportServerToLiteMenuItem(databaseMenuCommandParameters, dcmd));

            if (!databaseMenuCommandParameters.DatabaseInfo.FromServerExplorer)
            {
                Items.Add(new Separator());
                Items.Add(itemBuilder.BuildRemoveConnectionMenuItem(databaseMenuCommandParameters, dbcmd));
            }
        }

        public MenuItem BuildScriptServerDatabaseGraphMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            SqlServerDatabaseMenuCommandsHandler dcmd)
        {
            var scriptGraphCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                dcmd.GenerateServerDgmlFiles);

            var scriptDatabaseGraphMenuItem = new MenuItem
            {
                Header = "Create Database Graph (DGML)...",
                Icon = ImageHelper.GetImageFromResource("../resources/Diagram_16XLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
            };
            scriptDatabaseGraphMenuItem.CommandBindings.Add(scriptGraphCommandBinding);
            return scriptDatabaseGraphMenuItem;
        }

        private MenuItem BuildScriptDatabaseSchemaSqLiteMenuItem(
            DatabaseMenuCommandParameters databaseMenuCommandParameters, ToolTip toolTip,
            CommandBinding scriptDatabaseCommandBinding)
        {
            var scriptDatabaseSchemaSqLiteMenuItem = new MenuItem
            {
                Header = "Script Schema for SQLite...",
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
                ToolTip = toolTip,
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = SqlCeScripting.Scope.SchemaSQLite
            };
            scriptDatabaseSchemaSqLiteMenuItem.CommandBindings.Add(scriptDatabaseCommandBinding);
            return scriptDatabaseSchemaSqLiteMenuItem;
        }

#if VS2010
#else
        private MenuItem BuildScriptEfPocoDacPacMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            SqlServerDatabaseMenuCommandsHandler dcmd)
        {
            var scriptEfDacPacCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                dcmd.GenerateEfPocoFromDacPacInProject);
            var scriptEfPocoDacPacMenuItem = new MenuItem
            {
                Header = "Add EF 6 DbContext (Code Based from Dacpac) to current Project... (alpha)",
                Icon = ImageHelper.GetImageFromResource("../resources/Schema_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
            };
            scriptEfPocoDacPacMenuItem.CommandBindings.Add(scriptEfDacPacCommandBinding);
            return scriptEfPocoDacPacMenuItem;
        }
#endif
        private MenuItem BuildExportServerMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            SqlServerDatabaseMenuCommandsHandler dcmd, bool isSqlCe40Installed)
        {
            var exportServerCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                dcmd.ExportServerDatabaseTo40);
            var exportServerMenuItem = new MenuItem
            {
                Header = "Migrate to SQL Server Compact 4.0...",
                Icon = ImageHelper.GetImageFromResource("../resources/ExportReportData_10565.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters
            };
            exportServerMenuItem.CommandBindings.Add(exportServerCommandBinding);
            exportServerMenuItem.IsEnabled = (isSqlCe40Installed);
            return exportServerMenuItem;
        }

        private MenuItem BuildExportServerToLiteMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            SqlServerDatabaseMenuCommandsHandler dcmd)
        {
            var exportServerToLiteCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                dcmd.ExportServerDatabaseToSqlite);
            var exportServerToLiteMenuItem = new MenuItem
            {
                Header = "Migrate to SQLite...",
                Icon = ImageHelper.GetImageFromResource("../resources/ExportReportData_10565.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters
            };
            exportServerToLiteMenuItem.CommandBindings.Add(exportServerToLiteCommandBinding);
            return exportServerToLiteMenuItem;
        }
    }
}