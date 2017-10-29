using EFCorePowerTools;
using EnvDTE;
using ErikEJ.SqlCeScripting;
using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using VSLangProj;

// ReSharper disable once CheckNamespace
namespace ErikEJ.SqlCeToolbox.Helpers
{
    internal class EnvDteHelper
    {
        //TODO Update this when SQLite provider is updated!
        private static string SqliteEngineVersion = "3.18";

        internal static Dictionary<string, DatabaseInfo> GetDataConnections(EFCorePowerToolsPackage package,
    bool includeServerConnections = true)
        {
            // http://www.mztools.com/articles/2007/MZ2007018.aspx
            Dictionary<string, DatabaseInfo> databaseList = new Dictionary<string, DatabaseInfo>();
            var dataExplorerConnectionManager = package.GetService<IVsDataExplorerConnectionManager>();
            Guid provider40 = new Guid(Resources.SqlCompact40Provider);
            Guid provider40Private = new Guid(Resources.SqlCompact40PrivateProvider);
            Guid providerSqLite = new Guid(Resources.SQLiteProvider);
            Guid providerSqlitePrivate = new Guid(Resources.SqlitePrivateProvider);

            bool isV40Installed = RepositoryHelper.IsV40Installed() &&
                (DdexProviderIsInstalled(provider40) || DdexProviderIsInstalled(provider40Private));
            if (dataExplorerConnectionManager != null)
            {
                foreach (var connection in dataExplorerConnectionManager.Connections.Values)
                {
                    try
                    {
                        var objProviderGuid = connection.Provider;
                        if (objProviderGuid == provider40 && isV40Installed ||
                            objProviderGuid == provider40Private && isV40Installed)
                        {
                            DatabaseType dbType = DatabaseType.SQLCE40;
                            var serverVersion = "4.0";

                            var sConnectionString =
                                DataProtection.DecryptString(connection.EncryptedConnectionString);
                            if (!sConnectionString.Contains("Mobile Device"))
                            {
                                DatabaseInfo info = new DatabaseInfo()
                                {
                                    Caption = connection.DisplayName,
                                    FromServerExplorer = true,
                                    DatabaseType = dbType,
                                    ServerVersion = serverVersion,
                                    ConnectionString = sConnectionString
                                };
                                info.FileIsMissing = RepositoryHelper.IsMissing(info);
                                if (!databaseList.ContainsKey(sConnectionString))
                                    databaseList.Add(sConnectionString, info);
                            }
                        }

                        if (objProviderGuid == providerSqLite
                            || objProviderGuid == providerSqlitePrivate)
                        {
                            DatabaseType dbType = DatabaseType.SQLite;

                            var sConnectionString =
                                DataProtection.DecryptString(connection.EncryptedConnectionString);
                            DatabaseInfo info = new DatabaseInfo()
                            {
                                Caption = connection.DisplayName,
                                FromServerExplorer = true,
                                DatabaseType = dbType,
                                ServerVersion = SqliteEngineVersion,
                                ConnectionString = sConnectionString
                            };
                            info.FileIsMissing = RepositoryHelper.IsMissing(info);
                            if (!databaseList.ContainsKey(sConnectionString))
                                databaseList.Add(sConnectionString, info);
                        }
                        if (includeServerConnections && objProviderGuid == new Guid(Resources.SqlServerDotNetProvider))
                        {
                            var sConnectionString = DataProtection.DecryptString(connection.EncryptedConnectionString);
                            var info = new DatabaseInfo()
                            {
                                Caption = connection.DisplayName,
                                FromServerExplorer = true,
                                DatabaseType = DatabaseType.SQLServer,
                                ServerVersion = string.Empty,
                                ConnectionString = sConnectionString
                            };
                            if (!databaseList.ContainsKey(sConnectionString))
                                databaseList.Add(sConnectionString, info);
                        }
                    }
                    catch (KeyNotFoundException)
                    {
                    }
                    catch (NullReferenceException)
                    {
                    }
                }
            }
            return databaseList;
        }

        internal static bool DdexProviderIsInstalled(Guid id)
        {
            try
            {
                var objIVsDataProviderManager =
                    Package.GetGlobalService(typeof(IVsDataProviderManager)) as IVsDataProviderManager;
                return objIVsDataProviderManager != null &&
                    objIVsDataProviderManager.Providers.TryGetValue(id, out IVsDataProvider _);
            }
            catch
            {
                //Ignored
            }
            return false;
        }

        internal static DatabaseInfo PromptForInfo(EFCorePowerToolsPackage package)
        {
            // Show dialog with SqlClient selected by default
            var dialogFactory = package.GetService<IVsDataConnectionDialogFactory>();
            var dialog = dialogFactory.CreateConnectionDialog();
            dialog.AddAllSources();
            dialog.SelectedSource = new Guid("067ea0d9-ba62-43f7-9106-34930c60c528");
            var dialogResult = dialog.ShowDialog(connect: true);

            if (dialogResult == null) return new DatabaseInfo {DatabaseType = DatabaseType.SQLCE35};

            var info = GetDatabaseInfo(package, dialogResult.Provider, DataProtection.DecryptString(dialog.EncryptedConnectionString));
            SaveDataConnection(package, dialog.EncryptedConnectionString, info.DatabaseType, new Guid(info.Size));
            return info;
        }

        internal static void SaveDataConnection(EFCorePowerToolsPackage package, string encryptedConnectionString,
            DatabaseType dbType, Guid provider)
        {
            var dataExplorerConnectionManager = package.GetService<IVsDataExplorerConnectionManager>();
            var savedName = GetFileName(DataProtection.DecryptString(encryptedConnectionString), dbType);
            dataExplorerConnectionManager.AddConnection(savedName, provider, encryptedConnectionString, true);
        }

