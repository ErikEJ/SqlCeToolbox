using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EnvDTE;
using ErikEJ.SqlCeScripting;
using ErikEJ.SqlCeToolbox.Dialogs;
using ErikEJ.SqlCeToolbox.Helpers;
using ErikEJ.SqlCeToolbox.ToolWindows;
using Microsoft.Win32;
using Microsoft.VisualStudio.Shell.Interop;
using System.ComponentModel;

namespace ErikEJ.SqlCeToolbox.Commands
{
    public class DatabaseMenuCommandsHandler
    {
        private readonly ExplorerToolWindow _parentWindow;
        private SqlCeToolboxPackage package;

        public DatabaseMenuCommandsHandler(ExplorerToolWindow parent)
        {
            _parentWindow = parent;
            package = _parentWindow.Package as SqlCeToolboxPackage;
        }

        #region Database Level Commands
        public void CopyCeDatabase(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;
            try
            {
                var path = DataConnectionHelper.GetFilePath(databaseInfo.DatabaseInfo.ConnectionString, databaseInfo.DatabaseInfo.DatabaseType);
                Clipboard.Clear();
                Clipboard.SetData(DataFormats.FileDrop, new String[] { path });
                Helpers.DataConnectionHelper.LogUsage("DatabaseCopy");
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType);
            }
        }

        public void RemoveCeDatabase(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var databaseInfo = ValidateMenuInfo(sender);
                if (databaseInfo == null) return;

                if (databaseInfo.DatabaseInfo.FromServerExplorer)
                {
                    Guid provider = new Guid(Resources.SqlCompact40Provider);
                    if (databaseInfo.DatabaseInfo.DatabaseType == DatabaseType.SQLCE35)
                    {
                        provider = new Guid(Resources.SqlCompact35Provider);
                    }
                    if (!Helpers.DataConnectionHelper.DDEXProviderIsInstalled(provider))
                    {
                        EnvDTEHelper.ShowError("The DDEX provider is not installed, cannot remove connection");
                        return;
                    }            
                    Helpers.DataConnectionHelper.RemoveDataConnection(package, databaseInfo.DatabaseInfo.ConnectionString, provider);
                }
                else
                {
                    Helpers.DataConnectionHelper.RemoveDataConnection(databaseInfo.DatabaseInfo.ConnectionString);
                }
                ExplorerControl control = _parentWindow.Content as ExplorerControl;
                control.BuildDatabaseTree();
                Helpers.DataConnectionHelper.LogUsage("DatabaseRemoveCe");
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, DatabaseType.SQLCE35);
            }
        }

        public void RenameConnection(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;
            if (databaseInfo.DatabaseInfo.FromServerExplorer) return;
            try
            {
                RenameDialog ro = new RenameDialog(databaseInfo.DatabaseInfo.Caption);
                ro.ShowModal();
                if (ro.DialogResult.HasValue && ro.DialogResult.Value == true && !string.IsNullOrWhiteSpace(ro.NewName) && !databaseInfo.DatabaseInfo.Caption.Equals(ro.NewName))
                {
                    Helpers.DataConnectionHelper.RenameDataConnection(databaseInfo.DatabaseInfo.ConnectionString, ro.NewName);
                    ExplorerControl control = _parentWindow.Content as ExplorerControl;
                    control.BuildDatabaseTree();
                    Helpers.DataConnectionHelper.LogUsage("DatabaseRenameConnection");
                }
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType);
            }
        }

