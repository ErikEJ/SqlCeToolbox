using ErikEJ.SqlCeToolbox.Commands;
using ErikEJ.SqlCeToolbox.Helpers;
using System.Windows.Controls;
using System.Windows.Input;

namespace ErikEJ.SqlCeToolbox.ContextMenues
{
    public class DatabaseContextMenuItems
    {
        public MenuItem BuildShowSqlEditorMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
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

        public MenuItem BuildCreateTableMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
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

        public MenuItem BuildScriptDatabaseSchemaMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            ToolTip toolTip, CommandBinding scriptDatabaseCommandBinding)
        {
            var scriptDatabaseSchemaMenuItem = new MenuItem
            {
                Header = "Script Schema...",
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
                ToolTip = toolTip,
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = SqlCeScripting.Scope.Schema
            };
            scriptDatabaseSchemaMenuItem.CommandBindings.Add(scriptDatabaseCommandBinding);
            return scriptDatabaseSchemaMenuItem;
        }

        public MenuItem BuildScriptDatabaseSchemaDataMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            ToolTip toolTip, CommandBinding scriptDatabaseCommandBinding)
        {
            var scriptDatabaseSchemaDataMenuItem = new MenuItem
            {
                Header = "Script Schema and Data...",
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
                ToolTip = toolTip,
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = SqlCeScripting.Scope.SchemaData
            };
            scriptDatabaseSchemaDataMenuItem.CommandBindings.Add(scriptDatabaseCommandBinding);
            return scriptDatabaseSchemaDataMenuItem;
        }

        public MenuItem BuildScriptDatabaseDataMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            ToolTip toolTip, CommandBinding scriptDatabaseCommandBinding)
        {
            var scriptDatabaseDataMenuItem = new MenuItem
            {
                Header = "Script Data...",
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
                ToolTip = toolTip,
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = SqlCeScripting.Scope.DataOnly
            };
            scriptDatabaseDataMenuItem.CommandBindings.Add(scriptDatabaseCommandBinding);
            return scriptDatabaseDataMenuItem;
        }

        public MenuItem BuildScriptDatabaseSchemaDataBlobMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            ToolTip toolTip, CommandBinding scriptDatabaseCommandBinding)
        {
            var scriptDatabaseSchemaDataBlobMenuItem = new MenuItem
            {
                Header = "Script Schema and Data with BLOBs...",
                ToolTip = toolTip,
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = SqlCeScripting.Scope.SchemaDataBlobs
            };
            scriptDatabaseSchemaDataBlobMenuItem.CommandBindings.Add(scriptDatabaseCommandBinding);
            return scriptDatabaseSchemaDataBlobMenuItem;
        }

        public MenuItem BuildScriptDatabaseSchemaDataSqLiteMenuItem(
            DatabaseMenuCommandParameters databaseMenuCommandParameters, ToolTip toolTip,
            CommandBinding scriptDatabaseCommandBinding)
        {
            var scriptDatabaseSchemaDataSqLiteMenuItem = new MenuItem
            {
                Header = "Script Schema and Data for SQLite...",
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
                ToolTip = toolTip,
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = SqlCeScripting.Scope.SchemaDataSQLite
            };
            scriptDatabaseSchemaDataSqLiteMenuItem.CommandBindings.Add(scriptDatabaseCommandBinding);
            return scriptDatabaseSchemaDataSqLiteMenuItem;
        }


        public MenuItem BuildShrinkMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
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

        public MenuItem BuildCompactMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
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

        public MenuItem BuildScriptDatabaseExportMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
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

        public MenuItem BuildScriptDatabaseGraphMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
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

        public MenuItem BuildDocDatabaseMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
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

        public MenuItem BuildGenerateInfoMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
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

        public MenuItem BuildRemoveConnectionMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            DatabaseMenuCommandsHandler dcmd)
        {
            var removeCeConnectionCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                dcmd.RemoveDatabaseConnection);
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