        private static DatabaseInfo GetDatabaseInfo(EFCorePowerToolsPackage package, Guid provider, string connectionString)
        {
            var dbType = DatabaseType.SQLCE35;
            var providerInvariant = "N/A";
            var providerGuid = Guid.Empty.ToString();
            // Find provider
            var providerManager = package.GetService<IVsDataProviderManager>();
            IVsDataProvider dp;
            providerManager.Providers.TryGetValue(provider, out dp);
            if (dp != null)
            {
                providerInvariant = (string)dp.GetProperty("InvariantName");
                dbType = DatabaseType.SQLCE35;
                if (providerInvariant == "System.Data.SqlServerCe.4.0")
                {
                    dbType = DatabaseType.SQLCE40;
                    providerGuid = EFCorePowerTools.Resources.SqlCompact40PrivateProvider;
                }
                if (providerInvariant == "System.Data.SQLite.EF6")
                {
                    dbType = DatabaseType.SQLite;
                    providerGuid = EFCorePowerTools.Resources.SqlitePrivateProvider;
                }
                if (providerInvariant == "System.Data.SqlClient")
                {
                    dbType = DatabaseType.SQLServer;
                    providerGuid = EFCorePowerTools.Resources.SqlServerDotNetProvider;
                }
            }
            return new DatabaseInfo
            {
                DatabaseType = dbType,
                ConnectionString = connectionString,
                ServerVersion = providerInvariant,
                Size = providerGuid
            };
        }

        private static string GetFilePath(string connectionString, DatabaseType dbType)
        {
            var helper = RepositoryHelper.CreateEngineHelper(dbType);
            return helper.PathFromConnectionString(connectionString);
        }

        private static string GetFileName(string connectionString, DatabaseType dbType)
        {
            if (dbType == DatabaseType.SQLServer)
            {
                var helper = new SqlServerHelper();
                return helper.PathFromConnectionString(connectionString);
            }
            var filePath = GetFilePath(connectionString, dbType);
            return Path.GetFileName(filePath);
        }

        internal static bool IsSqLiteDbProviderInstalled()
        {
            try
            {
                System.Data.Common.DbProviderFactories.GetFactory("System.Data.SQLite.EF6");
            }
            catch
            {
                return false;
            }
            return true;
        }

        public Tuple<bool, string> ContainsEfCoreReference(Project project, DatabaseType dbType)
        {
            var providerPackage = "Microsoft.EntityFrameworkCore.SqlServer";
            if (dbType == DatabaseType.SQLCE40)
            {
                providerPackage = "EntityFrameworkCore.SqlServerCompact40";
            }
            if (dbType == DatabaseType.SQLite)
            {
                providerPackage = "Microsoft.EntityFrameworkCore.Sqlite";
            }

            var vsProject = project.Object as VSProject;
            if (vsProject == null) return new Tuple<bool, string>(false, providerPackage);
            for (var i = 1; i < vsProject.References.Count + 1; i++)
            {
                if (vsProject.References.Item(i).Name.Equals(providerPackage))
                {
                    return new Tuple<bool, string>(true, providerPackage);
                }
            }
            return new Tuple<bool, string>(false, providerPackage);
        }


        // <summary>
        //     Helper method to show an error message within the shell.  This should be used
        //     instead of MessageBox.Show();
        // </summary>
        // <param name="errorText">Text to display.</param>
        public static void ShowError(string errorText)
        {
            ShowMessageBox(
                errorText, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                OLEMSGICON.OLEMSGICON_CRITICAL);
        }

        public static DialogResult ShowMessage(string messageText)
        {
            return ShowMessageBox(messageText, null, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST, OLEMSGICON.OLEMSGICON_INFO);
        }

        // <summary>
        //     Helper method to show a message box within the shell.
        // </summary>
        // <param name="messageText">Text to show.</param>
        // <param name="messageButtons">Buttons which should appear in the dialog.</param>
        // <param name="defaultButton">Default button (invoked when user presses return).</param>
        // <param name="messageIcon">Icon (warning, error, informational, etc.) to display</param>
        // <returns>result corresponding to the button clicked by the user.</returns>
        private static void ShowMessageBox(string messageText, OLEMSGBUTTON messageButtons, OLEMSGDEFBUTTON defaultButton, OLEMSGICON messageIcon)
        {
            ShowMessageBox(messageText, null, messageButtons, defaultButton, messageIcon);
        }

        // <summary>
        //     Helper method to show a message box within the shell.
        // </summary>
        // <param name="messageText">Text to show.</param>
        // <param name="f1Keyword">F1-keyword.</param>
        // <param name="messageButtons">Buttons which should appear in the dialog.</param>
        // <param name="defaultButton">Default button (invoked when user presses return).</param>
        // <param name="messageIcon">Icon (warning, error, informational, etc.) to display</param>
        // <returns>result corresponding to the button clicked by the user.</returns>
        private static DialogResult ShowMessageBox(
            string messageText, string f1Keyword, OLEMSGBUTTON messageButtons,
            OLEMSGDEFBUTTON defaultButton, OLEMSGICON messageIcon)
        {
            var result = 0;
            var uiShell = (IVsUIShell)Package.GetGlobalService(typeof(SVsUIShell));

            if (uiShell != null)
            {
                var rclsidComp = Guid.Empty;
                uiShell.ShowMessageBox(
                        0, ref rclsidComp, "EF Core Power Tools", messageText, f1Keyword, 0, messageButtons, defaultButton, messageIcon, 0, out result);
            }

            return (DialogResult)result;
        }
    }
}