#region Maintenance menu items

        public void SetPassword(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;
            try
            {
                var dbInfo = databaseInfo.DatabaseInfo;
                var pwd = new PasswordDialog();
                pwd.ShowModal();
                if (pwd.DialogResult.HasValue && pwd.DialogResult.Value)
                {
                    var helper = DataConnectionHelper.CreateEngineHelper(databaseInfo.DatabaseInfo.DatabaseType);
                    var newConnectionString = helper.ChangeDatabasePassword(databaseInfo.DatabaseInfo.ConnectionString, pwd.Password);
                    if (dbInfo.FromServerExplorer)
                    {
                        var providerId = Resources.SqlCompact35Provider;
                        if (dbInfo.DatabaseType == DatabaseType.SQLCE40)
                            providerId = Resources.SqlCompact40Provider;
                        DataConnectionHelper.RemoveDataConnection(package, dbInfo.ConnectionString, new Guid(providerId));
                    }
                    else
                    {
                        DataConnectionHelper.RemoveDataConnection(databaseInfo.DatabaseInfo.ConnectionString);
                    }

                    if (!string.IsNullOrEmpty(newConnectionString))
                    {
                        DataConnectionHelper.SaveDataConnection(newConnectionString, dbInfo.DatabaseType, package);
                        EnvDTEHelper.ShowMessage("Password was set, and connection updated");
                    }
                    else
                    {
                        EnvDTEHelper.ShowMessage("Password was set, but could not update connection, please reconnect the database");
                    }

                    var control = _parentWindow.Content as ExplorerControl;
                    if (control != null) control.BuildDatabaseTree();
                    DataConnectionHelper.LogUsage("DatabaseMaintainSetPassword");                    
                }
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void ShrinkDatabase(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;
            try
            {
                ISqlCeHelper helper = Helpers.DataConnectionHelper.CreateEngineHelper(databaseInfo.DatabaseInfo.DatabaseType);
                helper.ShrinkDatabase(databaseInfo.DatabaseInfo.ConnectionString);
                Helpers.DataConnectionHelper.LogUsage("DatabaseMaintainShrink");
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void CompactDatabase(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;
            try
            {
                ISqlCeHelper helper = Helpers.DataConnectionHelper.CreateEngineHelper(databaseInfo.DatabaseInfo.DatabaseType);
                helper.CompactDatabase(databaseInfo.DatabaseInfo.ConnectionString);
                Helpers.DataConnectionHelper.LogUsage("DatabaseMaintainCompact");
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void VerifyDatabase(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;
            try
            {
                ISqlCeHelper helper = Helpers.DataConnectionHelper.CreateEngineHelper(databaseInfo.DatabaseInfo.DatabaseType);
                helper.VerifyDatabase(databaseInfo.DatabaseInfo.ConnectionString);
                Helpers.DataConnectionHelper.LogUsage("DatabaseMaintainVerify");
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void RepairDatabaseDeleteCorruptedRows(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;
            try
            {
                ISqlCeHelper helper = Helpers.DataConnectionHelper.CreateEngineHelper(databaseInfo.DatabaseInfo.DatabaseType);
                helper.RepairDatabaseDeleteCorruptedRows(databaseInfo.DatabaseInfo.ConnectionString);
                Helpers.DataConnectionHelper.LogUsage("DatabaseMaintainRepair");
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void RepairDatabaseRecoverAllOrFail(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;
            try
            {
                ISqlCeHelper helper = Helpers.DataConnectionHelper.CreateEngineHelper(databaseInfo.DatabaseInfo.DatabaseType);
                helper.RepairDatabaseRecoverAllOrFail(databaseInfo.DatabaseInfo.ConnectionString);
                Helpers.DataConnectionHelper.LogUsage("DatabaseMaintainRepair");
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void RepairDatabaseRecoverAllPossibleRows(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;
            try
            {
                ISqlCeHelper helper = Helpers.DataConnectionHelper.CreateEngineHelper(databaseInfo.DatabaseInfo.DatabaseType);
                helper.RepairDatabaseRecoverAllPossibleRows(databaseInfo.DatabaseInfo.ConnectionString);
                Helpers.DataConnectionHelper.LogUsage("DatabaseMaintainRepair");
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void BuildTable(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;
            try
            {
                using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(databaseInfo.DatabaseInfo))
                {
                    var generator = Helpers.DataConnectionHelper.CreateGenerator(repository, databaseInfo.DatabaseInfo.DatabaseType);
                    TableBuilderDialog tbd = new TableBuilderDialog(null, databaseInfo.DatabaseInfo.DatabaseType);
                    if (tbd.ShowModal() == true)
                    {
                        generator.GenerateTableCreate(tbd.TableName, tbd.TableColumns);
                        var script = generator.GeneratedScript.ToString();
                        if (databaseInfo.DatabaseInfo.DatabaseType == DatabaseType.SQLite)
                        {
                            script = script.Remove(script.Length - 6);
                            if (!string.IsNullOrEmpty(tbd.PkScript))
                            {
                                script += tbd.PkScript;
                            }
                            script += string.Format("{0});{1}", Environment.NewLine, Environment.NewLine);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(tbd.PkScript))
                            {
                                script += tbd.PkScript;
                            }
                        }
                        SpawnSqlEditorWindow(databaseInfo.DatabaseInfo, script);
                        Helpers.DataConnectionHelper.LogUsage("TableBuild");
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType);
            }
        }

#endregion

        private void SpawnSqlEditorWindow(DatabaseInfo databaseInfo,  string sqlScript)
        {
            try
            {
                if (databaseInfo == null) return;
                if (package == null) return;
                Debug.Assert(package != null, "Package property of the Explorere Tool Window should never be null, have you tried to create it manually and not through FindToolWindow()?");

                var sqlEditorWindow = package.CreateWindow<SqlEditorWindow>();
                var editorControl = sqlEditorWindow.Content as SqlEditorControl;
                Debug.Assert(editorControl != null);
                editorControl.DatabaseInfo = databaseInfo;
                editorControl.ExplorerControl = _parentWindow.Content as ExplorerControl;
                editorControl.SqlText = sqlScript;
                Helpers.DataConnectionHelper.LogUsage("DatabaseOpenEditor");
                Debug.Assert(editorControl != null, "The SqlEditorWindow *should* have a editorControl with content.");
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, DatabaseType.SQLServer);
            }

        }

        public void SpawnSqlEditorWindow(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var databaseInfo = ValidateMenuInfo(sender);
                if (databaseInfo == null) return;
                if (package == null) return;
                Debug.Assert(package != null, "Package property of the Explorere Tool Window should never be null, have you tried to create it manually and not through FindToolWindow()?");

                var sqlEditorWindow = package.CreateWindow<SqlEditorWindow>();
                var editorControl = sqlEditorWindow.Content as SqlEditorControl;
                Debug.Assert(editorControl != null);
                editorControl.DatabaseInfo = databaseInfo.DatabaseInfo;
                editorControl.ExplorerControl = _parentWindow.Content as ExplorerControl;
                editorControl.SqlText = null;
                Helpers.DataConnectionHelper.LogUsage("DatabaseOpenEditor");
                Debug.Assert(editorControl != null, "The SqlEditorWindow *should* have a editorControl with content.");
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, DatabaseType.SQLServer);
            }
        }

        public void AddDescription(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;
            try
            {
                using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(databaseInfo.DatabaseInfo))
                {
                    string desc = null;
                    ExplorerControl.DescriptionCache = new Helpers.DescriptionHelper().GetDescriptions(databaseInfo.DatabaseInfo);
                    desc = ExplorerControl.DescriptionCache.Where(d => d.Object == null && d.Parent == null).Select(d => d.Description).SingleOrDefault();
                    DescriptionDialog ro = new DescriptionDialog(desc);
                    ro.IsDatabase = true;
                    ro.ShowModal();
                    if (ro.DialogResult.HasValue && ro.DialogResult.Value == true && !string.IsNullOrWhiteSpace(ro.TableDescription) && ro.TableDescription != desc)
                    {
                        new Helpers.DescriptionHelper().SaveDescription(databaseInfo.DatabaseInfo, ExplorerControl.DescriptionCache, ro.TableDescription, null, null);
                    }
                    Helpers.DataConnectionHelper.LogUsage("DatabaseAddDescription");
                }
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void ScriptDatabase(object sender, ExecutedRoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null) return;
            Scope scope = (Scope)menuItem.Tag;

            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;

            try
            {
                int totalCount = 0;
                PickTablesDialog ptd = new PickTablesDialog();
                using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(databaseInfo.DatabaseInfo))
                {
                    ptd.Tables = repository.GetAllTableNamesForExclusion();
                    totalCount = ptd.Tables.Count;
                }

                bool? res = ptd.ShowModal();
                if (res.HasValue && res.Value == true && (ptd.Tables.Count < totalCount))
                {
                    SaveFileDialog fd = new SaveFileDialog();
                    fd.Title = "Save generated database script as";
                    fd.Filter = "SQL Server Compact Script (*.sqlce)|*.sqlce|SQL Server Script (*.sql)|*.sql|All Files(*.*)|*.*";
                    if (scope == Scope.SchemaDataSQLite || scope == Scope.SchemaSQLite || databaseInfo.DatabaseInfo.DatabaseType == DatabaseType.SQLite)
                    {
                        fd.Filter = "SQLite Script (*.sql)|*.sql|All Files(*.*)|*.*";    
                    }
                    fd.OverwritePrompt = true;
                    fd.ValidateNames = true;
                    bool? result = fd.ShowDialog();
                    if (result.HasValue && result.Value == true)
                    {
                        var fileName = fd.FileName;
                        using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(databaseInfo.DatabaseInfo))
                        {
                            var generator = Helpers.DataConnectionHelper.CreateGenerator(repository, fd.FileName, databaseInfo.DatabaseInfo.DatabaseType);
                            generator.ExcludeTables(ptd.Tables);
                            EnvDTEHelper.ShowMessage(generator.ScriptDatabaseToFile(scope));
                            Helpers.DataConnectionHelper.LogUsage("DatabaseScriptCe");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
            }

        }

        public void GenerateDiffScript(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var databaseInfo = ValidateMenuInfo(sender);
                if (databaseInfo == null) return;

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

                CompareDialog cd = new CompareDialog(databaseInfo.DatabaseInfo.Caption, databaseList);

                bool? result = cd.ShowModal();
                if (result.HasValue && result.Value == true && (cd.TargetDatabase.Key != null))
                {
                    var target = cd.TargetDatabase;
                    var swap = cd.SwapTarget;

                    var source = new KeyValuePair<string, DatabaseInfo>(databaseInfo.DatabaseInfo.ConnectionString, databaseInfo.DatabaseInfo);
                    if (swap)
                    {
                        source = target;
                        target = new KeyValuePair<string, DatabaseInfo>(databaseInfo.DatabaseInfo.ConnectionString, databaseInfo.DatabaseInfo);
                    }

                    var editorTarget = target;
                    if (editorTarget.Value.DatabaseType == DatabaseType.SQLServer)
                    {
                        editorTarget = source;
                    }

                    using (IRepository sourceRepository = Helpers.DataConnectionHelper.CreateRepository(source.Value))
                    {
                        var generator = Helpers.DataConnectionHelper.CreateGenerator(sourceRepository, databaseInfo.DatabaseInfo.DatabaseType);
                        using (IRepository targetRepository = Helpers.DataConnectionHelper.CreateRepository(target.Value))
                        {
                            try
                            {
                                SqlCeDiff.CreateDiffScript(sourceRepository, targetRepository, generator, Properties.Settings.Default.DropTargetTables);

                                var sqlEditorWindow = package.CreateWindow<SqlEditorWindow>();
                                var editorControl = sqlEditorWindow.Content as SqlEditorControl;
                                editorControl.ExplorerControl = _parentWindow.Content as ExplorerControl;
                                Debug.Assert(editorControl != null);
                                editorControl.DatabaseInfo = editorTarget.Value;

                                string explain = @"-- This database diff script contains the following objects:
-- - Tables:  Any that are not in the destination
-- -          (tables that are only in the destination are NOT dropped, unless that option is set)
-- - Columns: Any added, deleted, changed columns for existing tables
-- - Indexes: Any added, deleted indexes for existing tables
-- - Foreign keys: Any added, deleted foreign keys for existing tables
-- ** Make sure to test against a production version of the destination database! ** " + Environment.NewLine + Environment.NewLine;

                                if (swap)
                                {
                                    explain += "-- ** Please note that the soruce and target have been swapped! ** " + Environment.NewLine + Environment.NewLine;
                                }

                                editorControl.SqlText = explain + generator.GeneratedScript;
                                Helpers.DataConnectionHelper.LogUsage("DatabaseScriptDiff");
                            }
                            catch (Exception ex)
                            {
                                Helpers.DataConnectionHelper.SendError(ex, DatabaseType.SQLCE35);
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, DatabaseType.SQLCE35);
            }

        }

        public void ExportToServer(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var databaseInfo = ValidateMenuInfo(sender);
                if (databaseInfo == null) return;

                Dictionary<string, DatabaseInfo> databaseList = DataConnectionHelper.GetDataConnections(package, includeServerConnections: true, serverConnectionsOnly: true);

                ExportDialog cd = new ExportDialog(databaseInfo.DatabaseInfo.Caption, databaseList);

                bool? result = cd.ShowModal();
                if (result.HasValue && result.Value == true && (cd.TargetDatabase.Key != null))
                {
                    BackgroundWorker bw = new BackgroundWorker();
                    List<object> parameters = new List<object>();
                    parameters.Add(databaseInfo.DatabaseInfo);
                    parameters.Add(cd.TargetDatabase.Value);

                    bw.DoWork += new DoWorkEventHandler(bw_DoWork);
                    bw.RunWorkerCompleted += (s, ea) =>
                    {
                        try
                        {
                            if (ea.Error != null)
                            {
                                Helpers.DataConnectionHelper.SendError(ea.Error, databaseInfo.DatabaseInfo.DatabaseType, false);
                            }
                            Helpers.DataConnectionHelper.LogUsage("DatabasesExportToServer");
                        }
                        finally
                        {
                            bw.Dispose();
                        }
                    };
                    bw.RunWorkerAsync(parameters);
                }
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, DatabaseType.SQLServer);
            }

        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            var parameters = e.Argument as List<object>;
            DatabaseInfo source = parameters[0] as DatabaseInfo;
            DatabaseInfo target = parameters[1] as DatabaseInfo;
            package.SetStatus("Starting export");
            using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(source))
            {
                string scriptRoot = System.IO.Path.GetTempFileName();
                string tempScript = scriptRoot + ".sqltb";
                var generator = Helpers.DataConnectionHelper.CreateGenerator(repository, tempScript, DatabaseType.SQLCE40);
                package.SetStatus("Scripting local database...");
                if (source.DatabaseType == DatabaseType.SQLite && Properties.Settings.Default.TruncateSQLiteStrings)
                {
                    generator.TruncateSQLiteStrings = true;
                }
                generator.ScriptDatabaseToFile(Scope.SchemaData);
                using (IRepository serverRepository = Helpers.DataConnectionHelper.CreateRepository(target))
                {
                    package.SetStatus("Exporting to server...");
                    //Handles large exports also... 
                    if (File.Exists(tempScript)) // Single file
                    {
                        serverRepository.ExecuteSqlFile(tempScript);
                    }
                    else // possibly multiple files - tmp2BB9.tmp_0.sqlce
                    {
                        for (int i = 0; i < 400; i++)
                        {
                            string testFile = string.Format("{0}_{1}{2}", scriptRoot, i.ToString("D4"), ".sqltb");
                            if (File.Exists(testFile))
                            {
                                serverRepository.ExecuteSqlFile(testFile);
                                package.SetStatus(string.Format("Exporting to server...{0}", i + 1));
                            }
                        }
                    }
                    package.SetStatus("Export complete");
                }
            }
        }

        public void UpgradeTo40(object sender, ExecutedRoutedEventArgs e)
        {
            if (EnvDTEHelper.ShowMessageBox("This will upgrade the 3.5 database to 4.0 format, and leave a renamed backup of the 3.5 database. Do you wish to proceed?",
                OLEMSGBUTTON.OLEMSGBUTTON_YESNO, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_SECOND, OLEMSGICON.OLEMSGICON_QUERY) == System.Windows.Forms.DialogResult.No)
                return;

            if (!Helpers.DataConnectionHelper.IsV40Installed())
            {
                EnvDTEHelper.ShowError("The SQL Server Compact 4.0 runtime is not installed, cannot upgrade. Install the 4.0 runtime.");
                return;
            }

            try
            {
                var databaseInfo = ValidateMenuInfo(sender);
                if (databaseInfo == null) return;

                SqlCeScripting.SqlCeHelper4 helper = new SqlCeScripting.SqlCeHelper4();
                    
                string path = helper.PathFromConnectionString(databaseInfo.DatabaseInfo.ConnectionString);

                if (!File.Exists(path))
                {
                    EnvDTEHelper.ShowError(string.Format("Database file in path: {0} could not be found", path));
                    return;
                }

                string newFile = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + "_35" + Path.GetExtension(path));
                if (System.IO.File.Exists(newFile))
                {
                    for (int i = 0; i < 100; i++)
                    {
                        newFile = Path.Combine(Path.GetDirectoryName(newFile),  Path.GetFileNameWithoutExtension(newFile) + "_" + i.ToString() + "." + Path.GetExtension(newFile));
                        if (!File.Exists(newFile))
                            break;
                    }
                }

                if (System.IO.File.Exists(newFile))
                {
                    EnvDTEHelper.ShowError("Could not create unique file name...");
                    return;
                }
                System.IO.File.Copy(path, newFile);
                helper.UpgradeTo40(databaseInfo.DatabaseInfo.ConnectionString);
                EnvDTEHelper.ShowMessage(string.Format("Database upgraded, version 3.5 database backed up to: {0}", newFile));
                if (databaseInfo.DatabaseInfo.FromServerExplorer)
                {
                    Helpers.DataConnectionHelper.RemoveDataConnection(package, databaseInfo.DatabaseInfo.ConnectionString, new Guid(Resources.SqlCompact35Provider));
                }
                else
                {
                    Helpers.DataConnectionHelper.RemoveDataConnection(databaseInfo.DatabaseInfo.ConnectionString);
                }
                Helpers.DataConnectionHelper.SaveDataConnection(databaseInfo.DatabaseInfo.ConnectionString, DatabaseType.SQLCE40, package);
                ExplorerControl control = _parentWindow.Content as ExplorerControl;
                control.BuildDatabaseTree();
                Helpers.DataConnectionHelper.LogUsage("DatabaseUpgrade40");
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, DatabaseType.SQLCE35);
            }

        }

        public void GenerateCeDgmlFiles(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;

            if (package == null) return;
            var dte = package.GetServiceHelper(typeof(EnvDTE.DTE)) as EnvDTE.DTE;

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
                    using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(databaseInfo.DatabaseInfo))
                    {
                        var generator = Helpers.DataConnectionHelper.CreateGenerator(repository, fileName, databaseInfo.DatabaseInfo.DatabaseType);
                        generator.GenerateSchemaGraph(databaseInfo.DatabaseInfo.Caption, Properties.Settings.Default.IncludeSystemTablesInDocumentation, false);
                        dte.ItemOperations.OpenFile(fileName);
                        dte.ActiveDocument.Activate();
                        Helpers.DataConnectionHelper.LogUsage("DatabaseScriptDGML");
                    }
                }
                catch (Exception ex)
                {
                    Helpers.DataConnectionHelper.SendError(ex, DatabaseType.SQLCE35, false);
                }
            }
        }

        public void GenerateDatabaseInfo(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;

            if (package == null) return;

            try
            {
                using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(databaseInfo.DatabaseInfo))
                {
                    var generator = Helpers.DataConnectionHelper.CreateGenerator(repository, databaseInfo.DatabaseInfo.DatabaseType);
                    generator.GenerateDatabaseInfo();
                    SpawnSqlEditorWindow(databaseInfo.DatabaseInfo, generator.GeneratedScript);
                    Helpers.DataConnectionHelper.LogUsage("DatabaseScriptInfo");
                }
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
            }
        }


        public void GenerateDocFiles(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;

            if (package == null) return;
            var dte = package.GetServiceHelper(typeof(EnvDTE.DTE)) as EnvDTE.DTE;

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
                    sqlCeDoc.CreateDocumentation(databaseInfo.DatabaseInfo, fileName, true, null);
                    if (System.IO.File.Exists(fileName))
                    {
                        EnvDTEHelper.LaunchUrl(fileName);
                    }
                    Helpers.DataConnectionHelper.LogUsage("DatabaseScriptDoc");
                }
                catch (Exception ex)
                {
                    Helpers.DataConnectionHelper.SendError(ex, DatabaseType.SQLCE35, false);
                }
            }

        }

        public void GenerateEdmxInProject(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;

            bool isEF6 = package.VSSupportsEF6();
            if (databaseInfo.DatabaseInfo.DatabaseType == DatabaseType.SQLite && !isEF6)
            {
                EnvDTEHelper.ShowError("Only Entity Framework 6.x is supported with SQLite");
                return;                
            }
            try
            {
                if (package == null) return;
                var dte = package.GetServiceHelper(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
                if (dte.Mode == vsIDEMode.vsIDEModeDebug)
                {
                    EnvDTEHelper.ShowError("Cannot generate code while debugging");
                    return;
                }

                var dteH = new Helpers.EnvDTEHelper();

                var project = dteH.GetProject(dte);
                if (project == null)
                {
                    EnvDTEHelper.ShowError("Please select a project in Solution Explorer, where you want the EDM to be placed");
                    return;
                }
                if (dte.Solution.SolutionBuild.BuildState == vsBuildState.vsBuildStateNotStarted)
                {
                    EnvDTEHelper.ShowError("Please build the project before proceeding");
                    return;
                }
                if (isEF6)
                {
                    if (databaseInfo.DatabaseInfo.DatabaseType == DatabaseType.SQLCE40 && !dteH.ContainsEFSqlCeReference(project))
                    {
                        EnvDTEHelper.ShowError("Please add the EntityFramework.SqlServerCompact NuGet package to the project");
                        return;
                    }
                    if (databaseInfo.DatabaseInfo.DatabaseType == DatabaseType.SQLCE35 && !dteH.ContainsEFSqlCeLegacyReference(project))
                    {
                        EnvDTEHelper.ShowError("Please add the EntityFramework.SqlServerCompact.Legacy NuGet package to the project");
                        return;
                    }
                    if (databaseInfo.DatabaseInfo.DatabaseType == DatabaseType.SQLite && !dteH.ContainsEFSQLiteReference(project))
                    {
                        EnvDTEHelper.ShowError("Please add the System.Data.SQLite.EF6 NuGet package to the project");
                        return;
                    }
                    if (!File.Exists(Path.Combine(dteH.GetVisualStudioInstallationDir(SqlCeToolboxPackage.VisualStudioVersion), "ItemTemplates\\CSharp\\Data\\1033\\DbCtxCSEF6\\CSharpDbContext.Context.tt")))
                    {
                        EnvDTEHelper.ShowError("Please install the Entity Framework 6 Tools in order to proceed");
                        return;
                    }
                }
                if (!dteH.AllowedProjectKinds.Contains(new Guid(project.Kind)))
                {
                    EnvDTEHelper.ShowError("The selected project type does not support Entity Framework (please let me know if I am wrong)");
                    return;
                }

                if (project.Properties.Item("TargetFrameworkMoniker") == null)
                {
                    EnvDTEHelper.ShowError("The selected project type does not have a TargetFrameworkMoniker");
                    return;
                }
                if (!project.Properties.Item("TargetFrameworkMoniker").Value.ToString().Contains(".NETFramework"))
                {
                    EnvDTEHelper.ShowError("The selected project type does not support .NET Desktop - wrong TargetFrameworkMoniker: " + project.Properties.Item("TargetFrameworkMoniker").Value.ToString());
                    return;
                }

                string provider = Resources.SqlCompact40InvariantName;

                if (databaseInfo.DatabaseInfo.DatabaseType == DatabaseType.SQLCE35)
                    provider = Resources.SqlCompact35InvariantName;

                if (databaseInfo.DatabaseInfo.DatabaseType == DatabaseType.SQLite)
                    provider = Resources.SQLiteEF6InvariantName;

                string model = Remove(Path.GetFileNameWithoutExtension(databaseInfo.DatabaseInfo.Caption), new List<char> { '#', '.', '+', '-', ' ' });
                EdmxDialog edmxDialog = new EdmxDialog();
                edmxDialog.ModelName = model;
                edmxDialog.ProjectName = project.Name;
                if (isEF6)
                    edmxDialog.HideAddPrivateConfig();
                
                using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(databaseInfo.DatabaseInfo))
                {
                    var tables = repository.GetAllTableNames();
                    var pks = repository.GetAllPrimaryKeys();
                    var checkedTables = new List<string>();
                    foreach (string tableName in tables)
                    {
                        var pk = pks.Where(k => k.TableName == tableName).FirstOrDefault();
                        if (pk.TableName != null)
                        {
                            checkedTables.Add(tableName);
                        }
                    }
                    edmxDialog.Tables = checkedTables;
                }

                bool? result = edmxDialog.ShowModal();
                if (result.HasValue && result.Value == true && (!string.IsNullOrWhiteSpace(edmxDialog.ModelName)))
                {
                    model = edmxDialog.ModelName;
                    string edmxPath = Path.Combine(Path.GetTempPath(), model + ".edmx");
                    Version ver = new Version(2, 0, 0, 0);
                    if (isEF6)
                    {
                        ver = new Version(3, 0, 0, 0);
                    }
                    EdmGen2.EdmGen2.ModelGen(databaseInfo.DatabaseInfo.ConnectionString, provider, model, Path.GetTempPath(), edmxDialog.ForeignKeys, edmxDialog.Pluralize, edmxDialog.Tables, ver);
                    if (EdmGen2.EdmGen2.Errors.Count > 0)
                    {
                        var sb = new System.Text.StringBuilder();
                        foreach (var item in EdmGen2.EdmGen2.Errors)
                        {
                            sb.AppendLine(item);
                        }
                        EnvDTEHelper.ShowError("Errors encountered during edmx generation" + System.Environment.NewLine + sb.ToString());
                    }
                    if (!File.Exists(edmxPath))
                    {
                        return;
                    }

                    bool proceed = true;
                    ProjectItem edmxItem = dteH.GetProjectEdmx(project, model);
                    if (edmxItem == null)
                    {
                        project.ProjectItems.AddFromFileCopy(edmxPath);
                    }
                    else
                    {
                        if (EnvDTEHelper.ShowMessageBox("The Entity Data Model already exists in the project, do you wish to replace it?", OLEMSGBUTTON.OLEMSGBUTTON_YESNO, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_SECOND, OLEMSGICON.OLEMSGICON_QUERY) == System.Windows.Forms.DialogResult.Yes) 
                        {
                            proceed = true;
                            edmxItem.Delete();
                            project.ProjectItems.AddFromFileCopy(edmxPath);

                        }
                        else
                        {
                            proceed = false;
                        }
                    }
                    if (isEF6)
                    {
                        ProjectItem addedItem = dteH.GetProjectEdmx(project, model);
                        if (addedItem != null)
                        {
                            if (project.CodeModel.Language == CodeModelLanguageConstants.vsCMLanguageCSharp)
                            {
                                var template = File.ReadAllText(Path.Combine(dteH.GetVisualStudioInstallationDir(SqlCeToolboxPackage.VisualStudioVersion), "ItemTemplates\\CSharp\\Data\\1033\\DbCtxCSEF6\\CSharpDbContext.Context.tt"));
                                template = template.Replace("$edmxInputFile$", model + ".edmx");
                                File.WriteAllText(Path.Combine(Path.GetTempPath(),  model + ".Context.tt"), template);
                                addedItem.ProjectItems.AddFromFileCopy(Path.Combine(Path.GetTempPath(),  model + ".Context.tt"));

                                template = File.ReadAllText(Path.Combine(dteH.GetVisualStudioInstallationDir(SqlCeToolboxPackage.VisualStudioVersion), "ItemTemplates\\CSharp\\Data\\1033\\DbCtxCSEF6\\CSharpDbContext.Types.tt"));
                                template = template.Replace("$edmxInputFile$", model + ".edmx");
                                File.WriteAllText(Path.Combine(Path.GetTempPath(), model + ".tt"), template);
                                addedItem.ProjectItems.AddFromFileCopy(Path.Combine(Path.GetTempPath(),  model + ".tt"));
                            }
                            if (project.CodeModel.Language == CodeModelLanguageConstants.vsCMLanguageVB)
                            {
                                var template = File.ReadAllText(Path.Combine(dteH.GetVisualStudioInstallationDir(SqlCeToolboxPackage.VisualStudioVersion), "ItemTemplates\\VisualBasic\\Data\\1033\\DbCtxVBEF6\\VBDbContext.Context.tt"));
                                template = template.Replace("$edmxInputFile$", model + ".edmx");
                                File.WriteAllText(Path.Combine(Path.GetTempPath(), model + ".Context.tt"), template);
                                addedItem.ProjectItems.AddFromFileCopy(Path.Combine(Path.GetTempPath(), model + ".Context.tt"));

                                template = File.ReadAllText(Path.Combine(dteH.GetVisualStudioInstallationDir(SqlCeToolboxPackage.VisualStudioVersion), "ItemTemplates\\VisualBasic\\Data\\1033\\DbCtxVBEF6\\VBDbContext.Types.tt"));
                                template = template.Replace("$edmxInputFile$", model + ".edmx");
                                File.WriteAllText(Path.Combine(Path.GetTempPath(), model + ".tt"), template);
                                addedItem.ProjectItems.AddFromFileCopy(Path.Combine(Path.GetTempPath(), model + ".tt"));
                            }
                            string diagramPath = Path.Combine(Path.GetTempPath(), model + "edmx.diagram");
                            File.WriteAllText(diagramPath, EdmGen2.EdmGen2.GetEDMXDiagram());
                            addedItem.ProjectItems.AddFromFileCopy(diagramPath);
                        }
                        project.Save();
                    }
                    if (edmxDialog.SaveConfig && proceed)
                    {
                        string prefix = "App";
                        string configPath = Path.Combine(Path.GetTempPath(), prefix + ".config");

                        ProjectItem item = dteH.GetProjectConfig(project);
                        if (item == null)
                        {
                            //Add app.config file to project
                            var cfgSb = new System.Text.StringBuilder();
                            cfgSb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
                            cfgSb.AppendLine("<configuration>");
                            cfgSb.AppendLine("</configuration>");
                            File.WriteAllText(configPath, cfgSb.ToString());
                            item = project.ProjectItems.AddFromFileCopy(configPath);
                        }

                        if (item != null)
                        {
                            Helpers.AppConfigHelper.BuildConfig(databaseInfo.DatabaseInfo.ConnectionString, project.FullName, provider, model, prefix, item.Name);
                            if (edmxDialog.AddPrivateConfig)
                            {
                                Helpers.AppConfigHelper.WriteSettings(item.FileNames[0], databaseInfo.DatabaseInfo.DatabaseType);
                            }
                        }
                    }
                    Helpers.DataConnectionHelper.LogUsage("DatabaseCreateEDMX");
                }
            }
            // EDM end
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
            }
        }


        public void GenerateDataContextInProject(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;

            var isDesktop = (bool)((MenuItem)sender).Tag;

            if (package == null) return;
            var dte = package.GetServiceHelper(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
            if (dte == null) return;
            if (dte.Mode == vsIDEMode.vsIDEModeDebug)
            {
                EnvDTEHelper.ShowError("Cannot generate code while debugging");
                return;                
            }

            var helper = DataConnectionHelper.CreateEngineHelper(databaseInfo.DatabaseInfo.DatabaseType);
            if (!helper.IsV35DbProviderInstalled())
            {
                EnvDTEHelper.ShowError("This feature requires the SQL Server Compact 3.5 SP2 DbProvider to be properly installed");
                return;                                
            }

            var dteH = new Helpers.EnvDTEHelper();

            var project = dteH.GetProject(dte);
            if (project == null)
            {
                EnvDTEHelper.ShowError("Please select a project in Solution Explorer, where you want the DataContext to be placed");
                return;
            }
            if (!isDesktop && !dteH.AllowedWPProjectKinds.Contains(new Guid(project.Kind)))
            {
                EnvDTEHelper.ShowError("The selected project type does not support Windows Phone (please let me know if I am wrong)");
                return;
            }
            if (isDesktop && !dteH.AllowedProjectKinds.Contains(new Guid(project.Kind)))
            {
                EnvDTEHelper.ShowError("The selected project type does not support LINQ to SQL (please let me know if I am wrong)");
                return;
            }
            if (project.Properties.Item("TargetFrameworkMoniker") == null)
            {
                EnvDTEHelper.ShowError("The selected project type does not support Windows Phone - missing TargetFrameworkMoniker");
                return;
            }
            if (!isDesktop)
            {
                if (project.Properties.Item("TargetFrameworkMoniker").Value.ToString() == "Silverlight,Version=v4.0,Profile=WindowsPhone71" 
                    || project.Properties.Item("TargetFrameworkMoniker").Value.ToString() == "WindowsPhone,Version=v8.0"
                    || project.Properties.Item("TargetFrameworkMoniker").Value.ToString() == "WindowsPhone,Version=v8.1"
                    )
                { }
                else
                {
                    EnvDTEHelper.ShowError("The selected project type does not support Windows Phone 7.1/8.0 - wrong TargetFrameworkMoniker: " + project.Properties.Item("TargetFrameworkMoniker").Value.ToString());
                    return;
                }
            }
            if (isDesktop && !project.Properties.Item("TargetFrameworkMoniker").Value.ToString().Contains(".NETFramework"))
            {
                EnvDTEHelper.ShowError("The selected project type does not support .NET Desktop - wrong TargetFrameworkMoniker: " + project.Properties.Item("TargetFrameworkMoniker").Value.ToString());
                return;
            }
            if (!isDesktop && databaseInfo.DatabaseInfo.DatabaseType != DatabaseType.SQLCE35)
            {
                EnvDTEHelper.ShowError("Sorry, only version 3.5 databases are supported for now");
                return;
            }

            string sqlMetalPath = string.Empty;
            var sqlMetalPaths = ProbeSqlMetalRegPaths();
            if (sqlMetalPaths.Count == 0)
            {
                EnvDTEHelper.ShowError("Could not find SQLMetal location in registry");
                return;
            }

            foreach (var path in sqlMetalPaths)
            {
                if (File.Exists(path))
                {
                    sqlMetalPath = path;
                    break;
                }
            }
            if (string.IsNullOrEmpty(sqlMetalPath))
            {
                EnvDTEHelper.ShowError("Could not find SQLMetal file location");
                return;                
            }

            var sdfFileName = string.Empty;

            try
            {
                using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(databaseInfo.DatabaseInfo))
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
                        EnvDTEHelper.ShowError(message);
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

                string model = Path.GetFileNameWithoutExtension(databaseInfo.DatabaseInfo.Caption).Replace(" ", string.Empty).Replace("#", string.Empty).Replace(".", string.Empty).Replace("-", string.Empty);
                model = model + "Context";
                DataContextDialog dcDialog = new DataContextDialog();
                dcDialog.ModelName = model;
                dcDialog.IsDesktop = isDesktop;
                dcDialog.ProjectName = project.Name;
                dcDialog.NameSpace = project.Properties.Item("DefaultNamespace").Value.ToString();
                if (EnvDTEHelper.VBProject == new Guid(project.Kind))
                {
                    dcDialog.CodeLanguage = "VB";
                }
                else
                {
                    dcDialog.CodeLanguage = "C#";
                }
                bool? result = dcDialog.ShowModal();
                if (result.HasValue && result.Value == true && (!string.IsNullOrWhiteSpace(dcDialog.ModelName)))
                {
                    if (dcDialog.AddRowversionColumns)
                    {
                        AddRowVersionColumns(databaseInfo);
                    }

                    string sdfPath = databaseInfo.DatabaseInfo.ConnectionString;

                    //If version 4.0, create a 3.5 schema sdf, and use that as connection string
                    if (isDesktop && databaseInfo.DatabaseInfo.DatabaseType == DatabaseType.SQLCE40)
                    {
                        var tempFile = Path.GetTempFileName();
                        using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(databaseInfo.DatabaseInfo))
                        {
                            var generator = Helpers.DataConnectionHelper.CreateGenerator(repository, tempFile, databaseInfo.DatabaseInfo.DatabaseType);
                            generator.ScriptDatabaseToFile(Scope.Schema);
                        }
                        sdfPath = Path.Combine(Path.GetTempPath(), sdfFileName);
                        using (Stream stream = new MemoryStream(Resources.SqlCe35AddinStore))
                        {
                            // Create a FileStream object to write a stream to a file 
                            using (FileStream fileStream = File.Create(sdfPath, (int)stream.Length))
                            {
                                // Fill the bytes[] array with the stream data 
                                byte[] bytesInStream = new byte[stream.Length];
                                stream.Read(bytesInStream, 0, (int)bytesInStream.Length);
                                // Use FileStream object to write to the specified file 
                                fileStream.Write(bytesInStream, 0, bytesInStream.Length);
                            }
                        }
                        DatabaseInfo info = new DatabaseInfo();
                        info.ConnectionString = "Data Source=" + sdfPath;
                        info.DatabaseType = DatabaseType.SQLCE35;
                        using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(info))
                        {
                            string script = File.ReadAllText(tempFile);
                            repository.ExecuteSql(script);
                        }
                        sdfPath = info.ConnectionString;
                    }

                    int versionNumber = GetVersionTableNumber(databaseInfo.DatabaseInfo, isDesktop);

                    model = dcDialog.ModelName;
                    string dcPath = Path.Combine(Path.GetTempPath(), model + ".cs");
                    if (dcDialog.CodeLanguage == "VB")
                    {
                        dcPath = Path.Combine(Path.GetTempPath(), model + ".vb");
                    }
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
                        EnvDTEHelper.ShowError("Error during SQL Metal run: " + sqlmetalResult);
                        return;
                    }

                    if (!isDesktop)
                    {
                        using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(databaseInfo.DatabaseInfo))
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
                    if (dcDialog.MultipleFiles)
                    {
                        Dictionary<string, string> classes = DataContextHelper.SplitIntoMultipleFiles(dcPath, dcDialog.NameSpace, model);
                        string projectPath = project.Properties.Item("FullPath").Value.ToString();

                        foreach (var item in classes)
                        {
                            string fileName = Path.Combine(projectPath, item.Key + ".cs");
                            if (File.Exists(fileName))
                            {
                                File.Delete(fileName);
                            }
                            File.WriteAllText(fileName, item.Value);
                            ProjectItem classItem = dteH.GetProjectDataContextClass(project, fileName);
                            if (classItem != null)
                            {
                                classItem.Delete();
                            }
                            project.ProjectItems.AddFromFile(fileName);
                        }

                    }
                    else
                    {
                        string extension = ".cs";
                        if (dcDialog.CodeLanguage == "VB")
                            extension = ".vb";
                        ProjectItem dcItem = dteH.GetProjectDC(project, model, extension);
                        if (dcItem == null)
                        {
                            project.ProjectItems.AddFromFileCopy(dcPath);
                        }
                        else
                        {
                            if (EnvDTEHelper.ShowMessageBox("The Data Context class already exists in the project, do you wish to replace it?", OLEMSGBUTTON.OLEMSGBUTTON_YESNO, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_SECOND, OLEMSGICON.OLEMSGICON_QUERY) == System.Windows.Forms.DialogResult.Yes) 
                            {
                                dcItem.Delete();
                                project.ProjectItems.AddFromFileCopy(dcPath);
                            }
                        }
                    }
                    EnvDTEHelper.AddReference(project, "System.Data.Linq");
                    if (dcDialog.AddConnectionStringBuilder)
                    {
                        string projectPath = project.Properties.Item("FullPath").Value.ToString();

                        string fileName = "LocalDatabaseConnectionStringBuilder.cs";

                        string filePath = Path.Combine(projectPath, fileName);
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }
                        using (Stream stream = new MemoryStream(Resources.LocalDatabaseConnectionStringBuilder))
                        {
                            // Create a FileStream object to write a stream to a file 
                            using (FileStream fileStream = File.Create(filePath, (int)stream.Length))
                            {
                                // Fill the bytes[] array with the stream data 
                                byte[] bytesInStream = new byte[stream.Length];
                                stream.Read(bytesInStream, 0, (int)bytesInStream.Length);
                                // Use FileStream object to write to the specified file 
                                fileStream.Write(bytesInStream, 0, bytesInStream.Length);
                            }
                        }
                        project.ProjectItems.AddFromFile(filePath);
                    }

                    // Creates __Version table and adds one row if desired
                    if (dcDialog.AddVersionTable)
                    {
                        using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(databaseInfo.DatabaseInfo))
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
                    Helpers.DataConnectionHelper.LogUsage("DatabaseCreateDC");
                }

            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        private List<string> ProbeSqlMetalRegPaths()
        {
            var paths = new List<string>();

            var sqlMetalRegPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SDKs\Windows\v10.0A", "InstallationFolder", string.Empty);
            if (!string.IsNullOrEmpty(sqlMetalRegPath))
            {
                paths.Add(Path.Combine(sqlMetalRegPath, "bin\\NETFX 4.6 Tools", "sqlmetal.exe"));
            }

            sqlMetalRegPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SDKs\Windows\v8.1A", "InstallationFolder", string.Empty);
            if (!string.IsNullOrEmpty(sqlMetalRegPath))
            {
                paths.Add(Path.Combine(sqlMetalRegPath, "bin\\NETFX 4.5.1 Tools", "sqlmetal.exe"));
            }

            sqlMetalRegPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SDKs\Windows\v8.0A", "InstallationFolder", string.Empty);
            if (!string.IsNullOrEmpty(sqlMetalRegPath))
            {
                paths.Add(Path.Combine(sqlMetalRegPath, "bin\\NETFX 4.0 Tools", "sqlmetal.exe"));
            }

            sqlMetalRegPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SDKs\Windows\v7.0A\WinSDK-NetFx40Tools", "InstallationFolder", string.Empty);
            if (!string.IsNullOrEmpty(sqlMetalRegPath))
            {
                paths.Add(Path.Combine(sqlMetalRegPath, "sqlmetal.exe"));
            }

            return paths;
        }

        public void GenerateModelCodeInProject(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;

            if (package == null) return;
            var dte = package.GetServiceHelper(typeof(EnvDTE.DTE)) as EnvDTE.DTE;

            var dteH = new Helpers.EnvDTEHelper();

            var project = dteH.GetProject(dte);
            if (project == null)
            {
                EnvDTEHelper.ShowError("Please select a project in Solution Explorer, where you want the DataAccess.cs to be placed");
                return;
            }
            if (!dteH.AllowedProjectKinds.Contains(new Guid(project.Kind)))
            {
                EnvDTEHelper.ShowError(string.Format("The selected project type {0} does not support sqlite-net (please let me know if I am wrong)", project.Kind));
                return;
            }
            if (project.CodeModel != null && project.CodeModel.Language != CodeModelLanguageConstants.vsCMLanguageCSharp)
            {
                EnvDTEHelper.ShowError("Unsupported code language, only C# is currently supported");
                return;
            }
            if (databaseInfo.DatabaseInfo.DatabaseType != DatabaseType.SQLite)
            {
                EnvDTEHelper.ShowError("Sorry, only SQLite databases are supported");
                return;
            }
            try
            {
                var defaultNamespace = "Model";
                if (project.Properties.Item("DefaultNamespace") != null)
                {
                    defaultNamespace = project.Properties.Item("DefaultNamespace").Value.ToString();
                }
                var projectPath = project.Properties.Item("FullPath").Value.ToString();
                using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(databaseInfo.DatabaseInfo))
                {
                    var generator = Helpers.DataConnectionHelper.CreateGenerator(repository, databaseInfo.DatabaseInfo.DatabaseType);
                    generator.GenerateSqliteNetModel(defaultNamespace);
                    
                    string fileName = Path.Combine(projectPath, "DataAccess.cs");
                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }
                    string warning = @"//This code was generated by a tool.
//Changes to this file will be lost if the code is regenerated."
+ Environment.NewLine +
"// See the blog post here for help on using the generated code: http://erikej.blogspot.dk/2014/10/database-first-with-sqlite-in-universal.html"
+ Environment.NewLine;

                    File.WriteAllText(fileName, warning + generator.GeneratedScript);
                    project.ProjectItems.AddFromFile(fileName);
                    EnvDTEHelper.ShowMessage("DataAccess.cs generated.");
                    Helpers.DataConnectionHelper.LogUsage("DatabaseSqliteNetCodegen");
                }
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType);
            }
        }

        private int GetVersionTableNumber(DatabaseInfo databaseInfo, bool isDesktop)
        {
            if (isDesktop)
                return 0;

            int version = 0;
            using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(databaseInfo))
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



        public void SyncFxDeprovisionDatabase(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;

            if (databaseInfo.DatabaseInfo.DatabaseType != DatabaseType.SQLCE35)
            {
                EnvDTEHelper.ShowError("Sorry, only version 3.5 databases are supported for now");
                return;
            }
            if (!Helpers.SyncFxHelper.IsProvisioned(databaseInfo.DatabaseInfo))
            {
                EnvDTEHelper.ShowError("The database is not provisioned, cannot deprovision");
                return;                
            }
            try
            {
                new SyncFxHelper().DeprovisionDatabase(databaseInfo.DatabaseInfo.ConnectionString);
                databaseInfo.ExplorerControl.RefreshTables(databaseInfo.DatabaseInfo);
                EnvDTEHelper.ShowMessage("Database deprovisioned");
                Helpers.DataConnectionHelper.LogUsage("DatabaseSyncDeprovision");
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
            }

        }

        public void SyncFxGenerateSnapshot(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;
            if (databaseInfo.DatabaseInfo.DatabaseType != DatabaseType.SQLCE35)
            {
                EnvDTEHelper.ShowError("Sorry, only version 3.5 databases are supported for now");
                return;
            }
            if (!Helpers.SyncFxHelper.IsProvisioned(databaseInfo.DatabaseInfo))
            {
                EnvDTEHelper.ShowError("The database is not provisioned, cannot generate snapshots");
                return;
            }

            var fd = new SaveFileDialog();
            fd.Title = "Save generated snapshot database file as";
            fd.Filter = DataConnectionHelper.GetSqlCeFileFilter();
            fd.OverwritePrompt = true;
            fd.ValidateNames = true;
            bool? result = fd.ShowDialog();
            if (result.HasValue && result.Value == true)
            {
                var fileName = fd.FileName;
                try
                {
                    SyncFxHelper.GenerateSnapshot(databaseInfo.DatabaseInfo.ConnectionString, fileName);
                    EnvDTEHelper.ShowMessage("Database snapshot generated.");
                    Helpers.DataConnectionHelper.LogUsage("DatabaseSyncSnapshot");
                }
                catch (Exception ex)
                {
                    Helpers.DataConnectionHelper.SendError(ex, DatabaseType.SQLCE35, false);
                }
            }
        }

        public void SyncFxProvisionScope(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;

            if (databaseInfo.DatabaseInfo.DatabaseType != DatabaseType.SQLCE35)
            {
                EnvDTEHelper.ShowError("Sorry, only version 3.5 databases are supported for now");
                return;
            }

            try
            {
                string model = Path.GetFileNameWithoutExtension(databaseInfo.DatabaseInfo.Caption).Replace(" ", string.Empty).Replace("#", string.Empty).Replace(".", string.Empty).Replace("-", string.Empty);
                SyncFxDialog sfd = new SyncFxDialog();
                int totalCount = 0;
                totalCount = SyncFxGetObjectsForSync(sfd, databaseInfo);
                sfd.ModelName = model;

                bool? res = sfd.ShowModal();

                if (res.HasValue && res.Value == true && (sfd.Tables.Count > 0))
                {
                    if (SyncFxHelper.SqlCeScopeExists(databaseInfo.DatabaseInfo.ConnectionString, model))
                    {
                        EnvDTEHelper.ShowError("Scope name is already in use. Please enter a different scope name.");
                        return;
                    }

                    model = sfd.ModelName;
                    new SyncFxHelper().ProvisionScope(databaseInfo.DatabaseInfo.ConnectionString, model, sfd.Columns.Where(c => sfd.Tables.Contains(c.TableName)).ToList());
                    EnvDTEHelper.ShowMessage("Scope: " + model + " has been provisioned.");
                    Helpers.DataConnectionHelper.LogUsage("DatabaseSyncProvision");
                }
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void SyncFxGenerateSyncCodeInProject(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;

            if (package == null) return;
            var dte = package.GetServiceHelper(typeof(EnvDTE.DTE)) as EnvDTE.DTE;

            var dteH = new Helpers.EnvDTEHelper();

            var project = dteH.GetProject(dte);
            if (project == null)
            {
                EnvDTEHelper.ShowError("Please select a project in Solution Explorer, where you want the SyncFx classes to be placed");
                return;
            }
            if (!dteH.AllowedProjectKinds.Contains(new Guid(project.Kind)))
            {
                EnvDTEHelper.ShowError("The selected project type does not support Sync Framework (please let me know if I am wrong)");
                return;
            }
            if (project.CodeModel.Language != CodeModelLanguageConstants.vsCMLanguageCSharp)
            {
                EnvDTEHelper.ShowError("Unsupported code language, only C# is currently supported");
                return;
            }
            if (project.Properties.Item("TargetFrameworkMoniker") == null)
            {
                EnvDTEHelper.ShowError("The selected project type does not support Sync Framework - missing TargetFrameworkMoniker");
                return;
            }
            if (!project.Properties.Item("TargetFrameworkMoniker").Value.ToString().Contains(".NETFramework"))
            {
                EnvDTEHelper.ShowError("The selected project type does not support .NET Desktop - wrong TargetFrameworkMoniker: " + project.Properties.Item("TargetFrameworkMoniker").Value.ToString());
                return;
            }
            if (databaseInfo.DatabaseInfo.DatabaseType != DatabaseType.SQLCE35)
            {
                EnvDTEHelper.ShowError("Sorry, only version 3.5 databases are supported for now");
                return;
            }

            try
            {
                string model = Path.GetFileNameWithoutExtension(databaseInfo.DatabaseInfo.Caption).Replace(" ", string.Empty).Replace("#", string.Empty).Replace(".", string.Empty).Replace("-", string.Empty);
                SyncFxDialog sfd = new SyncFxDialog();
                int totalCount = 0;
                totalCount = SyncFxGetObjectsForSync(sfd, databaseInfo);
                sfd.ModelName = model;

                bool? res = sfd.ShowModal();
                if (res.HasValue && res.Value == true && (sfd.Tables.Count > 0))
                {
                    model = sfd.ModelName;
                    var defaultNamespace = project.Properties.Item("DefaultNamespace").Value.ToString();

                    var classes = new Dictionary<string, string>();

                    classes = new SyncFxHelper().GenerateCodeForScope(string.Empty, databaseInfo.DatabaseInfo.ConnectionString, "SQLCE", model, sfd.Columns.Where(c => sfd.Tables.Contains(c.TableName)).ToList(), defaultNamespace);
                    var projectPath = project.Properties.Item("FullPath").Value.ToString();

                    foreach (var item in classes)
                    {
                        string fileName = Path.Combine(projectPath, item.Key + ".cs");
                        if (File.Exists(fileName))
                        {
                            File.Delete(fileName);
                        }
                        File.WriteAllText(fileName, item.Value);
                        project.ProjectItems.AddFromFile(fileName);
                    }
                    //Adding references - http://blogs.msdn.com/b/murat/archive/2008/07/30/envdte-adding-a-refernce-to-a-project.aspx
                    EnvDTEHelper.AddReference(project, "System.Data.SqlServerCe, Version=3.5.1.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91");

                    EnvDTEHelper.AddReference(project, "Microsoft.Synchronization, Version=2.1.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91");
                    EnvDTEHelper.AddReference(project, "Microsoft.Synchronization.Data, Version=3.1.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91");
                    EnvDTEHelper.AddReference(project, "Microsoft.Synchronization.Data.SqlServer, Version=3.1.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91");
                    EnvDTEHelper.AddReference(project, "Microsoft.Synchronization.Data.SqlServerCe, Version=3.1.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91");
                    EnvDTEHelper.ShowMessage("Scope: " + model + " code generated.");
                    Helpers.DataConnectionHelper.LogUsage("DatabaseSyncCodegen");
                }
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
            }
        }


        private static void AddRowVersionColumns(DatabaseMenuCommandParameters databaseInfo)
        {
            using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(databaseInfo.DatabaseInfo))
            {
                var list = repository.GetAllTableNames();
                var allColumns = repository.GetAllColumns();
                foreach (var table in list)
                {
                    if (!table.StartsWith("__"))
                    {
                        var rowVersionCol = allColumns.SingleOrDefault(c => c.TableName == table && c.DataType == "rowversion");
                        if (rowVersionCol == null)
                        {
                            repository.ExecuteSql(string.Format("ALTER TABLE {0} ADD COLUMN VersionColumn rowversion NOT NULL;{1}GO", table, Environment.NewLine));
                        }
                    }
                }
            }
        }


        public void RefreshTables(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;
            try
            {
                databaseInfo.ExplorerControl.RefreshTables(databaseInfo.DatabaseInfo);
                Helpers.DataConnectionHelper.LogUsage("DatabaseRefreshTables");
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        #endregion

        private static int SyncFxGetObjectsForSync(SyncFxDialog sfd, DatabaseMenuCommandParameters databaseInfo)
        {
            int totalCount;
            using (var repository = Helpers.DataConnectionHelper.CreateRepository(databaseInfo.DatabaseInfo))
            {
                sfd.Tables =
                    repository.GetAllTableNames().Where(
                        t => !t.EndsWith("scope_info") && !t.EndsWith("scope_config") && !t.EndsWith("schema_info") && !t.EndsWith("_tracking")).ToList();
                sfd.Columns =
                    repository.GetAllColumns().Where(
                        t =>
                        !t.TableName.EndsWith("scope_info") && !t.TableName.EndsWith("scope_config") && !t.TableName.EndsWith("schema_info") &&
                        !t.TableName.EndsWith("_tracking")).ToList();
                sfd.PrimaryKeyColumns =
                    repository.GetAllPrimaryKeys().Where(
                        t =>
                        !t.TableName.EndsWith("scope_info") && !t.TableName.EndsWith("scope_config") && !t.TableName.EndsWith("schema_info") &&
                        !t.TableName.EndsWith("_tracking")).ToList();
                totalCount = sfd.Tables.Count;
            }
            return totalCount;
        }

        private string Remove(string s, IEnumerable<char> chars)
        {
            return new string(s.Where(c => !chars.Contains(c)).ToArray());
        }

        private static DatabaseMenuCommandParameters ValidateMenuInfo(object sender)
        {
            var menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                return menuItem.CommandParameter as DatabaseMenuCommandParameters;
            }
            return null;
        }

    }
}

