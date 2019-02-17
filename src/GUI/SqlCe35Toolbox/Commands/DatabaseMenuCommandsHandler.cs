using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using EnvDTE;
using EnvDTE80;
using ErikEJ.SqlCeScripting;
using ErikEJ.SqlCeToolbox.Dialogs;
using ErikEJ.SqlCeToolbox.Helpers;
using ErikEJ.SqlCeToolbox.ToolWindows;
using Microsoft.Win32;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Text;

namespace ErikEJ.SqlCeToolbox.Commands
{
    public class DatabaseMenuCommandsHandler
    {
        private readonly ExplorerToolWindow _parentWindow;
        private readonly SqlCeToolboxPackage package;

        public DatabaseMenuCommandsHandler(ExplorerToolWindow parent)
        {
            _parentWindow = parent;
            package = _parentWindow.Package as SqlCeToolboxPackage;
        }

        public void RemoveDatabaseConnection(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var databaseInfo = ValidateMenuInfo(sender);
                if (databaseInfo == null) return;

                if (databaseInfo.DatabaseInfo.FromServerExplorer)
                {
                    bool providerInstalled = true;
                    var provider = new Guid(Resources.SqlCompact40Provider);
                    if (databaseInfo.DatabaseInfo.DatabaseType == DatabaseType.SQLCE35)
                    {
                        provider = new Guid(Resources.SqlCompact35Provider);
                    }
                    if (databaseInfo.DatabaseInfo.DatabaseType == DatabaseType.SQLite)
                    {
                        provider = new Guid(Resources.SQLiteProvider);
                    }

                    providerInstalled = DataConnectionHelper.DdexProviderIsInstalled(provider);

                    if (!providerInstalled)
                    {
                        if (databaseInfo.DatabaseInfo.DatabaseType == DatabaseType.SQLCE40)
                        {
                            provider = new Guid(Resources.SqlCompact40PrivateProvider);
                        }

                        if (databaseInfo.DatabaseInfo.DatabaseType == DatabaseType.SQLite)
                        {
                            provider = new Guid(Resources.SqlitePrivateProvider);
                        }

                        if (!DataConnectionHelper.DdexProviderIsInstalled(provider))
                        {
                            EnvDteHelper.ShowError("The DDEX provider is not installed, cannot remove connection");
                            return;
                        }
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

#region Maintenance menu items

        public void ShrinkDatabase(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;
            try
            {
                var helper = Helpers.RepositoryHelper.CreateEngineHelper(databaseInfo.DatabaseInfo.DatabaseType);
                helper.ShrinkDatabase(databaseInfo.DatabaseInfo.ConnectionString);
                package.SetStatus("Shrink completed");
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
                var helper = Helpers.RepositoryHelper.CreateEngineHelper(databaseInfo.DatabaseInfo.DatabaseType);
                helper.CompactDatabase(databaseInfo.DatabaseInfo.ConnectionString);
                package.SetStatus("Compact completed");
                DataConnectionHelper.LogUsage("DatabaseMaintainCompact");
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
            }
        }
#endregion
        public void BuildTable(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;
            try
            {
                using (var repository = Helpers.RepositoryHelper.CreateRepository(databaseInfo.DatabaseInfo))
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

        private void SpawnSqlEditorWindow(DatabaseInfo databaseInfo,  string sqlScript)
        {
            try
            {
                if (databaseInfo == null) return;
                if (package == null) return;
                Debug.Assert(package != null, "Package property of the Explorere Tool Window should never be null, have you tried to create it manually and not through FindToolWindow()?");
                var sqlEditorWindow = package.CreateWindow<SqlEditorWindow>();
                if (sqlEditorWindow == null) return;
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
                string sqlScript = null;
                var databaseInfo = ValidateMenuInfo(sender);

                if (databaseInfo == null) return;
                if (package == null) return;
                Debug.Assert(package != null, "Package property of the Explorere Tool Window should never be null, have you tried to create it manually and not through FindToolWindow()?");
                var sqlEditorWindow = package.CreateWindow<SqlEditorWindow>();
                if (sqlEditorWindow == null) return;
                var editorControl = sqlEditorWindow.Content as SqlEditorControl;
                Debug.Assert(editorControl != null);
                editorControl.DatabaseInfo = databaseInfo.DatabaseInfo;
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
                using (var repository = Helpers.RepositoryHelper.CreateRepository(databaseInfo.DatabaseInfo))
                {
                    ptd.Tables = repository.GetAllTableNamesForExclusion();
                    totalCount = ptd.Tables.Count;
                }

                var res = ptd.ShowModal();
                if (res == true && (ptd.Tables.Count < totalCount))
                {
                    var fd = new SaveFileDialog
                    {
                        Title = "Save generated script as",
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
                    using (var repository = Helpers.RepositoryHelper.CreateRepository(databaseInfo.DatabaseInfo))
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

                using (var sourceRepository = Helpers.RepositoryHelper.CreateRepository(source.Value))
                {
                    var generator = DataConnectionHelper.CreateGenerator(sourceRepository, databaseInfo.DatabaseInfo.DatabaseType);
                    using (var targetRepository = Helpers.RepositoryHelper.CreateRepository(target.Value))
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

                bw.DoWork += bw_DoExportWork;
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
        private void bw_DoExportWork(object sender, DoWorkEventArgs e)
        {
            var parameters = e.Argument as List<object>;
            if (parameters == null) return;
            var source = parameters[0] as DatabaseInfo;
            var target = parameters[1] as DatabaseInfo;
            package.SetStatus("Starting export");
            using (var repository = Helpers.RepositoryHelper.CreateRepository(source))
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
                using (var serverRepository = Helpers.RepositoryHelper.CreateRepository(target))
                {
                    package.SetStatus("Exporting to server...");
                    //Handles large exports also... 
                    if (File.Exists(tempScript)) // Single file
                    {
                        serverRepository.ExecuteSqlFile(tempScript);
                    }
                    else // possibly multiple files - tmp2BB9.tmp_0001.sqltb
                    {
                        var count = Directory.GetFiles(Path.GetDirectoryName(scriptRoot), Path.GetFileName(scriptRoot) + "*", SearchOption.AllDirectories).Count();
                        for (var i = 0; i < count + 1; i++)
                        {
                            var testFile = string.Format("{0}_{1}{2}", scriptRoot, i.ToString("D4"), ".sqltb");
                            if (File.Exists(testFile))
                            {
                                serverRepository.ExecuteSqlFile(testFile);
                                package.SetProgress("Exporting to server...", (uint)i + 1, (uint)count - 1);
                                TryDeleteFile(testFile);
                            }
                            else
                            {
                                if (i > 0) break;
                            }
                        }
                        package.SetStatus(null);
                    }
                    package.SetStatus("Export complete");
                }
            }
        }

        private void TryDeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch
            {
                //Ignored
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
                using (var repository = Helpers.RepositoryHelper.CreateRepository(databaseInfo.DatabaseInfo))
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
                using (var repository = Helpers.RepositoryHelper.CreateRepository(databaseInfo.DatabaseInfo))
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
                using (var repository = Helpers.RepositoryHelper.CreateRepository(databaseInfo.DatabaseInfo))
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

        public void RefreshViews(object sender, ExecutedRoutedEventArgs e)
        {
            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;
            try
            {
                databaseInfo.ExplorerControl.RefreshViews(databaseInfo.DatabaseInfo);
                DataConnectionHelper.LogUsage("DatabaseRefreshViews");
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, databaseInfo.DatabaseInfo.DatabaseType, false);
            }
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
