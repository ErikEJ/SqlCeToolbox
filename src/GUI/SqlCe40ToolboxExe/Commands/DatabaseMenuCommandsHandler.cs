using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ErikEJ.SqlCeScripting;
using ErikEJ.SqlCeToolbox.Dialogs;
using ErikEJ.SqlCeToolbox.ToolWindows;
using FabTab;
using Microsoft.Data.ConnectionUI;
using Microsoft.Win32;
using System.ComponentModel;
using System.Reflection;

namespace ErikEJ.SqlCeToolbox.Commands
{
    public class DatabaseMenuCommandsHandler
    {
        private readonly ExplorerControl _parentWindow;
#if V35
        private SqlCeHelper helper = new SqlCeHelper();
#else
        private SqlCeHelper4 helper = new SqlCeHelper4();
#endif
        public DatabaseMenuCommandsHandler(ExplorerControl parent)
        {
            _parentWindow = parent;
        }

        public void AddCeDatabase(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                string connectionString = string.Empty;
#if V35
                var dialog = new Connection35Dialog();
#else
                var dialog = new ConnectionDialog();
#endif
                dialog.Owner = Application.Current.MainWindow;
                bool? result = dialog.ShowDialog();
                if (result.HasValue && result.Value == true)
                {
                    if (!string.IsNullOrWhiteSpace(dialog.ConnectionString))
                    {
                        Helpers.DataConnectionHelper.SaveDataConnection(dialog.ConnectionString);
                        _parentWindow.BuildDatabaseTree();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Helpers.DataConnectionHelper.ShowErrors(ex));
            }
        }

        public void RemoveCeDatabase(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;
            
            try
            {
                Helpers.DataConnectionHelper.RemoveDataConnection(databaseInfo.Connectionstring);
                _parentWindow.BuildDatabaseTree();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Helpers.DataConnectionHelper.ShowErrors(ex));
            }
        }

        public void ScriptServerDatabase(object sender, ExecutedRoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null) return;

            Scope scope = (Scope)menuItem.Tag;

            var databaseInfo = menuItem.CommandParameter as DatabasesMenuCommandParameters;
            if (databaseInfo == null) return;            
            var treeViewItem = databaseInfo.DatabasesTreeViewItem;

