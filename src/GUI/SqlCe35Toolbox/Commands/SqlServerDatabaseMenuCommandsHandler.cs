using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using ErikEJ.SQLiteScripting;
using System.Linq;

namespace ErikEJ.SqlCeToolbox.Commands
{
    public class SqlServerDatabaseMenuCommandsHandler
    {
        private readonly SqlCeToolboxPackage _package;

        public SqlServerDatabaseMenuCommandsHandler(ExplorerToolWindow parent)
        {
            _package = parent.Package as SqlCeToolboxPackage;
        }

        public SqlServerDatabaseMenuCommandsHandler(SqlCeToolboxPackage sqlCeToolboxPackage)
        {
            _package = sqlCeToolboxPackage;
        }

        public void ScriptServerDatabase(object sender, ExecutedRoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null) return;
            var scope = (Scope)menuItem.Tag;
            var databaseInfo = menuItem.CommandParameter as DatabaseMenuCommandParameters;
            if (databaseInfo == null) return;

            try
            {
                //var serverList = new ObjectExplorerManager(_package).GetAllServers();
                var connectionString = databaseInfo.DatabaseInfo != null
                    ? databaseInfo.DatabaseInfo.ConnectionString :
                    DataConnectionHelper.PromptForConnectionString(_package);
                if (string.IsNullOrEmpty(connectionString)) return;
                var ptd = new PickTablesDialog();
                int totalCount;
                using (var repository = Helpers.RepositoryHelper.CreateRepository(new DatabaseInfo { ConnectionString = connectionString, DatabaseType = DatabaseType.SQLServer }))
                {
                    ptd.Tables = repository.GetAllTableNamesForExclusion();
                    totalCount = ptd.Tables.Count;
                }

                var res = ptd.ShowModal();
                if (!res.HasValue || res.Value != true || (ptd.Tables.Count >= totalCount)) return;
                {
                    var fd = new SaveFileDialog
                    {
                        Title = "Save generated script as",
                        Filter =
                            "SQL Server Compact Script (*.sqlce)|*.sqlce|SQL Server Script (*.sql)|*.sql|All Files(*.*)|*.*"
                    };
                    if (scope == Scope.SchemaDataSQLite || scope == Scope.SchemaSQLite)
                        fd.Filter = "SQL Script (*.sql)|*.sql|All Files(*.*)|*.*";
                    fd.OverwritePrompt = true;
                    fd.ValidateNames = true;
                    var result = fd.ShowDialog();
                    if (!result.HasValue || result.Value != true) return;
                    using (var repository = Helpers.RepositoryHelper.CreateRepository(new DatabaseInfo { ConnectionString = connectionString, DatabaseType = DatabaseType.SQLServer }))
                    {
                        try
                        {
                            var generator = DataConnectionHelper.CreateGenerator(repository, fd.FileName, DatabaseType.SQLServer);
                            generator.ExcludeTables(ptd.Tables);
                            EnvDteHelper.ShowMessage(generator.ScriptDatabaseToFile(scope));
                            DataConnectionHelper.LogUsage("DatabasesScriptServer");
                        }
                        catch (Exception ex)
                        {
                            DataConnectionHelper.SendError(ex, DatabaseType.SQLServer, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, DatabaseType.SQLServer, false);
            }
        }

#if VS2010
#else
        public async void GenerateEfPocoFromDacPacInProject(object sender, ExecutedRoutedEventArgs e)
        {
            EnvDteHelper.LaunchUrl("https://github.com/sjh37/EntityFramework-Reverse-POCO-Code-First-Generator");

            var databaseInfo = ValidateMenuInfo(sender);
            if (databaseInfo == null) return;

            var isEf6 = SqlCeToolboxPackage.VsSupportsEf6();
            try
            {
                var dte = _package?.GetServiceHelper(typeof(DTE)) as DTE;
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
                    EnvDteHelper.ShowError("Please select a project in Solution Explorer, where you want the generated code to be placed");
                    return;
                }
                if (dte.Solution.SolutionBuild.BuildState == vsBuildState.vsBuildStateNotStarted)
                {
                    EnvDteHelper.ShowError("Please build the project before proceeding");
                    return;
                }
                if (isEf6)
                {
                    if (!dteH.ContainsEf6Reference(project))
                    {
                        EnvDteHelper.ShowError("Please add the EntityFramework 6.x NuGet package to the project");
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

                var ofd = new OpenFileDialog
                {
                    Filter = "Dacpac (*.dacpac)|*.dacpac|All Files(*.*)|*.*",
                    CheckFileExists = true,
                    Multiselect = false,
                    ValidateNames = true
                };
                if (ofd.ShowDialog() != true) return;

                var dacPacFileName = ofd.FileName;

                var connectionStringBuilder = new SqlConnectionStringBuilder
                {
                    DataSource = @"(localdb)\mssqllocaldb",
                    InitialCatalog = Path.GetFileNameWithoutExtension(dacPacFileName),
                    IntegratedSecurity = true
                };

                var dacFxHelper = new DacFxHelper(_package);
                await dacFxHelper.RunDacPackageAsync(connectionStringBuilder, dacPacFileName);

                var prefix = "App";
                var configPath = Path.Combine(Path.GetTempPath(), prefix + ".config");

                var item = dteH.GetProjectConfig(project);
                if (item == null)
                {
                    //Add app.config file to project
                    var cfgSb = new StringBuilder();
                    cfgSb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
                    cfgSb.AppendLine("<configuration>");
                    cfgSb.AppendLine("</configuration>");
                    File.WriteAllText(configPath, cfgSb.ToString(), Encoding.UTF8);
                    item = project.ProjectItems.AddFromFileCopy(configPath);
                }
                if (item != null)
                {
                    AppConfigHelper.WriteConnectionStringToAppConfig("MyDbContext", connectionStringBuilder.ConnectionString, project.FullName, "System.Data.SqlClient", prefix, item.Name);
                }

                var dte2 = (DTE2)_package.GetServiceHelper(typeof(DTE));
                // ReSharper disable once SuspiciousTypeConversion.Global
                var solution2 = dte2.Solution as Solution2;

                var projectItemTemplate = solution2?.GetProjectItemTemplate("EntityFramework Reverse POCO Code First Generator", "CSharp");
                if (!string.IsNullOrEmpty(projectItemTemplate))
                {
                    var projectItem = dteH.GetProjectDataContextClass(project, "Database.tt".ToLowerInvariant());
                    if (projectItem == null)
                    {
                        project.ProjectItems.AddFromTemplate(projectItemTemplate, "Database.tt");
                        EnvDteHelper.ShowMessage("Please run Custom Tool with the Database.tt file");
                    }
                    else
                    {
                        EnvDteHelper.ShowMessage("Database.tt already exists, please run Custom Tool with existing Database.tt file");
                    }
                }
                DataConnectionHelper.LogUsage("DatabaseCreateEFPOCODacpac");
            }
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
#endif

        private void ExportServerDatabaseToEmbedded(DatabaseType databaseType, DatabaseMenuCommandParameters parameters)
        {
            var filter = DataConnectionHelper.GetSqlCeFileFilter();
            var scope = Scope.SchemaData;
            if (databaseType == DatabaseType.SQLite)
            {
                filter = DataConnectionHelper.GetSqliteFileFilter();
                scope = Scope.SchemaDataSQLite;
            }
            Debug.Assert(databaseType == DatabaseType.SQLite || databaseType == DatabaseType.SQLCE40, "Unexpected database type");
            try
            {
                var connectionString = parameters.DatabaseInfo != null
                    ? parameters.DatabaseInfo.ConnectionString :
                    DataConnectionHelper.PromptForConnectionString(_package);
                if (!string.IsNullOrEmpty(connectionString))
                {
                    var ptd = new PickTablesDialog();
                    int totalCount;
                    using (var repository = Helpers.RepositoryHelper.CreateRepository(new DatabaseInfo
                    { ConnectionString = connectionString, DatabaseType = DatabaseType.SQLServer }))
                    {
                        ptd.Tables = repository.GetAllTableNamesForExclusion();
                        totalCount = ptd.Tables.Count;
                    }

                    var res = ptd.ShowModal();
                    if (!res.HasValue || res.Value != true || (ptd.Tables.Count >= totalCount)) return;
                    string dbConnectionString = null;
                    var fd = new SaveFileDialog
                    {
                        Title = "Export as",
                        Filter = filter,
                        OverwritePrompt = true,
                        ValidateNames = true
                    };
                    var result = fd.ShowDialog();
                    if (!result.HasValue || result.Value != true) return;
                    var dbName = fd.FileName;
                    try
                    {
                        if (databaseType == DatabaseType.SQLCE40)
                        {
                            _package.SetStatus("Creating SQL Server Compact database...");
                            var helper = new SqlCeHelper4();
                            dbConnectionString = string.Format("Data Source={0};Max Database Size=4091", dbName);
                            if (File.Exists(dbName))
                                File.Delete(dbName);
                            helper.CreateDatabase(dbConnectionString);
                        }
                        if (databaseType == DatabaseType.SQLite)
                        {
                            _package.SetStatus("Creating SQLite database...");
                            var helper = new SqliteHelper();
                            dbConnectionString = string.Format("Data Source={0};", dbName);
                            if (File.Exists(dbName))
                                File.Delete(dbName);
                            helper.CreateDatabase(dbConnectionString);
                        }

                        var bw = new BackgroundWorker();
                        var workerParameters = new List<object>
                        {
                            dbConnectionString,
                            connectionString,
                            ptd.Tables,
                            databaseType.ToString(),
                            scope.ToString()
                        };

                        bw.DoWork += bw_DoExportWork;
                        bw.RunWorkerCompleted += (s, ea) =>
                        {
                            try
                            {
                                if (ea.Error != null)
                                {
                                    DataConnectionHelper.SendError(ea.Error, databaseType, false);
                                }
                                DataConnectionHelper.LogUsage("DatabasesExportFromServer");
                            }
                            finally
                            {
                                bw.Dispose();
                            }
                        };
                        bw.RunWorkerAsync(workerParameters);

                    }
                    catch (Exception ex)
                    {
                        DataConnectionHelper.SendError(ex, databaseType, false);
                    }
                }
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, DatabaseType.SQLServer);
            }
        }

        public void ExportServerDatabaseTo40(object sender, ExecutedRoutedEventArgs e)
        {
            var parameters = ValidateMenuInfo(sender);
            if (parameters == null) return;
            ExportServerDatabaseToEmbedded(DatabaseType.SQLCE40, parameters);
        }

        public void ExportServerDatabaseToSqlite(object sender, ExecutedRoutedEventArgs e)
        {
            var parameters = ValidateMenuInfo(sender);
            if (parameters == null) return;
            ExportServerDatabaseToEmbedded(DatabaseType.SQLite, parameters);
        }


        void bw_DoExportWork(object sender, DoWorkEventArgs e)
        {
            var parameters = e.Argument as List<object>;
            if (parameters != null
                && parameters.Count == 5)
            {
                var dbConnectionString = parameters[0] as string;
                var connectionString = parameters[1] as string;
                var tables = parameters[2] as List<string>;
                // ReSharper disable once AssignNullToNotNullAttribute
                var dbType = (DatabaseType)Enum.Parse(typeof(DatabaseType), parameters[3] as string);
                // ReSharper disable once AssignNullToNotNullAttribute
                var scope = (Scope)Enum.Parse(typeof(Scope), parameters[4] as string);

                var scriptRoot = Path.GetTempFileName();
                var tempScript = scriptRoot + ".sqlce";
                _package.SetStatus("Starting import");
                using (var repository = Helpers.RepositoryHelper.CreateRepository(new DatabaseInfo { ConnectionString = connectionString, DatabaseType = DatabaseType.SQLServer }))
                {
                    var generator = DataConnectionHelper.CreateGenerator(repository, tempScript, DatabaseType.SQLServer);
                    generator.ExcludeTables(tables);
                    _package.SetStatus("Scripting server database...");
                    generator.ScriptDatabaseToFile(scope);
                }
                _package.SetStatus("Importing data...");

                using (var dbRepository = Helpers.RepositoryHelper.CreateRepository(new DatabaseInfo { ConnectionString = dbConnectionString, DatabaseType = dbType }))
                {
                    //Handles large exports also... 
                    if (File.Exists(tempScript)) // Single file
                    {
                        dbRepository.ExecuteSqlFile(tempScript);
                        TryDeleteFile(tempScript);
                    }
                    else // possibly multiple files - tmp2BB9.tmp_0.sqlce
                    {
                        var count = Directory.GetFiles(Path.GetDirectoryName(scriptRoot),  Path.GetFileName(scriptRoot) + "*", SearchOption.AllDirectories).Count();
                        for (var i = 0; i < 400; i++)
                        {
                            var testFile = string.Format("{0}_{1}{2}", scriptRoot, i.ToString("D4"), ".sqlce");
                            if (File.Exists(testFile))
                            {
                                dbRepository.ExecuteSqlFile(testFile);
                                _package.SetProgress("Importing data...", (uint)i + 1, (uint)count - 1);
                                TryDeleteFile(testFile);
                            }
                        }
                        _package.SetStatus(null);
                    }
                }
            }
            _package.SetStatus("Import complete");
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

        public void GenerateServerDgmlFiles(object sender, ExecutedRoutedEventArgs e)
        {
            // http://www.mztools.com/articles/2007/MZ2007011.aspx
            var menuItem = sender as MenuItem;
            if (menuItem == null) return;

            var databaseInfo = menuItem.CommandParameter as DatabaseMenuCommandParameters;
            if (databaseInfo == null) return;

            var originalValue = Properties.Settings.Default.KeepSchemaNames;
            var dte = _package.GetServiceHelper(typeof(DTE)) as DTE;
            try
            {
                var connectionString = databaseInfo.DatabaseInfo != null
                    ? databaseInfo.DatabaseInfo.ConnectionString :
                    DataConnectionHelper.PromptForConnectionString(_package);

                if (string.IsNullOrEmpty(connectionString)) return;
                var ptd = new PickTablesDialog();
                using (var repository = Helpers.RepositoryHelper.CreateRepository(new DatabaseInfo { ConnectionString = connectionString, DatabaseType = DatabaseType.SQLServer }))
                {
                    ptd.Tables = repository.GetAllTableNamesForExclusion();
                }

                var res = ptd.ShowModal();

                if (res.HasValue && res.Value)
                {
                    var fd = new SaveFileDialog();
                    fd.Title = "Save generated DGML file as";
                    fd.Filter = "DGML (*.dgml)|*.dgml";
                    fd.OverwritePrompt = true;
                    fd.ValidateNames = true;
                    var result = fd.ShowDialog();
                    if (!result.HasValue || result.Value != true) return;
                    Properties.Settings.Default.KeepSchemaNames = true;
                    var fileName = fd.FileName;
                    using (var repository = Helpers.RepositoryHelper.CreateRepository(new DatabaseInfo { ConnectionString = connectionString, DatabaseType = DatabaseType.SQLServer }))
                    {
                        try
                        {
                            var generator = DataConnectionHelper.CreateGenerator(repository, fileName, DatabaseType.SQLServer);
                            generator.GenerateSchemaGraph(connectionString, ptd.Tables);
                            if (dte != null)
                            {
                                dte.ItemOperations.OpenFile(fileName);
                                dte.ActiveDocument.Activate();
                            }
                            DataConnectionHelper.LogUsage("DatabasesScriptDGML");
                        }
                        catch (Exception ex)
                        {
                            DataConnectionHelper.SendError(ex, DatabaseType.SQLServer, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, DatabaseType.SQLServer, false);
            }
            finally
            {
                Properties.Settings.Default.KeepSchemaNames = originalValue;
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
