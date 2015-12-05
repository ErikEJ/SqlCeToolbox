using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.ServiceModel.Syndication;
using System.Xml;
using ErikEJ.SqlCeScripting;
using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.Win32;
using ErikEJ.SqlCeToolbox.Dialogs;
using ErikEJ.SQLiteScripting;
using System.Data.SqlClient;
using System.Data.SQLite;

namespace ErikEJ.SqlCeToolbox.Helpers
{

    internal class DataConnectionHelper
    {
        private static string separator = Environment.NewLine + "GO" + Environment.NewLine;

        internal static Dictionary<string, DatabaseInfo> GetDataConnections(SqlCeToolboxPackage package, bool includeServerConnections, bool serverConnectionsOnly)
        {
            // http://www.mztools.com/articles/2007/MZ2007018.aspx
            Dictionary<string, DatabaseInfo> databaseList = new Dictionary<string, DatabaseInfo>();
            var dataExplorerConnectionManager = package.GetServiceHelper(typeof(IVsDataExplorerConnectionManager)) as IVsDataExplorerConnectionManager;
            
            //Test code...

            //var objIVsDataProviderManager = package.GetServiceHelper(typeof(IVsDataProviderManager)) as IVsDataProviderManager;
            //var objIVsDataConnectionManager = package.GetServiceHelper(typeof(IVsDataConnectionManager)) as IVsDataConnectionManager;
            //var objIVsDataSourceManager = package.GetServiceHelper(typeof(IVsDataSourceManager)) as IVsDataSourceManager;

            //IVsDataProvider objIVsDataProvider;
            //foreach (var objIVsDataSource in objIVsDataSourceManager.Sources)
            //{
            //    //System.Diagnostics.Debug.WriteLine(objIVsDataSource.DisplayName);
            //    //System.Diagnostics.Debug.WriteLine(objIVsDataSource.Guid.ToString());

            //    //foreach (var objProviderGuid in objIVsDataSource.GetProviders())
            //    //{
            //    //    objIVsDataProvider = objIVsDataProviderManager.GetDataProvider(objProviderGuid);
            //    //    System.Diagnostics.Debug.WriteLine(objIVsDataProvider.DisplayName);
            //    //    System.Diagnostics.Debug.WriteLine(objIVsDataProvider.Guid.ToString());
            //    //}
            //}
            
            // End test code

            Guid provider35 = new Guid(Resources.SqlCompact35Provider);
            Guid provider40 = new Guid(Resources.SqlCompact40Provider);
            Guid providerSQLite = new Guid(Resources.SQLiteProvider);

            bool isV35Installed = IsV35Installed() && DDEXProviderIsInstalled(provider35);
            bool isV40Installed = IsV40Installed() && DDEXProviderIsInstalled(provider40);
            if (dataExplorerConnectionManager != null)
            {
                foreach (var connection in dataExplorerConnectionManager.Connections.Values)
                {
                    try
                    {
                        var objProviderGuid = connection.Provider;
                        if (!serverConnectionsOnly)
                        {
                            if ((objProviderGuid == provider35 && isV35Installed) || (objProviderGuid == provider40 && isV40Installed) )
                            {
                                DatabaseType dbType = DatabaseType.SQLCE40;
                                if (objProviderGuid == provider35)
                                    dbType = DatabaseType.SQLCE35;
                                var serverVersion = "4.0";
                                if (dbType == DatabaseType.SQLCE35)
                                    serverVersion = "3.5";

                                var sConnectionString = Microsoft.VisualStudio.Data.Services.DataProtection.DecryptString(connection.EncryptedConnectionString);
                                if (!sConnectionString.Contains("Mobile Device"))
                                {
                                    DatabaseInfo info = new DatabaseInfo();
                                    info.Caption = connection.DisplayName;
                                    info.FromServerExplorer = true;
                                    info.DatabaseType = dbType;
                                    info.ServerVersion = serverVersion;
                                    info.ConnectionString = sConnectionString;
                                    info.FileIsMissing = IsMissing(info);
                                    if (!databaseList.ContainsKey(sConnectionString))
                                        databaseList.Add(sConnectionString, info);
                                }
                            }

                            if (objProviderGuid == providerSQLite)
                            {
                                DatabaseType dbType = DatabaseType.SQLite;

                                var sConnectionString = Microsoft.VisualStudio.Data.Services.DataProtection.DecryptString(connection.EncryptedConnectionString);
                                DatabaseInfo info = new DatabaseInfo();
                                info.Caption = connection.DisplayName;
                                info.FromServerExplorer = true;
                                info.DatabaseType = dbType;
                                info.ServerVersion = "3.8";
                                info.ConnectionString = sConnectionString;
                                info.FileIsMissing = IsMissing(info);
                                if (!databaseList.ContainsKey(sConnectionString))
                                    databaseList.Add(sConnectionString, info);
                            }                            
                        }
                        if (includeServerConnections && objProviderGuid == new Guid(Resources.SqlServerDotNetProvider))
                        {
                            var sConnectionString = Microsoft.VisualStudio.Data.Services.DataProtection.DecryptString(connection.EncryptedConnectionString);
                            var info = new DatabaseInfo();
                            info.Caption = connection.DisplayName;
                            info.FromServerExplorer = true;
                            info.DatabaseType = DatabaseType.SQLServer;
                            info.ServerVersion = string.Empty;
                            info.ConnectionString = sConnectionString;
                            if (!databaseList.ContainsKey(sConnectionString))
                                databaseList.Add(sConnectionString, info);
                        }
                    }
                    catch (KeyNotFoundException)
                    {
                        continue;
                    }
                    catch (NullReferenceException)
                    {
                        continue;
                    }
                }
            }
            return databaseList;
        }

