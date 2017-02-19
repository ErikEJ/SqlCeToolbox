using System.Windows.Controls;
using System.Windows.Input;
using ErikEJ.SqlCeToolbox.Commands;
using ErikEJ.SqlCeToolbox.Helpers;
using ErikEJ.SqlCeToolbox.ToolWindows;

namespace ErikEJ.SqlCeToolbox.ContextMenues
{
    public class DatabaseContextMenu : ContextMenu
    {
        public DatabaseContextMenu(DatabaseMenuCommandParameters databaseMenuCommandParameters, ExplorerToolWindow parent)
        {
            var isSqlCe = databaseMenuCommandParameters.DatabaseInfo.DatabaseType == DatabaseType.SQLCE35
                || databaseMenuCommandParameters.DatabaseInfo.DatabaseType == DatabaseType.SQLCE40;

            var dcmd = new DatabaseMenuCommandsHandler(parent);

            var showSqlEditorCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                                                                dcmd.SpawnSqlEditorWindow);
            var showSqlEditorMenuItem = new MenuItem
            {
                Header = "New Query",
                Icon = ImageHelper.GetImageFromResource("../resources/NewQuery.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters
            };
            showSqlEditorMenuItem.CommandBindings.Add(showSqlEditorCommandBinding);
            Items.Add(showSqlEditorMenuItem);

            var createTableCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                                                    dcmd.BuildTable);
            var createTableMenuItem = new MenuItem
            {
                Header = "Build Table (beta)...",
                Icon = ImageHelper.GetImageFromResource("../resources/AddTable_5632.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters
            };
            createTableMenuItem.CommandBindings.Add(createTableCommandBinding);
            Items.Add(createTableMenuItem);
            Items.Add(new Separator());

            var scriptDatabaseRootMenuItem = new MenuItem
            {
                Header = "Script Database",
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
            };

            // Database scripting items
            var scriptDatabaseCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                                                    dcmd.ScriptDatabase);

            var scriptDatabaseSchemaMenuItem = new MenuItem
            {
                Header = "Script Database Schema...",
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = SqlCeScripting.Scope.Schema
            };
            scriptDatabaseSchemaMenuItem.CommandBindings.Add(scriptDatabaseCommandBinding);
            scriptDatabaseRootMenuItem.Items.Add(scriptDatabaseSchemaMenuItem);


            var scriptDatabaseSchemaDataMenuItem = new MenuItem
            {
                Header = "Script Database Schema and Data...",
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = SqlCeScripting.Scope.SchemaData
            };
            scriptDatabaseSchemaDataMenuItem.CommandBindings.Add(scriptDatabaseCommandBinding);
            scriptDatabaseRootMenuItem.Items.Add(scriptDatabaseSchemaDataMenuItem);

            var scriptAzureSchemaDataMenuItem = new MenuItem
            {
                Header = "Script Database Schema and Data for SQL Azure...",
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = SqlCeScripting.Scope.SchemaDataAzure
            };
            scriptAzureSchemaDataMenuItem.CommandBindings.Add(scriptDatabaseCommandBinding);
            if (isSqlCe)
                scriptDatabaseRootMenuItem.Items.Add(scriptAzureSchemaDataMenuItem);

            var scriptSqliteSchemaDataMenuItem = new MenuItem
            {
                Header = "Script Database Schema and Data for SQLite...",
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = SqlCeScripting.Scope.SchemaDataSQLite
            };
            scriptSqliteSchemaDataMenuItem.CommandBindings.Add(scriptDatabaseCommandBinding);
            if (isSqlCe)
                scriptDatabaseRootMenuItem.Items.Add(scriptSqliteSchemaDataMenuItem);

            var scriptDatabaseSchemaDataBlobMenuItem = new MenuItem
            {
                Header = "Script Database Schema and Data with BLOBs...",
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = SqlCeScripting.Scope.SchemaDataBlobs
            };
            scriptDatabaseSchemaDataBlobMenuItem.CommandBindings.Add(scriptDatabaseCommandBinding);
            if (isSqlCe)
                scriptDatabaseRootMenuItem.Items.Add(scriptDatabaseSchemaDataBlobMenuItem);

