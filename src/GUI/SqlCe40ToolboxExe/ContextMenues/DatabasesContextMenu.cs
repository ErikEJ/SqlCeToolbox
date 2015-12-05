using System.Windows.Controls;
using System.Windows.Input;
using ErikEJ.SqlCeToolbox.Commands;
using ErikEJ.SqlCeToolbox.Helpers;
using ErikEJ.SqlCeToolbox.ToolWindows;

namespace ErikEJ.SqlCeToolbox.ContextMenues
{
    public class DatabasesContextMenu : ContextMenu
    {
        public DatabasesContextMenu(DatabasesMenuCommandParameters databaseMenuCommandParameters, ExplorerControl parent)
        {
            var dcmd = new DatabaseMenuCommandsHandler(parent);

            var toolTip1 = new ToolTip();

            bool runtimeIsInstalled = DataConnectionHelper.IsRuntimeInstalled();

            if (runtimeIsInstalled)
            {
                toolTip1.Content = string.Format("Install SQL Server Compact {0} desktop runtime to enable this feature", RepoHelper.apiVer);
            }
            else
            {
                toolTip1.Content = string.Format("Add SQL Server Compact {0} Connection", RepoHelper.apiVer);
            }
            var addCeDatabaseCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                                        dcmd.AddCeDatabase);

            var addCeDatabaseMenuItem = new MenuItem
            {
                Header = string.Format("Add SQL Server Compact {0} Connection...", RepoHelper.apiVer),
                Icon = ImageHelper.GetImageFromResource("../resources/AddTableHS.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
				ToolTip = toolTip1,
                CommandParameter = databaseMenuCommandParameters,
            };
            addCeDatabaseMenuItem.CommandBindings.Add(addCeDatabaseCommandBinding);
			addCeDatabaseMenuItem.IsEnabled = runtimeIsInstalled;
            Items.Add(addCeDatabaseMenuItem);

            Items.Add(new Separator());

            var scriptGraphCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                                        dcmd.GenerateServerDgmlFiles);

            var toolTip = new ToolTip();
            toolTip.Content = "Generate a SQL Server Compact compatible database script from SQL Server 2005/2008";

            var scriptDatabaseGraphMenuItem = new MenuItem
            {
                Header = "Create SQL Server Database Graph (DGML)...",
                Icon = ImageHelper.GetImageFromResource("../resources/RelationshipsHS.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
            };
            scriptDatabaseGraphMenuItem.CommandBindings.Add(scriptGraphCommandBinding);
            Items.Add(scriptDatabaseGraphMenuItem);
            Items.Add(new Separator());

            var scriptDatabaseCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                                                                dcmd.ScriptServerDatabase);

            var scriptDatabaseSchemaMenuItem = new MenuItem
                                             {
                                                 Header = "Script SQL Server Database Schema...",
                                                 Icon = ImageHelper.GetImageFromResource("../resources/database.png"),
                                                 ToolTip = toolTip,
                                                 Command = DatabaseMenuCommands.DatabaseCommand,
                                                 CommandParameter = databaseMenuCommandParameters,
                                                 Tag = SqlCeScripting.Scope.Schema
                                             };
            scriptDatabaseSchemaMenuItem.CommandBindings.Add(scriptDatabaseCommandBinding);
            Items.Add(scriptDatabaseSchemaMenuItem);

            var scriptDatabaseDataMenuItem = new MenuItem
            {
                Header = "Script SQL Server Database Data...",
                Icon = ImageHelper.GetImageFromResource("../resources/database.png"),
                ToolTip = toolTip,
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = SqlCeScripting.Scope.DataOnly
            };
            scriptDatabaseDataMenuItem.CommandBindings.Add(scriptDatabaseCommandBinding);
            Items.Add(scriptDatabaseDataMenuItem);

            var scriptDatabaseSchemaDataMenuItem = new MenuItem
                                                {
                                                    Header = "Script SQL Server Database Schema and Data...",
                                                    Icon = ImageHelper.GetImageFromResource("../resources/database.png"),
                                                    ToolTip = toolTip,
                                                    Command = DatabaseMenuCommands.DatabaseCommand,
                                                    CommandParameter = databaseMenuCommandParameters,
                                                    Tag = SqlCeScripting.Scope.SchemaData 
                                                };
            scriptDatabaseSchemaDataMenuItem.CommandBindings.Add(scriptDatabaseCommandBinding);
            Items.Add(scriptDatabaseSchemaDataMenuItem);

            var scriptDatabaseSchemaDataBLOBMenuItem = new MenuItem
            {
                Header = "Script SQL Server Database Schema and Data with BLOBs...",
                ToolTip = toolTip,
                Icon = ImageHelper.GetImageFromResource("../resources/database.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
                Tag = SqlCeScripting.Scope.SchemaDataBlobs 
            };
            scriptDatabaseSchemaDataBLOBMenuItem.CommandBindings.Add(scriptDatabaseCommandBinding);
            Items.Add(scriptDatabaseSchemaDataBLOBMenuItem);

            Items.Add(new Separator());

            var exportServerCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                                                                dcmd.ExportServerDatabaseTo40);            
            var exportServerMenuItem = new MenuItem
            {
                Header = string.Format("Export SQL Server to SQL Server Compact {0} ...", RepoHelper.apiVer),
                Icon = ImageHelper.GetImageFromResource("../resources/data_out_small.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters
            };
            exportServerMenuItem.CommandBindings.Add(exportServerCommandBinding);
            exportServerMenuItem.IsEnabled = runtimeIsInstalled;
            Items.Add(exportServerMenuItem);

            Items.Add(new Separator());
            var detectDatabaseCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                            dcmd.CheckCeVersion);

            var versionDetectMenuItem = new MenuItem
            {
                Header = "Detect SQL Server Compact file version...",
                Icon = ImageHelper.GetImageFromResource("../resources/ZoomHS.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
            };
            versionDetectMenuItem.CommandBindings.Add(detectDatabaseCommandBinding);
            Items.Add(versionDetectMenuItem);

            //if (RepoHelper.apiVer == "4.0" && 1 == 0)
            //{
            //    Items.Add(new Separator());
            //    var installDDEX4CommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
            //                    dcmd.InstallDDEX4);

            //    var installDDEX4MenuItem = new MenuItem
            //    {
            //        Header = "Enable VS Express 2013 to use SQL Server Compact 4.0 with Entity Framework 6",
            //        Icon = ImageHelper.GetImageFromResource("../resources/RelationshipsHS.png"),
            //        Command = DatabaseMenuCommands.DatabaseCommand,
            //        CommandParameter = databaseMenuCommandParameters,
            //    };
            //    installDDEX4MenuItem.CommandBindings.Add(installDDEX4CommandBinding);
            //    Items.Add(installDDEX4MenuItem);
            //}
        }
    }
}