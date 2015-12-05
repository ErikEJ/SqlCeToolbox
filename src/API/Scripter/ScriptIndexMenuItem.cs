using System;
using System.Windows.Forms;
using ErikEJ.SqlCeScripting;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;

namespace SqlCeScripter
{
    internal class ScriptIndexMenuItem : ToolsMenuItemBase, IWinformsMenuHandler
    {
        internal ScriptIndexMenuItem()
        {
            this.Text = "Script";
        }

        protected override void Invoke()
        {
            
        }
        
        public override object Clone()
        {
            return new ScriptIndexMenuItem();
        }

        private enum Action
        {
            Create,
            Drop,
            DropAndCreate,
            Statistics
        }

        private enum Output
        {
            Editor,
            File,
            Clipboard
        }

        private ToolStripMenuItem AboutItem()
        {
            ToolStripMenuItem aboutItem = new ToolStripMenuItem("About...");
            aboutItem.Image = Properties.Resources.data_out;
            aboutItem.Click += new EventHandler(AboutItem_Click);
            return aboutItem;
        }

        private ToolStripMenuItem ScriptItem()
        {
            ToolStripMenuItem scriptItem = new ToolStripMenuItem("New Query Editor Window");
            scriptItem.Tag = Output.Editor;
            scriptItem.Click += new EventHandler(item_Click);
            return scriptItem;
        }

        private ToolStripMenuItem FileItem()
        {
            ToolStripMenuItem fileItem = new ToolStripMenuItem("File...");
            fileItem.Tag = Output.File;
            fileItem.Click += new EventHandler(item_Click);
            return fileItem;
        }

        private ToolStripMenuItem ClipboardItem()
        {
            ToolStripMenuItem clipboardItem = new ToolStripMenuItem("Clipboard");
            clipboardItem.Tag = Output.Clipboard;
            clipboardItem.Click += new EventHandler(item_Click);
            return clipboardItem;
        }

        #region IWinformsMenuHandler Members


        public System.Windows.Forms.ToolStripItem[] GetMenuItems()
        {
            ToolStripMenuItem item = new ToolStripMenuItem("Script Index as");

            ToolStripMenuItem createItem = new ToolStripMenuItem("CREATE To");
            createItem.Tag = Action.Create;
            createItem.DropDownItems.AddRange( 
                new ToolStripItem[] { ScriptItem(), new ToolStripSeparator(), FileItem(), ClipboardItem(), new ToolStripSeparator(), AboutItem() });

            ToolStripMenuItem dropItem = new ToolStripMenuItem("DROP To");
            dropItem.Tag = Action.Drop;
            dropItem.DropDownItems.AddRange(
                new ToolStripItem[] { ScriptItem(), new ToolStripSeparator(), FileItem(), ClipboardItem(), new ToolStripSeparator(), AboutItem() });

            ToolStripMenuItem dropCreateItem = new ToolStripMenuItem("DROP And CREATE To");
            dropCreateItem.Tag = Action.DropAndCreate;
            dropCreateItem.DropDownItems.AddRange(
                new ToolStripItem[] { ScriptItem(), new ToolStripSeparator(), FileItem(), ClipboardItem(), new ToolStripSeparator(), AboutItem() });

            ToolStripMenuItem statsItem = new ToolStripMenuItem("Statistics To");
            statsItem.Tag = Action.Statistics;
            statsItem.DropDownItems.AddRange(
                new ToolStripItem[] { ScriptItem(), new ToolStripSeparator(), FileItem(), ClipboardItem(), new ToolStripSeparator(), AboutItem() });

            item.DropDownItems.Add(createItem);
            item.DropDownItems.Add(dropItem);
            item.DropDownItems.Add(dropCreateItem);
            item.DropDownItems.Add(new ToolStripSeparator());
            item.DropDownItems.Add(statsItem);
            return new ToolStripItem[] { item };

        }

        #endregion


        void AboutItem_Click(object sender, EventArgs e)
        {
            new AboutDlg().ShowDialog();
        }

        void item_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;

            string fileName = string.Empty;

            Action action = (Action)item.OwnerItem.Tag;
            Output output = (Output)item.Tag;

            try
            {
                string connectionString = Helper.FixConnectionString(this.Parent.Connection.ConnectionString, this.Parent.Connection.ConnectionTimeout);
                using (IRepository repository = new DBRepository(connectionString))
                {
                    var generator = new Generator(repository);

                    switch (output)
                    {
                        case Output.Editor:
                            // create new document
                            ServiceCache.ScriptFactory.CreateNewBlankScript(Microsoft.SqlServer.Management.UI.VSIntegration.Editors.ScriptType.SqlCe);
                            break;
                        case Output.File:
                            SaveFileDialog fd = new SaveFileDialog();
                            fd.AutoUpgradeEnabled = true;
                            fd.Title = "Save generated script as";
                            fd.Filter = "SQL Server Compact Script (*.sqlce)|*.sqlce|SQL Server Script (*.sql)|*.sql|All Files(*.*)|";
                            fd.OverwritePrompt = true;
                            fd.ValidateNames = true;
                            if (fd.ShowDialog() == DialogResult.OK)
                            {
                                fileName = fd.FileName;
                            }
                            break;
                        default:
                            break;
                    }

                    switch (action)
                    {
                        case Action.Create:
                            generator.GenerateIndexScript(this.Parent.Parent.Name, this.Parent.Name);
                            break;
                        case Action.Drop:
                            generator.GenerateIndexDrop(this.Parent.Parent.Name, this.Parent.Name);
                            break;
                        case Action.DropAndCreate:
                            generator.GenerateIndexDrop(this.Parent.Parent.Name, this.Parent.Name);
                            generator.GenerateIndexScript(this.Parent.Parent.Name, this.Parent.Name);
                            break;
                        case Action.Statistics:
                            generator.GenerateIndexStatistics(this.Parent.Parent.Name, this.Parent.Name);
                            break;
                        default:
                            break;
                    }
                    switch (output)
                    {
                        case Output.Editor:
                            // insert SQL script to document
                            EnvDTE.TextDocument doc = (EnvDTE.TextDocument)ServiceCache.ExtensibilityModel.Application.ActiveDocument.Object(null);
                            doc.EndPoint.CreateEditPoint().Insert(generator.GeneratedScript);
                            doc.DTE.ActiveDocument.Saved = true;
                            break;
                        case Output.File:
                            if (!string.IsNullOrEmpty(fileName))
                            {
                                System.IO.File.WriteAllText(fileName, generator.GeneratedScript);
                            }
                            break;
                        case Output.Clipboard:
                            Clipboard.Clear();
                            Clipboard.SetText(generator.GeneratedScript, TextDataFormat.UnicodeText);
                            break;
                        default:
                            break;
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
