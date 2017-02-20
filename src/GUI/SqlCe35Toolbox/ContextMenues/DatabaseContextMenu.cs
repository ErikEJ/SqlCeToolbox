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
            var dbType = databaseMenuCommandParameters.DatabaseInfo.DatabaseType;
            var isSqlCe = dbType == DatabaseType.SQLCE35
                || dbType == DatabaseType.SQLCE40;
            var dcmd = new DatabaseMenuCommandsHandler(parent);

            Items.Add(BuildShowSqlEditorMenuItem(databaseMenuCommandParameters, dcmd));

            Items.Add(BuildCreateTableMenuItem(databaseMenuCommandParameters, dcmd));
            Items.Add(new Separator());

            var scriptDatabaseRootMenuItem = new MenuItem
            {
                Header = "Script Database",
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
            };

            // Database scripting items
            var scriptDatabaseCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                                                    dcmd.ScriptDatabase);

            scriptDatabaseRootMenuItem.Items.Add(BuildScriptDatabaseSchemaMenuItem(databaseMenuCommandParameters, scriptDatabaseCommandBinding));

            scriptDatabaseRootMenuItem.Items.Add(BuildScriptDatabaseSchemaDataMenuItem(databaseMenuCommandParameters, scriptDatabaseCommandBinding));

            if (isSqlCe)
                scriptDatabaseRootMenuItem.Items.Add(BuildScriptAzureSchemaDataMenuItem(databaseMenuCommandParameters, scriptDatabaseCommandBinding));

            if (isSqlCe)
                scriptDatabaseRootMenuItem.Items.Add(BuildScriptSqliteSchemaDataMenuItem(databaseMenuCommandParameters, scriptDatabaseCommandBinding));

            if (isSqlCe)
                scriptDatabaseRootMenuItem.Items.Add(BuildScriptDatabaseSchemaDataBlobMenuItem(databaseMenuCommandParameters, scriptDatabaseCommandBinding));

            scriptDatabaseRootMenuItem.Items.Add(BuildScriptDatabaseDataMenuItem(databaseMenuCommandParameters, scriptDatabaseCommandBinding));

            if (isSqlCe)
                scriptDatabaseRootMenuItem.Items.Add(BuildScriptDatabaseDataForServerMenuItem(databaseMenuCommandParameters, scriptDatabaseCommandBinding));

            if (isSqlCe)
                scriptDatabaseRootMenuItem.Items.Add(BuildScriptDatabaseDiffMenuItem(databaseMenuCommandParameters, dcmd));

            //End Database scripting items
            Items.Add(scriptDatabaseRootMenuItem);
            Items.Add(new Separator());

            var maintenanceMenuItem = new MenuItem
            {
                Header = "Maintenance",
                Icon = ImageHelper.GetImageFromResource("../resources/Hammer_Builder_16xLG.png"),
            };

            maintenanceMenuItem.Items.Add(BuildPwdMenuItem(databaseMenuCommandParameters, dcmd));

            var shrinkMenuItem = BuildShrinkMenuItem(databaseMenuCommandParameters, dcmd);
            maintenanceMenuItem.Items.Add(shrinkMenuItem);

            var compactMenuItem = BuildCompactMenuItem(databaseMenuCommandParameters, dcmd);
            maintenanceMenuItem.Items.Add(compactMenuItem);

            maintenanceMenuItem.Items.Add(BuildVerifyMenuItem(databaseMenuCommandParameters, dcmd));

            maintenanceMenuItem.Items.Add(BuildRepairDeleteMenuItem(databaseMenuCommandParameters, dcmd));

            maintenanceMenuItem.Items.Add(BuildRepairRecoverAllMenuItem(databaseMenuCommandParameters, dcmd));

            maintenanceMenuItem.Items.Add(BuildRepairRecoverPossibleMenuItem(databaseMenuCommandParameters, dcmd));

            if (isSqlCe)
            {
                Items.Add(maintenanceMenuItem);
                Items.Add(new Separator());
            }
            if (dbType == DatabaseType.SQLite)
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

            Items.Add(BuildScriptDatabaseExportMenuItem(databaseMenuCommandParameters, dcmd));

            if (dbType == DatabaseType.SQLCE35 
                && DataConnectionHelper.IsV40Installed())
            {
                Items.Add(BuildScriptUpgradeMenuItem(databaseMenuCommandParameters, dcmd));
            }

            Items.Add(new Separator());

            if (SqlCeToolboxPackage.IsVsExtension) Items.Add(BuildScriptDatabaseGraphMenuItem(databaseMenuCommandParameters, dcmd));

            Items.Add(BuildDocDatabaseMenuItem(databaseMenuCommandParameters, dcmd));
            Items.Add(new Separator());

            var generateCodeRootMenuItem = new MenuItem
            {
                Header = "Generate Code",
                Icon = ImageHelper.GetImageFromResource("../resources/Schema_16xLG.png"),
            };

