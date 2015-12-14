using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using ErikEJ.SqlCeScripting;
using ErikEJ.SqlCeToolbox.Dialogs;
using ErikEJ.SqlCeToolbox.Helpers;
using ErikEJ.SqlCeToolbox.ToolWindows;
using Kent.Boogaart.KBCsv;

namespace ErikEJ.SqlCeToolbox.Commands
{
    public class BaseCommandHandler : CommandHandlerBase
    {
        public BaseCommandHandler(ExplorerToolWindow parent)
        {
            ParentWindow = parent;
        }

        public void ReportTableData(object sender, ExecutedRoutedEventArgs e)
        {
            string sqlText = null;
            var menuInfo = ValidateMenuInfo(sender);
            var ds = new DataSet();
            if (menuInfo == null) return;

            try
            {
                using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    sqlText = string.Format(Environment.NewLine + "SELECT * FROM [{0}]", menuInfo.Name)
                        + Environment.NewLine + "GO";
                    ds = repository.ExecuteSql(sqlText);
                }
                var pkg = ParentWindow.Package as SqlCeToolboxPackage;
                Debug.Assert(pkg != null, "Package property of the Explorere Tool Window should never be null, have you tried to create it manually and not through FindToolWindow()?");

                string dbName = System.IO.Path.GetFileNameWithoutExtension(menuInfo.DatabaseInfo.Caption);
                var window = pkg.CreateWindow<ReportWindow>(Math.Abs(menuInfo.Name.GetHashCode() - dbName.GetHashCode()));
                window.Caption = menuInfo.Name + " (" + dbName + ")";
                pkg.ShowWindow(window);

                var control = window.Content as ReportControl;
                control.DatabaseInfo = menuInfo.DatabaseInfo;
                control.TableName = menuInfo.Name;
                control.DataSet = ds;
                control.ShowReport();
                DataConnectionHelper.LogUsage("TableReport");
            }
            catch (System.IO.FileNotFoundException)
            {
                EnvDTEHelper.ShowError("Microsoft Report Viewer 2010 not installed, please download and install to use this feature  http://www.microsoft.com/en-us/download/details.aspx?id=6442");
                return;
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }                
            ds.Dispose();
        }

