using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using ErikEJ.SqlCeScripting;
using ErikEJ.SqlCeToolbox.Dialogs;
using ErikEJ.SqlCeToolbox.ToolWindows;
using FabTab;
using Kent.Boogaart.KBCsv;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using ErikEJ.SqlCeToolbox.Helpers;

namespace ErikEJ.SqlCeToolbox.Commands
{
    public class BaseCommandHandler
    {
        private ExplorerControl _parent;
        public BaseCommandHandler(ExplorerControl parent)
        {
            _parent = parent;
        }

        public void ScriptAsCreate(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var menuInfo = ValidateMenuInfo(sender);
                if (menuInfo != null)
                {
                    using (IRepository repository = RepoHelper.CreateRepository(menuInfo.Connectionstring))
                    {
                        var generator = RepoHelper.CreateGenerator(repository);
                        generator.GenerateTableScript(menuInfo.Name);
                        OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(DataConnectionHelper.ShowErrors(ex));
            }
        }

        public void ScriptAsDrop(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var menuInfo = ValidateMenuInfo(sender);
                if (menuInfo != null)
                {
                    using (IRepository repository = RepoHelper.CreateRepository(menuInfo.Connectionstring))
                    {
                        var generator = RepoHelper.CreateGenerator(repository);
                        generator.GenerateTableDrop(menuInfo.Name);
                        OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(DataConnectionHelper.ShowErrors(ex));
            }
        }

        public void ScriptAsDropAndCreate(object sender, ExecutedRoutedEventArgs e)
        {
            var menuInfo = ValidateMenuInfo(sender);
            if (menuInfo != null)
            {
                try
                {
                    using (IRepository repository = RepoHelper.CreateRepository(menuInfo.Connectionstring))
                    {
                        var generator = RepoHelper.CreateGenerator(repository);
                        generator.GenerateTableDrop(menuInfo.Name);
                        generator.GenerateTableScript(menuInfo.Name);
                        OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(DataConnectionHelper.ShowErrors(ex));
                }
            }
        }

        public void ScriptAsSelect(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var menuInfo = ValidateMenuInfo(sender);
                if (menuInfo != null)
                {
                    using (IRepository repository = RepoHelper.CreateRepository(menuInfo.Connectionstring))
                    {
                        var generator = RepoHelper.CreateGenerator(repository);
                        generator.GenerateTableSelect(menuInfo.Name);
                        OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(DataConnectionHelper.ShowErrors(ex));
            }
        }

        public void ScriptAsInsert(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var menuInfo = ValidateMenuInfo(sender);
                if (menuInfo != null)
                {
                    using (IRepository repository = RepoHelper.CreateRepository(menuInfo.Connectionstring))
                    {
                        var generator = RepoHelper.CreateGenerator(repository);
                        generator.GenerateTableInsert(menuInfo.Name);
                        OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(DataConnectionHelper.ShowErrors(ex));
            }
        }

        public void ScriptAsUpdate(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var menuInfo = ValidateMenuInfo(sender);
                if (menuInfo != null)
                {
                    using (IRepository repository = RepoHelper.CreateRepository(menuInfo.Connectionstring))
                    {
                        var generator = RepoHelper.CreateGenerator(repository);
                        generator.GenerateTableUpdate(menuInfo.Name);
                        OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(DataConnectionHelper.ShowErrors(ex));
            }
        }

        public void ScriptAsDelete(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var menuInfo = ValidateMenuInfo(sender);
                if (menuInfo != null)
                {
                    using (IRepository repository = RepoHelper.CreateRepository(menuInfo.Connectionstring))
                    {
                        var generator = RepoHelper.CreateGenerator(repository);
                        generator.GenerateTableDelete(menuInfo.Name);
                        OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(DataConnectionHelper.ShowErrors(ex));
            }
        }

        public void ScriptAsData(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var menuInfo = ValidateMenuInfo(sender);
                if (menuInfo != null)
                {
                    using (IRepository repository = RepoHelper.CreateRepository(menuInfo.Connectionstring))
                    {
                        var generator = RepoHelper.CreateGenerator(repository);
                        generator.GenerateTableContent(menuInfo.Name, false, Properties.Settings.Default.IgnoreIdentityInInsertScript);
                        OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(DataConnectionHelper.ShowErrors(ex));
            }
        }

        public void ImportData(object sender, ExecutedRoutedEventArgs e)
        {
            var menuInfo = ValidateMenuInfo(sender);
            if (menuInfo != null)
            {
                try
                {
                    using (IRepository repository = RepoHelper.CreateRepository(menuInfo.Connectionstring))
                    {
                        var generator = RepoHelper.CreateGenerator(repository, string.Empty);

                        ImportDialog imo = new ImportDialog();

                        imo.SampleHeader = generator.GenerateTableColumns(menuInfo.Name);
                        imo.Separator = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator.ToCharArray()[0];
                        imo.Owner = Application.Current.MainWindow;
                        if (imo.ShowDialog() == true)
                        {
                            using (var reader = new CsvReader(imo.File, System.Text.Encoding.UTF8))
                            {
                                reader.ValueSeparator = imo.Separator;
                                HeaderRecord hr = reader.ReadHeaderRecord();
                                if (generator.ValidColumns(menuInfo.Name, hr.Values))
                                {
                                    int i = 1;
                                    foreach (DataRecord record in reader.DataRecords)
                                    {
                                        generator.GenerateTableInsert(menuInfo.Name, hr.Values, record.Values, i);
                                        i++;
                                    }
                                }
                            }
                            OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(DataConnectionHelper.ShowErrors(ex));
                }
            }
        }

        public void Rename(object sender, ExecutedRoutedEventArgs e)
        {
            var menuInfo = ValidateMenuInfo(sender);
            if (menuInfo != null)
            {
                try
                {
                    using (IRepository repository = RepoHelper.CreateRepository(menuInfo.Connectionstring))
                    {
                        RenameDialog ro = new RenameDialog(menuInfo.Name);
                        ro.Owner = Application.Current.MainWindow;
                        ro.ShowDialog();
                        if (ro.DialogResult.HasValue && ro.DialogResult.Value && !string.IsNullOrWhiteSpace(ro.NewName))
                        {
                            repository.RenameTable(menuInfo.Name, ro.NewName);
                            if (_parent != null)
                            {
                                _parent.BuildDatabaseTree();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(DataConnectionHelper.ShowErrors(ex));
                }
            }
        }

        public void AddDescription(object sender, ExecutedRoutedEventArgs e)
        {
            var menuInfo = ValidateMenuInfo(sender);
            if (menuInfo != null)
            {
                try
                {
                    var desc = ExplorerControl.DescriptionCache.Where(d => d.Object == menuInfo.Name && d.Parent == null).Select(d => d.Description).SingleOrDefault();
                    DescriptionDialog ro = new DescriptionDialog(desc);
                    ro.Owner = Application.Current.MainWindow;
                    ro.ShowDialog();
                    if (ro.DialogResult.HasValue && ro.DialogResult.Value && !string.IsNullOrWhiteSpace(ro.Description) && ro.Description != desc)
                    {
                        new Helpers.DescriptionHelper().SaveDescription(menuInfo.Connectionstring, ExplorerControl.DescriptionCache, ro.Description, null, menuInfo.Name);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(DataConnectionHelper.ShowErrors(ex));
                }
            }
        }

        public void OpenSqlEditorToolWindow(MenuCommandParameters menuInfo, string script)
        {
            SqlEditorControl editor = new SqlEditorControl();
            editor.Database = menuInfo.Connectionstring;
            editor.SqlText = script;
            FabTabItem tab = new FabTabItem();
            tab.Content = editor;
            string tabTitle = System.IO.Path.GetFileNameWithoutExtension(menuInfo.Caption) + "-" + menuInfo.Name;
            tab.Header = tabTitle;

            int i = -1;
            int insertAt = -1;
            foreach (var item in _parent.FabTab.Items)
            {
                i++;
                if (item is FabTabItem)
                {
                    FabTabItem ftItem = (FabTabItem)item;
                    if (ftItem.Header.ToString().StartsWith(tabTitle))
                    {
                        insertAt = i;
                    }
                }
            }
            if (insertAt > -1)
            {
                _parent.FabTab.Items.Insert(insertAt + 1, tab);
                if (_parent.FabTab.Items.Count == 3)
                    insertAt = insertAt + 1;
                _parent.FabTab.SelectedIndex = insertAt + 1;
            }
            else
            {
                _parent.FabTab.Items.Add(tab);
                _parent.FabTab.SelectedIndex = _parent.FabTab.Items.Count - 1;
            }
        }

        public void SpawnDataEditorWindow(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var menuItem = sender as MenuItem;
                if (menuItem == null)
                    return;
                var menuInfo = menuItem.CommandParameter as MenuCommandParameters;
                if (menuInfo == null)
                    return;

                WindowsFormsHost wh = new WindowsFormsHost();
                ResultsetGrid rg = new ResultsetGrid();
                List<int> readOnlyColumns = new List<int>();

                using (IRepository repository = RepoHelper.CreateRepository(menuInfo.Connectionstring))
                {
                    var tpks = repository.GetAllPrimaryKeys().Where(pk => pk.TableName == menuInfo.Name).ToList();
                    if (tpks.Count == 0)
                    {
                        rg.ReadOnly = true;
                    }
                    List<Column> cols = repository.GetAllColumns();
                    cols = cols.Where(c => c.TableName == menuInfo.Name).ToList();
                    int x = 0;
                    foreach (Column col in cols)
                    {
                        if (col.AutoIncrementBy > 0 || col.RowGuidCol)
                        {
                            readOnlyColumns.Add(x);
                        }
                        x++;
                    }
                }
                var sqlText = string.Format(Environment.NewLine + "SELECT TOP({0}) * FROM [{1}]", Properties.Settings.Default.MaxRowsToEdit, menuInfo.Name);
                rg.TableName = sqlText;
                rg.ConnectionString = menuInfo.Connectionstring;
                rg.Tag = wh;
                rg.ReadOnlyColumns = readOnlyColumns;
                wh.Child = rg;
                
                string tabTitle = System.IO.Path.GetFileNameWithoutExtension(menuInfo.Caption) + "-" + menuInfo.Name + "-Edit";
                if (rg.ReadOnly)
                    tabTitle = System.IO.Path.GetFileNameWithoutExtension(menuInfo.Caption) + "-" + menuInfo.Name + "-ReadOnly";
                bool alreadyThere = false;
                int i = -1;
                foreach (var item in _parent.FabTab.Items)
                {
                    i++;
                    if (item is FabTabItem)
                    {
                        FabTabItem ftItem = (FabTabItem)item;
                        if (ftItem.Header.ToString() == tabTitle)
                        {
                            alreadyThere = true;
                        }
                    }
                }
                if (alreadyThere)
                {
                    _parent.FabTab.SelectedIndex = i;
                    _parent.FabTab.Focus();
                }
                else
                {
                    FabTabItem tab = new FabTabItem();
                    tab.Content = wh;
                    tab.Header = tabTitle;
                    _parent.FabTab.Items.Add(tab);
                    _parent.FabTab.SelectedIndex = _parent.FabTab.Items.Count - 1;
                    rg.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(DataConnectionHelper.ShowErrors(ex));
            } 
        }

        public void SpawnReportViewerWindow(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var menuItem = sender as MenuItem;
                if (menuItem == null)
                    return;
                var menuInfo = menuItem.CommandParameter as MenuCommandParameters;
                if (menuInfo == null)
                    return;

                WindowsFormsHost wh = new WindowsFormsHost();
                ReportGrid rg = new ReportGrid();

                DataSet ds;

                using (IRepository repository = RepoHelper.CreateRepository(menuInfo.Connectionstring))
                {
                    var generator = RepoHelper.CreateGenerator(repository);
                    generator.GenerateTableSelect(menuInfo.Name);
                    var sqlText = generator.GeneratedScript;
                    ds = repository.ExecuteSql(sqlText);                
                }
                rg.DataSet = ds;
                rg.TableName = menuInfo.Name;
                wh.Child = rg;

                string tabTitle = System.IO.Path.GetFileNameWithoutExtension(menuInfo.Caption) + "-" + menuInfo.Name + "-Report";
                bool alreadyThere = false;
                int i = -1;
                foreach (var item in _parent.FabTab.Items)
                {
                    i++;
                    if (item is FabTabItem)
                    {
                        FabTabItem ftItem = (FabTabItem)item;
                        if (ftItem.Header.ToString() == tabTitle)
                        {
                            alreadyThere = true;
                        }
                    }
                }
                if (alreadyThere)
                {
                    _parent.FabTab.SelectedIndex = i;
                    _parent.FabTab.Focus();
                }
                else
                {
                    FabTabItem tab = new FabTabItem();
                    tab.Content = wh;
                    tab.Header = tabTitle;
                    _parent.FabTab.Items.Add(tab);
                    _parent.FabTab.SelectedIndex = _parent.FabTab.Items.Count - 1;
                    rg.Focus();
                }
            }
            catch (System.IO.FileNotFoundException)
            {
                MessageBox.Show("Microsoft Report Viewer 2010 not installed, please download and install to use this feature  http://www.microsoft.com/en-us/download/details.aspx?id=6442");
            }
            catch (Exception ex)
            {
                MessageBox.Show(DataConnectionHelper.ShowErrors(ex));
            }
        }

        private static MenuCommandParameters ValidateMenuInfo(object sender)
        {
            var menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                return menuItem.CommandParameter as MenuCommandParameters;
            }
            else
            {
                return null;
            }
        }        
    }
}