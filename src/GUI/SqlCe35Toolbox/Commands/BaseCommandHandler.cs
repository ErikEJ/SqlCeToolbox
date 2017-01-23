using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using EnvDTE;
using ErikEJ.SqlCeScripting;
using ErikEJ.SqlCeToolbox.Dialogs;
using ErikEJ.SqlCeToolbox.Helpers;
using ErikEJ.SqlCeToolbox.ToolWindows;
using Kent.Boogaart.KBCsv;
#if SSMS
using Microsoft.SqlServer.Management.UI.VSIntegration.Editors;
#endif

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
            var menuInfo = ValidateMenuInfo(sender);
            var ds = new DataSet();
            if (menuInfo == null) return;

            try
            {
                using (var repository = DataConnectionHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    var generator = DataConnectionHelper.CreateGenerator(repository, menuInfo.DatabaseInfo.DatabaseType);
                    generator.GenerateTableSelect(menuInfo.Name);
                    var sqlText = generator.GeneratedScript;
                    ds = repository.ExecuteSql(sqlText);
                }
                var pkg = ParentWindow.Package as SqlCeToolboxPackage;
                Debug.Assert(pkg != null, "Package property of the Explorere Tool Window should never be null, have you tried to create it manually and not through FindToolWindow()?");

                var dbName = System.IO.Path.GetFileNameWithoutExtension(menuInfo.DatabaseInfo.Caption);
                if (dbName != null)
                {
                    var window = pkg.CreateWindow<ReportWindow>(Math.Abs(menuInfo.Name.GetHashCode() - dbName.GetHashCode()));
                    window.Caption = menuInfo.Name + " (" + dbName + ")";
                    pkg.ShowWindow(window);

                    var control = window.Content as ReportControl;
                    if (control != null)
                    {
                        control.DatabaseInfo = menuInfo.DatabaseInfo;
                        control.TableName = menuInfo.Name;
                        control.DataSet = ds;
                        control.ShowReport();
                    }
                }
                DataConnectionHelper.LogUsage("TableReport");
            }
            catch (System.IO.FileNotFoundException)
            {
                EnvDteHelper.ShowError("Microsoft Report Viewer 2010 not installed, please download and install to use this feature  http://www.microsoft.com/en-us/download/details.aspx?id=6442");
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
            var menuInfo = ValidateMenuInfo(sender);
            if (menuInfo == null) return;
            var dbProviderPresent = menuInfo.DatabaseInfo.DatabaseType == DatabaseType.SQLCE35 && DataConnectionHelper.IsV35DbProviderInstalled() 
                                    || menuInfo.DatabaseInfo.DatabaseType == DatabaseType.SQLCE40 && DataConnectionHelper.IsV40DbProviderInstalled() 
                                    || menuInfo.DatabaseInfo.DatabaseType == DatabaseType.SQLite;
            if (!dbProviderPresent)
            {
                EnvDteHelper.ShowError("The required DbProvider registration is not present, please re-install/repair the SQL Server Compact runtime");
                return;
            }

            try
            {
                var readOnly = false;
                var readOnlyColumns = new List<int>();

                string sqlText;
                using (var repository = DataConnectionHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    var tpks = repository.GetAllPrimaryKeys()
                        .Where(pk => pk.TableName == menuInfo.Name)
                        .ToList();
                    if (tpks.Count == 0)
                    {
                        readOnly = true;
                    }
                    var cols = repository.GetAllColumns();
                    cols = cols
                        .Where(c => c.TableName == menuInfo.Name)
                        .ToList();
                    var x = 0;
                    foreach (var col in cols)
                    {
                        if (col.AutoIncrementBy > 0 || col.RowGuidCol)
                        {
                            readOnlyColumns.Add(x);
                        }
                        x++;
                    }
                    var generator = DataConnectionHelper.CreateGenerator(repository, menuInfo.DatabaseInfo.DatabaseType);
                    generator.GenerateTableSelect(menuInfo.Name, Properties.Settings.Default.MakeSQLiteDatetimeReadOnly);
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

                var dbName = System.IO.Path.GetFileNameWithoutExtension(menuInfo.DatabaseInfo.Caption);
                if (dbName != null)
                {
                    var window = pkg.CreateWindow<DataGridViewWindow>(Math.Abs(menuInfo.Name.GetHashCode() - dbName.GetHashCode()));
                    window.Caption = menuInfo.Name + " (" + dbName + ")";
                    pkg.ShowWindow(window);

                    var control = window.Content as DataEditControl;
                    if (control != null)
                    {
                        control.DatabaseInfo = menuInfo.DatabaseInfo;
                        control.TableName = menuInfo.Name;
                        control.ReadOnly = readOnly;
                        control.ReadOnlyColumns = readOnlyColumns;
                        control.SqlText = sqlText;
                        control.ShowGrid();
                    }
                }
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
                using (var repository = DataConnectionHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    var tbd = new TableBuilderDialog(menuInfo.Name, menuInfo.DatabaseInfo.DatabaseType);
                    tbd.Mode = 1;
                    if (tbd.ShowModal() != true || tbd.TableColumns.Count != 1) return;
                    var generator = DataConnectionHelper.CreateGenerator(repository, menuInfo.DatabaseInfo.DatabaseType);
                    generator.GenerateColumnAddScript(tbd.TableColumns[0]);
                    var script = generator.GeneratedScript;
                    OpenSqlEditorToolWindow(menuInfo, script);
                    DataConnectionHelper.LogUsage("TableBuildColumnAdd");
                }
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void AddIndex(object sender, ExecutedRoutedEventArgs e)
        {
            var menuInfo = ValidateMenuInfo(sender);
            if (menuInfo == null) return;
            try
            {
                using (var repository = DataConnectionHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    IndexDialog idxDlg = new IndexDialog(menuInfo.Name);
                    idxDlg.Columns = repository.GetAllColumns().Where(c => c.TableName == menuInfo.Name).ToList();
                    if (idxDlg.ShowModal() == true)
                    {
                        //var generator = Helpers.DataConnectionHelper.CreateGenerator(repository, menuInfo.DatabaseInfo.DatabaseType);
                        Index idx = idxDlg.NewIndex;
                        StringBuilder sbScript = new StringBuilder();
                        
                        sbScript.Append("CREATE ");
                        if (idx.Unique)
                            sbScript.Append("UNIQUE ");
                        sbScript.AppendFormat("INDEX [{0}] ON [{1}] (", idx.IndexName, idx.TableName);
                        sbScript.AppendFormat("[{0}] {1}", idx.ColumnName, idx.SortOrder.ToString());
                        sbScript.AppendLine(");");

                        //foreach (Index col in indexes)
                        //{
                        //    _sbScript.AppendFormat("[{0}] {1},", col.ColumnName, col.SortOrder.ToString());
                        //}
                        //// Remove the last comma
                        //_sbScript.Remove(_sbScript.Length - 1, 1);
                        //_sbScript.AppendLine(");");

                        sbScript.Append("GO" + Environment.NewLine);

                        OpenSqlEditorToolWindow(menuInfo, sbScript.ToString());
                        DataConnectionHelper.LogUsage("TableIndexAdd");
                    }
                }
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void AddForeignKey(object sender, ExecutedRoutedEventArgs e)
        {
            var menuInfo = ValidateMenuInfo(sender);
            if (menuInfo == null) return;
            try
            {
                using (var repository = DataConnectionHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    var fkDlg = new ForeignKeyDialog(menuInfo.Name);
                    fkDlg.AllColumns = repository.GetAllColumns().ToList();
                    fkDlg.AllPrimaryKeys = repository.GetAllPrimaryKeys();
                    if (fkDlg.ShowModal() != true) return;
                    var generator = DataConnectionHelper.CreateGenerator(repository, menuInfo.DatabaseInfo.DatabaseType);
                    generator.GenerateForeignKey(fkDlg.NewKey);
                    OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);
                    DataConnectionHelper.LogUsage("TableKeyAdd");
                }
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void ScriptAsCreate(object sender, ExecutedRoutedEventArgs e)
        {
            var menuInfo = ValidateMenuInfo(sender);
            if (menuInfo == null) return;
            try
            {
                using (IRepository repository = DataConnectionHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    var generator = DataConnectionHelper.CreateGenerator(repository, menuInfo.DatabaseInfo.DatabaseType);
                    generator.GenerateTableScript(menuInfo.Name);
                    OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);
                    DataConnectionHelper.LogUsage("TableScriptAsCreate");
                }
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void ScriptAsDrop(object sender, ExecutedRoutedEventArgs e)
        {
            var menuInfo = ValidateMenuInfo(sender);
            if (menuInfo == null) return;
            try
            {
                using (var repository = DataConnectionHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    var generator = DataConnectionHelper.CreateGenerator(repository, menuInfo.DatabaseInfo.DatabaseType);
                    generator.GenerateTableDrop(menuInfo.Name);
                    OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);
                    DataConnectionHelper.LogUsage("TableScriptAsDrop");
                }
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void ScriptAsDropAndCreate(object sender, ExecutedRoutedEventArgs e)
        {
            var menuInfo = ValidateMenuInfo(sender);
            if (menuInfo == null) return;
            try
            {
                using (IRepository repository = DataConnectionHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    var generator = DataConnectionHelper.CreateGenerator(repository, menuInfo.DatabaseInfo.DatabaseType);
                    generator.GenerateTableDrop(menuInfo.Name);
                    generator.GenerateTableScript(menuInfo.Name);
                    OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);
                    DataConnectionHelper.LogUsage("TableScriptAsDropAndCreate");
                }
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void ScriptAsSelect(object sender, ExecutedRoutedEventArgs e)
        {
            var menuInfo = ValidateMenuInfo(sender);
            if (menuInfo == null) return;
            try
            {
                using (IRepository repository = DataConnectionHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    var generator = DataConnectionHelper.CreateGenerator(repository, menuInfo.DatabaseInfo.DatabaseType);
                    generator.GenerateTableSelect(menuInfo.Name);
                    OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);
                    DataConnectionHelper.LogUsage("TableScriptAsSelect");
                }
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void ScriptAsInsert(object sender, ExecutedRoutedEventArgs e)
        {
            var menuInfo = ValidateMenuInfo(sender);
            if (menuInfo == null) return;
            try
            {
                using (IRepository repository = DataConnectionHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    var generator = DataConnectionHelper.CreateGenerator(repository, menuInfo.DatabaseInfo.DatabaseType);
                    generator.GenerateTableInsert(menuInfo.Name);
                    OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);
                    DataConnectionHelper.LogUsage("TableScriptAsInsert");
                }
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void ScriptAsUpdate(object sender, ExecutedRoutedEventArgs e)
        {
            var menuInfo = ValidateMenuInfo(sender);
            if (menuInfo == null) return;
            try
            {
                using (IRepository repository = DataConnectionHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    var generator = DataConnectionHelper.CreateGenerator(repository, menuInfo.DatabaseInfo.DatabaseType);
                    generator.GenerateTableUpdate(menuInfo.Name);
                    OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);
                    DataConnectionHelper.LogUsage("TableScriptAsUpdate");
                }
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void ScriptAsDelete(object sender, ExecutedRoutedEventArgs e)
        {
            var menuInfo = ValidateMenuInfo(sender);
            if (menuInfo == null) return;
            try
            {
                using (IRepository repository = DataConnectionHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    var generator = DataConnectionHelper.CreateGenerator(repository, menuInfo.DatabaseInfo.DatabaseType);
                    generator.GenerateTableDelete(menuInfo.Name);
                    OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);
                    DataConnectionHelper.LogUsage("TableScriptAsDelete");
                }
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void ScriptAsData(object sender, ExecutedRoutedEventArgs e)
        {
            var menuInfo = ValidateMenuInfo(sender);
            if (menuInfo == null) return;
            try
            {
                using (IRepository repository = DataConnectionHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    var generator = DataConnectionHelper.CreateGenerator(repository, menuInfo.DatabaseInfo.DatabaseType);
                    generator.GenerateTableContent(menuInfo.Name, false, Properties.Settings.Default.IgnoreIdentityInInsertScript);
                    if (!Properties.Settings.Default.IgnoreIdentityInInsertScript)
                    {
                        generator.GenerateIdentityReset(menuInfo.Name, false);
                    }
                    OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);
                    DataConnectionHelper.LogUsage("TableScriptAsData");
                }
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
        }


        // ReSharper disable once InconsistentNaming
        public void ScriptAsSQLCLRSample(object sender, ExecutedRoutedEventArgs e)
        {
#if SSMS

            var menuInfo = ValidateMenuInfo(sender);
            if (menuInfo == null) return;
            var package = ParentWindow.Package as SqlCeToolboxPackage;
            if (package == null) return;
            try
            {
                using (var repository = DataConnectionHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    var generator = DataConnectionHelper.CreateGenerator(repository, menuInfo.DatabaseInfo.DatabaseType);
                    generator.GenerateTableCreate(menuInfo.Name);
                    var sqlClrScript = new StringBuilder(Resources.InstallSqlClr);
                    sqlClrScript.AppendLine();

                    sqlClrScript.AppendFormat(
                        "EXEC dbo.GetSqlCeTable 'Provider=Microsoft.SQLSERVER.CE.OLEDB.4.0;OLE DB Services=-4;{0}', '{1}'", 
                        menuInfo.DatabaseInfo.ConnectionString, 
                        menuInfo.Name);
                    sqlClrScript.AppendLine();
                    sqlClrScript.AppendLine();
                    sqlClrScript.Append("-- Sample 2: Load data into SQL Server table");
                    sqlClrScript.AppendLine();
                    sqlClrScript.Append(generator.GeneratedScript);
                    sqlClrScript.AppendLine();
                    sqlClrScript.AppendFormat("INSERT INTO {0}", menuInfo.Name);
                    sqlClrScript.AppendLine();
                    sqlClrScript.AppendFormat(
                        "EXEC dbo.GetSqlCeTable 'Provider=Microsoft.SQLSERVER.CE.OLEDB.4.0;OLE DB Services=-4;{0}', '{1}'",
                        menuInfo.DatabaseInfo.ConnectionString,
                        menuInfo.Name);
                    sqlClrScript.AppendLine();
                    sqlClrScript.Append("GO");

                    //Add new script to SSMS editor with content
                    ScriptFactory.Instance.CreateNewBlankScript(ScriptType.Sql);
                    var dte = package.GetServiceHelper(typeof(DTE)) as DTE;
                    if (dte != null)
                    {
                        var doc = (TextDocument)dte.Application.ActiveDocument.Object(null);
                        doc.EndPoint.CreateEditPoint().Insert(sqlClrScript.ToString());
                        doc.DTE.ActiveDocument.Saved = true;
                    }
                    DataConnectionHelper.LogUsage("TableScriptAsSQLCLRSample");
                }
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
#endif
        }
        public void GenerateDataDiffScript(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var menuInfo = ValidateMenuInfo(sender);
                var package = ParentWindow.Package as SqlCeToolboxPackage;

                if (menuInfo == null) return;

                Dictionary<string, DatabaseInfo> databaseList = DataConnectionHelper.GetDataConnections(package, true, false);
                foreach (KeyValuePair<string, DatabaseInfo> info in DataConnectionHelper.GetOwnDataConnections())
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

                var cd = new CompareDialog(menuInfo.DatabaseInfo.Caption, databaseList, menuInfo.Name);

                var result = cd.ShowModal();
                if (!result.HasValue || !result.Value || (cd.TargetDatabase.Key == null)) return;
                var target = cd.TargetDatabase;
                var source = new KeyValuePair<string, DatabaseInfo>(menuInfo.DatabaseInfo.ConnectionString, menuInfo.DatabaseInfo);
                var editorTarget = target;
                if (editorTarget.Value.DatabaseType == DatabaseType.SQLServer)
                {
                    editorTarget = source;
                }

                using (var sourceRepository = DataConnectionHelper.CreateRepository(source.Value))
                {
                    using (var targetRepository = DataConnectionHelper.CreateRepository(target.Value))
                    {
                        var generator = DataConnectionHelper.CreateGenerator(targetRepository, target.Value.DatabaseType);
                        try
                        {
                            var script = SqlCeDiff.CreateDataDiffScript(sourceRepository, menuInfo.Name, targetRepository, menuInfo.Name, generator);

                            if (package != null)
                            {
                                var sqlEditorWindow = package.CreateWindow<SqlEditorWindow>();
                                var editorControl = sqlEditorWindow.Content as SqlEditorControl;
                                if (editorControl != null)
                                {
                                    editorControl.ExplorerControl = ParentWindow.Content as ExplorerControl;
                                    editorControl.DatabaseInfo = editorTarget.Value;
                                    editorControl.SqlText = script;
                                }
                            }
                            DataConnectionHelper.LogUsage("TableScriptDataDiff");
                        }
                        catch (Exception ex)
                        {
                            DataConnectionHelper.SendError(ex, source.Value.DatabaseType, false);
                        }

                    }
                }
            }
            catch (ArgumentException ae)
            {
                EnvDteHelper.ShowError(ae.Message);
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, DatabaseType.SQLCE35, false);
            }
        }

        public void ImportData(object sender, ExecutedRoutedEventArgs e)
        {
            var menuInfo = ValidateMenuInfo(sender);
            if (menuInfo == null) return;
            try
            {
                using (var repository = DataConnectionHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    var generator = DataConnectionHelper.CreateGenerator(repository, menuInfo.DatabaseInfo.DatabaseType);

                    var imo = new ImportDialog
                    {
                        SampleHeader = generator.GenerateTableColumns(menuInfo.Name),
                        Separator =
                            System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator.ToCharArray()[0]
                    };

                    if (imo.ShowModal() != true) return;
                    if (string.IsNullOrWhiteSpace(imo.File) || !System.IO.File.Exists(imo.File)) return;
                    using (var reader = new CsvReader(imo.File, Encoding.UTF8))
                    {
                        reader.ValueSeparator = imo.Separator;
                        var hr = reader.ReadHeaderRecord();
                        if (generator.ValidColumns(menuInfo.Name, hr.Values))
                        {
                            var i = 1;
                            foreach (var record in reader.DataRecords)
                            {
                                generator.GenerateTableInsert(menuInfo.Name, hr.Values, record.Values, i);
                                i++;
                            }
                        }
                    }
                    OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);
                    DataConnectionHelper.LogUsage("TableImport");
                }
            }
            catch (System.IO.IOException iox)
            {
                EnvDteHelper.ShowError(iox.Message);
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void Rename(object sender, ExecutedRoutedEventArgs e)
        {
            var menuInfo = ValidateMenuInfo(sender);
            if (menuInfo == null) return;
            try
            {
                using (var repository = DataConnectionHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    
                    var ro = new RenameDialog(menuInfo.Name);
                    ro.ShowModal();
                    if (!ro.DialogResult.HasValue || ro.DialogResult.Value != true ||
                        string.IsNullOrWhiteSpace(ro.NewName) || menuInfo.Name.Equals(ro.NewName)) return;
                    repository.RenameTable(menuInfo.Name, ro.NewName);
                    if (ParentWindow != null && ParentWindow.Content != null)
                    {
                        var control = ParentWindow.Content as ExplorerControl;
                        if (control != null) control.RefreshTables(menuInfo.DatabaseInfo);
                    }
                    DataConnectionHelper.LogUsage("TableRename");
                }
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
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
                using (var repository = DataConnectionHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    var tableDesc = ExplorerControl.DescriptionCache.Where(d => d.Object == name && d.Parent == null).Select(d => d.Description).SingleOrDefault();
                    var ro = new DescriptionDialog(tableDesc) {ColumnsInfo = GetSiblingColumnInfo(repository, name)};
                    ro.ShowModal();
                    if (!ro.DialogResult.HasValue || ro.DialogResult.Value != true) return;
                    //save table description
                    new Helpers.DescriptionHelper().SaveDescription(menuInfo.DatabaseInfo, ExplorerControl.DescriptionCache, ro.TableDescription, null, menuInfo.Name);
                    //save all columns
                    foreach (var item in ro.ColumnsInfo)
                    {
                        new Helpers.DescriptionHelper().SaveDescription(menuInfo.DatabaseInfo, ExplorerControl.DescriptionCache, item.Description, name, item.Name);
                    }
                    ExplorerControl.DescriptionCache = new Helpers.DescriptionHelper().GetDescriptions(menuInfo.DatabaseInfo);
                    ((ExplorerControl)ParentWindow.Content).RefreshTables(menuInfo.DatabaseInfo);
                    DataConnectionHelper.LogUsage("TableUpdateDescriptions");
                }
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
        }
        
        /// <summary>
        /// Gets the column information (name,metadata,description) for a given table. This method is internal static so it can be reused in ColumnMenuCommandHandler
        /// </summary>
        internal static IList<TableColumnInfo> GetSiblingColumnInfo(IRepository repo, string parentTable)
        {
            var lst = new List<TableColumnInfo>();
            var descCols = ExplorerControl.DescriptionCache.Where(d => d.Parent == parentTable).ToList();
            var cols = repo.GetAllColumns().Where(c => c.TableName == parentTable);
            var pkList = repo.GetAllPrimaryKeys().Where(p => p.TableName == parentTable).Select(p => p.ColumnName).ToList();
            var fkList = repo.GetAllForeignKeys().Where(f => f.ConstraintTableName == parentTable).Select(f => f.ColumnName).ToList();
            string isNull = "not null", fk = "", pk = "";
            foreach (var item in cols)
            {
                if (pkList.Contains(item.ColumnName)) { pk = "PK, "; }
                if (fkList.Contains(item.ColumnName)) { fk = "FK, "; }
                if (item.IsNullable == YesNoOption.YES) { isNull = "null"; }
                var type = item.ShortType;
                var desc = descCols.Where(d => d.Object == item.ColumnName).Select(s => s.Description).SingleOrDefault();
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
            return menuItem?.CommandParameter as MenuCommandParameters;
        }        
    }
}