            try
            {
                DataSource sqlDataSource = new DataSource("MicrosoftSqlServer", "Microsoft SQL Server");
                sqlDataSource.Providers.Add(DataProvider.SqlDataProvider);
                DataConnectionDialog dcd = new DataConnectionDialog();
                dcd.DataSources.Add(sqlDataSource);
                dcd.SelectedDataProvider = DataProvider.SqlDataProvider;
                dcd.SelectedDataSource = sqlDataSource;
                if (DataConnectionDialog.Show(dcd) == System.Windows.Forms.DialogResult.OK)
                {
                    string connectionString = dcd.ConnectionString;
                    string fileName;

                    PickTablesDialog ptd = new PickTablesDialog();
                    int totalCount = 0;
                    using (IRepository repository = RepoHelper.CreateServerRepository(dcd.ConnectionString))
                    {
                        ptd.Tables = repository.GetAllTableNamesForExclusion();
                        totalCount = ptd.Tables.Count;
                    }
                    ptd.Owner = Application.Current.MainWindow;
                    bool? res = ptd.ShowDialog();
                    if (res.HasValue && res.Value == true && (ptd.Tables.Count < totalCount))
                    {
                        SaveFileDialog fd = new SaveFileDialog();
                        fd.Title = "Save generated database script as";
                        fd.Filter = "SQL Server Compact Script (*.sqlce)|*.sqlce|SQL Server Script (*.sql)|*.sql|All Files(*.*)|*.*";
                        fd.OverwritePrompt = true;
                        fd.ValidateNames = true;
                        bool? result = fd.ShowDialog();
                        if (result.HasValue && result.Value == true)
                        {
                            fileName = fd.FileName;
                            using (IRepository repository = RepoHelper.CreateServerRepository(connectionString))
                            {
                                var generator = RepoHelper.CreateGenerator(repository, fd.FileName);
                                generator.ExcludeTables(ptd.Tables);
                                System.Windows.Forms.MessageBox.Show(generator.ScriptDatabaseToFile(scope));
                            }
                        }
                    }
                }
                dcd.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Helpers.DataConnectionHelper.ShowErrors(ex));
            }
        }

        public void ScriptDatabase(object sender, ExecutedRoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null) return;

            var databaseInfo = menuItem.CommandParameter as DatabaseMenuCommandParameters;
            if (databaseInfo == null) return;            

            Scope scope = (Scope)menuItem.Tag;

            SaveFileDialog fd = new SaveFileDialog();
            fd.Title = "Save generated database script as";
            fd.Filter = "SQL Server Compact Script (*.sqlce)|*.sqlce|SQL Server Script (*.sql)|*.sql|All Files(*.*)|*.*";
            fd.OverwritePrompt = true;
            fd.ValidateNames = true;
            bool? result = fd.ShowDialog();
            if (result.HasValue && result.Value == true)
            {
                var fileName = fd.FileName;
                                int totalCount = 0;
                PickTablesDialog ptd = new PickTablesDialog();
                using (IRepository repository = RepoHelper.CreateRepository(databaseInfo.Connectionstring))
                {
                    ptd.Tables = repository.GetAllTableNamesForExclusion();
                    totalCount = ptd.Tables.Count;
                }
                ptd.Owner = Application.Current.MainWindow;
                bool? res = ptd.ShowDialog();
                if (res.HasValue && res.Value == true && (ptd.Tables.Count < totalCount))
                {
                    try
                    {
                        using (IRepository repository = RepoHelper.CreateRepository(databaseInfo.Connectionstring))
                        {
                            var generator = RepoHelper.CreateGenerator(repository, fd.FileName);
                            generator.ExcludeTables(ptd.Tables);
                            System.Windows.Forms.MessageBox.Show(generator.ScriptDatabaseToFile(scope));
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(Helpers.DataConnectionHelper.ShowErrors(ex));
                    }
                }
            }
        }

        public void GenerateDiffScript(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;

            try
            {
                SortedDictionary<string, string> databaseList = Helpers.DataConnectionHelper.GetDataConnections();
                foreach (KeyValuePair<string, string> info in databaseList)
                {
                    if (!databaseList.ContainsKey(info.Key))
                        databaseList.Add(info.Key, info.Value);
                }

                CompareDialog cd = new CompareDialog(databaseInfo.Caption, databaseList);

                bool? result = cd.ShowDialog();
                if (result.HasValue && result.Value == true && (cd.TargetDatabase.Key != null))
                {
                    var target = cd.TargetDatabase.Value;
                    var source = databaseInfo.Connectionstring;

                    var editorTarget = target;
                    using (IRepository sourceRepository = RepoHelper.CreateRepository(source))
                    {
                        var generator = RepoHelper.CreateGenerator(sourceRepository);
                        using (IRepository targetRepository = RepoHelper.CreateRepository(target))
                        {
                            try
                            {
                                SqlCeDiff.CreateDiffScript(sourceRepository, targetRepository, generator, Properties.Settings.Default.DropTargetTables);

                                string explain = @"-- This database diff script contains the following objects:
-- - Tables:  Any that are not in the destination
-- -          (tables that are only in the destination are not dropped)
-- - Columns: Any added, deleted, changed columns for existing tables
-- - Indexes: Any added, deleted indexes for existing tables
-- - Foreign keys: Any added, deleted foreign keys for existing tables
-- ** Make sure to test against a production version of the destination database! ** " + Environment.NewLine + Environment.NewLine;
                                databaseInfo.Connectionstring = cd.TargetDatabase.Value;
                                databaseInfo.Caption = cd.TargetDatabase.Key;
                                OpenSqlEditorToolWindow(databaseInfo, explain + generator.GeneratedScript);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(Helpers.DataConnectionHelper.ShowErrors(ex));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Helpers.DataConnectionHelper.ShowErrors(ex));
            }
        }


        public void GenerateCeDgmlFiles(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;

            SaveFileDialog fd = new SaveFileDialog();
            fd.Title = "Save generated DGML file as";
            fd.Filter = "DGML (*.dgml)|*.dgml";
            fd.OverwritePrompt = true;
            fd.ValidateNames = true;
            bool? result = fd.ShowDialog();
            if (result.HasValue && result.Value == true)
            {
                var fileName = fd.FileName;
                try
                {
                    using (IRepository repository = RepoHelper.CreateRepository(databaseInfo.Connectionstring))
                    {
                        var generator = RepoHelper.CreateGenerator(repository, fileName);
                        generator.GenerateSchemaGraph(databaseInfo.Caption, Properties.Settings.Default.IncludeSystemTablesInDocumentation, false);
                        MessageBox.Show(string.Format("Saved {0}", fileName));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Helpers.DataConnectionHelper.ShowErrors(ex));
                }
            }
        }

        public void GenerateServerDgmlFiles(object sender, ExecutedRoutedEventArgs e)
        {
            // http://www.mztools.com/articles/2007/MZ2007011.aspx
            var menuItem = sender as MenuItem;
            if (menuItem == null) return;

            var databaseInfo = menuItem.CommandParameter as DatabasesMenuCommandParameters;
            if (databaseInfo == null) return;
            var treeViewItem = databaseInfo.DatabasesTreeViewItem;
            bool originalValue = Properties.Settings.Default.KeepServerSchemaNames;

            try
            {
                DataSource sqlDataSource = new DataSource("MicrosoftSqlServer", "Microsoft SQL Server");
                sqlDataSource.Providers.Add(DataProvider.SqlDataProvider);
                DataConnectionDialog dcd = new DataConnectionDialog();
                dcd.DataSources.Add(sqlDataSource);
                dcd.SelectedDataProvider = DataProvider.SqlDataProvider;
                dcd.SelectedDataSource = sqlDataSource;
                if (DataConnectionDialog.Show(dcd) == System.Windows.Forms.DialogResult.OK)
                {
                    string connectionString = dcd.ConnectionString;
                    string fileName;

                    PickTablesDialog ptd = new PickTablesDialog();
                    int totalCount = 0;
                    using (IRepository repository = RepoHelper.CreateServerRepository(dcd.ConnectionString))
                    {
                        ptd.Tables = repository.GetAllTableNamesForExclusion();
                        totalCount = ptd.Tables.Count;
                    }
                    ptd.Owner = Application.Current.MainWindow;
                    bool? res = ptd.ShowDialog();
                    if (res.HasValue && res.Value == true && (ptd.Tables.Count < totalCount))
                    {
                        SaveFileDialog fd = new SaveFileDialog();
                        fd.Title = "Save generated DGML file as";
                        fd.Filter = "DGML (*.dgml)|*.dgml";
                        fd.OverwritePrompt = true;
                        fd.ValidateNames = true;
                        bool? result = fd.ShowDialog();
                        if (result.HasValue && result.Value == true)
                        {
                            Properties.Settings.Default.KeepServerSchemaNames = true;
                            fileName = fd.FileName;
#if V35
                            using (IRepository repository = new ServerDBRepository(connectionString, Properties.Settings.Default.KeepServerSchemaNames))
#else
                            using (IRepository repository = new ServerDBRepository4(connectionString, Properties.Settings.Default.KeepServerSchemaNames))
#endif
                            {
                                var generator = RepoHelper.CreateGenerator(repository, fileName);
                                generator.GenerateSchemaGraph(connectionString, ptd.Tables);
                                MessageBox.Show(string.Format("Saved {0}", fileName));
                            }
                        }
                    }
                    dcd.Dispose();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Helpers.DataConnectionHelper.ShowErrors(ex));
            }
            finally
            {
                Properties.Settings.Default.KeepServerSchemaNames = originalValue;        
            }
        }

        public void ShrinkDatabase(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;

            try
            {
                helper.ShrinkDatabase(databaseInfo.Connectionstring);
                MessageBox.Show("Database Shrunk");
            }
            catch (Exception ex)
            {
                MessageBox.Show(Helpers.DataConnectionHelper.ShowErrors(ex));
            }            
        }

        public void CompactDatabase(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;

            try
            {
                helper.CompactDatabase(databaseInfo.Connectionstring);
                MessageBox.Show("Database Compacted");
            }
            catch (Exception ex)
            {
                MessageBox.Show(Helpers.DataConnectionHelper.ShowErrors(ex));
            }
        }

        public void VerifyDatabase(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;
            try
            {
                helper.VerifyDatabase(databaseInfo.Connectionstring);
                MessageBox.Show("Database Verified");
            }
            catch (Exception ex)
            {
                MessageBox.Show(Helpers.DataConnectionHelper.ShowErrors(ex));
            }
        }

        public void RepairDatabaseDeleteCorruptedRows(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;
            try
            {
                helper.RepairDatabaseDeleteCorruptedRows(databaseInfo.Connectionstring);
                MessageBox.Show("Database Repaired");
            }
            catch (Exception ex)
            {
                MessageBox.Show(Helpers.DataConnectionHelper.ShowErrors(ex));
            }
        }

        public void RepairDatabaseRecoverAllOrFail(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;
            try
            {
                helper.RepairDatabaseRecoverAllOrFail(databaseInfo.Connectionstring);
                MessageBox.Show("Database Repaired");
            }
            catch (Exception ex)
            {
                MessageBox.Show(Helpers.DataConnectionHelper.ShowErrors(ex));
            }
        }

        public void RepairDatabaseRecoverAllPossibleRows(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;
            try
            {
                helper.RepairDatabaseRecoverAllPossibleRows(databaseInfo.Connectionstring);
                MessageBox.Show("Database Repaired");
            }
            catch (Exception ex)
            {
                MessageBox.Show(Helpers.DataConnectionHelper.ShowErrors(ex));
            }
        }

        public void AddDescription(object sender, ExecutedRoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                var menuInfo = menuItem.CommandParameter as DatabaseMenuCommandParameters;
                if (menuInfo != null)
                {
                    try
                    {
                        using (IRepository repository = RepoHelper.CreateRepository(menuInfo.Connectionstring))
                        {
                            string desc = null;
                            ExplorerControl.DescriptionCache = new Helpers.DescriptionHelper().GetDescriptions(menuInfo.Connectionstring);
                            desc = ExplorerControl.DescriptionCache.Where(d => d.Object == null && d.Parent == null).Select(d => d.Description).SingleOrDefault();
                            DescriptionDialog ro = new DescriptionDialog(desc);
                            ro.Owner = Application.Current.MainWindow;
                            ro.ShowDialog();
                            if (ro.DialogResult.HasValue && ro.DialogResult.Value == true && !string.IsNullOrWhiteSpace(ro.Description) && ro.Description != desc)
                            {
                                new Helpers.DescriptionHelper().SaveDescription(menuInfo.Connectionstring, ExplorerControl.DescriptionCache, ro.Description, null, null);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(Helpers.DataConnectionHelper.ShowErrors(ex));
                    }
                }
            }
        }

        public void GenerateDocFiles(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;

            SaveFileDialog fd = new SaveFileDialog();
            fd.Title = "Save generated documentation as";
            //fd.Filter = "HTML (*.html)|*.html|WikiPlex (*.wiki)|*.wiki|Raw XML (*.xml)|*.xml";
            fd.Filter = "HTML (*.html)|*.html|Raw XML (*.xml)|*.xml";
            fd.OverwritePrompt = true;
            fd.ValidateNames = true;
            bool? result = fd.ShowDialog();
            if (result.HasValue && result.Value == true)
            {
                var fileName = fd.FileName;
                try
                {
                    var sqlCeDoc = new SqlCeDbDoc();
                    sqlCeDoc.CreateDocumentation(databaseInfo.Connectionstring, fileName, databaseInfo.Caption, true, null);
                    if (System.IO.File.Exists(fileName))
                    {
                        System.Diagnostics.Process.Start(fileName);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Helpers.DataConnectionHelper.ShowErrors(ex));
                }
            }
        }

        public void GenerateDataContext(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;

            bool isDesktop = false;
            if ((bool)((MenuItem)sender).Tag == true)
            {
                isDesktop = true;
            }

            SqlCeHelper helper = new SqlCeHelper();
            if (!helper.IsV35DbProviderInstalled())
            {
                MessageBox.Show("This feature requires the SQL Server Compact 3.5 SP2 runtime & DbProvider to be installed");
                return;                
            }

            string sqlMetalPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SDKs\Windows\v7.0A\WinSDK-NetFx40Tools", "InstallationFolder", null);
            if (string.IsNullOrEmpty(sqlMetalPath))
            {
                sqlMetalPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SDKs\Windows\v8.0A", "InstallationFolder", string.Empty) + "bin\\NETFX 4.0 Tools\\";
                if (string.IsNullOrEmpty(sqlMetalPath))
                {
                    MessageBox.Show("Could not find SQLMetal location in registry");
                    return;
                }
            } 
            sqlMetalPath = Path.Combine(sqlMetalPath, "sqlmetal.exe");
            if (!File.Exists(sqlMetalPath))
            {
                MessageBox.Show("Could not find SqlMetal in the expected location: " + sqlMetalPath);
                return;
            }
            string sdfFileName = string.Empty;

            string fileName = string.Empty;

            SaveFileDialog fd = new SaveFileDialog();
            fd.Title = "Save Data Context as";
            fd.Filter = "C# code (*.cs)|*.cs|VB code|*.vb";
            fd.OverwritePrompt = true;
            fd.ValidateNames = true;
            bool? fdresult = fd.ShowDialog();
            if (fdresult.HasValue && fdresult.Value == true)
            {
                fileName = fd.FileName;
            }
            if (string.IsNullOrEmpty(fileName))
                return;

            try
            {
                using (IRepository repository = RepoHelper.CreateRepository(databaseInfo.Connectionstring))
                {
                    var tables = repository.GetAllTableNames();
                    var pks = repository.GetAllPrimaryKeys();
                    string checkedTables = string.Empty;
                    foreach (string tableName in tables)
                    {
                        var pk = pks.Where(k => k.TableName == tableName).FirstOrDefault();
                        if (pk.TableName == null)
                        {
                            checkedTables += tableName + Environment.NewLine;
                        }
                    }
                    if (!string.IsNullOrEmpty(checkedTables))
                    {
                        string message = string.Format("The tables below do not have Primary Keys defined,{0}and will not be generated properly:{1}{2}", Environment.NewLine, Environment.NewLine, checkedTables);
                        MessageBox.Show(message);
                    }
                    List<KeyValuePair<string, string>> dbInfo = repository.GetDatabaseInfo();
                    foreach (var kvp in dbInfo)
                    {
                        if (kvp.Key == "Database")
                        {
                            sdfFileName = kvp.Value;
                            break;
                        }
                    }
                    sdfFileName = Path.GetFileName(sdfFileName);
                }

                string model = Path.GetFileNameWithoutExtension(databaseInfo.Caption).Replace(" ", string.Empty).Replace("#", string.Empty).Replace(".", string.Empty).Replace("-", string.Empty);
                model = model + "Context";
                DataContextDialog dcDialog = new DataContextDialog();
                dcDialog.ModelName = model;
                dcDialog.IsDesktop = isDesktop;
                dcDialog.NameSpace = string.Empty;
                dcDialog.CodeLanguage = "C#";
                bool? result = dcDialog.ShowDialog();
                if (result.HasValue && result.Value == true && (!string.IsNullOrWhiteSpace(dcDialog.ModelName)))
                {
                    if (dcDialog.AddRowversionColumns)
                    {
                        AddRowVersionColumns(databaseInfo);
                    }

                    string sdfPath = databaseInfo.Connectionstring;
                    
#if V35
#else
                    //If version 4.0, create a 3.5 schema sdf, and use that as connection string
                    if (isDesktop)
                    {
                        var tempFile = Path.GetTempFileName();
                        using (IRepository repository = RepoHelper.CreateRepository(databaseInfo.Connectionstring))
                        {
                            var generator = RepoHelper.CreateGenerator(repository, tempFile);
                            generator.ScriptDatabaseToFile(Scope.Schema);
                        }
                        sdfPath = Path.Combine(Path.GetTempPath(), sdfFileName);
                        if (File.Exists(sdfPath))
                        {
                            File.Delete(sdfPath);
                        }
                        sdfPath = "Data Source=" + sdfPath;
                        
                        helper.CreateDatabase(sdfPath);
                        using (IRepository repository = new DBRepository(sdfPath))
                        {
                            string script = File.ReadAllText(tempFile);
                            repository.ExecuteSql(script);
                        }
                    }
#endif
                    int versionNumber = GetVersionTableNumber(databaseInfo.Connectionstring, isDesktop);

                    model = dcDialog.ModelName;
                    string dcPath = fileName;
                    string parameters = " /provider:SQLCompact /code:\"" + dcPath + "\"";
                    parameters += " /conn:\"" + sdfPath + "\"";
                    parameters += " /context:" + model;
                    if (dcDialog.Pluralize)
                    {
                        parameters += " /pluralize";
                    }
                    if (!string.IsNullOrWhiteSpace(dcDialog.NameSpace))
                    {
                        parameters += " /namespace:" + dcDialog.NameSpace;
                    }
                    var dcH = new ErikEJ.SqlCeScripting.DataContextHelper();

                    string sqlmetalResult = dcH.RunSqlMetal(sqlMetalPath, parameters);

                    if (!File.Exists(dcPath))
                    {
                        MessageBox.Show("Error during SQL Metal run: " + sqlmetalResult);
                        return;
                    }

                    if (!isDesktop)
                    {
                        using (IRepository repository = RepoHelper.CreateRepository(databaseInfo.Connectionstring))
                        {
                            if (dcDialog.CodeLanguage == "VB")
                            {
                                DataContextHelper.FixDataContextVB(dcPath, model, dcDialog.NameSpace, sdfFileName, repository);
                            }
                            else
                            {
                                DataContextHelper.FixDataContextCS(dcPath, model, dcDialog.NameSpace, sdfFileName, repository);
                            }
                        }
                    }

                    // Creates __Version table and adds one row if desired
                    if (!isDesktop && dcDialog.AddVersionTable)
                    {
                        using (IRepository repository = RepoHelper.CreateRepository(databaseInfo.Connectionstring))
                        {
                            var list = repository.GetAllTableNames();
                            if (!list.Contains("__VERSION"))
                            {
                                repository.ExecuteSql(string.Format(@"
                                CREATE TABLE [__VERSION] (
                                  [SchemaVersion] int NOT NULL
                                , [DateUpdated] datetime NOT NULL DEFAULT (GETDATE())
                                );
                                GO
                                CREATE INDEX [IX_SchemaVersion] ON [__VERSION] ([SchemaVersion] DESC);
                                GO
                                INSERT INTO [__VERSION] ([SchemaVersion]) VALUES ({0});
                                GO", versionNumber));
                            }
                        }

                    }
                    MessageBox.Show("DataContext class successfully created");
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(Helpers.DataConnectionHelper.ShowErrors(ex));
            }
        }

        private static void AddRowVersionColumns(DatabaseMenuCommandParameters databaseInfo)
        {
            using (IRepository repository = RepoHelper.CreateRepository(databaseInfo.Connectionstring))
            {
                var list = repository.GetAllTableNames();
                var allColumns = repository.GetAllColumns();
                foreach (var table in list)
                {
                    if (!table.StartsWith("__"))
                    {
                        var rowVersionCol = allColumns.Where(c => c.TableName == table && c.DataType == "rowversion").SingleOrDefault();
                        if (rowVersionCol == null)
                        {
                            repository.ExecuteSql(string.Format("ALTER TABLE {0} ADD COLUMN VersionColumn rowversion NOT NULL;{1}GO", table, Environment.NewLine));
                        }
                    }
                }
            }
        }

        private int GetVersionTableNumber(string databaseInfo, bool isDesktop)
        {
            if (isDesktop)
                return 0;

            int version = 0;
            using (IRepository repository = RepoHelper.CreateRepository(databaseInfo))
            {
                var list = repository.GetAllTableNames();
                if (list.Contains("__VERSION"))
                {
                    System.Data.DataSet ds = repository.ExecuteSql(@"
                                SELECT MAX([SchemaVersion]) FROM __VERSION;
                                GO");
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        version = int.Parse(ds.Tables[0].Rows[0][0].ToString());
                    }

                    repository.ExecuteSql(@"
                                DROP TABLE [__VERSION];
                                GO");
                }
            }

            return version;
        }

        public void ExportServerDatabaseTo40(object sender, ExecutedRoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null) return;

            var databaseInfo = menuItem.CommandParameter as DatabasesMenuCommandParameters;
            if (databaseInfo == null) return;
            var treeViewItem = databaseInfo.DatabasesTreeViewItem;

            try
            {
                DataSource sqlDataSource = new DataSource("MicrosoftSqlServer", "Microsoft SQL Server");
                sqlDataSource.Providers.Add(DataProvider.SqlDataProvider);
                DataConnectionDialog dcd = new DataConnectionDialog();
                dcd.DataSources.Add(sqlDataSource);
                dcd.SelectedDataProvider = DataProvider.SqlDataProvider;
                dcd.SelectedDataSource = sqlDataSource;
                if (DataConnectionDialog.Show(dcd) == System.Windows.Forms.DialogResult.OK && !string.IsNullOrEmpty(dcd.ConnectionString))
                {
                    PickTablesDialog ptd = new PickTablesDialog();
                    int totalCount = 0;
                    using (IRepository repository = RepoHelper.CreateServerRepository(dcd.ConnectionString))
                    {
                        ptd.Tables = repository.GetAllTableNamesForExclusion();
                        totalCount = ptd.Tables.Count;
                    }
                    ptd.Owner = Application.Current.MainWindow;
                    bool? res = ptd.ShowDialog();
                    if (res.HasValue && res.Value == true && (ptd.Tables.Count < totalCount))
                    {
                        string sdfName;
                        SaveFileDialog fd = new SaveFileDialog();
                        fd.Title = "Export as";
                        fd.Filter = "SQL Server Compact Database (*.sdf)|*.sdf|All Files(*.*)|*.*";
                        fd.OverwritePrompt = true;
                        fd.ValidateNames = true;
                        bool? result = fd.ShowDialog();
                        if (result.HasValue && result.Value == true)
                        {
                            sdfName = fd.FileName;
                            using (IRepository repository = RepoHelper.CreateServerRepository(dcd.ConnectionString))
                            {
                                try
                                {
                                    string scriptRoot = System.IO.Path.GetTempFileName();
                                    string tempScript = scriptRoot + ".sqlce";
                                    var generator = RepoHelper.CreateGenerator(repository, tempScript);
                                    generator.ExcludeTables(ptd.Tables);
                                    SetStatus("Scripting server database...");
                                    generator.ScriptDatabaseToFile(Scope.SchemaData);

                                    SetStatus("Creating SQL Server Compact database...");

                                    ISqlCeHelper helper = RepoHelper.CreateHelper();
                                    string sdfConnectionString = string.Format("Data Source={0};Max Database Size=4091", sdfName);
                                    if (System.IO.File.Exists(sdfName))
                                        File.Delete(sdfName);
                                    helper.CreateDatabase(sdfConnectionString);

                                    BackgroundWorker bw = new BackgroundWorker();
                                    List<string> parameters = new List<string>();
                                    parameters.Add(sdfConnectionString);
                                    parameters.Add(tempScript);
                                    parameters.Add(scriptRoot);

                                    bw.DoWork += new DoWorkEventHandler(bw_DoWork);
                                    bw.RunWorkerCompleted += (s, ea) =>
                                    {
                                        try
                                        {
                                            if (ea.Error != null)
                                            {
                                                MessageBox.Show(Helpers.DataConnectionHelper.ShowErrors(ea.Error));
                                            }
                                            else
                                            {
                                                MessageBox.Show("Database successfully exported");   
                                            }
                                        }
                                        finally
                                        {
                                            bw.Dispose();
                                        }
                                    };
                                    bw.RunWorkerAsync(parameters);

                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(Helpers.DataConnectionHelper.ShowErrors(ex));
                                }
                            }
                        }
                    }
                    dcd.Dispose();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Helpers.DataConnectionHelper.ShowErrors(ex));
            }
        }

        private void SetStatus(string message)
        {
            //int frozen;
            //IVsStatusbar statusBar = package.GetServiceHelper(typeof(SVsStatusbar)) as IVsStatusbar;
            //statusBar.IsFrozen(out frozen);
            //if (!Convert.ToBoolean(frozen))
            //{
            //    statusBar.SetText(message);
            //}
        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            SetStatus("Importing data...");

            var parameters = e.Argument as List<string>;
            string sdfConnectionString = parameters[0];
            string tempScript = parameters[1];
            string scriptRoot = parameters[2];

            using (IRepository ce4Repository = RepoHelper.CreateRepository(sdfConnectionString))
            {
                //Handles large exports also... 
                if (File.Exists(tempScript)) // Single file
                {
                    ce4Repository.ExecuteSqlFile(tempScript);
                }
                else // possibly multiple files - tmp2BB9.tmp_0.sqlce
                {
                    for (int i = 0; i < 400; i++)
                    {
                        string testFile = string.Format("{0}_{1}{2}", scriptRoot, i.ToString("D4"), ".sqlce");
                        if (File.Exists(testFile))
                        {
                            ce4Repository.ExecuteSqlFile(testFile);
                        }
                    }
                }
            }
            SetStatus("Database file saved");
        }

        public void CheckCeVersion(object sender, ExecutedRoutedEventArgs e)
        {
#if V35
            var helper = new SqlCeHelper();
#else
            var helper = new SqlCeHelper4();
#endif

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "SQL Server Compact Database (*.sdf)|*.sdf|All Files(*.*)|*.*";
            ofd.CheckFileExists = true;
            ofd.Multiselect = false;
            ofd.ValidateNames = true;
            if (ofd.ShowDialog() == true)
            {
                try
                {
                    var sdfVersion = helper.DetermineVersion(ofd.FileName);
                    string found = "Unknown";
                    switch (sdfVersion)
                    {
                        case SQLCEVersion.SQLCE20:
                            found = "2.0";
                            break;
                        case SQLCEVersion.SQLCE30:
                            found = "3.0/3.1";
                            break;
                        case SQLCEVersion.SQLCE35:
                            found = "3.5";
                            break;
                        case SQLCEVersion.SQLCE40:
                            found = "4.0";
                            break;
                        default:
                            break;
                    }
                    MessageBox.Show(string.Format("{0} is SQL Server Compact version {1}", Path.GetFileName(ofd.FileName), found), "SQL Server Compact Version Detect");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Helpers.DataConnectionHelper.ShowErrors(ex));
                }
            }
        }

        public void BuildTable(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;
            try
            {
                using (IRepository repository = RepoHelper.CreateRepository(databaseInfo.Connectionstring))
                {
                    var generator = RepoHelper.CreateGenerator(repository);
                    TableBuilderDialog tbd = new TableBuilderDialog(null);
                    if (tbd.ShowDialog() == true)
                    {
                        generator.GenerateTableCreate(tbd.TableName, tbd.TableColumns);
                        var script = generator.GeneratedScript.ToString();
                        if (!string.IsNullOrEmpty(tbd.PkScript))
                        {
                            script += tbd.PkScript;
                        }
                        OpenSqlEditorToolWindow(databaseInfo, script);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Helpers.DataConnectionHelper.ShowErrors(ex));
            }
        }

        public void SpawnSqlEditorWindow(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;

            SqlEditorControl editor = new SqlEditorControl();
            editor.Database = databaseInfo.Connectionstring;
            FabTabItem tab = new FabTabItem();
            tab.Content = editor;
            tab.Header = databaseInfo.Caption;
            _parentWindow.FabTab.Items.Add(tab);
            _parentWindow.FabTab.SelectedIndex = _parentWindow.FabTab.Items.Count - 1; 
            return;
        }

        private void OpenSqlEditorToolWindow(DatabaseMenuCommandParameters databaseInfo, string script)
        {
            SqlEditorControl editor = new SqlEditorControl();
            editor.Database = databaseInfo.Connectionstring;
            editor.SqlText = script;
            FabTabItem tab = new FabTabItem();
            tab.Content = editor;
            tab.Header = databaseInfo.Caption;
            _parentWindow.FabTab.Items.Add(tab);
            _parentWindow.FabTab.SelectedIndex = _parentWindow.FabTab.Items.Count - 1;
            return;
        }

        private static DatabaseMenuCommandParameters ValidateMenuInfo(object sender)
        {
            var menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                return menuItem.CommandParameter as DatabaseMenuCommandParameters;
            }
            else
            {
                return null;
            }
        }


    }
}
