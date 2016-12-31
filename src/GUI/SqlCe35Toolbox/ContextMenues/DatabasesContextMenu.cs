using System.Windows.Controls;
using System.Windows.Input;
using ErikEJ.SqlCeToolbox.Commands;
using ErikEJ.SqlCeToolbox.Helpers;
using ErikEJ.SqlCeToolbox.ToolWindows;

namespace ErikEJ.SqlCeToolbox.ContextMenues
{
    public class DatabasesContextMenu : ContextMenu
    {
        public DatabasesContextMenu(DatabaseMenuCommandParameters databaseMenuCommandParameters, ExplorerToolWindow parent)
        {
            var dcmd = new DatabasesMenuCommandsHandler(parent);

            // Add 4.0 database menu
            bool ver40IsInstalled = DataConnectionHelper.IsV40Installed();
            bool ver35IsInstalled = DataConnectionHelper.IsV35Installed();
            
            var addCe4DatabaseCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                            dcmd.AddPrivateCe40Database);
            var addCeDatabaseMenuItem = new MenuItem
            {
                Header = "Add SQL Server Compact 4.0 Connection...",
                Icon = ImageHelper.GetImageFromResource("../resources/AddDatabase_16x.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
            };
            var pkg = parent.Package as SqlCeToolboxPackage;
            if (pkg != null && pkg.VsSupportsDdex40())
            {
                var addCe40DatabaseCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                            dcmd.AddCe40Database);
                addCeDatabaseMenuItem.CommandBindings.Add(addCe40DatabaseCommandBinding);
                addCeDatabaseMenuItem.IsEnabled = ver40IsInstalled;
                Items.Add(addCeDatabaseMenuItem);                
            }
            else
            {
                addCeDatabaseMenuItem.CommandBindings.Add(addCe4DatabaseCommandBinding);
                addCeDatabaseMenuItem.IsEnabled = ver40IsInstalled;
                Items.Add(addCeDatabaseMenuItem);
            }

            // Add 3.5 database menu
            var addCe35DatabaseCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                                        dcmd.AddCe35Database);
            var addCe35DatabaseMenuItem = new MenuItem
            {
                Header = "Add SQL Server Compact 3.5 Connection...",
                Icon = ImageHelper.GetImageFromResource("../resources/AddDatabase_16x.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
            };
            if (pkg != null && pkg.VsSupportsDdex35())
            {
                addCe35DatabaseMenuItem.CommandBindings.Add(addCe35DatabaseCommandBinding);
                addCe35DatabaseMenuItem.IsEnabled = ver35IsInstalled;
                Items.Add(addCe35DatabaseMenuItem);
            }
            else
            {
                var addCe3511DatabaseCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                            dcmd.AddPrivateCe35Database);
                addCe35DatabaseMenuItem.CommandBindings.Add(addCe3511DatabaseCommandBinding);
                addCe35DatabaseMenuItem.IsEnabled = ver35IsInstalled;
                Items.Add(addCe35DatabaseMenuItem);
            }

