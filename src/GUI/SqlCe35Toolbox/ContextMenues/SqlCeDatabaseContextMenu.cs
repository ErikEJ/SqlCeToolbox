using System.Windows.Controls;
using System.Windows.Input;
using ErikEJ.SqlCeToolbox.Commands;
using ErikEJ.SqlCeToolbox.Helpers;
using ErikEJ.SqlCeToolbox.ToolWindows;

namespace ErikEJ.SqlCeToolbox.ContextMenues
{
    public class SqlCeDatabaseContextMenu : ContextMenu
    {
        public SqlCeDatabaseContextMenu(DatabaseMenuCommandParameters databaseMenuCommandParameters, ExplorerToolWindow parent)
        {
            var itemBuilder = new DatabaseContextMenuItems();
            var dbType = databaseMenuCommandParameters.DatabaseInfo.DatabaseType;
            if (!(dbType == DatabaseType.SQLCE35
                || dbType == DatabaseType.SQLCE40))
                return;

            var dcmd = new DatabaseMenuCommandsHandler(parent);
            var cecmd = new SqlCeDatabaseMenuCommandsHandler(parent);

            Items.Add(itemBuilder.BuildShowSqlEditorMenuItem(databaseMenuCommandParameters, dcmd));

            Items.Add(itemBuilder.BuildCreateTableMenuItem(databaseMenuCommandParameters, dcmd));
            Items.Add(new Separator());

            var scriptDatabaseRootMenuItem = new MenuItem
            {
                Header = "Script Database",
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
            };

            var toolTip = new ToolTip
            {
                Content = "Generate a SQL Server Compact/SQLite compatible database script"
            };

            // Database scripting items
            var scriptDatabaseCommandBinding = 
                new CommandBinding(DatabaseMenuCommands.DatabaseCommand, dcmd.ScriptDatabase);

            scriptDatabaseRootMenuItem.Items.Add(itemBuilder.BuildScriptDatabaseSchemaMenuItem(databaseMenuCommandParameters, toolTip, scriptDatabaseCommandBinding));

            scriptDatabaseRootMenuItem.Items.Add(itemBuilder.BuildScriptDatabaseSchemaDataMenuItem(databaseMenuCommandParameters, toolTip, scriptDatabaseCommandBinding));

            scriptDatabaseRootMenuItem.Items.Add(BuildScriptAzureSchemaDataMenuItem(databaseMenuCommandParameters, scriptDatabaseCommandBinding));

            scriptDatabaseRootMenuItem.Items.Add(itemBuilder.BuildScriptDatabaseSchemaDataSqLiteMenuItem(databaseMenuCommandParameters, toolTip, scriptDatabaseCommandBinding));

            scriptDatabaseRootMenuItem.Items.Add(itemBuilder.BuildScriptDatabaseSchemaDataBlobMenuItem(databaseMenuCommandParameters, toolTip, scriptDatabaseCommandBinding));

            scriptDatabaseRootMenuItem.Items.Add(itemBuilder.BuildScriptDatabaseDataMenuItem(databaseMenuCommandParameters, toolTip, scriptDatabaseCommandBinding));

            scriptDatabaseRootMenuItem.Items.Add(BuildScriptDatabaseDataForServerMenuItem(databaseMenuCommandParameters, scriptDatabaseCommandBinding));

            scriptDatabaseRootMenuItem.Items.Add(BuildScriptDatabaseDiffMenuItem(databaseMenuCommandParameters, dcmd));

            //End Database scripting items
            Items.Add(scriptDatabaseRootMenuItem);
            Items.Add(new Separator());

            var maintenanceMenuItem = new MenuItem
            {
                Header = "Maintenance",
                Icon = ImageHelper.GetImageFromResource("../resources/Hammer_Builder_16xLG.png"),
            };

            maintenanceMenuItem.Items.Add(BuildPwdMenuItem(databaseMenuCommandParameters, cecmd));

            var shrinkMenuItem = itemBuilder.BuildShrinkMenuItem(databaseMenuCommandParameters, dcmd);
            maintenanceMenuItem.Items.Add(shrinkMenuItem);

            var compactMenuItem = itemBuilder.BuildCompactMenuItem(databaseMenuCommandParameters, dcmd);
            maintenanceMenuItem.Items.Add(compactMenuItem);

            maintenanceMenuItem.Items.Add(BuildVerifyMenuItem(databaseMenuCommandParameters, cecmd));

            maintenanceMenuItem.Items.Add(BuildRepairDeleteMenuItem(databaseMenuCommandParameters, cecmd));

            maintenanceMenuItem.Items.Add(BuildRepairRecoverAllMenuItem(databaseMenuCommandParameters, cecmd));

            maintenanceMenuItem.Items.Add(BuildRepairRecoverPossibleMenuItem(databaseMenuCommandParameters, cecmd));

            Items.Add(maintenanceMenuItem);
            Items.Add(new Separator());

            Items.Add(itemBuilder.BuildScriptDatabaseExportMenuItem(databaseMenuCommandParameters, dcmd));

            if (dbType == DatabaseType.SQLCE35 
                && Helpers.RepositoryHelper.IsV40Installed())
            {
                Items.Add(BuildScriptUpgradeMenuItem(databaseMenuCommandParameters, cecmd));
            }

            Items.Add(new Separator());

            if (SqlCeToolboxPackage.IsVsExtension) Items.Add(itemBuilder.BuildScriptDatabaseGraphMenuItem(databaseMenuCommandParameters, dcmd));

            Items.Add(itemBuilder.BuildDocDatabaseMenuItem(databaseMenuCommandParameters, dcmd));
            Items.Add(new Separator());

            var generateCodeRootMenuItem = new MenuItem
            {
                Header = "Generate Code",
                Icon = ImageHelper.GetImageFromResource("../resources/Schema_16xLG.png"),
            };

            if (SqlCeToolboxPackage.VsSupportsEf6())
                generateCodeRootMenuItem.Items.Add(BuildScriptEfPocoMenuItem(databaseMenuCommandParameters, dcmd));

#if SSMS
#else
            var scriptDcCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                            cecmd.GenerateDataContextInProject);

