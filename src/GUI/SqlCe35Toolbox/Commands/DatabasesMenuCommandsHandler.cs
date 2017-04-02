using System;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using EnvDTE;
using ErikEJ.SqlCeScripting;
using ErikEJ.SqlCeToolbox.Dialogs;
using ErikEJ.SqlCeToolbox.Helpers;
using ErikEJ.SqlCeToolbox.ToolWindows;
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
                dialog.ShowDdexInfo = _package.VsSupportsSimpleDdex4Provider() && dbType == DatabaseType.SQLCE40;
                dialog.CouldSupportPrivateProvider =
                   dbType == DatabaseType.SQLCE40 && (SqlCeToolboxPackage.VisualStudioVersion >= new Version(12, 0));
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
                    DataConnectionHelper.SaveDataConnection(_package, objIVsDataConnectionDialog.EncryptedConnectionString, DatabaseType.SQLCE35, new Guid(Resources.SqlCompact35Provider));
                    var control = _parentWindow.Content as ExplorerControl;
                    control?.BuildDatabaseTree();
                    DataConnectionHelper.LogUsage("DatabasesAddCe35Database");
                }
                objIVsDataConnectionDialog.Dispose();
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, DatabaseType.SQLCE35);
            }
        }

        public void AddSqlServerDatabase(object sender, ExecutedRoutedEventArgs e)
        {
            // http://www.mztools.com/articles/2007/MZ2007011.aspx
            try
            {
                var objIVsDataConnectionDialogFactory = _package.GetServiceHelper(typeof(IVsDataConnectionDialogFactory)) as IVsDataConnectionDialogFactory;
                if (objIVsDataConnectionDialogFactory == null) return;
                var objIVsDataConnectionDialog = objIVsDataConnectionDialogFactory.CreateConnectionDialog();
                objIVsDataConnectionDialog.AddAllSources();
                objIVsDataConnectionDialog.SelectedSource = new Guid("067EA0D9-BA62-43f7-9106-34930C60C528");
                objIVsDataConnectionDialog.SelectedProvider = new Guid(Resources.SqlServerDotNetProvider);

                if (objIVsDataConnectionDialog.ShowDialog() && objIVsDataConnectionDialog.SelectedProvider == new Guid(Resources.SqlServerDotNetProvider))
                {
                    DataConnectionHelper.SaveDataConnection(_package, objIVsDataConnectionDialog.EncryptedConnectionString, DatabaseType.SQLServer, new Guid(Resources.SqlServerDotNetProvider));
                    var control = _parentWindow.Content as ExplorerControl;
                    control?.BuildDatabaseTree();
                    DataConnectionHelper.LogUsage("DatabasesAddSqlServerDatabase");
                }
                objIVsDataConnectionDialog.Dispose();
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, DatabaseType.SQLServer);
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
                        DataConnectionHelper.SaveDataConnection(_package, objIVsDataConnectionDialog.EncryptedConnectionString, DatabaseType.SQLCE40, new Guid(Resources.SqlCompact40Provider));
                        var control = _parentWindow.Content as ExplorerControl;
                        control?.BuildDatabaseTree();
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

        public void DesignDatabase(object sender, ExecutedRoutedEventArgs e)
        {
            EnvDteHelper.LaunchUrl("http://sqlcompact.dk/sqldesigner/");
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

        private static void SyncFxGetObjectsForSync(SyncFxDialog sfd, DatabaseMenuCommandParameters databaseInfo)
        {
            using (var repository = DataConnectionHelper.CreateRepository(databaseInfo.DatabaseInfo))
            {
                sfd.Tables = repository.GetAllTableNames().Where(t => !t.EndsWith("scope_info") && !t.EndsWith("scope_config") && !t.EndsWith("schema_info") && !t.EndsWith("_tracking")).ToList();
                sfd.Columns = repository.GetAllColumns().Where(t => !t.TableName.EndsWith("scope_info") && !t.TableName.EndsWith("scope_config") && !t.TableName.EndsWith("schema_info") && !t.TableName.EndsWith("_tracking")).ToList();
                sfd.PrimaryKeyColumns = repository.GetAllPrimaryKeys().Where(t => !t.TableName.EndsWith("scope_info") && !t.TableName.EndsWith("scope_config") && !t.TableName.EndsWith("schema_info") && !t.TableName.EndsWith("_tracking")).ToList();
            }
        }
    }
}
