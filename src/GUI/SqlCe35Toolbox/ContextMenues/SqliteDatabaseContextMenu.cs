using System.Windows.Controls;
using System.Windows.Input;
using ErikEJ.SqlCeToolbox.Commands;
using ErikEJ.SqlCeToolbox.Helpers;
using ErikEJ.SqlCeToolbox.ToolWindows;

namespace ErikEJ.SqlCeToolbox.ContextMenues
{
    public class SqliteDatabaseContextMenu : ContextMenu
    {
        public SqliteDatabaseContextMenu(DatabaseMenuCommandParameters databaseMenuCommandParameters, ExplorerToolWindow parent)
        {
            var itemBuilder = new DatabaseContextMenuItems();
            var dbType = databaseMenuCommandParameters.DatabaseInfo.DatabaseType;
            if (dbType != DatabaseType.SQLite)
                return;
            var dcmd = new DatabaseMenuCommandsHandler(parent);

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
            var scriptDatabaseCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                                                    dcmd.ScriptDatabase);

            scriptDatabaseRootMenuItem.Items.Add(itemBuilder.BuildScriptDatabaseSchemaMenuItem(databaseMenuCommandParameters, toolTip, scriptDatabaseCommandBinding));

            scriptDatabaseRootMenuItem.Items.Add(itemBuilder.BuildScriptDatabaseSchemaDataMenuItem(databaseMenuCommandParameters, toolTip, scriptDatabaseCommandBinding));

            scriptDatabaseRootMenuItem.Items.Add(itemBuilder.BuildScriptDatabaseDataMenuItem(databaseMenuCommandParameters, toolTip, scriptDatabaseCommandBinding));

            //End Database scripting items
            Items.Add(scriptDatabaseRootMenuItem);
            Items.Add(new Separator());

            var maintenanceMenuItem = new MenuItem
            {
                Header = "Maintenance",
                Icon = ImageHelper.GetImageFromResource("../resources/Hammer_Builder_16xLG.png"),
            };


            var shrinkMenuItem = itemBuilder.BuildShrinkMenuItem(databaseMenuCommandParameters, dcmd);
            maintenanceMenuItem.Items.Add(shrinkMenuItem);

            var compactMenuItem = itemBuilder.BuildCompactMenuItem(databaseMenuCommandParameters, dcmd);

            maintenanceMenuItem.Items.Clear();
            compactMenuItem.Header = "Vacuum";
            compactMenuItem.ToolTip = "Rebuilds the database file, repacking it into a minimal amount of disk space";
            shrinkMenuItem.Header = "Re-index";
            shrinkMenuItem.ToolTip = "Deletes and recreates indexes";
            maintenanceMenuItem.Items.Add(compactMenuItem);
            maintenanceMenuItem.Items.Add(shrinkMenuItem);
            Items.Add(maintenanceMenuItem);
            Items.Add(new Separator());

            Items.Add(itemBuilder.BuildScriptDatabaseExportMenuItem(databaseMenuCommandParameters, dcmd));

            Items.Add(new Separator());

            if (SqlCeToolboxPackage.IsVsExtension) Items.Add(itemBuilder.BuildScriptDatabaseGraphMenuItem(databaseMenuCommandParameters, dcmd));

            Items.Add(itemBuilder.BuildDocDatabaseMenuItem(databaseMenuCommandParameters, dcmd));
            Items.Add(new Separator());

            var generateCodeRootMenuItem = new MenuItem
            {
                Header = "Generate Code",
                Icon = ImageHelper.GetImageFromResource("../resources/Schema_16xLG.png"),
            };

            generateCodeRootMenuItem.Items.Add(BuildScriptModelMenuItem(databaseMenuCommandParameters, dcmd));

            if (SqlCeToolboxPackage.IsVsExtension) Items.Add(generateCodeRootMenuItem);
            if (SqlCeToolboxPackage.IsVsExtension) Items.Add(new Separator());

            Items.Add(itemBuilder.BuildGenerateInfoMenuItem(databaseMenuCommandParameters, dcmd));
            Items.Add(new Separator());

            Items.Add(itemBuilder.BuildRemoveConnectionMenuItem(databaseMenuCommandParameters, dcmd));
        }

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
    }
}