#if SSMS
#else
            if (dbType != DatabaseType.SQLCE35)
                generateCodeRootMenuItem.Items.Add(BuildEfCoreModelMenuItem(databaseMenuCommandParameters, dcmd));
#endif
            if (dbType == DatabaseType.SQLite)
                generateCodeRootMenuItem.Items.Add(BuildScriptModelMenuItem(databaseMenuCommandParameters, dcmd));

            if (isSqlCe && SqlCeToolboxPackage.VsSupportsEf6())
                generateCodeRootMenuItem.Items.Add(BuildScriptEfPocoMenuItem(databaseMenuCommandParameters, dcmd));

#if SSMS
#else
            var scriptDcCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                            dcmd.GenerateDataContextInProject);

            if (isSqlCe) generateCodeRootMenuItem.Items.Add(BuildScriptDcMenuItem(databaseMenuCommandParameters, scriptDcCommandBinding));

            if (isSqlCe) generateCodeRootMenuItem.Items.Add(BuildScriptWpdcMenuItem(databaseMenuCommandParameters, scriptDcCommandBinding, dbType));
            if (isSqlCe) generateCodeRootMenuItem.Items.Add(new Separator());
#endif

            var syncFxRootMenuItem = new MenuItem
            {
                Header = "Sync Framework Tools",
                Icon = ImageHelper.GetImageFromResource("../resources/Synchronize_16xLG.png"),
            };

            var isSyncFxInstalled = DataConnectionHelper.IsSyncFx21Installed();

            syncFxRootMenuItem.Items.Add(BuildSyncFxProvisionMenuItem(databaseMenuCommandParameters, dcmd, dbType, isSyncFxInstalled));

            syncFxRootMenuItem.Items.Add(BuildSyncFxDeprovisionMenuItem(databaseMenuCommandParameters, dcmd, dbType, isSyncFxInstalled));

            syncFxRootMenuItem.Items.Add(BuildSyncFxGenerateSnapshotMenuItem(databaseMenuCommandParameters, dcmd, dbType, isSyncFxInstalled));

            if (isSqlCe) generateCodeRootMenuItem.Items.Add(BuildSyncFxMenuItem(databaseMenuCommandParameters, dcmd, dbType, isSyncFxInstalled));
            if (isSqlCe) generateCodeRootMenuItem.Items.Add(syncFxRootMenuItem);

            if (SqlCeToolboxPackage.IsVsExtension) Items.Add(generateCodeRootMenuItem);
            if (SqlCeToolboxPackage.IsVsExtension) Items.Add(new Separator());

            if (isSqlCe) Items.Add(BuildAddDescriptionMenuItem(databaseMenuCommandParameters, dcmd));

            Items.Add(BuildGenerateInfoMenuItem(databaseMenuCommandParameters, dcmd));
            Items.Add(new Separator());

            Items.Add(BuildCopyCeConnectionMenuItem(databaseMenuCommandParameters, dcmd));

            if (!databaseMenuCommandParameters.DatabaseInfo.FromServerExplorer)
            {
                Items.Add(BuildRenameConnectionMenuItem(databaseMenuCommandParameters, dcmd));
            }

            Items.Add(BuildRemoveCeConnectionMenuItem(databaseMenuCommandParameters, dcmd));
        }

        private MenuItem BuildShowSqlEditorMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            DatabaseMenuCommandsHandler dcmd)
        {
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
            return showSqlEditorMenuItem;
        }

        private MenuItem BuildCreateTableMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            DatabaseMenuCommandsHandler dcmd)
        {
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
            return createTableMenuItem;
        }

        private MenuItem BuildScriptDatabaseSchemaMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            CommandBinding scriptDatabaseCommandBinding)
        {
            var scriptDatabaseSchemaMenuItem = new MenuItem
            {
                Header = "Script Database Schema...",
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = SqlCeScripting.Scope.Schema
            };
            scriptDatabaseSchemaMenuItem.CommandBindings.Add(scriptDatabaseCommandBinding);
            return scriptDatabaseSchemaMenuItem;
        }

        private MenuItem BuildScriptDatabaseSchemaDataMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            CommandBinding scriptDatabaseCommandBinding)
        {
            var scriptDatabaseSchemaDataMenuItem = new MenuItem
            {
                Header = "Script Database Schema and Data...",
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = SqlCeScripting.Scope.SchemaData
            };
            scriptDatabaseSchemaDataMenuItem.CommandBindings.Add(scriptDatabaseCommandBinding);
            return scriptDatabaseSchemaDataMenuItem;
        }

        private MenuItem BuildScriptAzureSchemaDataMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            CommandBinding scriptDatabaseCommandBinding)
        {
            var scriptAzureSchemaDataMenuItem = new MenuItem
            {
                Header = "Script Database Schema and Data for SQL Azure...",
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = SqlCeScripting.Scope.SchemaDataAzure
            };
            scriptAzureSchemaDataMenuItem.CommandBindings.Add(scriptDatabaseCommandBinding);
            return scriptAzureSchemaDataMenuItem;
        }

        private MenuItem BuildScriptSqliteSchemaDataMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            CommandBinding scriptDatabaseCommandBinding)
        {
            var scriptSqliteSchemaDataMenuItem = new MenuItem
            {
                Header = "Script Database Schema and Data for SQLite...",
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = SqlCeScripting.Scope.SchemaDataSQLite
            };
            scriptSqliteSchemaDataMenuItem.CommandBindings.Add(scriptDatabaseCommandBinding);
            return scriptSqliteSchemaDataMenuItem;
        }

        private MenuItem BuildScriptDatabaseSchemaDataBlobMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            CommandBinding scriptDatabaseCommandBinding)
        {
            var scriptDatabaseSchemaDataBlobMenuItem = new MenuItem
            {
                Header = "Script Database Schema and Data with BLOBs...",
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = SqlCeScripting.Scope.SchemaDataBlobs
            };
            scriptDatabaseSchemaDataBlobMenuItem.CommandBindings.Add(scriptDatabaseCommandBinding);
            return scriptDatabaseSchemaDataBlobMenuItem;
        }

        private MenuItem BuildScriptDatabaseDataMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            CommandBinding scriptDatabaseCommandBinding)
        {
            var scriptDatabaseDataMenuItem = new MenuItem
            {
                Header = "Script Database Data...",
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = SqlCeScripting.Scope.DataOnly
            };
            scriptDatabaseDataMenuItem.CommandBindings.Add(scriptDatabaseCommandBinding);
            return scriptDatabaseDataMenuItem;
        }

        private MenuItem BuildScriptDatabaseDataForServerMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            CommandBinding scriptDatabaseCommandBinding)
        {
            var scriptDatabaseDataForServerMenuItem = new MenuItem
            {
                Header = "Script Database Data for SQL Server...",
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = SqlCeScripting.Scope.DataOnlyForSqlServer
            };
            scriptDatabaseDataForServerMenuItem.CommandBindings.Add(scriptDatabaseCommandBinding);
            return scriptDatabaseDataForServerMenuItem;
        }

        private MenuItem BuildScriptDatabaseDiffMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            DatabaseMenuCommandsHandler dcmd)
        {
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
            scriptDatabaseDiffMenuItem.ToolTip =
                "Script all tables, columns and constraints in this database\r\nthat are missing/different in the target database.";
            return scriptDatabaseDiffMenuItem;
        }

        private MenuItem BuildPwdMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            DatabaseMenuCommandsHandler dcmd)
        {
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
            return pwdMenuItem;
        }

        private MenuItem BuildShrinkMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            DatabaseMenuCommandsHandler dcmd)
        {
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
            return shrinkMenuItem;
        }

        private MenuItem BuildCompactMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            DatabaseMenuCommandsHandler dcmd)
        {
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
            return compactMenuItem;
        }

        private MenuItem BuildVerifyMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            DatabaseMenuCommandsHandler dcmd)
        {
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
            return verifyMenuItem;
        }

        private MenuItem BuildRepairDeleteMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            DatabaseMenuCommandsHandler dcmd)
        {
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
            return repairDeleteMenuItem;
        }

        private static MenuItem BuildRepairRecoverAllMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            DatabaseMenuCommandsHandler dcmd)
        {
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
            return repairRecoverAllMenuItem;
        }

        private MenuItem BuildRepairRecoverPossibleMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            DatabaseMenuCommandsHandler dcmd)
        {
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
            return repairRecoverPossibleMenuItem;
        }

        private MenuItem BuildScriptDatabaseExportMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            DatabaseMenuCommandsHandler dcmd)
        {
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
            return scriptDatabaseExportMenuItem;
        }

        private MenuItem BuildScriptUpgradeMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            DatabaseMenuCommandsHandler dcmd)
        {
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
            return scriptUpgradeMenuItem;
        }

        private MenuItem BuildScriptDatabaseGraphMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            DatabaseMenuCommandsHandler dcmd)
        {
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
            return scriptDatabaseGraphMenuItem;
        }

        private MenuItem BuildDocDatabaseMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            DatabaseMenuCommandsHandler dcmd)
        {
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
            return docDatabaseMenuItem;
        }

