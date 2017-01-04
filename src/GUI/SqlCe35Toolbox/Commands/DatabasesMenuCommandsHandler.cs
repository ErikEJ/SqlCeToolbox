using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using EnvDTE;
using EnvDTE80;
using ErikEJ.SqlCeScripting;
using ErikEJ.SqlCeToolbox.Dialogs;
using ErikEJ.SqlCeToolbox.Helpers;
using ErikEJ.SqlCeToolbox.ToolWindows;
using ErikEJ.SQLiteScripting;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.Win32;

namespace ErikEJ.SqlCeToolbox.Commands
{
    public class DatabasesMenuCommandsHandler
    {
        private readonly ExplorerToolWindow _parentWindow;
        private readonly SqlCeToolboxPackage _package;

        public DatabasesMenuCommandsHandler(ExplorerToolWindow parent)
        {
            _parentWindow = parent;
            _package = _parentWindow.Package as SqlCeToolboxPackage;
        }

        public DatabasesMenuCommandsHandler(SqlCeToolboxPackage package)
        {
            _package = package;
        }

        #region Root Level Commands
        public void AddPrivateCe35Database(object sender, ExecutedRoutedEventArgs e)
        {
            AddCeDatabase(DatabaseType.SQLCE35);
        }

        public void AddPrivateCe40Database(object sender, ExecutedRoutedEventArgs e)
        {
            AddCeDatabase(DatabaseType.SQLCE40);
        }

        private void AddCeDatabase(DatabaseType dbType)
        {
            try
            {
                var dialog = new ConnectionDialog();
                string path;
                if (TryGetInitialPath(_package, out path))
                {
                    dialog.InitialPath = path;                
                }
                dialog.DbType = dbType;
                dialog.ShowDdexInfo = _package.VsSupportsSimpleDdex4Provider() || _package.VsSupportsSimpleDdex35Provider();
                dialog.CouldSupportPrivateProvider =
                    (dbType == DatabaseType.SQLCE40 && (SqlCeToolboxPackage.VisualStudioVersion >= new Version(12, 0)) )
                    || (dbType == DatabaseType.SQLCE35 && SqlCeToolboxPackage.VsSupportsEf6());
                var result = dialog.ShowModal();
                if (!result.HasValue || result.Value != true) return;
                if (string.IsNullOrWhiteSpace(dialog.ConnectionString)) return;
                DataConnectionHelper.SaveDataConnection(dialog.ConnectionString, dbType, _package);
                var control = _parentWindow.Content as ExplorerControl;
                if (control != null) control.BuildDatabaseTree();
                DataConnectionHelper.LogUsage("DatabasesAddCeDatabase");
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, dbType);
            }
        }

