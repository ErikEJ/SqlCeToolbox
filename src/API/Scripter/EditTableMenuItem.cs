using System;
using System.Windows.Forms;
using ErikEJ.SqlCeScripting;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;
using Microsoft.SqlServer.Management.SqlStudio.Explorer;
using EnvDTE80;

namespace SqlCeScripter
{
    internal class EditTableMenuItem : ToolsMenuItemBase, IWinformsMenuHandler
    {
        internal EditTableMenuItem()
        {
            this.Text = "Edit";
        }

        protected override void Invoke()
        {
            
        }
        
        public override object Clone()
        {
            return new RenameTableMenuItem();
        }

        #region IWinformsMenuHandler Members


        public System.Windows.Forms.ToolStripItem[] GetMenuItems()
        {
            ToolStripMenuItem item = new ToolStripMenuItem("Show Table Data");
            item.Click += new EventHandler(item_Click);
            return new ToolStripItem[] { item };
        }

        #endregion


        void item_Click(object sender, EventArgs e)
        {
            if (Connect.CurrentAddin == null)
                return;

            if (Connect.CurrentApplication == null)
                return;

            Connect.CurrentTable = this.Parent.Name;
            Connect.ConnectionString = Helper.FixConnectionString(this.Parent.Connection.ConnectionString, this.Parent.Connection.ConnectionTimeout);

            try
            {

                if (!Connect.EditTracked)
                {
                    Connect.EditTracked = true;
                }

                Windows2 windows2 = Connect.CurrentApplication.Windows as Windows2;

                if (windows2 != null)
                {
                    object controlObject = null;
                    System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();

                    string dbName = System.IO.Path.GetFileNameWithoutExtension(this.Parent.Connection.ServerName);

                    EnvDTE.Window toolWindow = windows2.CreateToolWindow2(Connect.CurrentAddin,
                                                                   asm.Location,
                                                                   "SqlCeScripter.ResultsetGrid",
                                                                   string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0} - {1}", dbName, this.Parent.Name), 
                                                                   "{" + Guid.NewGuid().ToString() + "}",
                                                                   ref controlObject);

                    if (toolWindow != null)
                    {
                        toolWindow.IsFloating = false;
                        toolWindow.Linkable = false;
                        toolWindow.Visible = true;
                    }
                }
            }
            
            catch (System.Data.SqlServerCe.SqlCeException sqlCe)
            {
                Connect.ShowErrors(sqlCe);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

    }
}
