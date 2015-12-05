using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using EnvDTE;
using ErikEJ.SqlCeScripting;
using ErikEJ.SqlCeToolbox.Dialogs;
using ErikEJ.SqlCeToolbox.Helpers;
using ErikEJ.SqlCeToolbox.ToolWindows;
using ErikEJ.SQLiteScripting;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;

namespace ErikEJ.SqlCeToolbox.Commands
{
    public class DatabasesMenuCommandsHandler
    {
        private readonly ExplorerToolWindow _parentWindow;
        private SqlCeToolboxPackage package;

        public DatabasesMenuCommandsHandler(ExplorerToolWindow parent)
        {
            _parentWindow = parent;
            package = _parentWindow.Package as SqlCeToolboxPackage;
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
                if (TryGetInitialPath(package, out path))
                {
                    dialog.InitialPath = path;                
                }
                dialog.DbType = dbType;
                dialog.ShowDDEXInfo = package.VSSupportsSimpleDDEX4Provider() || package.VSSupportsSimpleDDEX35Provider();
                dialog.CouldSupportPrivateProvider =
                    (dbType == DatabaseType.SQLCE40 && (SqlCeToolboxPackage.VisualStudioVersion == new Version(12, 0) || SqlCeToolboxPackage.VisualStudioVersion == new Version(14, 0)) )
                    || (dbType == DatabaseType.SQLCE35 && package.VSSupportsEF6());
                bool? result = dialog.ShowModal();
                if (result.HasValue && result.Value == true)
                {
                    if (!string.IsNullOrWhiteSpace(dialog.ConnectionString))
                    {
                        Helpers.DataConnectionHelper.SaveDataConnection(dialog.ConnectionString, dbType, package);
                        ExplorerControl control = _parentWindow.Content as ExplorerControl;
                        control.BuildDatabaseTree();
                        Helpers.DataConnectionHelper.LogUsage("DatabasesAddCeDatabase");
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, dbType);
            }
        }

        public void AddCe35Database(object sender, ExecutedRoutedEventArgs e)
        {
            // http://www.mztools.com/articles/2007/MZ2007011.aspx
            if (!Helpers.DataConnectionHelper.DDEXProviderIsInstalled(new Guid(Resources.SqlCompact35Provider)))
            {
                EnvDTEHelper.ShowError("The version 3.5 Visual Studio DDEX provider is not installed, cannot add connection");
                return;
            }
            try
            {
                var objIVsDataConnectionDialogFactory = package.GetServiceHelper(typeof(IVsDataConnectionDialogFactory)) as IVsDataConnectionDialogFactory;
                var objIVsDataConnectionDialog = objIVsDataConnectionDialogFactory.CreateConnectionDialog();
                objIVsDataConnectionDialog.AddAllSources();
                objIVsDataConnectionDialog.SelectedSource = new Guid("130BADA6-E128-423c-9D07-02E4734D45D4");
                objIVsDataConnectionDialog.SelectedProvider = new Guid(Resources.SqlCompact35Provider);

                if (objIVsDataConnectionDialog.ShowDialog() && objIVsDataConnectionDialog.SelectedProvider == new Guid(Resources.SqlCompact35Provider))
                {
                    Helpers.DataConnectionHelper.SaveDataConnection(package, objIVsDataConnectionDialog.EncryptedConnectionString, objIVsDataConnectionDialog.DisplayConnectionString, DatabaseType.SQLCE35, new Guid(Resources.SqlCompact35Provider));
                    ExplorerControl control = _parentWindow.Content as ExplorerControl;
                    control.BuildDatabaseTree();
                    Helpers.DataConnectionHelper.LogUsage("DatabasesAddCe35Database");
                }
                objIVsDataConnectionDialog.Dispose();
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, DatabaseType.SQLCE35);
            }
        }