#if SSMS
#else

        private MenuItem BuildEfCoreModelMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            DatabaseMenuCommandsHandler dcmd)
        {
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
            return efCoreModelMenuItem;
        }
#endif
        private MenuItem BuildScriptModelMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            DatabaseMenuCommandsHandler dcmd)
        {
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
            return scriptModelMenuItem;
        }

        private MenuItem BuildScriptEfPocoMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            DatabaseMenuCommandsHandler dcmd)
        {
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
            return scriptEfPocoMenuItem;
        }

        private MenuItem BuildScriptDcMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            CommandBinding scriptDcCommandBinding)
        {
            var scriptDcMenuItem = new MenuItem
            {
                Header = "Add LINQ to SQL DataContext to current Project...",
                Icon = ImageHelper.GetImageFromResource("../resources/Schema_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = true
            };
            scriptDcMenuItem.CommandBindings.Add(scriptDcCommandBinding);
            scriptDcMenuItem.IsEnabled = DataConnectionHelper.IsV35Installed() &&
                                         DataConnectionHelper.IsV35DbProviderInstalled();
            return scriptDcMenuItem;
        }

        private MenuItem BuildScriptWpdcMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            CommandBinding scriptDcCommandBinding, DatabaseType dbType)
        {
            var scriptWpdcMenuItem = new MenuItem
            {
                Header = "Add Windows Phone DataContext to current Project...",
                Icon = ImageHelper.GetImageFromResource("../resources/Schema_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = false
            };
            scriptWpdcMenuItem.CommandBindings.Add(scriptDcCommandBinding);
            scriptWpdcMenuItem.IsEnabled = DataConnectionHelper.IsV35Installed() &&
                                           DataConnectionHelper.IsV35DbProviderInstalled();
            if (dbType != DatabaseType.SQLCE35)
            {
                scriptWpdcMenuItem.IsEnabled = false;
            }
            return scriptWpdcMenuItem;
        }

        private MenuItem BuildSyncFxProvisionMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            DatabaseMenuCommandsHandler dcmd, DatabaseType dbType, bool isSyncFxInstalled)
        {
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
            syncFxProvisionMenuItem.IsEnabled = dbType == DatabaseType.SQLCE35
                                                && isSyncFxInstalled;
            return syncFxProvisionMenuItem;
        }

        private MenuItem BuildSyncFxDeprovisionMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            DatabaseMenuCommandsHandler dcmd, DatabaseType dbType, bool isSyncFxInstalled)
        {
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

            syncFxDeprovisionMenuItem.IsEnabled = dbType == DatabaseType.SQLCE35
                                                  && isSyncFxInstalled;
            return syncFxDeprovisionMenuItem;
        }

        private MenuItem BuildSyncFxGenerateSnapshotMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            DatabaseMenuCommandsHandler dcmd, DatabaseType dbType, bool isSyncFxInstalled)
        {
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

            syncFxGenerateSnapshotMenuItem.IsEnabled = dbType == DatabaseType.SQLCE35
                                                       && isSyncFxInstalled;
            return syncFxGenerateSnapshotMenuItem;
        }

        private MenuItem BuildSyncFxMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            DatabaseMenuCommandsHandler dcmd, DatabaseType dbType, bool isSyncFxInstalled)
        {
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

            syncFxMenuItem.IsEnabled = dbType == DatabaseType.SQLCE35
                                       && isSyncFxInstalled;
            return syncFxMenuItem;
        }

        private MenuItem BuildAddDescriptionMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            DatabaseMenuCommandsHandler dcmd)
        {
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
            return addDescriptionMenuItem;
        }

        private MenuItem BuildGenerateInfoMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            DatabaseMenuCommandsHandler dcmd)
        {
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
            return generateInfoMenuItem;
        }

        private static MenuItem BuildCopyCeConnectionMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            DatabaseMenuCommandsHandler dcmd)
        {
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
            return copyCeConnectionMenuItem;
        }

        private MenuItem BuildRenameConnectionMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            DatabaseMenuCommandsHandler dcmd)
        {
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
            return renameConnectionMenuItem;
        }

        private MenuItem BuildRemoveCeConnectionMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            DatabaseMenuCommandsHandler dcmd)
        {
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
            return removeCeConnectionMenuItem;
        }
    }
}