            // Add SQLite database menu
            var addSqLiteDatabaseCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                                        dcmd.AddSqLiteDatabase);
            var addSqLiteDatabaseMenuItem = new MenuItem
            {
                Header = "Add SQLite Connection...",
                Icon = ImageHelper.GetImageFromResource("../resources/AddDatabase_16x.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
            };
            addSqLiteDatabaseMenuItem.CommandBindings.Add(addSqLiteDatabaseCommandBinding);
            Items.Add(addSqLiteDatabaseMenuItem);
            
            // Add from solution
            var addFromSolutionCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                            dcmd.ScanConnections);

            var addFromSolutionMenuItem = new MenuItem
            {
                Header = "Add Connections from Solution",
                Icon = ImageHelper.GetImageFromResource("../resources/AddConnection_477.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                ToolTip = "Scan current Solution for SQL Compact and SQLite files",
                CommandParameter = databaseMenuCommandParameters,
            };
            addFromSolutionMenuItem.CommandBindings.Add(addFromSolutionCommandBinding);
            addFromSolutionMenuItem.IsEnabled = ver40IsInstalled || ver35IsInstalled;
            if (SqlCeToolboxPackage.IsVsExtension) Items.Add(addFromSolutionMenuItem);

            // Fix connections
            var fixConnectionsCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                            dcmd.FixConnections);

            var fixConnectionsMenuItem = new MenuItem
            {
                Header = "Remove broken connections",
                Icon = ImageHelper.GetImageFromResource("../resources/action_Cancel_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                ToolTip = "Remove invalid connections",
                CommandParameter = databaseMenuCommandParameters,
            };
            fixConnectionsMenuItem.CommandBindings.Add(fixConnectionsCommandBinding);
            Items.Add(fixConnectionsMenuItem);

            Items.Add(new Separator());

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
            if (SqlCeToolboxPackage.IsVsExtension) Items.Add(scriptDatabaseGraphMenuItem);

            var designDatabaseCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                dcmd.DesignDatabase);
            var designDatabaseMenuItem = new MenuItem
            {
                Header = "Database designer (alpha)...",
                Icon = ImageHelper.GetImageFromResource("../resources/Schema_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
            };
            designDatabaseMenuItem.CommandBindings.Add(designDatabaseCommandBinding);
            Items.Add(designDatabaseMenuItem);
            Items.Add(new Separator());

            var scriptDatabaseRootMenuItem = new MenuItem
            {
                Header = "Script SQL Server Database",
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
            };

            var toolTip = new ToolTip();
            toolTip.Content = "Generate a SQL Server Compact compatible database script from SQL Server 2005+";

            var scriptDatabaseCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                                                                dcmd.ScriptServerDatabase);

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
            scriptDatabaseRootMenuItem.Items.Add(scriptDatabaseSchemaMenuItem);

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
            scriptDatabaseRootMenuItem.Items.Add(scriptDatabaseDataMenuItem);

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
            scriptDatabaseRootMenuItem.Items.Add(scriptDatabaseSchemaDataMenuItem);

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
            scriptDatabaseRootMenuItem.Items.Add(scriptDatabaseSchemaDataSqLiteMenuItem);

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
            scriptDatabaseRootMenuItem.Items.Add(scriptDatabaseSchemaSqLiteMenuItem);

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
            scriptDatabaseRootMenuItem.Items.Add(scriptDatabaseSchemaDataBlobMenuItem);

            Items.Add(scriptDatabaseRootMenuItem);

            Items.Add(new Separator());

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
            exportServerMenuItem.IsEnabled = (ver40IsInstalled);
            Items.Add(exportServerMenuItem);

            var exportServerToLiteCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                                                               dcmd.ExportServerDatabaseToSqlite);
            var exportServerToLiteMenuItem = new MenuItem
            {
                Header = "Export SQL Server to SQLite... (beta)",
                Icon = ImageHelper.GetImageFromResource("../resources/ExportReportData_10565.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters
            };
            exportServerToLiteMenuItem.CommandBindings.Add(exportServerToLiteCommandBinding);
            Items.Add(exportServerToLiteMenuItem);

            Items.Add(new Separator());

            var scriptEfDacPacCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                dcmd.GenerateEfPocoFromDacPacInProject);

            var scriptEfPocoDacPacMenuItem = new MenuItem
            {
                Header = "Add Entity Data Model (Code Based from Dacpac) to current Project (alpha)...",
                Icon = ImageHelper.GetImageFromResource("../resources/Schema_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
            };
            scriptEfPocoDacPacMenuItem.CommandBindings.Add(scriptEfDacPacCommandBinding);
            if (SqlCeToolboxPackage.VsSupportsEf6()) Items.Add(scriptEfPocoDacPacMenuItem);

            var localDatabaseCacheCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                                                                dcmd.SyncFxGenerateLocalDatabaseCacheCode);
            var localDatabaseCacheMenuItem = new MenuItem
            {
                Header = "Generate Local Database Cache code...",
                Icon = ImageHelper.GetImageFromResource("../resources/Synchronize_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = SqlCeScripting.Scope.SchemaDataBlobs
            };
            localDatabaseCacheMenuItem.CommandBindings.Add(localDatabaseCacheCommandBinding);
            localDatabaseCacheMenuItem.IsEnabled = (ver35IsInstalled && DataConnectionHelper.IsSyncFx21Installed());
            if (SqlCeToolboxPackage.IsVsExtension) Items.Add(localDatabaseCacheMenuItem);

            if (SqlCeToolboxPackage.IsVsExtension) Items.Add(new Separator());
            var detectDatabaseCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                            dcmd.CheckCeVersion);

            var versionDetectMenuItem = new MenuItem
            {
                Header = "Detect SQL Server Compact file version...",
                Icon = ImageHelper.GetImageFromResource("../resources/Find_5650.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
            };
            versionDetectMenuItem.CommandBindings.Add(detectDatabaseCommandBinding);
            Items.Add(versionDetectMenuItem);
        }
    }
}