        public void AddCe40Database(object sender, ExecutedRoutedEventArgs e)
        {
            // http://www.mztools.com/articles/2007/MZ2007011.aspx
            if (!Helpers.DataConnectionHelper.DDEXProviderIsInstalled(new Guid(Resources.SqlCompact40Provider)))
            {
                EnvDTEHelper.ShowError("The version 4.0 Visual Studio DDEX provider is not installed, cannot add connection");
                return;
            }
            try
            {
                var objIVsDataConnectionDialogFactory = package.GetServiceHelper(typeof(IVsDataConnectionDialogFactory)) as IVsDataConnectionDialogFactory;
                var objIVsDataConnectionDialog = objIVsDataConnectionDialogFactory.CreateConnectionDialog();
                objIVsDataConnectionDialog.AddAllSources();
                objIVsDataConnectionDialog.SelectedSource = new Guid("34A4B3E8-C54D-466F-92EA-E5814B97CA43");
                objIVsDataConnectionDialog.SelectedProvider = new Guid(Resources.SqlCompact40Provider);

                if (objIVsDataConnectionDialog.ShowDialog() && objIVsDataConnectionDialog.SelectedProvider == new Guid(Resources.SqlCompact40Provider))
                {
                    Helpers.DataConnectionHelper.SaveDataConnection(package, objIVsDataConnectionDialog.EncryptedConnectionString, objIVsDataConnectionDialog.DisplayConnectionString, DatabaseType.SQLCE40, new Guid(Resources.SqlCompact40Provider));
                    ExplorerControl control = _parentWindow.Content as ExplorerControl;
                    control.BuildDatabaseTree();
                    Helpers.DataConnectionHelper.LogUsage("DatabasesAddCe40Database");
                }
                objIVsDataConnectionDialog.Dispose();
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, DatabaseType.SQLCE40, false);
            }
        }

        private bool TryGetInitialPath(SqlCeToolboxPackage package, out string path)
        {
            var dte = package.GetServiceHelper(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
            EnvDTEHelper dteHelper = new EnvDTEHelper();
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

        public void AddSQLiteDatabase(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var dialog = new SQLiteConnectionDialog();
                string path;
                if (TryGetInitialPath(package, out path))
                {
                    dialog.InitialPath = path;
                }
                bool? result = dialog.ShowModal();
                if (result.HasValue && result.Value == true)
                {
                    if (!string.IsNullOrWhiteSpace(dialog.ConnectionString))
                    {
                        Helpers.DataConnectionHelper.SaveDataConnection(dialog.ConnectionString, DatabaseType.SQLite, package);
                        ExplorerControl control = _parentWindow.Content as ExplorerControl;
                        control.BuildDatabaseTree();
                        Helpers.DataConnectionHelper.LogUsage("DatabasesAddSQLiteDatabase");
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, DatabaseType.SQLite);
            }
        }

        public void ScanConnections(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                new DataConnectionHelper().ScanConnections(package);
                ExplorerControl control = _parentWindow.Content as ExplorerControl;
                control.BuildDatabaseTree();
                Helpers.DataConnectionHelper.LogUsage("DatabasesScanConnections");
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, DatabaseType.SQLServer, false);
            }
        }

        public void FixConnections(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                new DataConnectionHelper().ValidateConnections(package);
                ExplorerControl control = _parentWindow.Content as ExplorerControl;
                control.BuildDatabaseTree();
                Helpers.DataConnectionHelper.LogUsage("DatabasesFixConnections");
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, DatabaseType.SQLServer, false);
            }
        }

        public void ScriptServerDatabase(object sender, ExecutedRoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null) return;
            Scope scope = (Scope)menuItem.Tag;
            var databaseInfo = menuItem.CommandParameter as DatabaseMenuCommandParameters;
            if (databaseInfo == null) return;

            try
            {
                string connectionString =  databaseInfo.DatabaseInfo != null
                    ? databaseInfo.DatabaseInfo.ConnectionString :
                    DataConnectionHelper.PromptForConnectionString(package);
                if (!string.IsNullOrEmpty(connectionString))
                {                  
                    string fileName;
                    PickTablesDialog ptd = new PickTablesDialog();
                    int totalCount = 0;
                    using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(new DatabaseInfo { ConnectionString = connectionString, DatabaseType = DatabaseType.SQLServer }))
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
                        if (scope == Scope.SchemaDataSQLite || scope == Scope.SchemaSQLite)
                            fd.Filter = "SQL Script (*.sql)|*.sql|All Files(*.*)|*.*";
                        fd.OverwritePrompt = true;
                        fd.ValidateNames = true;
                        bool? result = fd.ShowDialog();
                        if (result.HasValue && result.Value == true)
                        {
                            fileName = fd.FileName;
                            using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(new DatabaseInfo { ConnectionString = connectionString, DatabaseType = DatabaseType.SQLServer }))
                            {
                                try
                                {
                                    var generator = Helpers.DataConnectionHelper.CreateGenerator(repository, fd.FileName, DatabaseType.SQLServer);
                                    generator.ExcludeTables(ptd.Tables);
                                    EnvDTEHelper.ShowMessage(generator.ScriptDatabaseToFile(scope));
                                    Helpers.DataConnectionHelper.LogUsage("DatabasesScriptServer");
                                }
                                catch (Exception ex)
                                {
                                    Helpers.DataConnectionHelper.SendError(ex, DatabaseType.SQLServer, false);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, DatabaseType.SQLServer, false);
            }
        }

        private void ExportServerDatabaseToEmbedded(DatabaseType databaseType, DatabaseMenuCommandParameters parameters)
        {
            string filter = DataConnectionHelper.GetSqlCeFileFilter();
            Scope scope = Scope.SchemaData;
            if (databaseType == DatabaseType.SQLite)
            {
                filter = DataConnectionHelper.GetSqliteFileFilter();
                scope = Scope.SchemaDataSQLite;
            }
            Debug.Assert(databaseType == DatabaseType.SQLite || databaseType == DatabaseType.SQLCE40, "Unexpected database type");
            try
            {
                string connectionString = parameters.DatabaseInfo != null
                    ? parameters.DatabaseInfo.ConnectionString :
                    DataConnectionHelper.PromptForConnectionString(package);
                if (!string.IsNullOrEmpty(connectionString))
                {
                    PickTablesDialog ptd = new PickTablesDialog();
                    int totalCount = 0;
                    using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(new DatabaseInfo 
                        { ConnectionString = connectionString, DatabaseType = DatabaseType.SQLServer }))
                    {
                        ptd.Tables = repository.GetAllTableNamesForExclusion();
                        totalCount = ptd.Tables.Count;
                    }

                    bool? res = ptd.ShowModal();
                    if (res.HasValue && res.Value == true && (ptd.Tables.Count < totalCount))
                    {
                        string dbName;
                        string dbConnectionString = null;
                        SaveFileDialog fd = new SaveFileDialog();
                        fd.Title = "Export as";
                        fd.Filter = filter;
                        fd.OverwritePrompt = true;
                        fd.ValidateNames = true;
                        bool? result = fd.ShowDialog();
                        if (result.HasValue && result.Value == true)
                        {
                            dbName = fd.FileName;
                            try
                            {
                                if (databaseType == DatabaseType.SQLCE40)
                                {
                                    package.SetStatus("Creating SQL Server Compact database...");
                                    SqlCeScripting.SqlCeHelper4 helper = new SqlCeScripting.SqlCeHelper4();
                                    dbConnectionString = string.Format("Data Source={0};Max Database Size=4091", dbName);
                                    if (System.IO.File.Exists(dbName))
                                        File.Delete(dbName);
                                    helper.CreateDatabase(dbConnectionString);
                                }
                                if (databaseType == DatabaseType.SQLite)
                                {
                                    package.SetStatus("Creating SQLite database...");
                                    var helper = new SqliteHelper();
                                    dbConnectionString = string.Format("Data Source={0};", dbName);
                                    if (System.IO.File.Exists(dbName))
                                        File.Delete(dbName);
                                    helper.CreateDatabase(dbConnectionString);
                                }

                                BackgroundWorker bw = new BackgroundWorker();
                                List<object> workerParameters = new List<object>();
                                workerParameters.Add(dbConnectionString);
                                workerParameters.Add(connectionString);
                                workerParameters.Add(ptd.Tables);
                                workerParameters.Add(databaseType.ToString());
                                workerParameters.Add(scope.ToString());

                                bw.DoWork += new DoWorkEventHandler(bw_DoWork);
                                bw.RunWorkerCompleted += (s, ea) =>
                                {
                                    try
                                    {
                                        if (ea.Error != null)
                                        {
                                            Helpers.DataConnectionHelper.SendError(ea.Error, databaseType, false);
                                        }
                                        Helpers.DataConnectionHelper.LogUsage("DatabasesExportFromServer");
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
                                Helpers.DataConnectionHelper.SendError(ex, databaseType, false);
                            }
                        }
                   }
                }
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, DatabaseType.SQLServer);
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
            string dbConnectionString = parameters[0] as string;
            string connectionString = parameters[1] as string;
            var tables = parameters[2] as List<string>;
            DatabaseType dbType = (DatabaseType)Enum.Parse(typeof(DatabaseType), parameters[3] as string);
            Scope scope = (Scope)Enum.Parse(typeof(Scope), parameters[4] as string);

            string scriptRoot = System.IO.Path.GetTempFileName();
            string tempScript = scriptRoot + ".sqlce";
            package.SetStatus("Starting import");
            using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(new DatabaseInfo { ConnectionString = connectionString, DatabaseType = DatabaseType.SQLServer }))
            {
                var generator = Helpers.DataConnectionHelper.CreateGenerator(repository, tempScript, DatabaseType.SQLServer);
                generator.ExcludeTables(tables);
                package.SetStatus("Scripting server database...");
                generator.ScriptDatabaseToFile(scope);
            }
            package.SetStatus("Importing data...");

            using (IRepository dbRepository = Helpers.DataConnectionHelper.CreateRepository(new DatabaseInfo { ConnectionString = dbConnectionString, DatabaseType = dbType }))
            {
                //Handles large exports also... 
                if (File.Exists(tempScript)) // Single file
                {
                    dbRepository.ExecuteSqlFile(tempScript);
                }
                else // possibly multiple files - tmp2BB9.tmp_0.sqlce
                {
                    for (int i = 0; i < 400; i++)
                    {
                        string testFile = string.Format("{0}_{1}{2}", scriptRoot, i.ToString("D4"), ".sqlce");
                        if (File.Exists(testFile))
                        {
                            dbRepository.ExecuteSqlFile(testFile);
                            package.SetStatus(string.Format("Importing data...{0}", i + 1));
                        }
                    }
                }
            }
            package.SetStatus("Import complete");
        }

        public void DesignDatabase(object sender, ExecutedRoutedEventArgs e)
        {
            EnvDTEHelper.LaunchUrl("http://sqlcompact.dk/sqldesigner/");
        }

        public void GenerateServerDgmlFiles(object sender, ExecutedRoutedEventArgs e)
        {
            // http://www.mztools.com/articles/2007/MZ2007011.aspx
            var menuItem = sender as MenuItem;
            if (menuItem == null) return;

            var databaseInfo = menuItem.CommandParameter as DatabaseMenuCommandParameters;
            if (databaseInfo == null) return;

            bool originalValue = Properties.Settings.Default.KeepSchemaNames;
            var dte = package.GetServiceHelper(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
            try
            {
                string connectionString = databaseInfo.DatabaseInfo != null
                    ? databaseInfo.DatabaseInfo.ConnectionString :
                    DataConnectionHelper.PromptForConnectionString(package);

                if (string.IsNullOrEmpty(connectionString)) return;
                string fileName;
                PickTablesDialog ptd = new PickTablesDialog();
                using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(new DatabaseInfo { ConnectionString = connectionString, DatabaseType = DatabaseType.SQLServer }))
                {
                    ptd.Tables = repository.GetAllTableNamesForExclusion();                        
                }

                bool? res = ptd.ShowModal();

                if (res.HasValue && res.Value == true)
                {
                    SaveFileDialog fd = new SaveFileDialog();
                    fd.Title = "Save generated DGML file as";
                    fd.Filter = "DGML (*.dgml)|*.dgml";
                    fd.OverwritePrompt = true;
                    fd.ValidateNames = true;
                    bool? result = fd.ShowDialog();
                    if (result.HasValue && result.Value == true)
                    {
                        Properties.Settings.Default.KeepSchemaNames = true;
                        fileName = fd.FileName;
                        using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(new DatabaseInfo { ConnectionString = connectionString, DatabaseType = DatabaseType.SQLServer }))
                        {
                            try
                            {
                                var generator = Helpers.DataConnectionHelper.CreateGenerator(repository, fileName, DatabaseType.SQLServer);
                                generator.GenerateSchemaGraph(connectionString, ptd.Tables);
                                dte.ItemOperations.OpenFile(fileName);
                                dte.ActiveDocument.Activate();
                                Helpers.DataConnectionHelper.LogUsage("DatabasesScriptDGML");
                            }
                            catch (Exception ex)
                            {
                                Helpers.DataConnectionHelper.SendError(ex, DatabaseType.SQLServer, false);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, DatabaseType.SQLServer, false);
            }
            finally
            {
                Properties.Settings.Default.KeepSchemaNames = originalValue;
            }
        }

        public void SyncFxGenerateLocalDatabaseCacheCode(object sender, ExecutedRoutedEventArgs e)
        {
            // http://www.mztools.com/articles/2007/MZ2007011.aspx
            var menuItem = sender as MenuItem;
            if (menuItem == null) return;

            Scope scope = (Scope)menuItem.Tag;

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

            try
            {
                string serverConnectionString = DataConnectionHelper.PromptForConnectionString(package);
                if (!string.IsNullOrEmpty(serverConnectionString))
                {         
                    string clientConnectionString = string.Empty;

                    //grab target SQL CE Database
                    var dialog = new ConnectionDialog();
                    dialog.DbType = DatabaseType.SQLCE35;
                    bool? result = dialog.ShowModal();
                    if (result.HasValue && result.Value == true && !string.IsNullOrWhiteSpace(dialog.ConnectionString))
                    {
                        clientConnectionString = dialog.ConnectionString;
                    }
                    else
                    {
                        return;
                    }

                    string model = string.Empty;
                    SyncFxDialog sfd = new SyncFxDialog();
                    int totalCount = 0;
                    var databaseInfo = new DatabaseMenuCommandParameters
                    {
                        DatabaseInfo = new DatabaseInfo
                        {
                            ConnectionString = serverConnectionString,
                            DatabaseType = DatabaseType.SQLServer
                        }
                    };
                    totalCount = SyncFxGetObjectsForSync(sfd, databaseInfo);
                    sfd.ModelName = model;

                    bool? res = sfd.ShowModal();
                    if (res.HasValue && res.Value == true && (sfd.Tables.Count > 0) && !string.IsNullOrWhiteSpace(sfd.ModelName))
                    {
                        model = sfd.ModelName;
                        var defaultNamespace = project.Properties.Item("DefaultNamespace").Value.ToString();

                        var columns = sfd.Columns.Where(c => sfd.Tables.Contains(c.TableName)).ToList();
                        var classes = new SyncFxHelper().GenerateCodeForScope(serverConnectionString, clientConnectionString, "SQL", model, columns, defaultNamespace);
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
                        Helpers.DataConnectionHelper.LogUsage("DatabasesSyncAddLocalDBCache");
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, DatabaseType.SQLServer);
            }
        }

        public void CheckCeVersion(object sender, ExecutedRoutedEventArgs e)
        {
            var helper = Helpers.DataConnectionHelper.CreateEngineHelper(DatabaseType.SQLCE40);
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = DataConnectionHelper.GetSqlCeFileFilter();
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
                    EnvDTEHelper.ShowMessage(string.Format("{0} is SQL Server Compact version {1}", Path.GetFileName(ofd.FileName), found));
                    Helpers.DataConnectionHelper.LogUsage("DatabaseVersionDetect");
                }
                catch (Exception ex)
                {
                    EnvDTEHelper.ShowError(ex.Message);
                }
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

