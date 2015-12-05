using System.Windows.Controls;
using System.Windows.Input;
using ErikEJ.SqlCeToolbox.Commands;
using ErikEJ.SqlCeToolbox.Helpers;
using ErikEJ.SqlCeToolbox.ToolWindows;

namespace ErikEJ.SqlCeToolbox.ContextMenues
{
    public class DatabaseContextMenu : ContextMenu
    {
        public DatabaseContextMenu(DatabaseMenuCommandParameters databaseMenuCommandParameters, ExplorerControl parent)
        {
            var dcmd = new DatabaseMenuCommandsHandler(parent);

            var showSqlEditorCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                                                                dcmd.SpawnSqlEditorWindow);
            var showSqlEditorMenuItem = new MenuItem
            {
                Header = "Open SQL Editor",
                Icon = ImageHelper.GetImageFromResource("../resources/sqlEditor.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters
            };
            showSqlEditorMenuItem.CommandBindings.Add(showSqlEditorCommandBinding);
            Items.Add(showSqlEditorMenuItem);

            var createTableCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                                        dcmd.BuildTable);
            var createTableMenuItem = new MenuItem
            {
                Header = "Build Table (alpha)...",
                Icon = ImageHelper.GetImageFromResource("../resources/sqlEditor.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters
            };
            createTableMenuItem.CommandBindings.Add(createTableCommandBinding);
            Items.Add(createTableMenuItem);

            Items.Add(new Separator());

            var scriptMenuItem = new MenuItem
            {
                Header = "Script",
                Icon = ImageHelper.GetImageFromResource("../resources/database.png")
            };

            var scriptDatabaseCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                                                                dcmd.ScriptDatabase);
            var scriptDatabaseSchemaMenuItem = new MenuItem
            {
                Header = "Script Database Schema...",
                Icon = ImageHelper.GetImageFromResource("../resources/database.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = SqlCeScripting.Scope.Schema
            };
            scriptDatabaseSchemaMenuItem.CommandBindings.Add(scriptDatabaseCommandBinding);
            scriptMenuItem.Items.Add(scriptDatabaseSchemaMenuItem);

            var scriptDatabaseSchemaDataMenuItem = new MenuItem
            {
                Header = "Script Database Schema and Data...",
                Icon = ImageHelper.GetImageFromResource("../resources/database.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = SqlCeScripting.Scope.SchemaData
            };
            scriptDatabaseSchemaDataMenuItem.CommandBindings.Add(scriptDatabaseCommandBinding);
            scriptMenuItem.Items.Add(scriptDatabaseSchemaDataMenuItem);

            var scriptAzureSchemaDataMenuItem = new MenuItem
            {
                Header = "Script Database Schema and Data for SQL Azure...",
                Icon = ImageHelper.GetImageFromResource("../resources/database.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = SqlCeScripting.Scope.SchemaDataAzure
            };
            scriptAzureSchemaDataMenuItem.CommandBindings.Add(scriptDatabaseCommandBinding);
            scriptMenuItem.Items.Add(scriptAzureSchemaDataMenuItem);

            var scriptSqliteSchemaDataMenuItem = new MenuItem
            {
                Header = "Script Database Schema and Data for SQLite (beta)...",
                Icon = ImageHelper.GetImageFromResource("../resources/database.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = SqlCeScripting.Scope.SchemaDataSQLite
            };
            scriptSqliteSchemaDataMenuItem.CommandBindings.Add(scriptDatabaseCommandBinding);
            scriptMenuItem.Items.Add(scriptSqliteSchemaDataMenuItem);

            var scriptDatabaseSchemaDataBLOBMenuItem = new MenuItem
            {
                Header = "Script Database Schema and Data with BLOBs...",
                Icon = ImageHelper.GetImageFromResource("../resources/database.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = SqlCeScripting.Scope.SchemaDataBlobs
            };
            scriptDatabaseSchemaDataBLOBMenuItem.CommandBindings.Add(scriptDatabaseCommandBinding);
            scriptMenuItem.Items.Add(scriptDatabaseSchemaDataBLOBMenuItem);

            var scriptDatabaseDataMenuItem = new MenuItem
            {
                Header = "Script Database Data...",
                Icon = ImageHelper.GetImageFromResource("../resources/database.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = SqlCeScripting.Scope.DataOnly
            };
            scriptDatabaseDataMenuItem.CommandBindings.Add(scriptDatabaseCommandBinding);
            scriptMenuItem.Items.Add(scriptDatabaseDataMenuItem);

            var scriptDatabaseDataForServerMenuItem = new MenuItem
            {
                Header = "Script Database Data for SQL Server...",
                Icon = ImageHelper.GetImageFromResource("../resources/database.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = SqlCeScripting.Scope.DataOnlyForSqlServer
            };
            scriptDatabaseDataForServerMenuItem.CommandBindings.Add(scriptDatabaseCommandBinding);
            scriptMenuItem.Items.Add(scriptDatabaseDataForServerMenuItem);

            Items.Add(scriptMenuItem);

            Items.Add(new Separator());

            var scriptDiffCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                            dcmd.GenerateDiffScript);

            var scriptDatabaseDiffMenuItem = new MenuItem
            {
                Header = "Script Database Diff...",
                Icon = ImageHelper.GetImageFromResource("../resources/database.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
            };
            scriptDatabaseDiffMenuItem.CommandBindings.Add(scriptDiffCommandBinding);
            scriptDatabaseDiffMenuItem.ToolTip = "Script all tables, columns and constraints in this database\r\nthat are missing/different in the target database.";
            Items.Add(scriptDatabaseDiffMenuItem);

            Items.Add(new Separator());

            var scriptGraphCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                                                    dcmd.GenerateCeDgmlFiles);

            var scriptDatabaseGraphMenuItem = new MenuItem
            {
                Header = "Create Database Graph (DGML)...",
                Icon = ImageHelper.GetImageFromResource("../resources/RelationshipsHS.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
            };
            scriptDatabaseGraphMenuItem.CommandBindings.Add(scriptGraphCommandBinding);
            Items.Add(scriptDatabaseGraphMenuItem);

            // Documentation menu item

            var docDbCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                                        dcmd.GenerateDocFiles);
            var docDatabaseMenuItem = new MenuItem
            {
                Header = "Create Database Documentation...",
                Icon = ImageHelper.GetImageFromResource("../resources/RelationshipsHS.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
            };
            docDatabaseMenuItem.CommandBindings.Add(docDbCommandBinding);
            Items.Add(docDatabaseMenuItem);

            Items.Add(new Separator());

            var scriptDCCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                            dcmd.GenerateDataContext);

            // Desktop Data Context
            var scriptDCMenuItem = new MenuItem
            {
                Header = "Create LINQ to SQL DataContext class...",
                Icon = ImageHelper.GetImageFromResource("../resources/RelationshipsHS.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = true
            };
            scriptDCMenuItem.CommandBindings.Add(scriptDCCommandBinding);
            Items.Add(scriptDCMenuItem);

            //Windows Phone Data Context
            var scriptWPDCMenuItem = new MenuItem
            {
                Header = "Create Windows Phone DataContext class...",
                Icon = ImageHelper.GetImageFromResource("../resources/Phone.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = false
            };
            scriptWPDCMenuItem.CommandBindings.Add(scriptDCCommandBinding);            
#if V35
            scriptWPDCMenuItem.IsEnabled = true;
#else
            scriptWPDCMenuItem.IsEnabled = false;
#endif
            Items.Add(scriptWPDCMenuItem);
            Items.Add(new Separator());

            var maintenanceMenuItem = new MenuItem
            {
                Header = "Maintenance",
                Icon = ImageHelper.GetImageFromResource("../resources/SplitSubdocumentHS.png"),
            };

            var shrinkCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                                                    dcmd.ShrinkDatabase);
            var shrinkMenuItem = new MenuItem
            {
                Header = "Shrink",
                Icon = ImageHelper.GetImageFromResource("../resources/SplitSubdocumentHS.png"),
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
                Icon = ImageHelper.GetImageFromResource("../resources/SplitSubdocumentHS.png"),
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
                Icon = ImageHelper.GetImageFromResource("../resources/SplitSubdocumentHS.png"),
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
                Icon = ImageHelper.GetImageFromResource("../resources/SplitSubdocumentHS.png"),
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
                Icon = ImageHelper.GetImageFromResource("../resources/SplitSubdocumentHS.png"),
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
                Icon = ImageHelper.GetImageFromResource("../resources/SplitSubdocumentHS.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                ToolTip = "Repairs a corrupted database"
            };
            repairRecoverPossibleMenuItem.CommandBindings.Add(repairRecoverPossibleCommandBinding);
            maintenanceMenuItem.Items.Add(repairRecoverPossibleMenuItem);

            Items.Add(maintenanceMenuItem);

            Items.Add(new Separator());

            var addDescriptionCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                                        dcmd.AddDescription);
            var addDescriptionMenuItem = new MenuItem
            {
                Header = "Edit description",
                Icon = ImageHelper.GetImageFromResource("../resources/propes.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters
            };
            addDescriptionMenuItem.CommandBindings.Add(addDescriptionCommandBinding);
            Items.Add(addDescriptionMenuItem);

            Items.Add(new Separator());

            var removeCeConnectionCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                                                    dcmd.RemoveCeDatabase);
            var removeCeConnectionMenuItem = new MenuItem
            {
                Header = "Remove Connection",
                Icon = ImageHelper.GetImageFromResource("../resources/delete.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters
            };
            removeCeConnectionMenuItem.CommandBindings.Add(removeCeConnectionCommandBinding);
            Items.Add(removeCeConnectionMenuItem);

        }
    }

}