        public void AddCe35Database(object sender, ExecutedRoutedEventArgs e)
        {
            // http://www.mztools.com/articles/2007/MZ2007011.aspx
            if (!DataConnectionHelper.DdexProviderIsInstalled(new Guid(Resources.SqlCompact35Provider)))
            {
                EnvDteHelper.ShowError("The version 3.5 Visual Studio DDEX provider is not installed, cannot add connection");
                return;
            }
            try
            {
                var objIVsDataConnectionDialogFactory = _package.GetServiceHelper(typeof(IVsDataConnectionDialogFactory)) as IVsDataConnectionDialogFactory;
                if (objIVsDataConnectionDialogFactory == null) return;
                var objIVsDataConnectionDialog = objIVsDataConnectionDialogFactory.CreateConnectionDialog();
                objIVsDataConnectionDialog.AddAllSources();
                objIVsDataConnectionDialog.SelectedSource = new Guid("130BADA6-E128-423c-9D07-02E4734D45D4");
                objIVsDataConnectionDialog.SelectedProvider = new Guid(Resources.SqlCompact35Provider);

                if (objIVsDataConnectionDialog.ShowDialog() && objIVsDataConnectionDialog.SelectedProvider == new Guid(Resources.SqlCompact35Provider))
                {
                    DataConnectionHelper.SaveDataConnection(_package, objIVsDataConnectionDialog.EncryptedConnectionString, objIVsDataConnectionDialog.DisplayConnectionString, DatabaseType.SQLCE35, new Guid(Resources.SqlCompact35Provider));
                    var control = _parentWindow.Content as ExplorerControl;
                    if (control != null) control.BuildDatabaseTree();
                    DataConnectionHelper.LogUsage("DatabasesAddCe35Database");
                }
                objIVsDataConnectionDialog.Dispose();
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, DatabaseType.SQLCE35);
            }
        }

        public void AddCe40Database(object sender, ExecutedRoutedEventArgs e)
        {
            // http://www.mztools.com/articles/2007/MZ2007011.aspx
            if (!DataConnectionHelper.DdexProviderIsInstalled(new Guid(Resources.SqlCompact40Provider)))
            {
                EnvDteHelper.ShowError("The version 4.0 Visual Studio DDEX provider is not installed, cannot add connection");
                return;
            }
            try
            {
                var objIVsDataConnectionDialogFactory = _package.GetServiceHelper(typeof(IVsDataConnectionDialogFactory)) as IVsDataConnectionDialogFactory;
                if (objIVsDataConnectionDialogFactory != null)
                {
                    var objIVsDataConnectionDialog = objIVsDataConnectionDialogFactory.CreateConnectionDialog();
                    objIVsDataConnectionDialog.AddAllSources();
                    objIVsDataConnectionDialog.SelectedSource = new Guid("34A4B3E8-C54D-466F-92EA-E5814B97CA43");
                    objIVsDataConnectionDialog.SelectedProvider = new Guid(Resources.SqlCompact40Provider);

                    if (objIVsDataConnectionDialog.ShowDialog() && objIVsDataConnectionDialog.SelectedProvider == new Guid(Resources.SqlCompact40Provider))
                    {
                        DataConnectionHelper.SaveDataConnection(_package, objIVsDataConnectionDialog.EncryptedConnectionString, objIVsDataConnectionDialog.DisplayConnectionString, DatabaseType.SQLCE40, new Guid(Resources.SqlCompact40Provider));
                        var control = _parentWindow.Content as ExplorerControl;
                        if (control != null) control.BuildDatabaseTree();
                        DataConnectionHelper.LogUsage("DatabasesAddCe40Database");
                    }
                    objIVsDataConnectionDialog.Dispose();
                }
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, DatabaseType.SQLCE40, false);
            }
        }

        private bool TryGetInitialPath(SqlCeToolboxPackage package, out string path)
        {
            var dte = package.GetServiceHelper(typeof(DTE)) as DTE;
            var dteHelper = new EnvDteHelper();
            try
            {
                path = dteHelper.GetInitialFolder(dte);
                return true;
            }
            catch
            {
                path = null;
                return false;
            }
        }

        public void AddSqLiteDatabase(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var dialog = new SQLiteConnectionDialog();
                string path;
                if (TryGetInitialPath(_package, out path))
                {
                    dialog.InitialPath = path;
                }
                var result = dialog.ShowModal();
                if (!result.HasValue || result.Value != true) return;
                if (string.IsNullOrWhiteSpace(dialog.ConnectionString)) return;
                DataConnectionHelper.SaveDataConnection(dialog.ConnectionString, DatabaseType.SQLite, _package);
                var control = _parentWindow.Content as ExplorerControl;
                if (control != null) control.BuildDatabaseTree();
                DataConnectionHelper.LogUsage("DatabasesAddSQLiteDatabase");
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, DatabaseType.SQLite);
            }
        }

        public void ScanConnections(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                new DataConnectionHelper().ScanConnections(_package);
                var control = _parentWindow.Content as ExplorerControl;
                if (control != null) control.BuildDatabaseTree();
                DataConnectionHelper.LogUsage("DatabasesScanConnections");
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, DatabaseType.SQLServer, false);
            }
        }

        public void FixConnections(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                new DataConnectionHelper().ValidateConnections(_package);
                var control = _parentWindow.Content as ExplorerControl;
                if (control != null) control.BuildDatabaseTree();
                DataConnectionHelper.LogUsage("DatabasesFixConnections");
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, DatabaseType.SQLServer, false);
            }
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
                var connectionString =  databaseInfo.DatabaseInfo != null
                    ? databaseInfo.DatabaseInfo.ConnectionString :
                    DataConnectionHelper.PromptForConnectionString(_package);
                if (string.IsNullOrEmpty(connectionString)) return;
                var ptd = new PickTablesDialog();
                int totalCount;
                using (var repository = DataConnectionHelper.CreateRepository(new DatabaseInfo { ConnectionString = connectionString, DatabaseType = DatabaseType.SQLServer }))
                {
                    ptd.Tables = repository.GetAllTableNamesForExclusion();
                    totalCount = ptd.Tables.Count;
                }

                var res = ptd.ShowModal();
                if (!res.HasValue || res.Value != true || (ptd.Tables.Count >= totalCount)) return;
                {
                    var fd = new SaveFileDialog
                    {
                        Title = "Save generated database script as",
                        Filter =
                            "SQL Server Compact Script (*.sqlce)|*.sqlce|SQL Server Script (*.sql)|*.sql|All Files(*.*)|*.*"
                    };
                    if (scope == Scope.SchemaDataSQLite || scope == Scope.SchemaSQLite)
                        fd.Filter = "SQL Script (*.sql)|*.sql|All Files(*.*)|*.*";
                    fd.OverwritePrompt = true;
                    fd.ValidateNames = true;
                    var result = fd.ShowDialog();
                    if (!result.HasValue || result.Value != true) return;
                    using (var repository = DataConnectionHelper.CreateRepository(new DatabaseInfo { ConnectionString = connectionString, DatabaseType = DatabaseType.SQLServer }))
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
                    using (var repository = DataConnectionHelper.CreateRepository(new DatabaseInfo 
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

                        bw.DoWork += bw_DoWork;
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


        void bw_DoWork(object sender, DoWorkEventArgs e)
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
                using (var repository = DataConnectionHelper.CreateRepository(new DatabaseInfo { ConnectionString = connectionString, DatabaseType = DatabaseType.SQLServer }))
                {
                    var generator = DataConnectionHelper.CreateGenerator(repository, tempScript, DatabaseType.SQLServer);
                    generator.ExcludeTables(tables);
                    _package.SetStatus("Scripting server database...");
                    generator.ScriptDatabaseToFile(scope);
                }
                _package.SetStatus("Importing data...");

                using (var dbRepository = DataConnectionHelper.CreateRepository(new DatabaseInfo { ConnectionString = dbConnectionString, DatabaseType = dbType }))
                {
                    //Handles large exports also... 
                    if (File.Exists(tempScript)) // Single file
                    {
                        dbRepository.ExecuteSqlFile(tempScript);
                    }
                    else // possibly multiple files - tmp2BB9.tmp_0.sqlce
                    {
                        for (var i = 0; i < 400; i++)
                        {
                            var testFile = string.Format("{0}_{1}{2}", scriptRoot, i.ToString("D4"), ".sqlce");
                            if (File.Exists(testFile))
                            {
                                dbRepository.ExecuteSqlFile(testFile);
                                _package.SetStatus(string.Format("Importing data...{0}", i + 1));
                            }
                        }
                    }
                }
            }
            _package.SetStatus("Import complete");
        }

        public void DesignDatabase(object sender, ExecutedRoutedEventArgs e)
        {
            EnvDteHelper.LaunchUrl("http://sqlcompact.dk/sqldesigner/");
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
                using (var repository = DataConnectionHelper.CreateRepository(new DatabaseInfo { ConnectionString = connectionString, DatabaseType = DatabaseType.SQLServer }))
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
                    using (var repository = DataConnectionHelper.CreateRepository(new DatabaseInfo { ConnectionString = connectionString, DatabaseType = DatabaseType.SQLServer }))
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

        public void SyncFxGenerateLocalDatabaseCacheCode(object sender, ExecutedRoutedEventArgs e)
        {
            // http://www.mztools.com/articles/2007/MZ2007011.aspx
            var menuItem = sender as MenuItem;
            if (menuItem == null) return;
            var dte = _package.GetServiceHelper(typeof(DTE)) as DTE;
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

            try
            {
                var serverConnectionString = DataConnectionHelper.PromptForConnectionString(_package);
                if (string.IsNullOrEmpty(serverConnectionString)) return;
                string clientConnectionString;

                //grab target SQL CE Database
                var dialog = new ConnectionDialog();
                dialog.DbType = DatabaseType.SQLCE35;
                var result = dialog.ShowModal();
                if (result.HasValue && result.Value && !string.IsNullOrWhiteSpace(dialog.ConnectionString))
                {
                    clientConnectionString = dialog.ConnectionString;
                }
                else
                {
                    return;
                }

                var model = string.Empty;
                var sfd = new SyncFxDialog();
                var databaseInfo = new DatabaseMenuCommandParameters
                {
                    DatabaseInfo = new DatabaseInfo
                    {
                        ConnectionString = serverConnectionString,
                        DatabaseType = DatabaseType.SQLServer
                    }
                };
                SyncFxGetObjectsForSync(sfd, databaseInfo);
                sfd.ModelName = model;

                var res = sfd.ShowModal();
                if (res.HasValue && res.Value && (sfd.Tables.Count > 0) && !string.IsNullOrWhiteSpace(sfd.ModelName))
                {
                    model = sfd.ModelName;
                    var defaultNamespace = project.Properties.Item("DefaultNamespace").Value.ToString();

                    var columns = sfd.Columns.Where(c => sfd.Tables.Contains(c.TableName)).ToList();
                    var classes = new SyncFxHelper().GenerateCodeForScope(serverConnectionString, clientConnectionString, "SQL", model, columns, defaultNamespace);
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
                    DataConnectionHelper.LogUsage("DatabasesSyncAddLocalDBCache");
                }
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, DatabaseType.SQLServer);
            }
        }

        public void CheckCeVersion(object sender, ExecutedRoutedEventArgs e)
        {
            var helper = DataConnectionHelper.CreateEngineHelper(DatabaseType.SQLCE40);
            var ofd = new OpenFileDialog();
            ofd.Filter = DataConnectionHelper.GetSqlCeFileFilter();
            ofd.CheckFileExists = true;
            ofd.Multiselect = false;
            ofd.ValidateNames = true;
            if (ofd.ShowDialog() == true)
            {
                try
                {
                    var sdfVersion = helper.DetermineVersion(ofd.FileName);
                    string found;
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
                            throw new ArgumentOutOfRangeException();
                    }
                    EnvDteHelper.ShowMessage(string.Format("{0} is SQL Server Compact version {1}", Path.GetFileName(ofd.FileName), found));
                    DataConnectionHelper.LogUsage("DatabaseVersionDetect");
                }
                catch (Exception ex)
                {
                    EnvDteHelper.ShowError(ex.Message);
                }
            }
        }

        #endregion

        private static void SyncFxGetObjectsForSync(SyncFxDialog sfd, DatabaseMenuCommandParameters databaseInfo)
        {
            using (var repository = DataConnectionHelper.CreateRepository(databaseInfo.DatabaseInfo))
            {
                sfd.Tables = repository.GetAllTableNames().Where(t => !t.EndsWith("scope_info") && !t.EndsWith("scope_config") && !t.EndsWith("schema_info") && !t.EndsWith("_tracking")).ToList();
                sfd.Columns = repository.GetAllColumns().Where(t => !t.TableName.EndsWith("scope_info") && !t.TableName.EndsWith("scope_config") && !t.TableName.EndsWith("schema_info") && !t.TableName.EndsWith("_tracking")).ToList();
                sfd.PrimaryKeyColumns = repository.GetAllPrimaryKeys().Where(t => !t.TableName.EndsWith("scope_info") && !t.TableName.EndsWith("scope_config") && !t.TableName.EndsWith("schema_info") && !t.TableName.EndsWith("_tracking")).ToList();
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