            var scriptDatabaseDataMenuItem = new MenuItem
            {
                Header = "Script Database Data...",
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = SqlCeScripting.Scope.DataOnly
            };
            scriptDatabaseDataMenuItem.CommandBindings.Add(scriptDatabaseCommandBinding);
            scriptDatabaseRootMenuItem.Items.Add(scriptDatabaseDataMenuItem);

            var scriptDatabaseDataForServerMenuItem = new MenuItem
            {
                Header = "Script Database Data for SQL Server...",
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = SqlCeScripting.Scope.DataOnlyForSqlServer
            };
            scriptDatabaseDataForServerMenuItem.CommandBindings.Add(scriptDatabaseCommandBinding);
            if (isSqlCe)
                scriptDatabaseRootMenuItem.Items.Add(scriptDatabaseDataForServerMenuItem);

            var scriptDiffCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                            dcmd.GenerateDiffScript);

            var scriptDatabaseDiffMenuItem = new MenuItem
            {
                Header = "Script Database Diff...",
                Icon = ImageHelper.GetImageFromResource("../resources/DataCompare_9880.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
            };
            scriptDatabaseDiffMenuItem.CommandBindings.Add(scriptDiffCommandBinding);
            scriptDatabaseDiffMenuItem.ToolTip = "Script all tables, columns and constraints in this database\r\nthat are missing/different in the target database.";
            if (isSqlCe)
                scriptDatabaseRootMenuItem.Items.Add(scriptDatabaseDiffMenuItem);

            //End Database scripting options
            Items.Add(scriptDatabaseRootMenuItem);
            Items.Add(new Separator());

            var maintenanceMenuItem = new MenuItem
            {
                Header = "Maintenance",
                Icon = ImageHelper.GetImageFromResource("../resources/Hammer_Builder_16xLG.png"),
            };

            var setPwdCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                                                    dcmd.SetPassword);
            var pwdMenuItem = new MenuItem
            {
                Header = "Set Password",
                Icon = ImageHelper.GetImageFromResource("../resources/Hammer_Builder_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                ToolTip = "Set the database password"
            };
            pwdMenuItem.CommandBindings.Add(setPwdCommandBinding);
            maintenanceMenuItem.Items.Add(pwdMenuItem);

            var shrinkCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                                                    dcmd.ShrinkDatabase);
            var shrinkMenuItem = new MenuItem
            {
                Header = "Shrink",
                Icon = ImageHelper.GetImageFromResource("../resources/Hammer_Builder_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                ToolTip = "Shrink database by deleting free pages"
            };
            shrinkMenuItem.CommandBindings.Add(shrinkCommandBinding);
            maintenanceMenuItem.Items.Add(shrinkMenuItem);

            var compactCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                                        dcmd.CompactDatabase);
            var compactMenuItem = new MenuItem
            {
                Header = "Compact",
                Icon = ImageHelper.GetImageFromResource("../resources/Hammer_Builder_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                ToolTip = "Perform full database compaction"
            };
            compactMenuItem.CommandBindings.Add(compactCommandBinding);
            maintenanceMenuItem.Items.Add(compactMenuItem);

            var verifyCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                            dcmd.VerifyDatabase);
            var verifyMenuItem = new MenuItem
            {
                Header = "Verify",
                Icon = ImageHelper.GetImageFromResource("../resources/Hammer_Builder_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                ToolTip = "Verify the integrity of the database (enhanced)"
            };
            verifyMenuItem.CommandBindings.Add(verifyCommandBinding);
            maintenanceMenuItem.Items.Add(verifyMenuItem);

            var repairDeleteCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                            dcmd.RepairDatabaseDeleteCorruptedRows);
            var repairDeleteMenuItem = new MenuItem
            {
                Header = "Repair (delete corrupted rows)",
                Icon = ImageHelper.GetImageFromResource("../resources/Hammer_Builder_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                ToolTip = "Repairs a corrupted database"
            };
            repairDeleteMenuItem.CommandBindings.Add(repairDeleteCommandBinding);
            maintenanceMenuItem.Items.Add(repairDeleteMenuItem);

            var repairRecoverAllCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                            dcmd.RepairDatabaseRecoverAllOrFail);
            var repairRecoverAllMenuItem = new MenuItem
            {
                Header = "Repair (recover all or fail)",
                Icon = ImageHelper.GetImageFromResource("../resources/Hammer_Builder_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                ToolTip = "Repairs a corrupted database"
            };
            repairRecoverAllMenuItem.CommandBindings.Add(repairRecoverAllCommandBinding);
            maintenanceMenuItem.Items.Add(repairRecoverAllMenuItem);

            var repairRecoverPossibleCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                            dcmd.RepairDatabaseRecoverAllPossibleRows);
            var repairRecoverPossibleMenuItem = new MenuItem
            {
                Header = "Repair (recover all possible)",
                Icon = ImageHelper.GetImageFromResource("../resources/Hammer_Builder_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                ToolTip = "Repairs a corrupted database"
            };
            repairRecoverPossibleMenuItem.CommandBindings.Add(repairRecoverPossibleCommandBinding);
            maintenanceMenuItem.Items.Add(repairRecoverPossibleMenuItem);

            if (isSqlCe)
            {
                Items.Add(maintenanceMenuItem);
                Items.Add(new Separator());
            }
            if (databaseMenuCommandParameters.DatabaseInfo.DatabaseType == DatabaseType.SQLite)
            {
                maintenanceMenuItem.Items.Clear();
                compactMenuItem.Header = "Vacuum";
                compactMenuItem.ToolTip = "Rebuilds the database file, repacking it into a minimal amount of disk space";
                shrinkMenuItem.Header = "Re-index";
                shrinkMenuItem.ToolTip = "Deletes and recreates indexes";
                maintenanceMenuItem.Items.Add(compactMenuItem);
                maintenanceMenuItem.Items.Add(shrinkMenuItem);
                Items.Add(maintenanceMenuItem);
                Items.Add(new Separator());
            }

            var scriptExportCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                dcmd.ExportToServer);

