using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using ErikEJ.SqlCeScripting;
using ErikEJ.SqlCeToolbox.Commands;
using ErikEJ.SqlCeToolbox.Helpers;
using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;
using MenuItem = System.Windows.Controls.MenuItem;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.SqlServer.Management.UI.VSIntegration.Editors;
using EnvDTE;

namespace ErikEJ.SqlCeToolbox.SSMSEngine
{
    internal class TableMenuItem : ToolsMenuItemBase, IWinformsMenuHandler
    {
        private readonly SqlCeToolboxPackage _package;
        private readonly BaseCommandHandler _handler;

        public TableMenuItem(SqlCeToolboxPackage package)
        {
            _package = package;
            _handler = new BaseCommandHandler();
            Text = "Script table...";
        }

        protected override void Invoke()
        {
        }
        
        public override object Clone() => new TableMenuItem(_package);

        public ToolStripItem[] GetMenuItems()
        {
            var exportImage = SqlCeToolboxPackage.ExportImage;
            var logo = SqlCeToolboxPackage.Logo;
            var scriptImage =  SqlCeToolboxPackage.ScriptImage;

            var item = new ToolStripMenuItem("SQLite / SQL Server Compact Toolbox", logo);

            var scriptItem = BuildScriptMenuItem(scriptImage);

            var aboutItem = new ToolStripMenuItem("Open Toolbox", logo);
            aboutItem.Click += AboutItem_Click;

            item.DropDownItems.Add(scriptItem);
            item.DropDownItems.Add(aboutItem);

            return new ToolStripItem[] { item };
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
            try
            {
                var menuItem = BuildMenuItemForCommandHandler();

                if (menuItem == null)
                    return;

                var menuInfo = menuItem.CommandParameter as MenuCommandParameters;

                using (IRepository repository = new ServerDBRepository(menuInfo.DatabaseInfo.ConnectionString, true))
                {
                    var generator = DataConnectionHelper.CreateGenerator(repository, menuInfo.DatabaseInfo.DatabaseType);
                    generator.GenerateTableContent(menuItem.Tag as string, false, Properties.Settings.Default.IgnoreIdentityInInsertScript);
                    if (!Properties.Settings.Default.IgnoreIdentityInInsertScript)
                    {
                        generator.GenerateIdentityReset(menuItem.Tag as string, false);
                    }
                    ScriptFactory.Instance.CreateNewBlankScript(ScriptType.Sql);
                    var dte = _package.GetServiceHelper(typeof(DTE)) as DTE;
                    if (dte != null)
                    {
                        var doc = (TextDocument)dte.Application.ActiveDocument.Object(null);
                        doc.EndPoint.CreateEditPoint().Insert(generator.GeneratedScript);
                        doc.DTE.ActiveDocument.Saved = true;
                    }
                    DataConnectionHelper.LogUsage("TableScriptAsData");
                }
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, DatabaseType.SQLServer, false);
            }
        }

        private MenuItem BuildMenuItemForCommandHandler()
        {
            var parent = Parent?.Parent;
            if (parent?.Connection == null) return null;

            var builder = new SqlConnectionStringBuilder(parent.Connection.ConnectionString);
            builder.InitialCatalog = parent.Name;

            var dbInfo = new DatabaseInfo
            {
                DatabaseType = DatabaseType.SQLServer,
                ConnectionString = builder.ConnectionString
            };

            var menuItem = new MenuItem
            {
                CommandParameter = new MenuCommandParameters
                {
                    DatabaseInfo = dbInfo
                },
                Tag = Parent.InvariantName
            };
            return menuItem;
        }

        private ToolStripMenuItem BuildScriptMenuItem(System.Drawing.Bitmap scriptImage)
        { 
            var scriptItem = new ToolStripMenuItem("Script table", scriptImage);

            var scriptItem1 = new ToolStripMenuItem("Script table data (SQLCE)...", scriptImage);
            scriptItem1.Tag = Scope.DataOnly;
            scriptItem1.Click += item_Click;

            scriptItem.DropDownItems.Add(scriptItem1);
            return scriptItem;
        }
    }
}
