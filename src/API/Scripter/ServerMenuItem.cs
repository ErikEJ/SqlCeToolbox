using System;
using System.Windows.Forms;
using ErikEJ.SqlCeScripting;
using Microsoft.Data.ConnectionUI;
using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;

namespace SqlCeScripter
{
    internal class ServerMenuItem : ToolsMenuItemBase, IWinformsMenuHandler
    {
        internal ServerMenuItem()
        {
            this.Text = "Script server database...";
        }

        protected override void Invoke()
        {
            
        }
        
        public override object Clone()
        {
            return new DatabaseMenuItem();
        }


        #region IWinformsMenuHandler Members

        public System.Windows.Forms.ToolStripItem[] GetMenuItems()
        {
            ToolStripMenuItem item = new ToolStripMenuItem("Script Server Database");
            
            ToolStripMenuItem insertItem = new ToolStripMenuItem("Schema and Data...");
            insertItem.Tag = Scope.SchemaData;
            insertItem.Click += new EventHandler(item_Click);
            insertItem.ToolTipText = "Generate a SQL Compact compatible database script from SQL Server 2005/2008";

            ToolStripMenuItem insertItem1 = new ToolStripMenuItem("Schema and Data with BLOB files...");
            insertItem1.Tag = Scope.SchemaDataBlobs;
            insertItem1.Click += new EventHandler(item_Click);
            insertItem1.ToolTipText = "Generate a SQL Compact compatible database script from SQL Server 2005/2008";

            ToolStripMenuItem insertItem2 = new ToolStripMenuItem("Schema...");
            insertItem2.Tag = Scope.Schema;
            insertItem2.Click += new EventHandler(item_Click);
            insertItem2.ToolTipText = "Generate a SQL Compact compatible database script from SQL Server 2005/2008";

            item.DropDownItems.Add(insertItem);
            item.DropDownItems.Add(insertItem1);
            item.DropDownItems.Add(insertItem2);

            item.DropDownItems.Add(new ToolStripSeparator());

            ToolStripMenuItem aboutItem = new ToolStripMenuItem("About...");
            aboutItem.Image = Properties.Resources.data_out;
            aboutItem.Click += new EventHandler(AboutItem_Click);
            item.DropDownItems.Add(aboutItem);

            return new ToolStripItem[] { item };

        }

        #endregion

        void AboutItem_Click(object sender, EventArgs e)
        {
            new AboutDlg().ShowDialog();
        }

        void item_Click(object sender, EventArgs e)
        {
            try
            {
                DataSource sqlDataSource = new DataSource("MicrosoftSqlServer", "Microsoft SQL Server");
                sqlDataSource.Providers.Add(DataProvider.SqlDataProvider);
                DataConnectionDialog dcd = new DataConnectionDialog();
                dcd.DataSources.Add(sqlDataSource);
                dcd.SelectedDataProvider = DataProvider.SqlDataProvider;
                dcd.SelectedDataSource = sqlDataSource;
                if (DataConnectionDialog.Show(dcd) == DialogResult.OK)
                {
                    string connectionString = dcd.ConnectionString;
                    string fileName;

                    ToolStripMenuItem item = (ToolStripMenuItem)sender;
                    Scope scope = (Scope)item.Tag;

                    SaveFileDialog fd = new SaveFileDialog();
                    fd.AutoUpgradeEnabled = true;
                    fd.Title = "Save generated database script as";
                    fd.Filter = "SQL Server Compact Script (*.sqlce)|*.sqlce|SQL Server Script (*.sql)|*.sql|All Files(*.*)|";
                    fd.OverwritePrompt = true;
                    fd.ValidateNames = true;
                    if (fd.ShowDialog() == DialogResult.OK)
                    {
                        fileName = fd.FileName;
                        using (IRepository repository = new ServerDBRepository(connectionString))
                        {
                            var generator = new Generator(repository, fd.FileName);
                            System.Windows.Forms.MessageBox.Show(generator.ScriptDatabaseToFile(scope));
                        }
                   }
                }
            }
            catch (System.Data.SqlClient.SqlException sql)
            {
                Connect.ShowErrors(sql);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }
    }
}
