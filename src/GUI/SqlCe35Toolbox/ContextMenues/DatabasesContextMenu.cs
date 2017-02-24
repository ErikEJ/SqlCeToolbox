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
            var ver40IsInstalled = DataConnectionHelper.IsV40Installed();
            var ver35IsInstalled = DataConnectionHelper.IsV35Installed();
            var pkg = parent.Package as SqlCeToolboxPackage;

            Items.Add(BuildAddCeDatabaseMenuItem(databaseMenuCommandParameters, dcmd, pkg, ver40IsInstalled));

            Items.Add(BuildAddCe35DatabaseMenuItem(databaseMenuCommandParameters, dcmd, pkg, ver35IsInstalled));

            Items.Add(BuildAddSqLiteDatabaseMenuItem(databaseMenuCommandParameters, dcmd));

            if (SqlCeToolboxPackage.IsVsExtension) Items.Add(BuildAddSqlServerDatabaseMenuItem(databaseMenuCommandParameters, dcmd));

            if (SqlCeToolboxPackage.IsVsExtension) Items.Add(BuildAddFromSolutionMenuItem(databaseMenuCommandParameters, dcmd, ver40IsInstalled, ver35IsInstalled));

            Items.Add(BuildFixConnectionsMenuItem(databaseMenuCommandParameters, dcmd));
            Items.Add(new Separator());

            Items.Add(BuildDesignDatabaseMenuItem(databaseMenuCommandParameters, dcmd));
            Items.Add(new Separator());

            if (SqlCeToolboxPackage.IsVsExtension) Items.Add(BuildLocalDatabaseCacheMenuItem(databaseMenuCommandParameters, dcmd, ver35IsInstalled));

            if (SqlCeToolboxPackage.IsVsExtension) Items.Add(new Separator());

            Items.Add(BuildVersionDetectMenuItem(databaseMenuCommandParameters, dcmd));
        }

        private MenuItem BuildAddCeDatabaseMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            DatabasesMenuCommandsHandler dcmd, SqlCeToolboxPackage pkg, bool ver40IsInstalled)
        {
            var addCe4DatabaseCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                dcmd.AddPrivateCe40Database);
            var addCeDatabaseMenuItem = new MenuItem
            {
                Header = "Add SQL Server Compact 4.0 Connection...",
                Icon = ImageHelper.GetImageFromResource("../resources/AddDatabase_16x.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
            };
            if (pkg != null && pkg.VsSupportsDdex40())
            {
                var addCe40DatabaseCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                    dcmd.AddCe40Database);
                addCeDatabaseMenuItem.CommandBindings.Add(addCe40DatabaseCommandBinding);
                addCeDatabaseMenuItem.IsEnabled = ver40IsInstalled;
            }
            else
            {
                addCeDatabaseMenuItem.CommandBindings.Add(addCe4DatabaseCommandBinding);
                addCeDatabaseMenuItem.IsEnabled = ver40IsInstalled;
            }
            return addCeDatabaseMenuItem;
        }

        private MenuItem BuildAddCe35DatabaseMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            DatabasesMenuCommandsHandler dcmd, SqlCeToolboxPackage pkg, bool ver35IsInstalled)
        {
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
            }
            else
            {
                var addCe3511DatabaseCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                    dcmd.AddPrivateCe35Database);
                addCe35DatabaseMenuItem.CommandBindings.Add(addCe3511DatabaseCommandBinding);
                addCe35DatabaseMenuItem.IsEnabled = ver35IsInstalled;
            }
            return addCe35DatabaseMenuItem;
        }

        private MenuItem BuildAddSqLiteDatabaseMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            DatabasesMenuCommandsHandler dcmd)
        {
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
            return addSqLiteDatabaseMenuItem;
        }

        private MenuItem BuildAddSqlServerDatabaseMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            DatabasesMenuCommandsHandler dcmd)
        {
            var addSqlServerDatabaseCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                dcmd.AddSqlServerDatabase);
            var addSqlServerDatabaseMenuItem = new MenuItem
            {
                Header = "Add SQL Server Connection...",
                Icon = ImageHelper.GetImageFromResource("../resources/AddDatabase_16x.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = databaseMenuCommandParameters,
            };
            addSqlServerDatabaseMenuItem.CommandBindings.Add(addSqlServerDatabaseCommandBinding);
            return addSqlServerDatabaseMenuItem;
        }

        private MenuItem BuildAddFromSolutionMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            DatabasesMenuCommandsHandler dcmd, bool ver40IsInstalled, bool ver35IsInstalled)
        {
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
            return addFromSolutionMenuItem;
        }

        private static MenuItem BuildFixConnectionsMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            DatabasesMenuCommandsHandler dcmd)
        {
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
            return fixConnectionsMenuItem;
        }

        private MenuItem BuildDesignDatabaseMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            DatabasesMenuCommandsHandler dcmd)
        {
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
            return designDatabaseMenuItem;
        }

        private MenuItem BuildLocalDatabaseCacheMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            DatabasesMenuCommandsHandler dcmd, bool ver35IsInstalled)
        {
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
            return localDatabaseCacheMenuItem;
        }

        private MenuItem BuildVersionDetectMenuItem(DatabaseMenuCommandParameters databaseMenuCommandParameters,
            DatabasesMenuCommandsHandler dcmd)
        {
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
            return versionDetectMenuItem;
        }
    }
}