        private static DatabaseType GetPreferredDatabaseType()
        {
            //Assume 3.5 is installed
            DatabaseType dbType = DatabaseType.SQLCE35;
            if (!IsV35Installed()) //So 4.0 is installed
            {
                if (!IsV40Installed())
                {
                    dbType = DatabaseType.SQLite;
                }
                else
                {
                    dbType = DatabaseType.SQLCE40;
                }
            }
            return dbType;
        }

        internal static Dictionary<string, DatabaseInfo> GetOwnDataConnections()
        {
            Dictionary<string, DatabaseInfo> databaseList = new Dictionary<string, DatabaseInfo>();
            DatabaseType dbType = GetPreferredDatabaseType();
            DatabaseInfo dbInfo = new DatabaseInfo { ConnectionString = CreateStore(dbType), DatabaseType = dbType };
            using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(dbInfo))
            {
                string script = "SELECT FileName, Source, CeVersion FROM Databases" + separator;
                var dataset = repository.ExecuteSql(script);
                foreach (DataRow row in dataset.Tables[0].Rows)
                {
                    string key = row[1].ToString();
                    DatabaseType type = (DatabaseType)int.Parse(row[2].ToString());
                    var info = new DatabaseInfo();
                    try
                    {
                        info.Caption = System.IO.Path.GetFileName(row[0].ToString());
                    }
                    catch (ArgumentException)
                    {
                        info.Caption = row[0].ToString();
                    }
                    info.DatabaseType = type;
                    info.FromServerExplorer = false;
                    info.ConnectionString = key;
                    info.ServerVersion = "4.0.0.0";
                    if (type == DatabaseType.SQLCE35)
                        info.ServerVersion = "3.5.1.0";
                    if (type == DatabaseType.SQLite)
                        info.ServerVersion = "3.8";
                    info.FileIsMissing = IsMissing(info);
                    if (!databaseList.ContainsKey(key))
                    {
                        databaseList.Add(key, info);
                    }
                }                
            }
            return databaseList;
        }

        internal static bool DDEXProviderIsInstalled(Guid id)
        {
            IVsDataProvider provider = null;
            var objIVsDataProviderManager = SqlCeToolboxPackage.GetGlobalService(typeof(IVsDataProviderManager)) as IVsDataProviderManager;
            return objIVsDataProviderManager.Providers.TryGetValue(id, out provider);
        }