        public void EditTableData(object sender, ExecutedRoutedEventArgs e)
        {
            string sqlText = null;

            var menuInfo = ValidateMenuInfo(sender);
            if (menuInfo == null) return;
            bool dbProviderPresent = menuInfo.DatabaseInfo.DatabaseType == DatabaseType.SQLCE35 && Helpers.DataConnectionHelper.IsV35DbProviderInstalled();
            if (menuInfo.DatabaseInfo.DatabaseType == DatabaseType.SQLCE40 && Helpers.DataConnectionHelper.IsV40DbProviderInstalled())
            {
                dbProviderPresent = true;
            }
            if (menuInfo.DatabaseInfo.DatabaseType == DatabaseType.SQLite)
            {
                dbProviderPresent = true;
            }
            if (!dbProviderPresent)
            {
                EnvDTEHelper.ShowError("The required DbProvider registration is not present, please re-install/repair the SQL Server Compact runtime");
                return;
            }

            try
            {
                bool readOnly = false;
                List<int> readOnlyColumns = new List<int>();

                using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    List<PrimaryKey> pks = repository.GetAllPrimaryKeys();
                    var tpks = repository.GetAllPrimaryKeys().Where(pk => pk.TableName == menuInfo.Name).ToList();
                    if (tpks.Count == 0)
                    {
                        readOnly = true;
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
                    var generator = Helpers.DataConnectionHelper.CreateGenerator(repository, menuInfo.DatabaseInfo.DatabaseType);
                    generator.GenerateTableSelect(menuInfo.Name);
                    sqlText = generator.GeneratedScript.Replace(";" + Environment.NewLine + "GO", "");
                    sqlText = sqlText.Replace(";" + Environment.NewLine, "");
                    if (menuInfo.DatabaseInfo.DatabaseType == DatabaseType.SQLite)
                    {
                        sqlText = sqlText + string.Format(" LIMIT {0}", Properties.Settings.Default.MaxRowsToEdit);
                    }
                    else
                    {
                        sqlText = sqlText.Replace(Environment.NewLine + "SELECT ", string.Format(Environment.NewLine + "SELECT TOP({0}) ", Properties.Settings.Default.MaxRowsToEdit));
                    }
                }

                var pkg = ParentWindow.Package as SqlCeToolboxPackage;
                Debug.Assert(pkg != null, "Package property of the Explorere Tool Window should never be null, have you tried to create it manually and not through FindToolWindow()?");

                string dbName = System.IO.Path.GetFileNameWithoutExtension(menuInfo.DatabaseInfo.Caption);
                var window = pkg.CreateWindow<DataGridViewWindow>(Math.Abs(menuInfo.Name.GetHashCode() - dbName.GetHashCode()));
                window.Caption = menuInfo.Name + " (" + dbName + ")";
                pkg.ShowWindow(window);

                var control = window.Content as DataEditControl;
                control.DatabaseInfo = menuInfo.DatabaseInfo;
                control.TableName = menuInfo.Name;
                control.ReadOnly = readOnly;
                control.ReadOnlyColumns = readOnlyColumns;
                control.SqlText = sqlText;
                control.ShowGrid();
                DataConnectionHelper.LogUsage("TableEdit");
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void AddColumn(object sender, ExecutedRoutedEventArgs e)
        {
            var menuInfo = ValidateMenuInfo(sender);
            if (menuInfo == null) return;
            try
            {
                using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    TableBuilderDialog tbd = new TableBuilderDialog(menuInfo.Name, menuInfo.DatabaseInfo.DatabaseType);
                    tbd.Mode = 1;
                    if (tbd.ShowModal() == true && tbd.TableColumns.Count == 1)
                    {
                        var generator = Helpers.DataConnectionHelper.CreateGenerator(repository, menuInfo.DatabaseInfo.DatabaseType);
                        generator.GenerateColumnAddScript(tbd.TableColumns[0]);
                        var script = generator.GeneratedScript.ToString();
                        OpenSqlEditorToolWindow(menuInfo, script);
                        Helpers.DataConnectionHelper.LogUsage("TableBuildColumnAdd");
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void AddIndex(object sender, ExecutedRoutedEventArgs e)
        {
            var menuInfo = ValidateMenuInfo(sender);
            if (menuInfo == null) return;
            try
            {
                using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    IndexDialog idxDlg = new IndexDialog(menuInfo.Name);
                    idxDlg.Columns = repository.GetAllColumns().Where(c => c.TableName == menuInfo.Name).ToList();
                    if (idxDlg.ShowModal() == true)
                    {
                        //var generator = Helpers.DataConnectionHelper.CreateGenerator(repository, menuInfo.DatabaseInfo.DatabaseType);
                        Index idx = idxDlg.NewIndex;
                        StringBuilder _sbScript = new StringBuilder();
                        
                        _sbScript.Append("CREATE ");
                        if (idx.Unique)
                            _sbScript.Append("UNIQUE ");
                        _sbScript.AppendFormat("INDEX [{0}] ON [{1}] (", idx.IndexName, idx.TableName);
                        _sbScript.AppendFormat("[{0}] {1}", idx.ColumnName, idx.SortOrder.ToString());
                        _sbScript.AppendLine(");");

                        //foreach (Index col in indexes)
                        //{
                        //    _sbScript.AppendFormat("[{0}] {1},", col.ColumnName, col.SortOrder.ToString());
                        //}
                        //// Remove the last comma
                        //_sbScript.Remove(_sbScript.Length - 1, 1);
                        //_sbScript.AppendLine(");");

                        _sbScript.Append("GO" + Environment.NewLine);

                        OpenSqlEditorToolWindow(menuInfo, _sbScript.ToString());
                        Helpers.DataConnectionHelper.LogUsage("TableIndexAdd");
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void AddForeignKey(object sender, ExecutedRoutedEventArgs e)
        {
            var menuInfo = ValidateMenuInfo(sender);
            if (menuInfo == null) return;
            try
            {
                using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    ForeignKeyDialog fkDlg = new ForeignKeyDialog(menuInfo.Name);
                    fkDlg.AllColumns = repository.GetAllColumns().ToList();
                    fkDlg.AllPrimaryKeys = repository.GetAllPrimaryKeys();
                    if (fkDlg.ShowModal() == true)
                    {
                        var generator = Helpers.DataConnectionHelper.CreateGenerator(repository, menuInfo.DatabaseInfo.DatabaseType);
                        generator.GenerateForeignKey(fkDlg.NewKey);
                        OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);
                        Helpers.DataConnectionHelper.LogUsage("TableKeyAdd");
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void ScriptAsCreate(object sender, ExecutedRoutedEventArgs e)
        {
            var menuInfo = ValidateMenuInfo(sender);
            if (menuInfo == null) return;
            try
            {
                using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    var generator = Helpers.DataConnectionHelper.CreateGenerator(repository, menuInfo.DatabaseInfo.DatabaseType);
                    generator.GenerateTableScript(menuInfo.Name);
                    OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);
                    Helpers.DataConnectionHelper.LogUsage("TableScriptAsCreate");
                }
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void ScriptAsDrop(object sender, ExecutedRoutedEventArgs e)
        {
            var menuInfo = ValidateMenuInfo(sender);
            if (menuInfo == null) return;
            try
            {
                using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    var generator = Helpers.DataConnectionHelper.CreateGenerator(repository, menuInfo.DatabaseInfo.DatabaseType);
                    generator.GenerateTableDrop(menuInfo.Name);
                    OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);
                    Helpers.DataConnectionHelper.LogUsage("TableScriptAsDrop");
                }
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void ScriptAsDropAndCreate(object sender, ExecutedRoutedEventArgs e)
        {
            var menuInfo = ValidateMenuInfo(sender);
            if (menuInfo == null) return;
            try
            {
                using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    var generator = Helpers.DataConnectionHelper.CreateGenerator(repository, menuInfo.DatabaseInfo.DatabaseType);
                    generator.GenerateTableDrop(menuInfo.Name);
                    generator.GenerateTableScript(menuInfo.Name);
                    OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);
                    Helpers.DataConnectionHelper.LogUsage("TableScriptAsDropAndCreate");
                }
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void ScriptAsSelect(object sender, ExecutedRoutedEventArgs e)
        {
            var menuInfo = ValidateMenuInfo(sender);
            if (menuInfo == null) return;
            try
            {
                using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    var generator = Helpers.DataConnectionHelper.CreateGenerator(repository, menuInfo.DatabaseInfo.DatabaseType);
                    generator.GenerateTableSelect(menuInfo.Name);
                    //TODO Something like this for intellisense (maybe a single object)
                    //OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript, false, repository.GetAllTableNames(), repository.GetAllColumns());
                    OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);
                    Helpers.DataConnectionHelper.LogUsage("TableScriptAsSelect");
                }
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void ScriptAsInsert(object sender, ExecutedRoutedEventArgs e)
        {
            var menuInfo = ValidateMenuInfo(sender);
            if (menuInfo == null) return;
            try
            {
                using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    var generator = Helpers.DataConnectionHelper.CreateGenerator(repository, menuInfo.DatabaseInfo.DatabaseType);
                    generator.GenerateTableInsert(menuInfo.Name);
                    OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);
                    Helpers.DataConnectionHelper.LogUsage("TableScriptAsInsert");
                }
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void ScriptAsUpdate(object sender, ExecutedRoutedEventArgs e)
        {
            var menuInfo = ValidateMenuInfo(sender);
            if (menuInfo == null) return;
            try
            {
                using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    var generator = Helpers.DataConnectionHelper.CreateGenerator(repository, menuInfo.DatabaseInfo.DatabaseType);
                    generator.GenerateTableUpdate(menuInfo.Name);
                    OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);
                    Helpers.DataConnectionHelper.LogUsage("TableScriptAsUpdate");
                }
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void ScriptAsDelete(object sender, ExecutedRoutedEventArgs e)
        {
            var menuInfo = ValidateMenuInfo(sender);
            if (menuInfo == null) return;
            try
            {
                using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    var generator = Helpers.DataConnectionHelper.CreateGenerator(repository, menuInfo.DatabaseInfo.DatabaseType);
                    generator.GenerateTableDelete(menuInfo.Name);
                    OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);
                    Helpers.DataConnectionHelper.LogUsage("TableScriptAsDelete");
                }
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void ScriptAsData(object sender, ExecutedRoutedEventArgs e)
        {
            var menuInfo = ValidateMenuInfo(sender);
            if (menuInfo == null) return;
            try
            {
                using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    var generator = Helpers.DataConnectionHelper.CreateGenerator(repository, menuInfo.DatabaseInfo.DatabaseType);
                    generator.GenerateTableContent(menuInfo.Name, false, Properties.Settings.Default.IgnoreIdentityInInsertScript);
                    if (!Properties.Settings.Default.IgnoreIdentityInInsertScript)
                    {
                        generator.GenerateIdentityReset(menuInfo.Name, false);
                    }
                    OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);
                    Helpers.DataConnectionHelper.LogUsage("TableScriptAsData");
                }
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void GenerateDataDiffScript(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var menuInfo = ValidateMenuInfo(sender);
                var package = ParentWindow.Package as SqlCeToolboxPackage;

                if (menuInfo == null) return;

                Dictionary<string, DatabaseInfo> databaseList = Helpers.DataConnectionHelper.GetDataConnections(package, true, false);
                foreach (KeyValuePair<string, DatabaseInfo> info in Helpers.DataConnectionHelper.GetOwnDataConnections())
                {
                    if (!databaseList.ContainsKey(info.Key))
                        databaseList.Add(info.Key, info.Value);
                }
                foreach (KeyValuePair<string, DatabaseInfo> info in databaseList)
                {
                    string sourceType = string.Empty;
                    switch (info.Value.DatabaseType)
                    {
                        case DatabaseType.SQLCE35:
                            sourceType = "3.5";
                            break;
                        case DatabaseType.SQLCE40:
                            sourceType = "4.0";
                            break;
                        case DatabaseType.SQLServer:
                            sourceType = "Server";
                            break;
                    }
                    info.Value.Caption = string.Format("{0} ({1})", info.Value.Caption, sourceType);
                }

                CompareDialog cd = new CompareDialog(menuInfo.DatabaseInfo.Caption, databaseList, menuInfo.Name);

                bool? result = cd.ShowModal();
                if (result.HasValue && result.Value == true && (cd.TargetDatabase.Key != null))
                {
                    var target = cd.TargetDatabase;
                    var source = new KeyValuePair<string, DatabaseInfo>(menuInfo.DatabaseInfo.ConnectionString, menuInfo.DatabaseInfo);
                    var editorTarget = target;
                    if (editorTarget.Value.DatabaseType == DatabaseType.SQLServer)
                    {
                        editorTarget = source;
                    }

                    using (IRepository sourceRepository = Helpers.DataConnectionHelper.CreateRepository(source.Value))
                    {
                        using (IRepository targetRepository = Helpers.DataConnectionHelper.CreateRepository(target.Value))
                        {
                            var generator = Helpers.DataConnectionHelper.CreateGenerator(targetRepository, target.Value.DatabaseType);
                            try
                            {
                                var script = SqlCeDiff.CreateDataDiffScript(sourceRepository, menuInfo.Name, targetRepository, menuInfo.Name, generator);

                                var sqlEditorWindow = package.CreateWindow<SqlEditorWindow>();
                                var editorControl = sqlEditorWindow.Content as SqlEditorControl;
                                editorControl.ExplorerControl = ParentWindow.Content as ExplorerControl;
                                Debug.Assert(editorControl != null);
                                editorControl.DatabaseInfo = editorTarget.Value;
                                editorControl.SqlText = script;
                                Helpers.DataConnectionHelper.LogUsage("TableScriptDataDiff");
                            }
                            catch (Exception ex)
                            {
                                Helpers.DataConnectionHelper.SendError(ex, source.Value.DatabaseType, false);
                            }

                        }
                    }
                }
            }
            catch (ArgumentException ae)
            {
                EnvDTEHelper.ShowError(ae.Message);
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, DatabaseType.SQLCE35, false);
            }
        }

        public void ImportData(object sender, ExecutedRoutedEventArgs e)
        {
            var menuInfo = ValidateMenuInfo(sender);
            if (menuInfo == null) return;
            try
            {
                using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    var generator = Helpers.DataConnectionHelper.CreateGenerator(repository, menuInfo.DatabaseInfo.DatabaseType);

                    ImportDialog imo = new ImportDialog();

                    imo.SampleHeader = generator.GenerateTableColumns(menuInfo.Name);
                    imo.Separator = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator.ToCharArray()[0];

                    if (imo.ShowModal() == true)
                    {
                        if (!string.IsNullOrWhiteSpace(imo.File) && System.IO.File.Exists(imo.File))
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
                            Helpers.DataConnectionHelper.LogUsage("TableImport");
                        }
                    }
                }
            }
            catch (System.IO.IOException iox)
            {
                EnvDTEHelper.ShowError(iox.Message);
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void Rename(object sender, ExecutedRoutedEventArgs e)
        {
            var menuInfo = ValidateMenuInfo(sender);
            if (menuInfo == null) return;
            try
            {
                using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    
                    RenameDialog ro = new RenameDialog(menuInfo.Name);
                    ro.ShowModal();
                    if (ro.DialogResult.HasValue && ro.DialogResult.Value == true && !string.IsNullOrWhiteSpace(ro.NewName) && !menuInfo.Name.Equals(ro.NewName))
                    {
                        repository.RenameTable(menuInfo.Name, ro.NewName);
                        if (ParentWindow != null && ParentWindow.Content != null)
                        {
                            ExplorerControl control = ParentWindow.Content as ExplorerControl;
                            control.RefreshTables(menuInfo.DatabaseInfo);
                        }
                        Helpers.DataConnectionHelper.LogUsage("TableRename");
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void SpawnSqlEditorWindow(object sender, ExecutedRoutedEventArgs e)
        {
            var menuInfo = ValidateMenuInfo(sender);
            if (menuInfo == null) return;
            OpenSqlEditorToolWindow(menuInfo, string.Empty);
        }

        public void AddDescription(object sender, ExecutedRoutedEventArgs e)
        {
            var menuInfo = ValidateMenuInfo(sender);
            if (menuInfo == null) return;
            string name = menuInfo.Name;
            UpdateDescriptions(menuInfo, name);
        }

        internal void UpdateDescriptions(MenuCommandParameters menuInfo, string name)
        {
            try
            {
                using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    var tableDesc = ExplorerControl.DescriptionCache.Where(d => d.Object == name && d.Parent == null).Select(d => d.Description).SingleOrDefault();
                    DescriptionDialog ro = new DescriptionDialog(tableDesc);
                    ro.ColumnsInfo = GetSiblingColumnInfo(repository, name);
                    ro.ShowModal();
                    if (ro.DialogResult.HasValue && ro.DialogResult.Value == true)
                    {
                        //save table description
                        new Helpers.DescriptionHelper().SaveDescription(menuInfo.DatabaseInfo, ExplorerControl.DescriptionCache, ro.TableDescription, null, menuInfo.Name);
                        //save all columns
                        foreach (var item in ro.ColumnsInfo)
                        {
                            new Helpers.DescriptionHelper().SaveDescription(menuInfo.DatabaseInfo, ExplorerControl.DescriptionCache, item.Description, name, item.Name);
                        }
                        ExplorerControl.DescriptionCache = new Helpers.DescriptionHelper().GetDescriptions(menuInfo.DatabaseInfo);
                        ((ExplorerControl)ParentWindow.Content).RefreshTables(menuInfo.DatabaseInfo);
                        Helpers.DataConnectionHelper.LogUsage("TableUpdateDescriptions");
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
        }
        
        /// <summary>
        /// Gets the column information (name,metadata,description) for a given table. This method is internal static so it can be reused in ColumnMenuCommandHandler
        /// </summary>
        internal static IList<TableColumnInfo> GetSiblingColumnInfo(IRepository repo, string parentTable)
        {
            List<TableColumnInfo> lst = new List<TableColumnInfo>();
            var desc_cols = ExplorerControl.DescriptionCache.Where(d => d.Parent == parentTable).ToList();
            var cols = repo.GetAllColumns().Where(c => c.TableName == parentTable);
            var pkList = repo.GetAllPrimaryKeys().Where(p => p.TableName == parentTable).Select(p => p.ColumnName);
            var fkList = repo.GetAllForeignKeys().Where(f => f.ConstraintTableName == parentTable).Select(f => f.ColumnName);
            string isNull = "not null", fk = "", pk = "", type = "";
            foreach (var item in cols)
            {
                if (pkList.Contains(item.ColumnName)) { pk = "PK, "; }
                if (fkList.Contains(item.ColumnName)) { fk = "FK, "; }
                if (item.IsNullable == YesNoOption.YES) { isNull = "null"; }
                type = item.ShortType;
                string desc = desc_cols.Where(d => d.Object == item.ColumnName).Select(s => s.Description).SingleOrDefault();
                lst.Add(new TableColumnInfo()
                {
                    Name = item.ColumnName,
                    Metadata = string.Format("{0}{1}{2} {3}", pk, fk, type, isNull),//space between type & isNUll always exists
                    Description = desc
                });
                pk = "";
                fk = "";
                isNull = "not null";
            }
            return lst;
        }

        private static MenuCommandParameters ValidateMenuInfo(object sender)
        {
            var menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                return menuItem.CommandParameter as MenuCommandParameters;
            }
            return null;
        }        
    }
}