            var scriptDatabaseExportMenuItem = new MenuItem
            {
                Header = "Migrate to SQL Server (incl. Azure/Express)...",
                Icon = ImageHelper.GetImageFromResource("../resources/ExportReportData_10565.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
            };
            scriptDatabaseExportMenuItem.CommandBindings.Add(scriptExportCommandBinding);
            scriptDatabaseExportMenuItem.ToolTip = "Migrate entire database to a SQL Server database";
            Items.Add(scriptDatabaseExportMenuItem);

            var scriptUpgradeCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                dcmd.UpgradeTo40);

            var scriptUpgradeMenuItem = new MenuItem
            {
                Header = "Upgrade to version 4.0...",
                Icon = ImageHelper.GetImageFromResource("../resources/ExportReportData_10565.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
            };
            scriptUpgradeMenuItem.CommandBindings.Add(scriptUpgradeCommandBinding);
            scriptUpgradeMenuItem.ToolTip = "Create a copy of this database in 4.0 format";

            if (databaseMenuCommandParameters.DatabaseInfo.DatabaseType == DatabaseType.SQLCE35 
                && DataConnectionHelper.IsV40Installed())
            {
                Items.Add(scriptUpgradeMenuItem);
            }

            Items.Add(new Separator());

            var scriptGraphCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                                                    dcmd.GenerateCeDgmlFiles);

            var scriptDatabaseGraphMenuItem = new MenuItem
            {
                Header = "Create Database Graph (DGML)...",
                Icon = ImageHelper.GetImageFromResource("../resources/Diagram_16XLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
            };
            scriptDatabaseGraphMenuItem.CommandBindings.Add(scriptGraphCommandBinding);
            if (SqlCeToolboxPackage.IsVsExtension) Items.Add(scriptDatabaseGraphMenuItem);

            // Documentation menu item

            var docDbCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                                        dcmd.GenerateDocFiles);
            var docDatabaseMenuItem = new MenuItem
            {
                Header = "Create Database Documentation...",
                Icon = ImageHelper.GetImageFromResource("../resources/Schema_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
            };
            docDatabaseMenuItem.CommandBindings.Add(docDbCommandBinding);
            Items.Add(docDatabaseMenuItem);

            Items.Add(new Separator());
            var generateCodeRootMenuItem = new MenuItem
            {
                Header = "Generate Code",
                Icon = ImageHelper.GetImageFromResource("../resources/Schema_16xLG.png"),
            };

            var efCoreModelCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                            dcmd.GenerateEFCoreModelInProject);
            var efCoreModelMenuItem = new MenuItem
            {
                Header = "Add Entity Framework Core Model to current Project... (alpha)",
                Icon = ImageHelper.GetImageFromResource("../resources/Schema_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters
            };
            efCoreModelMenuItem.CommandBindings.Add(efCoreModelCommandBinding);
            generateCodeRootMenuItem.Items.Add(efCoreModelMenuItem);

            var scriptModelCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
            dcmd.GenerateModelCodeInProject);