        internal void ValidateConnections(SqlCeToolboxPackage package)
        {
            var dataExplorerConnectionManager = package.GetServiceHelper(typeof(IVsDataExplorerConnectionManager)) as IVsDataExplorerConnectionManager;
            var removals = new List<IVsDataExplorerConnection>();

            foreach (var connection in dataExplorerConnectionManager.Connections.Values)
            {
                try
                {
                    var objProviderGuid = connection.Provider;
                    if ((objProviderGuid == new Guid (Resources.SqlCompact35Provider) && IsV35Installed()) || (objProviderGuid == new Guid (Resources.SqlCompact40Provider) && IsV40Installed()))
                    {
                        connection.Connection.Open();
                        connection.Connection.Close();
                    }
                }
                catch (Exception ex)
                {
                    if (ex.GetType().Name == "SqlCeException")
                    {
                        removals.Add(connection);
                    }
                    if (ex.GetType() == typeof(ArgumentException))
                    {
                        removals.Add(connection);
                    }
                    if (ex.GetType() == typeof(KeyNotFoundException))
                    {
                        removals.Add(connection);
                    }
                    throw;
                }

            }
            for (int i = removals.Count - 1; i >= 0; i--)
            {
                try
                {
                    dataExplorerConnectionManager.RemoveConnection(removals[i]);
                }
                catch (ArgumentException)
                {
                }
                catch (IndexOutOfRangeException)
                {
                }
                catch (KeyNotFoundException)
                {
                }
            }

            var ownConnections = GetOwnDataConnections();
            foreach (var item in ownConnections)
            {
                try
                {
                    using (var test = CreateRepository(item.Value))
                    { }
                }
                catch (Exception ex)
                {
                    if (ex.GetType().Name == "SqlCeException")
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                        RemoveDataConnection(item.Value.ConnectionString);
                    }
                    throw;
                }
            }
        }

        internal void ScanConnections(SqlCeToolboxPackage package)
        {
            var dte = package.GetServiceHelper(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
            var helper = Helpers.DataConnectionHelper.CreateEngineHelper(DatabaseType.SQLCE40);
            EnvDTEHelper dteHelper = new EnvDTEHelper();
            var list = dteHelper.GetSqlCeFilesInActiveSolution(dte);
            foreach (var path in list)
            {
                if (File.Exists(path))
                {
                    bool versionFound = false;
                    SQLCEVersion version = SQLCEVersion.SQLCE20;
                    try
                    {
                        version = helper.DetermineVersion(path);
                        versionFound = true;
                    }
                    catch {}
                    string connectionString = string.Format("Data Source={0}", path);
                    if (versionFound)
                    {                        
                        if (version == SQLCEVersion.SQLCE35)
                        {
                            SaveDataConnection(connectionString, DatabaseType.SQLCE35, package);
                        }
                        else if (version == SQLCEVersion.SQLCE40)
                        {
                            SaveDataConnection(connectionString, DatabaseType.SQLCE40, package);
                        }
                    }
                    else
                    { 
                        var dbInfo = new DatabaseInfo();
                        dbInfo.DatabaseType = DatabaseType.SQLite;
                        dbInfo.ConnectionString = connectionString;
                        try
                        {
                            using (var repo = CreateRepository(dbInfo))
                            {
                                repo.GetAllTableNames();
                            }
                            SaveDataConnection(connectionString, DatabaseType.SQLite, package);
                        }
                        catch { }
                    }
                }
            }
        }

        internal static void SaveDataConnection(SqlCeToolboxPackage package, string connectionString, string testString, DatabaseType dbType, Guid provider, bool encryptedString = true)
        {
            var dataExplorerConnectionManager = package.GetServiceHelper(typeof(IVsDataExplorerConnectionManager)) as IVsDataExplorerConnectionManager;
            if (dataExplorerConnectionManager != null)
            {
                string savedName = GetFileName(testString, dbType);
                dataExplorerConnectionManager.AddConnection(savedName, provider, connectionString, encryptedString);
            }
        }

        public static string GetFilePath(string connectionString, DatabaseType dbType)
        {
            var helper = CreateEngineHelper(dbType);
            return helper.PathFromConnectionString(connectionString);            
        }

        private static string GetFileName(string connectionString, DatabaseType dbType)
        {
            var filePath = GetFilePath(connectionString, dbType);
            return Path.GetFileName(filePath);;
        }

        internal static void SaveDataConnection(string connectionString, DatabaseType dbType, SqlCeToolboxPackage package)
        {
            var storeDbType = GetPreferredDatabaseType();
            var helper = CreateEngineHelper(storeDbType);
            string path = CreateEngineHelper(dbType).PathFromConnectionString(connectionString);
            helper.SaveDataConnection(CreateStore(storeDbType), connectionString, path, dbType.GetHashCode());

            if (package.VSSupportsSimpleDDEX35Provider() && dbType == DatabaseType.SQLCE35)
            {
                SaveDataConnection(package, connectionString, connectionString, dbType, new Guid(Resources.SqlCompact35PrivateProvider), false);
            }
            if (package.VSSupportsSimpleDDEX4Provider() && dbType == DatabaseType.SQLCE40)
            {
                SaveDataConnection(package, connectionString, connectionString, dbType, new Guid(Resources.SqlCompact40PrivateProvider), false);
            }
        }

        internal static void RemoveDataConnection(string connectionString)
        {
            var storeType = GetPreferredDatabaseType();
            var helper = CreateEngineHelper(storeType);
            helper.DeleteDataConnnection(CreateStore(storeType), connectionString);
        }

        internal static void RenameDataConnection(string connectionString, string description)
        {
            var storeType = GetPreferredDatabaseType();
            var helper = CreateEngineHelper(storeType);
            helper.UpdateDataConnection(CreateStore(storeType), connectionString, description);
        }

        internal static void RemoveDataConnection(SqlCeToolboxPackage package, string connectionString, Guid provider)
        {
            var removals = new List<IVsDataExplorerConnection>();
            var dataExplorerConnectionManager = package.GetServiceHelper(typeof(IVsDataExplorerConnectionManager)) as IVsDataExplorerConnectionManager;
            foreach  (var connection in dataExplorerConnectionManager.Connections.Values)
            {
                var objProviderGuid = connection.Provider;
                if ((objProviderGuid == new Guid(Resources.SqlCompact35Provider)) || (objProviderGuid == new Guid(Resources.SqlCompact40Provider)))
                {
                    if (Microsoft.VisualStudio.Data.Services.DataProtection.DecryptString(connection.EncryptedConnectionString) == connectionString)
                    {
                        removals.Add(connection);
                    }
                }
            }

            for (int i = removals.Count - 1; i >= 0; i--)
            {
                try
                {
                    dataExplorerConnectionManager.RemoveConnection(removals[i]);
                }
                catch (ArgumentException)
                {
                }
                catch (IndexOutOfRangeException)
                {
                }
            }
        }

        public static string PromptForConnectionString(SqlCeToolboxPackage package)
        {
            var databaseList = DataConnectionHelper.GetDataConnections(package, true, true);
            PickServerDatabaseDialog psd = new PickServerDatabaseDialog(databaseList);
            bool? res = psd.ShowModal();
            if (res.HasValue && res.Value == true && (psd.SelectedDatabase.Value != null))
            {
                return psd.SelectedDatabase.Value.ConnectionString;
            }
            return null;
        }

        private static string CreateStore(DatabaseType storeDbType)
        {
            string fileName = GetStoreName(storeDbType);
            string connString = string.Format("Data Source={0};", fileName);
            if (!File.Exists(fileName))
            {
                if (storeDbType == DatabaseType.SQLite)
                {
                    var helper = CreateEngineHelper(storeDbType);
                    helper.CreateDatabase(connString);
                }
                else
                {
                    var sdf = Resources.SqlCe35AddinStore;
                    if (storeDbType == DatabaseType.SQLCE40)
                        sdf = Resources.SqlCe40AddinStore;
                    using (Stream stream = new MemoryStream(sdf))
                    {
                        // Create a FileStream object to write a stream to a file 
                        using (FileStream fileStream = File.Create(fileName, (int)stream.Length))
                        {
                            // Fill the bytes[] array with the stream data 
                            byte[] bytesInStream = new byte[stream.Length];
                            stream.Read(bytesInStream, 0, (int)bytesInStream.Length);
                            // Use FileStream object to write to the specified file 
                            fileStream.Write(bytesInStream, 0, bytesInStream.Length);
                        }
                    }
                }
            }

            var dbInfo = new DatabaseInfo { DatabaseType = storeDbType, ConnectionString = connString };
            using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(dbInfo))
            {
                var tables = repository.GetAllTableNames();
                if (!tables.Contains("Databases"))
                {
                    var  script = "CREATE TABLE Databases (Id INT IDENTITY, Source nvarchar(2048) NOT NULL, FileName nvarchar(512) NOT NULL, CeVersion int NOT NULL)" + separator;
                    if (storeDbType == DatabaseType.SQLite)
                        script = "CREATE TABLE Databases (Id INTEGER PRIMARY KEY, Source nvarchar(2048) NOT NULL, FileName nvarchar(512) NOT NULL, CeVersion int NOT NULL)" + separator;
                    repository.ExecuteSql(script);
                }
            }
            return connString;
        }

        private static string GetStoreName(DatabaseType storeDbType)
        {
            string file = "SqlCe35AddinStore.sdf";
            if (storeDbType == DatabaseType.SQLCE40)
                file = "SqlCe40AddinStore.sdf";
            if (storeDbType == DatabaseType.SQLite)
            {
                file = "SQLiteAddinStore.db";
            }
            string fileName = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), file);
            return fileName;
        }

        public static IGenerator CreateGenerator(IRepository repository, string outFile, DatabaseType databaseType)
        {
            switch (databaseType)
            {
                case DatabaseType.SQLServer:
                    return new Generator(repository, outFile, false, Properties.Settings.Default.PreserveSqlDates, false, Properties.Settings.Default.KeepSchemaNames);
                case DatabaseType.SQLCE35:
                    return new Generator(repository, outFile);
                case DatabaseType.SQLCE40:
                    return new Generator4(repository, outFile);
                case DatabaseType.SQLite:
                    return new Generator(repository, outFile, false, false, true);
                default:
                    return null;
            }
        }

        public static ISqlCeHelper CreateEngineHelper(DatabaseType databaseType)
        {
            switch (databaseType)
            {
                case DatabaseType.SQLServer:
                case DatabaseType.SQLCE35:
                    return new SqlCeHelper();
                case DatabaseType.SQLCE40:
                    return new SqlCeHelper4();
                case DatabaseType.SQLite:
                    return new SqliteHelper();
                default:
                    return null;
            }
        }

        public static IGenerator CreateGenerator(IRepository repository, DatabaseType databaseType)
        {
            return CreateGenerator(repository, null, databaseType);
        }

        public static IRepository CreateRepository(DatabaseInfo databaseInfo)
        {
            switch (databaseInfo.DatabaseType)
            {
                case DatabaseType.SQLCE35:
                    return new DBRepository(databaseInfo.ConnectionString);
                case DatabaseType.SQLCE40:
                    return new DB4Repository(databaseInfo.ConnectionString);
                case DatabaseType.SQLServer:
                    return new ServerDBRepository(databaseInfo.ConnectionString, Properties.Settings.Default.KeepSchemaNames);
                case DatabaseType.SQLite:
                    return new SQLiteRepository(databaseInfo.ConnectionString);
                default:
                    return null;
            }
        }

        public static bool IsPremiumOrUltimate()
        {
            // From http://blogs.msdn.com/b/heaths/archive/2010/05/04/detection-keys-for-net-framework-4-0-and-visual-studio-2010.aspx
            //Key HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\DevDiv\VS\Servicing\10.0\$(var.ProductEdition)\$(var.LCID) 
            //The values for $(var.ProductEdition) include the following table. 
            //Visual Studio 2010 Ultimate VSTSCore 
            //Visual Studio 2010 Premium VSTDCore 
            //Visual Studio 2010 Professional PROCore 
            //Visual Studio 2010 Shell (Integrated) IntShell 
            using (var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
            {
                var ultimateKey = key.OpenSubKey(@"SOFTWARE\Microsoft\DevDiv\VS\Servicing\10.0\VSTSCore");
                if (ultimateKey != null)
                    return true;

                var premiumKey = key.OpenSubKey(@"SOFTWARE\Microsoft\DevDiv\VS\Servicing\10.0\VSTDCore");
                if (premiumKey != null)
                    return true;
            }
            return false;
        }

        public static void RegisterDDEXProviders(bool force)
        {
            if (SqlCeToolboxPackage.VisualStudioVersion == new Version(12, 0) || SqlCeToolboxPackage.VisualStudioVersion == new Version(14, 0))
            {
                RegisterDDEX4Provider(force);
                RegisterDDEX35Provider(force);
            }
#if DEBUG
            //if (VisualStudioVersion == new Version(10, 0))
            //{
            //    Helpers.DataConnectionHelper.RegisterDDEX35Provider("12");
            //    Helpers.DataConnectionHelper.RegisterDDEX35Provider("14");
            //    Helpers.DataConnectionHelper.RegisterDDEX35VS10DebugProvider();
            //}
#endif
            if (SqlCeToolboxPackage.VisualStudioVersion == new Version(11, 0))
            {
                RegisterDDEX35Provider(force);
            }
        }

        private static void RegisterDDEX4Provider(bool force)
        {
            string ver = SqlCeToolboxPackage.VisualStudioVersion.ToString(1);
            try
            {
                if (force)
                {
                    DDEXRegistry.AddDDEX4Registrations(ver);
                }
                else
                {
                    //Check if provider keys exists
                    using (var key = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32))
                    {
                        var ddexKey = key.OpenSubKey(string.Format(@"SOFTWARE\Microsoft\VisualStudio\{0}.0_Config\DataProviders\{{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}}", ver));
                        if (ddexKey == null)
                        {
                            DDEXRegistry.AddDDEX4Registrations(ver);
                        }
                    }
                }
                string ddexDllPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "SqlCeToolbox.DDEX4.dll");
                if (File.Exists(ddexDllPath))
                {
                    Registry.SetValue(string.Format(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VisualStudio\{0}.0_Config\DataProviders\{{673BE80C-CB41-47A7-B0F3-9872B6DDE5E5}}", ver),
                        "Codebase",
                        ddexDllPath,
                        RegistryValueKind.String);
                }
            }
            catch (Exception ex)
            {
                SendError(ex, DatabaseType.SQLServer, true);
            }
        }

        private static void RegisterDDEX35Provider(bool force)
        {
            string ver = SqlCeToolboxPackage.VisualStudioVersion.ToString(1);
            try
            {
                if (force)
                {
                    DDEXRegistry.AddDDEX4Registrations(ver);
                }
                else
                {
                    //Check if provider keys exists
                    using (var key = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32))
                    {
                        var ddexKey = key.OpenSubKey(string.Format(@"SOFTWARE\Microsoft\VisualStudio\{0}.0_Config\DataProviders\{{303D8BB1-D62A-4560-9742-79C93E828222}}", ver));
                        if (ddexKey == null)
                        {
                            DDEXRegistry.AddDDEX35Registrations(ver);
                        }
                    }
                }
                string ddexDllPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "SqlCeToolbox.DDEX35.dll");
                if (File.Exists(ddexDllPath))
                {
                    Registry.SetValue(string.Format(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VisualStudio\{0}.0_Config\DataProviders\{{303D8BB1-D62A-4560-9742-79C93E828222}}", ver),
                        "Codebase",
                        ddexDllPath,
                        RegistryValueKind.String);
                }
            }
            catch (Exception ex)
            {
                SendError(ex, DatabaseType.SQLServer, true);
            }
        }

        public static void RegisterDDEX35VS10DebugProvider()
        {
            try
            {
                DDEXRegistry.AddDDEX35VS10DebugRegistrations();
                string ddexDllPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "SqlCeToolbox.DDEX35.dll");
                if (File.Exists(ddexDllPath))
                {
                    Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\VisualStudio\10.0Exp_Config\DataProviders\{303D8BB1-D62A-4560-9742-79C93E828222}",
                        "Codebase",
                        ddexDllPath,
                        RegistryValueKind.String);
                }
            }
            catch (Exception ex)
            {
                SendError(ex, DatabaseType.SQLServer, true);
            }
        }

        public static bool CheckVersion(string lookingFor)
        {
            try
            {
                using (var wc = new System.Net.WebClient())
                {
                    wc.Proxy = System.Net.WebRequest.GetSystemWebProxy();
                    var xDoc = new System.Xml.XmlDocument();
                    string s = wc.DownloadString(@"http://www.sqlcompact.dk/SqlCeToolboxVersions.xml");
                    xDoc.LoadXml(s);

                    string newVersion = xDoc.DocumentElement.Attributes[lookingFor].Value;
                    
                    Version vN = new Version(newVersion );
                    if (vN > Assembly.GetExecutingAssembly().GetName().Version)
                    {
                        return true;
                    }

                }
            }
            catch { }
            return false;
        }

        public static string GetDownloadCount()
        {
            try
            {
                XmlReader reader = XmlReader.Create("http://sqlcompact.dk/vsgallerycounter/downloadfeed.axd?extensionId=0e313dfd-be80-4afb-b5e9-6e74d369f7a1");
                SyndicationFeed feed = SyndicationFeed.Load(reader);
                foreach (var item in feed.Items)
                {
                    return string.Format("- {0:0,0} downloads", double.Parse(item.Summary.Text));
                }
            }
            catch { }
            return string.Format("- more than {0:0,0} downloads", 340000d);
        }

        public static string GetSqlCeFileFilter()
        {
            return string.Format("SQL Server Compact Database|{0}|All Files|*.*", Properties.Settings.Default.FileFilterSqlCe);
        }

        public static string GetSqliteFileFilter()
        {
            return string.Format("SQLite Database file|{0}|All Files|*.*", Properties.Settings.Default.FileFilterSqlite);
        }

        internal static void LogUsage(string feature)
        {
            Telemetry.TrackEvent(feature);
        }

        internal static string SendError(Exception ex, DatabaseType dbType, bool report = true)
        {
            if (ex != null)
            {
                var dontTrack = ex.GetType().Name == "SqlCeException"
                    || ex is SqlException
                    || ex is SQLiteException;

                if (!dontTrack)
                {
                    Telemetry.TrackException(ex);
                }
                EnvDTEHelper.ShowError(CreateEngineHelper(dbType).FormatError(ex));
            }
            return string.Empty; 
        }

        internal static bool IsMissing(DatabaseInfo info)
        {
            try
            {
                var path = GetFilePath(info.ConnectionString, info.DatabaseType);
                return !File.Exists(path);
            }
            catch
            {
                return false;
            }
        }

        internal static bool IsV40Installed()
        {
            return new SqlCeHelper4().IsV40Installed();
        }

        internal static bool IsV35Installed()
        {
            return new SqlCeHelper4().IsV35Installed();
        }

        internal static bool IsV40DbProviderInstalled()
        {
            return new SqlCeHelper4().IsV40DbProviderInstalled();
        }

        internal static bool IsSQLiteDbProviderInstalled()
        {
            try
            {
                System.Data.Common.DbProviderFactories.GetFactory("System.Data.SQLite.EF6");
            }
            catch (System.Configuration.ConfigurationException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
            return true;
        }

        internal static bool IsV35DbProviderInstalled()
        {
            return new SqlCeHelper4().IsV35DbProviderInstalled();
        }

        internal static bool IsSyncFx21Installed()
        {
            try
            {
                System.Reflection.Assembly.Load("Microsoft.Synchronization.Data.SqlServerCe, Version=3.1.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91");
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