            generateCodeRootMenuItem.Items.Add(BuildScriptDcMenuItem(databaseMenuCommandParameters, scriptDcCommandBinding));

            generateCodeRootMenuItem.Items.Add(BuildScriptWpdcMenuItem(databaseMenuCommandParameters, scriptDcCommandBinding, dbType));
            generateCodeRootMenuItem.Items.Add(new Separator());
#endif

            var syncFxRootMenuItem = new MenuItem
            {
                Header = "Sync Framework Tools",
                Icon = ImageHelper.GetImageFromResource("../resources/Synchronize_16xLG.png"),
            };

            var isSyncFxInstalled = DataConnectionHelper.IsSyncFx21Installed();

            syncFxRootMenuItem.Items.Add(BuildSyncFxProvisionMenuItem(databaseMenuCommandParameters, cecmd, dbType, isSyncFxInstalled));

            syncFxRootMenuItem.Items.Add(BuildSyncFxDeprovisionMenuItem(databaseMenuCommandParameters, cecmd, dbType, isSyncFxInstalled));

            syncFxRootMenuItem.Items.Add(BuildSyncFxGenerateSnapshotMenuItem(databaseMenuCommandParameters, cecmd, dbType, isSyncFxInstalled));

            generateCodeRootMenuItem.Items.Add(BuildSyncFxMenuItem(databaseMenuCommandParameters, cecmd, dbType, isSyncFxInstalled));
            generateCodeRootMenuItem.Items.Add(syncFxRootMenuItem);

            if (SqlCeToolboxPackage.IsVsExtension) Items.Add(generateCodeRootMenuItem);
            if (SqlCeToolboxPackage.IsVsExtension) Items.Add(new Separator());

            Items.Add(BuildAddDescriptionMenuItem(databaseMenuCommandParameters, dcmd));

            Items.Add(itemBuilder.BuildGenerateInfoMenuItem(databaseMenuCommandParameters, dcmd));
            Items.Add(new Separator());

            Items.Add(BuildCopyConnectionMenuItem(databaseMenuCommandParameters, cecmd));

            Items.Add(itemBuilder.BuildRemoveConnectionMenuItem(databaseMenuCommandParameters, dcmd));
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

        private MenuItem BuildScriptAzureSchemaDataMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            CommandBinding scriptDatabaseCommandBinding)
        {
            var scriptAzureSchemaDataMenuItem = new MenuItem
            {
                Header = "Script Schema and Data for SQL Azure...",
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = SqlCeScripting.Scope.SchemaDataAzure
            };
            scriptAzureSchemaDataMenuItem.CommandBindings.Add(scriptDatabaseCommandBinding);
            return scriptAzureSchemaDataMenuItem;
        }

        private MenuItem BuildScriptDatabaseDataForServerMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            CommandBinding scriptDatabaseCommandBinding)
        {
            var scriptDatabaseDataForServerMenuItem = new MenuItem
            {
                Header = "Script Data for SQL Server...",
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
                Header = "Script Diff...",
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
            SqlCeDatabaseMenuCommandsHandler dcmd)
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

        private MenuItem BuildVerifyMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            SqlCeDatabaseMenuCommandsHandler dcmd)
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
            SqlCeDatabaseMenuCommandsHandler dcmd)
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
            SqlCeDatabaseMenuCommandsHandler dcmd)
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
            SqlCeDatabaseMenuCommandsHandler dcmd)
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

        private MenuItem BuildScriptUpgradeMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            SqlCeDatabaseMenuCommandsHandler dcmd)
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

        private MenuItem BuildScriptEfPocoMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            DatabaseMenuCommandsHandler dcmd)
        {
            var scriptEfPocoCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                dcmd.GenerateEfPocoInProject);

            var scriptEfPocoMenuItem = new MenuItem
            {
                Header = "Add EF 6 DbContext (Code First from Database) to current Project... (beta)",
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
            scriptDcMenuItem.IsEnabled = Helpers.RepositoryHelper.IsV35Installed() &&
                                         Helpers.RepositoryHelper.IsV35DbProviderInstalled();
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
            scriptWpdcMenuItem.IsEnabled = Helpers.RepositoryHelper.IsV35Installed() &&
                                           Helpers.RepositoryHelper.IsV35DbProviderInstalled();
            if (dbType != DatabaseType.SQLCE35)
            {
                scriptWpdcMenuItem.IsEnabled = false;
            }
            return scriptWpdcMenuItem;
        }

        private MenuItem BuildSyncFxProvisionMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            SqlCeDatabaseMenuCommandsHandler dcmd, DatabaseType dbType, bool isSyncFxInstalled)
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
            SqlCeDatabaseMenuCommandsHandler dcmd, DatabaseType dbType, bool isSyncFxInstalled)
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
            SqlCeDatabaseMenuCommandsHandler dcmd, DatabaseType dbType, bool isSyncFxInstalled)
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
            SqlCeDatabaseMenuCommandsHandler dcmd, DatabaseType dbType, bool isSyncFxInstalled)
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

        private MenuItem BuildCopyConnectionMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            SqlCeDatabaseMenuCommandsHandler dcmd)
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
    }
}