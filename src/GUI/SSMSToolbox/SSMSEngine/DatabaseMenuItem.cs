using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using ErikEJ.SqlCeScripting;
using ErikEJ.SqlCeToolbox.Commands;
using ErikEJ.SqlCeToolbox.Helpers;
using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;
using MenuItem = System.Windows.Controls.MenuItem;

namespace ErikEJ.SqlCeToolbox.SSMSEngine
{
    internal class DatabaseMenuItem : ToolsMenuItemBase, IWinformsMenuHandler
    {
        private readonly SqlCeToolboxPackage _package;
        private readonly SqlServerDatabaseMenuCommandsHandler _handler;

        public DatabaseMenuItem(SqlCeToolboxPackage package)
        {
            _package = package;
            _handler = new SqlServerDatabaseMenuCommandsHandler(_package);
            Text = "Script database...";
        }

        protected override void Invoke()
        {
        }
        
        public override object Clone() => new DatabaseMenuItem(_package);

        public ToolStripItem[] GetMenuItems()
        {
            var exportImage = SqlCeToolboxPackage.ExportImage;
            var logo = SqlCeToolboxPackage.Logo;
            var scriptImage =  SqlCeToolboxPackage.ScriptImage;

            var item = new ToolStripMenuItem("SQLite / SQL Server Compact Toolbox", logo);

            var scriptItem = BuildScriptMenuItem(scriptImage);

            var exportSqlCeItem = new ToolStripMenuItem("Export SQL Server to SQL Server Compact 4.0...", exportImage);
            exportSqlCeItem.Click += ExportSqlCeItem_Click;

            var exportSqliteItem = new ToolStripMenuItem("Export SQL Server to SQLite (beta)...", exportImage);
            exportSqliteItem.Click += ExportSqliteItem_Click;

            var aboutItem = new ToolStripMenuItem("Open Toolbox", logo);
            aboutItem.Click += AboutItem_Click;

            item.DropDownItems.Add(scriptItem);
            item.DropDownItems.Add(exportSqlCeItem);
            item.DropDownItems.Add(exportSqliteItem);
            item.DropDownItems.Add(aboutItem);

            return new ToolStripItem[] { item };
        }

        private void ExportSqliteItem_Click(object sender, EventArgs e)
        {
            try
            {
                var menuItem = BuildMenuItemForCommandHandler();
                _handler.ExportServerDatabaseToSqlite(menuItem, null);
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, DatabaseType.SQLServer, false);
            }
        }

        private void ExportSqlCeItem_Click(object sender, EventArgs e)
        {
            try
            {
                var menuItem = BuildMenuItemForCommandHandler();
                _handler.ExportServerDatabaseTo40(menuItem, null);
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, DatabaseType.SQLServer, false);
            }
        }

        private void AboutItem_Click(object sender, EventArgs e)
        {
            try
            {
                _package.ShowToolWindow(this, new EventArgs());
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, DatabaseType.SQLServer, false);
            }
        }

        void item_Click(object sender, EventArgs e)
        {
            var scope = Scope.DataOnly;
            var item = sender as ToolStripMenuItem;
            if (item != null)
            {
                scope = (Scope)item.Tag;
            }
            try
            {
                var menuItem = BuildMenuItemForCommandHandler();

                if (menuItem != null)
                    menuItem.Tag = scope;
                _handler.ScriptServerDatabase(menuItem, null);
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, DatabaseType.SQLServer, false);
            }
        }

        private MenuItem BuildMenuItemForCommandHandler()
        {
            var parent = Parent;
            if (parent?.Connection == null) return null;

            var builder = new SqlConnectionStringBuilder(parent.Connection.ConnectionString);
            builder.InitialCatalog = parent.InvariantName;

            var dbInfo = new DatabaseInfo
            {
                DatabaseType = DatabaseType.SQLServer,
                ConnectionString = builder.ConnectionString
            };

            var menuItem = new MenuItem
            {
                CommandParameter = new DatabaseMenuCommandParameters
                {
                    ExplorerControl = null,
                    DatabaseInfo = dbInfo
                }
            };
            return menuItem;
        }

        private ToolStripMenuItem BuildScriptMenuItem(System.Drawing.Bitmap scriptImage)
        { 
            var scriptItem = new ToolStripMenuItem("Script SQL Server Database", scriptImage);

            var scriptItem3 = new ToolStripMenuItem("Script SQL Server Database Schema (SQLite)...", scriptImage);
            scriptItem3.Tag = Scope.SchemaSQLite;
            scriptItem3.Click += item_Click;

            var scriptItem4 = new ToolStripMenuItem("Script SQL Server Database Schema and Data (SQLite)...", scriptImage);
            scriptItem4.Tag = Scope.SchemaDataSQLite;
            scriptItem4.Click += item_Click;

            var scriptItem0 = new ToolStripMenuItem("Script SQL Server Database Schema (SQLCE)...", scriptImage);
            scriptItem0.Tag = Scope.Schema;
            scriptItem0.Click += item_Click;

            var scriptItem1 = new ToolStripMenuItem("Script SQL Server Database Data (SQLCE)...", scriptImage);
            scriptItem1.Tag = Scope.DataOnly;
            scriptItem1.Click += item_Click;

            var scriptItem2 = new ToolStripMenuItem("Script SQL Server Database Schema and Data (SQLCE)...", scriptImage);
            scriptItem2.Tag = Scope.SchemaData;
            scriptItem2.Click += item_Click;

            var scriptItem5 = new ToolStripMenuItem("Script SQL Server Database Schema and Data with BLOBs (SQLCE)...", scriptImage);
            scriptItem5.Tag = Scope.SchemaDataBlobs;
            scriptItem5.Click += item_Click;

            scriptItem.DropDownItems.Add(scriptItem3);
            scriptItem.DropDownItems.Add(scriptItem4);
            scriptItem.DropDownItems.Add(scriptItem0);
            scriptItem.DropDownItems.Add(scriptItem1);
            scriptItem.DropDownItems.Add(scriptItem2);
            scriptItem.DropDownItems.Add(scriptItem5);
            return scriptItem;
        }
    }
}
