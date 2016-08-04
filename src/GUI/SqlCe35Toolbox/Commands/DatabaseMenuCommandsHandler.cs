using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EnvDTE;
using EnvDTE80;
using ErikEJ.SqlCeScripting;
using ErikEJ.SqlCeToolbox.Dialogs;
using ErikEJ.SqlCeToolbox.Helpers;
using ErikEJ.SqlCeToolbox.ToolWindows;
using Microsoft.Win32;
using Microsoft.VisualStudio.Shell.Interop;
using System.ComponentModel;
using System.Text;

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
                Clipboard.SetData(DataFormats.FileDrop, new[] { path });
                DataConnectionHelper.LogUsage("DatabaseCopy");
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType);
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
                    var provider = new Guid(Resources.SqlCompact40Provider);
                    if (databaseInfo.DatabaseInfo.DatabaseType == DatabaseType.SQLCE35)
                    {
                        provider = new Guid(Resources.SqlCompact35Provider);
                    }
                    if (!DataConnectionHelper.DdexProviderIsInstalled(provider))
                    {
                        EnvDteHelper.ShowError("The DDEX provider is not installed, cannot remove connection");
                        return;
                    }            
                    DataConnectionHelper.RemoveDataConnection(package, databaseInfo.DatabaseInfo.ConnectionString, provider);
                }
                else
                {
                    DataConnectionHelper.RemoveDataConnection(databaseInfo.DatabaseInfo.ConnectionString);
                }
                var control = _parentWindow.Content as ExplorerControl;
                if (control != null) control.BuildDatabaseTree();
                DataConnectionHelper.LogUsage("DatabaseRemoveCe");
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, DatabaseType.SQLCE35);
            }
        }

        public void RenameConnection(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;
            if (databaseInfo.DatabaseInfo.FromServerExplorer) return;
            try
            {
                var ro = new RenameDialog(databaseInfo.DatabaseInfo.Caption);
                ro.ShowModal();
                if (ro.DialogResult.HasValue && ro.DialogResult.Value && !string.IsNullOrWhiteSpace(ro.NewName) && !databaseInfo.DatabaseInfo.Caption.Equals(ro.NewName))
                {
                    DataConnectionHelper.RenameDataConnection(databaseInfo.DatabaseInfo.ConnectionString, ro.NewName);
                    var control = _parentWindow.Content as ExplorerControl;
                    if (control != null) control.BuildDatabaseTree();
                    DataConnectionHelper.LogUsage("DatabaseRenameConnection");
                }
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType);
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
                if (!pwd.DialogResult.HasValue || !pwd.DialogResult.Value) return;
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
                    EnvDteHelper.ShowMessage("Password was set, and connection updated");
                }
                else
                {
                    EnvDteHelper.ShowMessage("Password was set, but could not update connection, please reconnect the database");
                }

                var control = _parentWindow.Content as ExplorerControl;
                if (control != null) control.BuildDatabaseTree();
                DataConnectionHelper.LogUsage("DatabaseMaintainSetPassword");
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
                var helper = DataConnectionHelper.CreateEngineHelper(databaseInfo.DatabaseInfo.DatabaseType);
                helper.ShrinkDatabase(databaseInfo.DatabaseInfo.ConnectionString);
                package.SetStatus("Shrink Database completed");
                DataConnectionHelper.LogUsage("DatabaseMaintainShrink");
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void CompactDatabase(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;
            try
            {
                var helper = DataConnectionHelper.CreateEngineHelper(databaseInfo.DatabaseInfo.DatabaseType);
                helper.CompactDatabase(databaseInfo.DatabaseInfo.ConnectionString);
                package.SetStatus("Compact Database completed");
                DataConnectionHelper.LogUsage("DatabaseMaintainCompact");
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void VerifyDatabase(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;
            try
            {
                var helper = DataConnectionHelper.CreateEngineHelper(databaseInfo.DatabaseInfo.DatabaseType);
                helper.VerifyDatabase(databaseInfo.DatabaseInfo.ConnectionString);
                package.SetStatus("Verify Database completed");
                DataConnectionHelper.LogUsage("DatabaseMaintainVerify");
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void RepairDatabaseDeleteCorruptedRows(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;
            try
            {
                var helper = DataConnectionHelper.CreateEngineHelper(databaseInfo.DatabaseInfo.DatabaseType);
                helper.RepairDatabaseDeleteCorruptedRows(databaseInfo.DatabaseInfo.ConnectionString);
                package.SetStatus("Repair Database completed");
                DataConnectionHelper.LogUsage("DatabaseMaintainRepair");
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void RepairDatabaseRecoverAllOrFail(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;
            try
            {
                var helper = DataConnectionHelper.CreateEngineHelper(databaseInfo.DatabaseInfo.DatabaseType);
                helper.RepairDatabaseRecoverAllOrFail(databaseInfo.DatabaseInfo.ConnectionString);
                package.SetStatus("Repair Database completed");
                DataConnectionHelper.LogUsage("DatabaseMaintainRepair");
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void RepairDatabaseRecoverAllPossibleRows(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;
            try
            {
                var helper = DataConnectionHelper.CreateEngineHelper(databaseInfo.DatabaseInfo.DatabaseType);
                helper.RepairDatabaseRecoverAllPossibleRows(databaseInfo.DatabaseInfo.ConnectionString);
                package.SetStatus("Repair Database completed");
                DataConnectionHelper.LogUsage("DatabaseMaintainRepair");
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void BuildTable(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;
            try
            {
                using (var repository = DataConnectionHelper.CreateRepository(databaseInfo.DatabaseInfo))
                {
                    var generator = DataConnectionHelper.CreateGenerator(repository, databaseInfo.DatabaseInfo.DatabaseType);
                    var tbd = new TableBuilderDialog(null, databaseInfo.DatabaseInfo.DatabaseType);
                    if (tbd.ShowModal() == true)
                    {
                        generator.GenerateTableCreate(tbd.TableName, tbd.TableColumns);
                        var script = generator.GeneratedScript;
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
                        DataConnectionHelper.LogUsage("TableBuild");
                    }
                }
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType);
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
                DataConnectionHelper.LogUsage("DatabaseOpenEditor");
                Debug.Assert(editorControl != null, "The SqlEditorWindow *should* have a editorControl with content.");
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, DatabaseType.SQLServer);
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
                DataConnectionHelper.LogUsage("DatabaseOpenEditor");
                Debug.Assert(editorControl != null, "The SqlEditorWindow *should* have a editorControl with content.");
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, DatabaseType.SQLServer);
            }
        }

        public void AddDescription(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;
            try
            {
                string desc;
                ExplorerControl.DescriptionCache = new Helpers.DescriptionHelper().GetDescriptions(databaseInfo.DatabaseInfo);
                desc = ExplorerControl.DescriptionCache.Where(d => d.Object == null && d.Parent == null).Select(d => d.Description).SingleOrDefault();
                var ro = new DescriptionDialog(desc) {IsDatabase = true};
                ro.ShowModal();
                if (ro.DialogResult.HasValue && ro.DialogResult.Value && !string.IsNullOrWhiteSpace(ro.TableDescription) && ro.TableDescription != desc)
                {
                    new Helpers.DescriptionHelper().SaveDescription(databaseInfo.DatabaseInfo, ExplorerControl.DescriptionCache, ro.TableDescription, null, null);
                }
                DataConnectionHelper.LogUsage("DatabaseAddDescription");
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void ScriptDatabase(object sender, ExecutedRoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null) return;
            var scope = (Scope)menuItem.Tag;

            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;

            try
            {
                int totalCount;
                var ptd = new PickTablesDialog();
                using (var repository = DataConnectionHelper.CreateRepository(databaseInfo.DatabaseInfo))
                {
                    ptd.Tables = repository.GetAllTableNamesForExclusion();
                    totalCount = ptd.Tables.Count;
                }

                var res = ptd.ShowModal();
                if (res == true && (ptd.Tables.Count < totalCount))
                {
                    var fd = new SaveFileDialog
                    {
                        Title = "Save generated database script as",
                        Filter =
                            "SQL Server Compact Script (*.sqlce)|*.sqlce|SQL Server Script (*.sql)|*.sql|All Files(*.*)|*.*"
                    };
                    if (scope == Scope.SchemaDataSQLite || scope == Scope.SchemaSQLite || databaseInfo.DatabaseInfo.DatabaseType == DatabaseType.SQLite)
                    {
                        fd.Filter = "SQLite Script (*.sql)|*.sql|All Files(*.*)|*.*";    
                    }
                    fd.OverwritePrompt = true;
                    fd.ValidateNames = true;
                    var result = fd.ShowDialog();
                    if (!result.HasValue || result.Value != true) return;
                    using (var repository = DataConnectionHelper.CreateRepository(databaseInfo.DatabaseInfo))
                    {
                        var generator = DataConnectionHelper.CreateGenerator(repository, fd.FileName, databaseInfo.DatabaseInfo.DatabaseType);
                        generator.ExcludeTables(ptd.Tables);
                        EnvDteHelper.ShowMessage(generator.ScriptDatabaseToFile(scope));
                        DataConnectionHelper.LogUsage("DatabaseScriptCe");
                    }
                }
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
            }

        }

        public void GenerateDiffScript(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var databaseInfo = ValidateMenuInfo(sender);
                if (databaseInfo == null) return;

                var databaseList = DataConnectionHelper.GetDataConnections(package, true, false);
                foreach (var info in DataConnectionHelper.GetOwnDataConnections())
                {
                    if (!databaseList.ContainsKey(info.Key))
                        databaseList.Add(info.Key, info.Value);
                }
                foreach (var info in databaseList)
                {
                    var sourceType = string.Empty;
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

                var cd = new CompareDialog(databaseInfo.DatabaseInfo.Caption, databaseList);

                var result = cd.ShowModal();
                if (!result.HasValue || !result.Value || (cd.TargetDatabase.Key == null)) return;
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

                using (var sourceRepository = DataConnectionHelper.CreateRepository(source.Value))
                {
                    var generator = DataConnectionHelper.CreateGenerator(sourceRepository, databaseInfo.DatabaseInfo.DatabaseType);
                    using (var targetRepository = DataConnectionHelper.CreateRepository(target.Value))
                    {
                        try
                        {
                            SqlCeDiff.CreateDiffScript(sourceRepository, targetRepository, generator, Properties.Settings.Default.DropTargetTables);

                            var sqlEditorWindow = package.CreateWindow<SqlEditorWindow>();
                            var editorControl = sqlEditorWindow.Content as SqlEditorControl;
                            if (editorControl != null)
                            {
                                editorControl.ExplorerControl = _parentWindow.Content as ExplorerControl;
                                Debug.Assert(editorControl != null);
                                editorControl.DatabaseInfo = editorTarget.Value;

                                var explain = @"-- This database diff script contains the following objects:
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
                            }
                            DataConnectionHelper.LogUsage("DatabaseScriptDiff");
                        }
                        catch (Exception ex)
                        {
                            DataConnectionHelper.SendError(ex, DatabaseType.SQLCE35);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, DatabaseType.SQLCE35);
            }

        }

        public void ExportToServer(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var databaseInfo = ValidateMenuInfo(sender);
                if (databaseInfo == null) return;
#if SSMS
                var connStr = DataConnectionHelper.PromptForConnectionString(package);
                if (string.IsNullOrEmpty(connStr)) return;
                var targetInfo = new DatabaseInfo {DatabaseType = DatabaseType.SQLServer, ConnectionString = connStr};
#else
                var databaseList = DataConnectionHelper.GetDataConnections(package, includeServerConnections: true, serverConnectionsOnly: true);
                var cd = new ExportDialog(databaseList);
                var result = cd.ShowModal();
                if (!result.HasValue || result.Value != true || (cd.TargetDatabase.Key == null)) return;
                var targetInfo = cd.TargetDatabase.Value;
#endif
                var bw = new BackgroundWorker();
                var parameters = new List<object> {databaseInfo.DatabaseInfo, targetInfo};

                bw.DoWork += bw_DoWork;
                bw.RunWorkerCompleted += (s, ea) =>
                {
                    try
                    {
                        if (ea.Error != null)
                        {
                            DataConnectionHelper.SendError(ea.Error, databaseInfo.DatabaseInfo.DatabaseType, false);
                        }
                        DataConnectionHelper.LogUsage("DatabasesExportToServer");
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
                DataConnectionHelper.SendError(ex, DatabaseType.SQLServer);
            }
        }
        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            var parameters = e.Argument as List<object>;
            if (parameters == null) return;
            var source = parameters[0] as DatabaseInfo;
            var target = parameters[1] as DatabaseInfo;
            package.SetStatus("Starting export");
            using (var repository = DataConnectionHelper.CreateRepository(source))
            {
                var scriptRoot = Path.GetTempFileName();
                var tempScript = scriptRoot + ".sqltb";
                var generator = DataConnectionHelper.CreateGenerator(repository, tempScript, DatabaseType.SQLCE40);
                package.SetStatus("Scripting local database...");
                if (source != null && (source.DatabaseType == DatabaseType.SQLite && Properties.Settings.Default.TruncateSQLiteStrings))
                {
                    generator.TruncateSQLiteStrings = true;
                }
                generator.ScriptDatabaseToFile(Scope.SchemaData);
                using (var serverRepository = DataConnectionHelper.CreateRepository(target))
                {
                    package.SetStatus("Exporting to server...");
                    //Handles large exports also... 
                    if (File.Exists(tempScript)) // Single file
                    {
                        serverRepository.ExecuteSqlFile(tempScript);
                    }
                    else // possibly multiple files - tmp2BB9.tmp_0.sqlce
                    {
                        for (var i = 0; i < 400; i++)
                        {
                            var testFile = string.Format("{0}_{1}{2}", scriptRoot, i.ToString("D4"), ".sqltb");
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
            if (EnvDteHelper.ShowMessageBox("This will upgrade the 3.5 database to 4.0 format, and leave a renamed backup of the 3.5 database. Do you wish to proceed?",
                OLEMSGBUTTON.OLEMSGBUTTON_YESNO, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_SECOND, OLEMSGICON.OLEMSGICON_QUERY) == System.Windows.Forms.DialogResult.No)
                return;

            if (!DataConnectionHelper.IsV40Installed())
            {
                EnvDteHelper.ShowError("The SQL Server Compact 4.0 runtime is not installed, cannot upgrade. Install the 4.0 runtime.");
                return;
            }
            try
            {
                var databaseInfo = ValidateMenuInfo(sender);
                if (databaseInfo == null) return;

                var helper = new SqlCeHelper4();
                var path = helper.PathFromConnectionString(databaseInfo.DatabaseInfo.ConnectionString);

                if (!File.Exists(path))
                {
                    EnvDteHelper.ShowError(string.Format("Database file in path: {0} could not be found", path));
                    return;
                }
                var path1 = Path.GetDirectoryName(path);
                if (path1 != null)
                {
                    var newFile = Path.Combine(path1, Path.GetFileNameWithoutExtension(path) + "_35" + Path.GetExtension(path));
                    if (File.Exists(newFile))
                    {
                        for (var i = 0; i < 100; i++)
                        {
                            newFile = Path.Combine(path1, Path.GetFileNameWithoutExtension(newFile) + "_" + i.ToString() + "." + Path.GetExtension(newFile));
                            if (!File.Exists(newFile))
                                break;
                        }
                    }

                    if (File.Exists(newFile))
                    {
                        EnvDteHelper.ShowError("Could not create unique file name...");
                        return;
                    }
                    File.Copy(path, newFile);
                    helper.UpgradeTo40(databaseInfo.DatabaseInfo.ConnectionString);
                    EnvDteHelper.ShowMessage(string.Format("Database upgraded, version 3.5 database backed up to: {0}", newFile));
                }
                if (databaseInfo.DatabaseInfo.FromServerExplorer)
                {
                    DataConnectionHelper.RemoveDataConnection(package, databaseInfo.DatabaseInfo.ConnectionString, new Guid(Resources.SqlCompact35Provider));
                }
                else
                {
                    DataConnectionHelper.RemoveDataConnection(databaseInfo.DatabaseInfo.ConnectionString);
                }
                DataConnectionHelper.SaveDataConnection(databaseInfo.DatabaseInfo.ConnectionString, DatabaseType.SQLCE40, package);
                var control = _parentWindow.Content as ExplorerControl;
                if (control != null) control.BuildDatabaseTree();
                DataConnectionHelper.LogUsage("DatabaseUpgrade40");
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, DatabaseType.SQLCE35);
            }

        }

        public void GenerateCeDgmlFiles(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;

            if (package == null) return;
            var dte = package.GetServiceHelper(typeof(DTE)) as DTE;

            var fd = new SaveFileDialog
            {
                Title = "Save generated DGML file as",
                Filter = "DGML (*.dgml)|*.dgml",
                OverwritePrompt = true,
                ValidateNames = true
            };
            var result = fd.ShowDialog();
            if (!result.HasValue || result.Value != true) return;
            var fileName = fd.FileName;
            try
            {
                using (var repository = DataConnectionHelper.CreateRepository(databaseInfo.DatabaseInfo))
                {
                    var generator = DataConnectionHelper.CreateGenerator(repository, fileName, databaseInfo.DatabaseInfo.DatabaseType);
                    generator.GenerateSchemaGraph(databaseInfo.DatabaseInfo.Caption, Properties.Settings.Default.IncludeSystemTablesInDocumentation, false);
                    if (dte != null)
                    {
                        dte.ItemOperations.OpenFile(fileName);
                        dte.ActiveDocument.Activate();
                    }
                    DataConnectionHelper.LogUsage("DatabaseScriptDGML");
                }
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, DatabaseType.SQLCE35, false);
            }
        }

        public void GenerateDatabaseInfo(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;

            if (package == null) return;

            try
            {
                using (var repository = DataConnectionHelper.CreateRepository(databaseInfo.DatabaseInfo))
                {
                    var generator = DataConnectionHelper.CreateGenerator(repository, databaseInfo.DatabaseInfo.DatabaseType);
                    generator.GenerateDatabaseInfo();
                    SpawnSqlEditorWindow(databaseInfo.DatabaseInfo, generator.GeneratedScript);
                    DataConnectionHelper.LogUsage("DatabaseScriptInfo");
                }
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
            }
        }


        public void GenerateDocFiles(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;

            var fd = new SaveFileDialog
            {
                Title = "Save generated documentation as",
                Filter = "HTML (*.html)|*.html|Raw XML (*.xml)|*.xml",
                OverwritePrompt = true,
                ValidateNames = true
            };
            //fd.Filter = "HTML (*.html)|*.html|WikiPlex (*.wiki)|*.wiki|Raw XML (*.xml)|*.xml";
            var result = fd.ShowDialog();
            if (!result.HasValue || result.Value != true) return;
            var fileName = fd.FileName;
            try
            {
                var sqlCeDoc = new SqlCeDbDoc();
                sqlCeDoc.CreateDocumentation(databaseInfo.DatabaseInfo, fileName, true, null);
                if (File.Exists(fileName))
                {
                    EnvDteHelper.LaunchUrl(fileName);
                }
                DataConnectionHelper.LogUsage("DatabaseScriptDoc");
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, DatabaseType.SQLCE35, false);
            }
        }

        public void GenerateEfPocoInProject(object sender, ExecutedRoutedEventArgs e)
        {
            EnvDteHelper.LaunchUrl("https://github.com/ErikEJ/SqlCeToolbox/wiki/EntityFramework-Reverse-POCO-Code-First-Generator");

            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;

            var isEf6 = SqlCeToolboxPackage.VsSupportsEf6();
            try
            {
                if (package == null) return;
                var dte = package.GetServiceHelper(typeof(DTE)) as DTE;
                if (dte == null) return;
                if (dte.Mode == vsIDEMode.vsIDEModeDebug)
                {
                    EnvDteHelper.ShowError("Cannot generate code while debugging");
                    return;
                }

                var dteH = new EnvDteHelper();

                var project = dteH.GetProject(dte);
                if (project == null)
                {
                    EnvDteHelper.ShowError("Please select a project in Solution Explorer, where you want the code to be placed");
                    return;
                }
                if (dte.Solution.SolutionBuild.BuildState == vsBuildState.vsBuildStateNotStarted)
                {
                    EnvDteHelper.ShowError("Please build the project before proceeding");
                    return;
                }
                if (isEf6)
                {
                    if (databaseInfo.DatabaseInfo.DatabaseType == DatabaseType.SQLCE40 && !dteH.ContainsEfSqlCeReference(project))
                    {
                        EnvDteHelper.ShowError("Please add the EntityFramework.SqlServerCompact NuGet package to the project");
                        return;
                    }
                    if (databaseInfo.DatabaseInfo.DatabaseType == DatabaseType.SQLCE35 && !dteH.ContainsEfSqlCeLegacyReference(project))
                    {
                        EnvDteHelper.ShowError("Please add the EntityFramework.SqlServerCompact.Legacy NuGet package to the project");
                        return;
                    }
                    if (!File.Exists(Path.Combine(dteH.GetVisualStudioInstallationDir(SqlCeToolboxPackage.VisualStudioVersion), "ItemTemplates\\CSharp\\Data\\1033\\DbCtxCSEF6\\CSharpDbContext.Context.tt")))
                    {
                        EnvDteHelper.ShowError("Please install the Entity Framework 6 Tools in order to proceed");
                        return;
                    }
                }
                if (!dteH.AllowedProjectKinds.Contains(new Guid(project.Kind)))
                {
                    EnvDteHelper.ShowError("The selected project type does not support Entity Framework (please let me know if I am wrong)");
                    return;
                }

                if (project.Properties.Item("TargetFrameworkMoniker") == null)
                {
                    EnvDteHelper.ShowError("The selected project type does not have a TargetFrameworkMoniker");
                    return;
                }
                if (!project.Properties.Item("TargetFrameworkMoniker").Value.ToString().Contains(".NETFramework"))
                {
                    EnvDteHelper.ShowError("The selected project type does not support .NET Desktop - wrong TargetFrameworkMoniker: " + project.Properties.Item("TargetFrameworkMoniker").Value);
                    return;
                }

                var dte2 = (DTE2)package.GetServiceHelper(typeof(DTE));
                // ReSharper disable once SuspiciousTypeConversion.Global
                var solution2 = dte2.Solution as Solution2;

                if (solution2 != null)
                {
                    var projectItemTemplate = solution2.GetProjectItemTemplate("EntityFramework Reverse POCO Code First Generator", "CSharp");
                    if (!string.IsNullOrEmpty(projectItemTemplate))
                    {
                        project.ProjectItems.AddFromTemplate(projectItemTemplate, "Database.tt");
                    }
                }
                DataConnectionHelper.LogUsage("DatabaseCreateEFPOCO");
            }
            // EDM end
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(FileNotFoundException))
                {
                    EnvDteHelper.ShowMessage("Unable to find the EF Reverse POCO Template, is it installed?");
                }
                else
                {
                    DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
                }
            }
        }

        public void GenerateEdmxInProject(object sender, ExecutedRoutedEventArgs e)
        {
            if (
                EnvDteHelper.ShowMessageBox(
                    "EDMX is becoming obsolete, consider using Code First from Database instead - do you wish to proceed?",
                    OLEMSGBUTTON.OLEMSGBUTTON_YESNO, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_SECOND, OLEMSGICON.OLEMSGICON_QUERY) !=
                System.Windows.Forms.DialogResult.Yes)
            {
                return;
            }
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;

            var isEf6 = SqlCeToolboxPackage.VsSupportsEf6();
            if (databaseInfo.DatabaseInfo.DatabaseType == DatabaseType.SQLite && !isEf6)
            {
                EnvDteHelper.ShowError("Only Entity Framework 6.x is supported with SQLite");
                return;                
            }
            try
            {
                if (package == null) return;
                var dte = package.GetServiceHelper(typeof(DTE)) as DTE;
                if (dte != null && dte.Mode == vsIDEMode.vsIDEModeDebug)
                {
                    EnvDteHelper.ShowError("Cannot generate code while debugging");
                    return;
                }

                var dteH = new EnvDteHelper();

                var project = dteH.GetProject(dte);
                if (project == null)
                {
                    EnvDteHelper.ShowError("Please select a project in Solution Explorer, where you want the EDM to be placed");
                    return;
                }
                if (dte != null && dte.Solution.SolutionBuild.BuildState == vsBuildState.vsBuildStateNotStarted)
                {
                    EnvDteHelper.ShowError("Please build the project before proceeding");
                    return;
                }
                if (isEf6)
                {
                    if (databaseInfo.DatabaseInfo.DatabaseType == DatabaseType.SQLCE40 && !dteH.ContainsEfSqlCeReference(project))
                    {
                        EnvDteHelper.ShowError("Please add the EntityFramework.SqlServerCompact NuGet package to the project");
                        return;
                    }
                    if (databaseInfo.DatabaseInfo.DatabaseType == DatabaseType.SQLCE35 && !dteH.ContainsEfSqlCeLegacyReference(project))
                    {
                        EnvDteHelper.ShowError("Please add the EntityFramework.SqlServerCompact.Legacy NuGet package to the project");
                        return;
                    }
                    if (databaseInfo.DatabaseInfo.DatabaseType == DatabaseType.SQLite && !dteH.ContainsEfsqLiteReference(project))
                    {
                        EnvDteHelper.ShowError("Please add the System.Data.SQLite.EF6 NuGet package to the project");
                        return;
                    }
                    if (!File.Exists(Path.Combine(dteH.GetVisualStudioInstallationDir(SqlCeToolboxPackage.VisualStudioVersion), "ItemTemplates\\CSharp\\Data\\1033\\DbCtxCSEF6\\CSharpDbContext.Context.tt")))
                    {
                        EnvDteHelper.ShowError("Please install the Entity Framework 6 Tools in order to proceed");
                        return;
                    }
                }
                if (!dteH.AllowedProjectKinds.Contains(new Guid(project.Kind)))
                {
                    EnvDteHelper.ShowError("The selected project type does not support Entity Framework (please let me know if I am wrong)");
                    return;
                }

                if (project.Properties.Item("TargetFrameworkMoniker") == null)
                {
                    EnvDteHelper.ShowError("The selected project type does not have a TargetFrameworkMoniker");
                    return;
                }
                if (!project.Properties.Item("TargetFrameworkMoniker").Value.ToString().Contains(".NETFramework"))
                {
                    EnvDteHelper.ShowError("The selected project type does not support .NET Desktop - wrong TargetFrameworkMoniker: " + project.Properties.Item("TargetFrameworkMoniker").Value);
                    return;
                }

                var provider = Resources.SqlCompact40InvariantName;
                if (databaseInfo.DatabaseInfo.DatabaseType == DatabaseType.SQLCE35)
                    provider = Resources.SqlCompact35InvariantName;
                if (databaseInfo.DatabaseInfo.DatabaseType == DatabaseType.SQLite)
                    provider = Resources.SQLiteEF6InvariantName;

                var model = Remove(Path.GetFileNameWithoutExtension(databaseInfo.DatabaseInfo.Caption), new List<char> { '#', '.', '+', '-', ' ' });
                var edmxDialog = new EdmxDialog
                {
                    ModelName = model,
                    ProjectName = project.Name
                };
                if (isEf6)
                    edmxDialog.HideAddPrivateConfig();
                
                using (var repository = DataConnectionHelper.CreateRepository(databaseInfo.DatabaseInfo))
                {
                    var tables = repository.GetAllTableNames();
                    var pks = repository.GetAllPrimaryKeys();
                    var checkedTables = (from tableName in tables let pk = pks.FirstOrDefault(k => k.TableName == tableName) where pk.TableName != null select tableName).ToList();
                    edmxDialog.Tables = checkedTables;
                }

                var result = edmxDialog.ShowModal();
                if (!result.HasValue || result.Value != true || (string.IsNullOrWhiteSpace(edmxDialog.ModelName)))
                    return;
                model = edmxDialog.ModelName;
                var edmxPath = Path.Combine(Path.GetTempPath(), model + ".edmx");
                var ver = new Version(2, 0, 0, 0);
                if (isEf6)
                {
                    ver = new Version(3, 0, 0, 0);
                }
                EdmGen2.ModelGen(databaseInfo.DatabaseInfo.ConnectionString, provider, model, Path.GetTempPath(), edmxDialog.ForeignKeys, edmxDialog.Pluralize, edmxDialog.Tables, ver);
                if (EdmGen2.Errors.Count > 0)
                {
                    var sb = new System.Text.StringBuilder();
                    foreach (var item in EdmGen2.Errors)
                    {
                        sb.AppendLine(item);
                    }
                    EnvDteHelper.ShowError("Errors encountered during edmx generation" + Environment.NewLine + sb);
                }
                if (!File.Exists(edmxPath))
                {
                    return;
                }

                var proceed = true;
                var edmxItem = dteH.GetProjectEdmx(project, model);
                if (edmxItem == null)
                {
                    project.ProjectItems.AddFromFileCopy(edmxPath);
                }
                else
                {
                    if (EnvDteHelper.ShowMessageBox("The Entity Data Model already exists in the project, do you wish to replace it?", OLEMSGBUTTON.OLEMSGBUTTON_YESNO, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_SECOND, OLEMSGICON.OLEMSGICON_QUERY) == System.Windows.Forms.DialogResult.Yes) 
                    {
                        edmxItem.Delete();
                        project.ProjectItems.AddFromFileCopy(edmxPath);
                    }
                    else
                    {
                        proceed = false;
                    }
                }
                if (isEf6)
                {
                    var addedItem = dteH.GetProjectEdmx(project, model);
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
                        var diagramPath = Path.Combine(Path.GetTempPath(), model + "edmx.diagram");
                        File.WriteAllText(diagramPath, EdmGen2.GetEdmxDiagram());
                        addedItem.ProjectItems.AddFromFileCopy(diagramPath);
                    }
                    project.Save();
                }
                if (edmxDialog.SaveConfig && proceed)
                {
                    var prefix = "App";
                    var configPath = Path.Combine(Path.GetTempPath(), prefix + ".config");

                    var item = dteH.GetProjectConfig(project);
                    if (item == null)
                    {
                        //Add app.config file to project
                        var cfgSb = new System.Text.StringBuilder();
                        cfgSb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
                        cfgSb.AppendLine("<configuration>");
                        cfgSb.AppendLine("</configuration>");
                        File.WriteAllText(configPath, cfgSb.ToString(), Encoding.UTF8);
                        item = project.ProjectItems.AddFromFileCopy(configPath);
                    }
                    if (item != null)
                    {
                        AppConfigHelper.BuildEfConfig(databaseInfo.DatabaseInfo.ConnectionString, project.FullName, provider, model, prefix, item.Name);
                        if (edmxDialog.AddPrivateConfig)
                        {
                            AppConfigHelper.WriteSettings(item.FileNames[0], databaseInfo.DatabaseInfo.DatabaseType);
                        }
                    }
                }
                DataConnectionHelper.LogUsage("DatabaseCreateEDMX");
            }
            // EDM end
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void GenerateDataContextInProject(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;

            var isDesktop = (bool)((MenuItem)sender).Tag;

            if (package == null) return;
            var dte = package.GetServiceHelper(typeof(DTE)) as DTE;
            if (dte == null) return;
            if (dte.Mode == vsIDEMode.vsIDEModeDebug)
            {
                EnvDteHelper.ShowError("Cannot generate code while debugging");
                return;                
            }

            var helper = DataConnectionHelper.CreateEngineHelper(databaseInfo.DatabaseInfo.DatabaseType);
            if (!helper.IsV35DbProviderInstalled())
            {
                EnvDteHelper.ShowError("This feature requires the SQL Server Compact 3.5 SP2 DbProvider to be properly installed");
                return;                                
            }

            var dteH = new EnvDteHelper();

            var project = dteH.GetProject(dte);
            if (project == null)
            {
                EnvDteHelper.ShowError("Please select a project in Solution Explorer, where you want the DataContext to be placed");
                return;
            }
            if (!isDesktop && !dteH.AllowedWpProjectKinds.Contains(new Guid(project.Kind)))
            {
                EnvDteHelper.ShowError("The selected project type does not support Windows Phone (please let me know if I am wrong)");
                return;
            }
            if (isDesktop && !dteH.AllowedProjectKinds.Contains(new Guid(project.Kind)))
            {
                EnvDteHelper.ShowError("The selected project type does not support LINQ to SQL (please let me know if I am wrong)");
                return;
            }
            if (project.Properties.Item("TargetFrameworkMoniker") == null)
            {
                EnvDteHelper.ShowError("The selected project type does not support Windows Phone - missing TargetFrameworkMoniker");
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
                    EnvDteHelper.ShowError("The selected project type does not support Windows Phone 7.1/8.0 - wrong TargetFrameworkMoniker: " + project.Properties.Item("TargetFrameworkMoniker").Value);
                    return;
                }
            }
            if (isDesktop && !project.Properties.Item("TargetFrameworkMoniker").Value.ToString().Contains(".NETFramework"))
            {
                EnvDteHelper.ShowError("The selected project type does not support .NET Desktop - wrong TargetFrameworkMoniker: " + project.Properties.Item("TargetFrameworkMoniker").Value);
                return;
            }
            if (!isDesktop && databaseInfo.DatabaseInfo.DatabaseType != DatabaseType.SQLCE35)
            {
                EnvDteHelper.ShowError("Sorry, only version 3.5 databases are supported for now");
                return;
            }

            var sqlMetalPath = string.Empty;
            var sqlMetalPaths = ProbeSqlMetalRegPaths();
            if (sqlMetalPaths.Count == 0)
            {
                EnvDteHelper.ShowError("Could not find SQLMetal location in registry");
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
                EnvDteHelper.ShowError("Could not find SQLMetal file location");
                return;                
            }

            var sdfFileName = string.Empty;

            try
            {
                using (var repository = DataConnectionHelper.CreateRepository(databaseInfo.DatabaseInfo))
                {
                    var tables = repository.GetAllTableNames();
                    var pks = repository.GetAllPrimaryKeys();
                    var checkedTables = string.Empty;
                    foreach (var tableName in tables)
                    {
                        var pk = pks.Where(k => k.TableName == tableName).FirstOrDefault();
                        if (pk.TableName == null)
                        {
                            checkedTables += tableName + Environment.NewLine;
                        }
                    }
                    if (!string.IsNullOrEmpty(checkedTables))
                    {
                        var message = string.Format("The tables below do not have Primary Keys defined,{0}and will not be generated properly:{1}{2}", Environment.NewLine, Environment.NewLine, checkedTables);
                        EnvDteHelper.ShowError(message);
                    }
                    var dbInfo = repository.GetDatabaseInfo();
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

                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(databaseInfo.DatabaseInfo.Caption);
                if (fileNameWithoutExtension != null)
                {
                    var model = fileNameWithoutExtension.Replace(" ", string.Empty).Replace("#", string.Empty).Replace(".", string.Empty).Replace("-", string.Empty);
                    model = model + "Context";
                    var dcDialog = new DataContextDialog();
                    dcDialog.ModelName = model;
                    dcDialog.IsDesktop = isDesktop;
                    dcDialog.ProjectName = project.Name;
                    dcDialog.NameSpace = project.Properties.Item("DefaultNamespace").Value.ToString();
                    if (EnvDteHelper.VbProject == new Guid(project.Kind))
                    {
                        dcDialog.CodeLanguage = "VB";
                    }
                    else
                    {
                        dcDialog.CodeLanguage = "C#";
                    }
                    var result = dcDialog.ShowModal();
                    if (!result.HasValue || result.Value != true || string.IsNullOrWhiteSpace(dcDialog.ModelName))
                        return;
                    if (dcDialog.AddRowversionColumns)
                    {
                        AddRowVersionColumns(databaseInfo);
                    }

                    var sdfPath = databaseInfo.DatabaseInfo.ConnectionString;

                    //If version 4.0, create a 3.5 schema sdf, and use that as connection string
                    if (isDesktop && databaseInfo.DatabaseInfo.DatabaseType == DatabaseType.SQLCE40)
                    {
                        var tempFile = Path.GetTempFileName();
                        using (var repository = DataConnectionHelper.CreateRepository(databaseInfo.DatabaseInfo))
                        {
                            var generator = DataConnectionHelper.CreateGenerator(repository, tempFile, databaseInfo.DatabaseInfo.DatabaseType);
                            generator.ScriptDatabaseToFile(Scope.Schema);
                        }
                        if (sdfFileName != null)
                        {
                            sdfPath = Path.Combine(Path.GetTempPath(), sdfFileName);
                        }
                        using (Stream stream = new MemoryStream(Resources.SqlCe35AddinStore))
                        {
                            // Create a FileStream object to write a stream to a file 
                            using (var fileStream = File.Create(sdfPath, (int)stream.Length))
                            {
                                // Fill the bytes[] array with the stream data 
                                var bytesInStream = new byte[stream.Length];
                                stream.Read(bytesInStream, 0, bytesInStream.Length);
                                // Use FileStream object to write to the specified file 
                                fileStream.Write(bytesInStream, 0, bytesInStream.Length);
                            }
                        }
                        var info = new DatabaseInfo
                        {
                            ConnectionString = "Data Source=" + sdfPath,
                            DatabaseType = DatabaseType.SQLCE35
                        };
                        using (var repository = DataConnectionHelper.CreateRepository(info))
                        {
                            var script = File.ReadAllText(tempFile);
                            repository.ExecuteSql(script);
                        }
                        sdfPath = info.ConnectionString;
                    }

                    var versionNumber = GetVersionTableNumber(databaseInfo.DatabaseInfo, isDesktop);

                    model = dcDialog.ModelName;
                    var dcPath = Path.Combine(Path.GetTempPath(), model + ".cs");
                    if (dcDialog.CodeLanguage == "VB")
                    {
                        dcPath = Path.Combine(Path.GetTempPath(), model + ".vb");
                    }
                    var parameters = " /provider:SQLCompact /code:\"" + dcPath + "\"";
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
                    var dcH = new DataContextHelper();

                    var sqlmetalResult = dcH.RunSqlMetal(sqlMetalPath, parameters);
                    if (!File.Exists(dcPath))
                    {
                        EnvDteHelper.ShowError("Error during SQL Metal run: " + sqlmetalResult);
                        return;
                    }

                    if (!isDesktop)
                    {
                        using (var repository = DataConnectionHelper.CreateRepository(databaseInfo.DatabaseInfo))
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
                        var classes = DataContextHelper.SplitIntoMultipleFiles(dcPath, dcDialog.NameSpace, model);
                        var projectPath = project.Properties.Item("FullPath").Value.ToString();

                        foreach (var item in classes)
                        {
                            var fileName = Path.Combine(projectPath, item.Key + ".cs");
                            if (File.Exists(fileName))
                            {
                                File.Delete(fileName);
                            }
                            File.WriteAllText(fileName, item.Value);
                            var classItem = dteH.GetProjectDataContextClass(project, fileName);
                            if (classItem != null)
                            {
                                classItem.Delete();
                            }
                            project.ProjectItems.AddFromFile(fileName);
                        }

                    }
                    else
                    {
                        var extension = ".cs";
                        if (dcDialog.CodeLanguage == "VB")
                            extension = ".vb";
                        var dcItem = dteH.GetProjectDc(project, model, extension);
                        if (dcItem == null)
                        {
                            project.ProjectItems.AddFromFileCopy(dcPath);
                        }
                        else
                        {
                            if (EnvDteHelper.ShowMessageBox("The Data Context class already exists in the project, do you wish to replace it?", OLEMSGBUTTON.OLEMSGBUTTON_YESNO, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_SECOND, OLEMSGICON.OLEMSGICON_QUERY) == System.Windows.Forms.DialogResult.Yes) 
                            {
                                dcItem.Delete();
                                project.ProjectItems.AddFromFileCopy(dcPath);
                            }
                        }
                    }
                    EnvDteHelper.AddReference(project, "System.Data.Linq");
                    if (dcDialog.AddConnectionStringBuilder)
                    {
                        var projectPath = project.Properties.Item("FullPath").Value.ToString();

                        var fileName = "LocalDatabaseConnectionStringBuilder.cs";

                        var filePath = Path.Combine(projectPath, fileName);
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }
                        using (Stream stream = new MemoryStream(Resources.LocalDatabaseConnectionStringBuilder))
                        {
                            // Create a FileStream object to write a stream to a file 
                            using (var fileStream = File.Create(filePath, (int)stream.Length))
                            {
                                // Fill the bytes[] array with the stream data 
                                var bytesInStream = new byte[stream.Length];
                                stream.Read(bytesInStream, 0, bytesInStream.Length);
                                // Use FileStream object to write to the specified file 
                                fileStream.Write(bytesInStream, 0, bytesInStream.Length);
                            }
                        }
                        project.ProjectItems.AddFromFile(filePath);
                    }

                    // Creates __Version table and adds one row if desired
                    if (dcDialog.AddVersionTable)
                    {
                        using (var repository = DataConnectionHelper.CreateRepository(databaseInfo.DatabaseInfo))
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
                    DataConnectionHelper.LogUsage("DatabaseCreateDC");
                }
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
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
            var dte = package.GetServiceHelper(typeof(DTE)) as DTE;

            var dteH = new EnvDteHelper();

            var project = dteH.GetProject(dte);
            if (project == null)
            {
                EnvDteHelper.ShowError("Please select a project in Solution Explorer, where you want the DataAccess.cs to be placed");
                return;
            }
            if (!dteH.AllowedProjectKinds.Contains(new Guid(project.Kind)))
            {
                EnvDteHelper.ShowError(string.Format("The selected project type {0} does not support sqlite-net (please let me know if I am wrong)", project.Kind));
                return;
            }
            if (project.CodeModel != null && project.CodeModel.Language != CodeModelLanguageConstants.vsCMLanguageCSharp)
            {
                EnvDteHelper.ShowError("Unsupported code language, only C# is currently supported");
                return;
            }
            if (databaseInfo.DatabaseInfo.DatabaseType != DatabaseType.SQLite)
            {
                EnvDteHelper.ShowError("Sorry, only SQLite databases are supported");
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
                using (var repository = DataConnectionHelper.CreateRepository(databaseInfo.DatabaseInfo))
                {
                    var generator = DataConnectionHelper.CreateGenerator(repository, databaseInfo.DatabaseInfo.DatabaseType);
                    generator.GenerateSqliteNetModel(defaultNamespace);
                    
                    var fileName = Path.Combine(projectPath, "DataAccess.cs");
                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }
                    var warning = @"//This code was generated by a tool.
//Changes to this file will be lost if the code is regenerated."
+ Environment.NewLine +
"// See the blog post here for help on using the generated code: http://erikej.blogspot.dk/2014/10/database-first-with-sqlite-in-universal.html"
+ Environment.NewLine;

                    File.WriteAllText(fileName, warning + generator.GeneratedScript);
                    project.ProjectItems.AddFromFile(fileName);
                    EnvDteHelper.ShowMessage("DataAccess.cs generated.");
                    DataConnectionHelper.LogUsage("DatabaseSqliteNetCodegen");
                }
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType);
            }
        }

        private int GetVersionTableNumber(DatabaseInfo databaseInfo, bool isDesktop)
        {
            if (isDesktop)
                return 0;

            var version = 0;
            using (var repository = DataConnectionHelper.CreateRepository(databaseInfo))
            {
                var list = repository.GetAllTableNames();
                if (list.Contains("__VERSION"))
                {
                    var ds = repository.ExecuteSql(@"
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
                EnvDteHelper.ShowError("Sorry, only version 3.5 databases are supported for now");
                return;
            }
            if (!SyncFxHelper.IsProvisioned(databaseInfo.DatabaseInfo))
            {
                EnvDteHelper.ShowError("The database is not provisioned, cannot deprovision");
                return;                
            }
            try
            {
                new SyncFxHelper().DeprovisionDatabase(databaseInfo.DatabaseInfo.ConnectionString);
                databaseInfo.ExplorerControl.RefreshTables(databaseInfo.DatabaseInfo);
                EnvDteHelper.ShowMessage("Database deprovisioned");
                DataConnectionHelper.LogUsage("DatabaseSyncDeprovision");
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
            }

        }

        public void SyncFxGenerateSnapshot(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;
            if (databaseInfo.DatabaseInfo.DatabaseType != DatabaseType.SQLCE35)
            {
                EnvDteHelper.ShowError("Sorry, only version 3.5 databases are supported for now");
                return;
            }
            if (!SyncFxHelper.IsProvisioned(databaseInfo.DatabaseInfo))
            {
                EnvDteHelper.ShowError("The database is not provisioned, cannot generate snapshots");
                return;
            }

            var fd = new SaveFileDialog
            {
                Title = "Save generated snapshot database file as",
                Filter = DataConnectionHelper.GetSqlCeFileFilter(),
                OverwritePrompt = true,
                ValidateNames = true
            };
            var result = fd.ShowDialog();
            if (!result.HasValue || !result.Value) return;
            var fileName = fd.FileName;
            try
            {
                SyncFxHelper.GenerateSnapshot(databaseInfo.DatabaseInfo.ConnectionString, fileName);
                EnvDteHelper.ShowMessage("Database snapshot generated.");
                DataConnectionHelper.LogUsage("DatabaseSyncSnapshot");
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, DatabaseType.SQLCE35, false);
            }
        }

        public void SyncFxProvisionScope(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;

            if (databaseInfo.DatabaseInfo.DatabaseType != DatabaseType.SQLCE35)
            {
                EnvDteHelper.ShowError("Sorry, only version 3.5 databases are supported for now");
                return;
            }

            try
            {
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(databaseInfo.DatabaseInfo.Caption);
                if (fileNameWithoutExtension == null) return;
                var model = fileNameWithoutExtension.Replace(" ", string.Empty).Replace("#", string.Empty).Replace(".", string.Empty).Replace("-", string.Empty);
                var sfd = new SyncFxDialog {ModelName = model};

                var res = sfd.ShowModal();

                if (!res.HasValue || res.Value != true || (sfd.Tables.Count <= 0)) return;
                if (SyncFxHelper.SqlCeScopeExists(databaseInfo.DatabaseInfo.ConnectionString, model))
                {
                    EnvDteHelper.ShowError("Scope name is already in use. Please enter a different scope name.");
                    return;
                }

                model = sfd.ModelName;
                new SyncFxHelper().ProvisionScope(databaseInfo.DatabaseInfo.ConnectionString, model, sfd.Columns.Where(c => sfd.Tables.Contains(c.TableName)).ToList());
                EnvDteHelper.ShowMessage("Scope: " + model + " has been provisioned.");
                DataConnectionHelper.LogUsage("DatabaseSyncProvision");
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void SyncFxGenerateSyncCodeInProject(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;

            if (package == null) return;
            var dte = package.GetServiceHelper(typeof(DTE)) as DTE;

            var dteH = new EnvDteHelper();

            var project = dteH.GetProject(dte);
            if (project == null)
            {
                EnvDteHelper.ShowError("Please select a project in Solution Explorer, where you want the SyncFx classes to be placed");
                return;
            }
            if (!dteH.AllowedProjectKinds.Contains(new Guid(project.Kind)))
            {
                EnvDteHelper.ShowError("The selected project type does not support Sync Framework (please let me know if I am wrong)");
                return;
            }
            if (project.CodeModel.Language != CodeModelLanguageConstants.vsCMLanguageCSharp)
            {
                EnvDteHelper.ShowError("Unsupported code language, only C# is currently supported");
                return;
            }
            if (project.Properties.Item("TargetFrameworkMoniker") == null)
            {
                EnvDteHelper.ShowError("The selected project type does not support Sync Framework - missing TargetFrameworkMoniker");
                return;
            }
            if (!project.Properties.Item("TargetFrameworkMoniker").Value.ToString().Contains(".NETFramework"))
            {
                EnvDteHelper.ShowError("The selected project type does not support .NET Desktop - wrong TargetFrameworkMoniker: " + project.Properties.Item("TargetFrameworkMoniker").Value);
                return;
            }
            if (databaseInfo.DatabaseInfo.DatabaseType != DatabaseType.SQLCE35)
            {
                EnvDteHelper.ShowError("Sorry, only version 3.5 databases are supported for now");
                return;
            }

            try
            {
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(databaseInfo.DatabaseInfo.Caption);
                if (fileNameWithoutExtension != null)
                {
                    var model = fileNameWithoutExtension.Replace(" ", string.Empty).Replace("#", string.Empty).Replace(".", string.Empty).Replace("-", string.Empty);
                    var sfd = new SyncFxDialog {ModelName = model};

                    var res = sfd.ShowModal();
                    if (!res.HasValue || res.Value != true || (sfd.Tables.Count <= 0)) return;
                    model = sfd.ModelName;
                    var defaultNamespace = project.Properties.Item("DefaultNamespace").Value.ToString();

                    var classes = new SyncFxHelper().GenerateCodeForScope(string.Empty, databaseInfo.DatabaseInfo.ConnectionString, "SQLCE", model, sfd.Columns.Where(c => sfd.Tables.Contains(c.TableName)).ToList(), defaultNamespace);
                    var projectPath = project.Properties.Item("FullPath").Value.ToString();

                    foreach (var item in classes)
                    {
                        var fileName = Path.Combine(projectPath, item.Key + ".cs");
                        if (File.Exists(fileName))
                        {
                            File.Delete(fileName);
                        }
                        File.WriteAllText(fileName, item.Value);
                        project.ProjectItems.AddFromFile(fileName);
                    }
                    //Adding references - http://blogs.msdn.com/b/murat/archive/2008/07/30/envdte-adding-a-refernce-to-a-project.aspx
                    EnvDteHelper.AddReference(project, "System.Data.SqlServerCe, Version=3.5.1.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91");

                    EnvDteHelper.AddReference(project, "Microsoft.Synchronization, Version=2.1.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91");
                    EnvDteHelper.AddReference(project, "Microsoft.Synchronization.Data, Version=3.1.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91");
                    EnvDteHelper.AddReference(project, "Microsoft.Synchronization.Data.SqlServer, Version=3.1.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91");
                    EnvDteHelper.AddReference(project, "Microsoft.Synchronization.Data.SqlServerCe, Version=3.1.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91");
                    EnvDteHelper.ShowMessage("Scope: " + model + " code generated.");
                    DataConnectionHelper.LogUsage("DatabaseSyncCodegen");
                }
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        private static void AddRowVersionColumns(DatabaseMenuCommandParameters databaseInfo)
        {
            using (var repository = DataConnectionHelper.CreateRepository(databaseInfo.DatabaseInfo))
            {
                var list = repository.GetAllTableNames();
                var allColumns = repository.GetAllColumns();
                foreach (var table in list)
                {
                    if (table.StartsWith("__")) continue;
                    var rowVersionCol = allColumns.SingleOrDefault(c => c.TableName == table && c.DataType == "rowversion");
                    if (rowVersionCol == null)
                    {
                        repository.ExecuteSql(string.Format("ALTER TABLE {0} ADD COLUMN VersionColumn rowversion NOT NULL;{1}GO", table, Environment.NewLine));
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
                DataConnectionHelper.LogUsage("DatabaseRefreshTables");
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
            }
        }

#endregion

        private static string Remove(string s, IEnumerable<char> chars)
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
