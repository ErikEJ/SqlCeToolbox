using System;
using System.Windows.Forms;
using ErikEJ.SqlCeScripting;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;
using Kent.Boogaart.KBCsv;

namespace SqlCeScripter
{
    internal class ImportTableMenuItem : ToolsMenuItemBase, IWinformsMenuHandler
    {
        internal ImportTableMenuItem()
        {
            this.Text = "Import";
        }

        protected override void Invoke()
        {
            
        }
        
        public override object Clone()
        {
            return new ImportTableMenuItem();
        }

        private enum Action
        {
            Csv
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
            ToolStripMenuItem item = new ToolStripMenuItem("Import Data from");

            ToolStripMenuItem impCsvItem = new ToolStripMenuItem("CSV To");
            impCsvItem.Tag = Action.Csv;
            impCsvItem.DropDownItems.AddRange( 
                new ToolStripItem[] { ScriptItem(), new ToolStripSeparator(), FileItem(), ClipboardItem(), new ToolStripSeparator(), AboutItem() });

            item.DropDownItems.Add(impCsvItem);
            return new ToolStripItem[] { item };
        }

        #endregion


        void AboutItem_Click(object sender, EventArgs e)
        {
            using (AboutDlg about = new AboutDlg())
            {
                about.ShowDialog();
            }
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

                    using (ImportOptions imo = new ImportOptions(this.Parent.Name))
                    {
                        imo.SampleHeader = generator.GenerateTableColumns(this.Parent.Name);
                        imo.Separator = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator.ToCharArray()[0];
                        
                        if (imo.ShowDialog() == DialogResult.OK)
                        {
                            switch (output)
                            {
                                case Output.Editor:
                                    // create new document
                                    ServiceCache.ScriptFactory.CreateNewBlankScript(Microsoft.SqlServer.Management.UI.VSIntegration.Editors.ScriptType.SqlCe);
                                    break;
                                case Output.File:
                                    SaveFileDialog fd = new SaveFileDialog();
                                    fd.AutoUpgradeEnabled = true;
                                    fd.Title = "Save generated database script as";
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
                                case Action.Csv:
                                    using (var reader = new CsvReader(imo.FileName))
                                    {
                                        reader.ValueSeparator = imo.Separator;
                                        HeaderRecord hr = reader.ReadHeaderRecord();
                                        if (generator.ValidColumns(this.Parent.Name, hr.Values))
                                        {
                                            int i = 1;
                                            foreach (DataRecord record in reader.DataRecords)
                                            {
                                                generator.GenerateTableInsert(this.Parent.Name, hr.Values, record.Values, i);
                                                i++;
                                            }
                                        }
                                    }                                
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
