using EnvDTE;
using EnvDTE80;
using ErikEJ.SqlCeScripting;
using ErikEJ.SqlCeToolbox.Dialogs;
using ErikEJ.SqlCeToolbox.Helpers;
using ErikEJ.SqlCeToolbox.ToolWindows;
#if !SSMS
using Microsoft.VisualStudio.Data.Services;
#endif
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

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

        public void AddPrivateSqlServerDatabase(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var serverConnectionString = DataConnectionHelper.PromptForConnectionString(_package);
                if (string.IsNullOrEmpty(serverConnectionString)) return;
                DataConnectionHelper.SaveDataConnection(serverConnectionString, DatabaseType.SQLServer, _package);
                var control = _parentWindow.Content as ExplorerControl;
                if (control != null) control.BuildDatabaseTree();
                DataConnectionHelper.LogUsage("DatabasesAddCeDatabase");
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, DatabaseType.SQLServer);
            }
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
#if !SSMS
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
#endif
        }

        public void AddSqlServerDatabase(object sender, ExecutedRoutedEventArgs e)
        {
#if !SSMS
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
#endif
        }

        public void AddCe40Database(object sender, ExecutedRoutedEventArgs e)
        {
#if !SSMS
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
#endif
        }

        private bool TryGetInitialPath(SqlCeToolboxPackage package, out string path)
        {
            var dte = package.GetServiceHelper(typeof(DTE)) as DTE2;
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

        public void CheckCeVersion(object sender, ExecutedRoutedEventArgs e)
        {
            var helper = Helpers.RepositoryHelper.CreateEngineHelper(DatabaseType.SQLCE40);
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
    }
}