            var scriptModelMenuItem = new MenuItem
            {
                Header = "Add sqlite-net DataAccess.cs to current Project...",
                Icon = ImageHelper.GetImageFromResource("../resources/Schema_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
            };
            scriptModelMenuItem.CommandBindings.Add(scriptModelCommandBinding);
            if (databaseMenuCommandParameters.DatabaseInfo.DatabaseType == DatabaseType.SQLite)
                generateCodeRootMenuItem.Items.Add(scriptModelMenuItem);

            var scriptEfPocoCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                            dcmd.GenerateEfPocoInProject);

            var scriptEfPocoMenuItem = new MenuItem
            {
                Header = "Add Entity Data Model (Code First from Database) to current Project... (beta)",
                Icon = ImageHelper.GetImageFromResource("../resources/Schema_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
            };
            scriptEfPocoMenuItem.CommandBindings.Add(scriptEfPocoCommandBinding);
            if (isSqlCe && SqlCeToolboxPackage.VsSupportsEf6())
                generateCodeRootMenuItem.Items.Add(scriptEfPocoMenuItem);

#if SSMS
#else
            var scriptDcCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                            dcmd.GenerateDataContextInProject);

            var scriptDcMenuItem = new MenuItem
            {
                Header = "Add LINQ to SQL DataContext to current Project...",
                Icon = ImageHelper.GetImageFromResource("../resources/Schema_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = true
            };
            scriptDcMenuItem.CommandBindings.Add(scriptDcCommandBinding);
            scriptDcMenuItem.IsEnabled = DataConnectionHelper.IsV35Installed() && DataConnectionHelper.IsV35DbProviderInstalled();
            if (isSqlCe) generateCodeRootMenuItem.Items.Add(scriptDcMenuItem);

            var scriptWpdcMenuItem = new MenuItem
            {
                Header = "Add Windows Phone DataContext to current Project...",
                Icon = ImageHelper.GetImageFromResource("../resources/Schema_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = false
            };
            scriptWpdcMenuItem.CommandBindings.Add(scriptDcCommandBinding);
            scriptWpdcMenuItem.IsEnabled = DataConnectionHelper.IsV35Installed() && DataConnectionHelper.IsV35DbProviderInstalled();
            if (databaseMenuCommandParameters.DatabaseInfo.DatabaseType != DatabaseType.SQLCE35)
            {
                scriptWpdcMenuItem.IsEnabled = false;
            }            
            if (isSqlCe) generateCodeRootMenuItem.Items.Add(scriptWpdcMenuItem);
            if (isSqlCe) generateCodeRootMenuItem.Items.Add(new Separator());
#endif

            var syncFxRootMenuItem = new MenuItem
            {
                Header = "Sync Framework Tools",
                Icon = ImageHelper.GetImageFromResource("../resources/Synchronize_16xLG.png"),
            };

            var isSyncFxInstalled = DataConnectionHelper.IsSyncFx21Installed();

            var syncFxProvisionCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                dcmd.SyncFxProvisionScope);

            var syncFxProvisionMenuItem = new MenuItem
            {
                Header = "Provision Sync Framework Scope...",
                Icon = ImageHelper.GetImageFromResource("../resources/Synchronize_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
            };
            syncFxProvisionMenuItem.CommandBindings.Add(syncFxProvisionCommandBinding);
            syncFxProvisionMenuItem.IsEnabled = databaseMenuCommandParameters.DatabaseInfo.DatabaseType == DatabaseType.SQLCE35 
                && isSyncFxInstalled;
            
            syncFxRootMenuItem.Items.Add(syncFxProvisionMenuItem);

            var syncFxDeprovisionCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                dcmd.SyncFxDeprovisionDatabase);

            var syncFxDeprovisionMenuItem = new MenuItem
            {
                Header = "Deprovision Sync Framework Objects from Database",
                Icon = ImageHelper.GetImageFromResource("../resources/Synchronize_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
            };
            syncFxDeprovisionMenuItem.CommandBindings.Add(syncFxDeprovisionCommandBinding);

            syncFxDeprovisionMenuItem.IsEnabled = databaseMenuCommandParameters.DatabaseInfo.DatabaseType == DatabaseType.SQLCE35 
                && isSyncFxInstalled;

            syncFxRootMenuItem.Items.Add(syncFxDeprovisionMenuItem);

            var syncFxGenerateSnapshotCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                dcmd.SyncFxGenerateSnapshot);

            var syncFxGenerateSnapshotMenuItem = new MenuItem
            {
                Header = "Generate snapshot database to initialize other clients...",
                Icon = ImageHelper.GetImageFromResource("../resources/Synchronize_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
            };
            syncFxGenerateSnapshotMenuItem.CommandBindings.Add(syncFxGenerateSnapshotCommandBinding);

            syncFxGenerateSnapshotMenuItem.IsEnabled = databaseMenuCommandParameters.DatabaseInfo.DatabaseType == DatabaseType.SQLCE35
                && isSyncFxInstalled;

            syncFxRootMenuItem.Items.Add(syncFxGenerateSnapshotMenuItem);

            var syncFxCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                dcmd.SyncFxGenerateSyncCodeInProject);

            var syncFxMenuItem = new MenuItem
            {
                Header = "Add Sync Framework Class to current Project...",
                Icon = ImageHelper.GetImageFromResource("../resources/Synchronize_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
            };
            syncFxMenuItem.CommandBindings.Add(syncFxCommandBinding);

            syncFxMenuItem.IsEnabled = databaseMenuCommandParameters.DatabaseInfo.DatabaseType == DatabaseType.SQLCE35 
                && isSyncFxInstalled;

            if (isSqlCe) generateCodeRootMenuItem.Items.Add(syncFxMenuItem);
            if (isSqlCe) generateCodeRootMenuItem.Items.Add(syncFxRootMenuItem);

            if (SqlCeToolboxPackage.IsVsExtension) Items.Add(generateCodeRootMenuItem);
            if (SqlCeToolboxPackage.IsVsExtension) Items.Add(new Separator());

            var addDescriptionCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                                                    dcmd.AddDescription);
            var addDescriptionMenuItem = new MenuItem
            {
                Header = "Edit description...",
                Icon = ImageHelper.GetImageFromResource("../resources/properties_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters
            };
            addDescriptionMenuItem.CommandBindings.Add(addDescriptionCommandBinding);
            if (isSqlCe)
            {
                Items.Add(addDescriptionMenuItem);
            }

            var generateInfoCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                                        dcmd.GenerateDatabaseInfo);
            var generateInfoMenuItem = new MenuItem
            {
                Header = "Database Information",
                Icon = ImageHelper.GetImageFromResource("../resources/properties_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters
            };
            generateInfoMenuItem.CommandBindings.Add(generateInfoCommandBinding);
            Items.Add(generateInfoMenuItem);
            
            Items.Add(new Separator());

            var copyCeConnectionCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                                        dcmd.CopyCeDatabase);
            var copyCeConnectionMenuItem = new MenuItem
            {
                InputGestureText = "Ctrl+C",
                Header = "Copy Database File",
                Icon = ImageHelper.GetImageFromResource("../resources/Copy_6524.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters
            };
            copyCeConnectionMenuItem.CommandBindings.Add(copyCeConnectionCommandBinding);
            Items.Add(copyCeConnectionMenuItem);

            var renameConnectionCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                                                   dcmd.RenameConnection);
            var renameConnectionMenuItem = new MenuItem
            {
                Header = "Rename Connection... (beta)",
                Icon = ImageHelper.GetImageFromResource("../resources/Rename_6779.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters
            };
            renameConnectionMenuItem.CommandBindings.Add(renameConnectionCommandBinding);
            if (!databaseMenuCommandParameters.DatabaseInfo.FromServerExplorer)
            {
                Items.Add(renameConnectionMenuItem);
            }

            var removeCeConnectionCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                                                    dcmd.RemoveCeDatabase);
            var removeCeConnectionMenuItem = new MenuItem
            {
                Header = "Remove Connection",
                Icon = ImageHelper.GetImageFromResource("../resources/action_Cancel_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters
            };
            removeCeConnectionMenuItem.CommandBindings.Add(removeCeConnectionCommandBinding);
            Items.Add(removeCeConnectionMenuItem);
        }
    }
}