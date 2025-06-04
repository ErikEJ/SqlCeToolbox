using ErikEJ.SqlCeScripting;
using ErikEJ.SQLiteScripting;
using System.Data.SqlClient;
using System.IO;

namespace ErikEJ.SqlCeToolbox.Helpers
{
    internal static class RepositoryHelper
    {
        internal static IRepository CreateRepository(DatabaseInfo databaseInfo)
        {
            switch (databaseInfo.DatabaseType)
            {
                case DatabaseType.SQLCE35:
                    return new DBRepository(databaseInfo.ConnectionString);
                case DatabaseType.SQLCE40:
                    return new DB4Repository(databaseInfo.ConnectionString);
                case DatabaseType.SQLServer:
                    return new ServerDBRepository(SqlConnectionStringBuilderHelper.GetBuilder(databaseInfo.ConnectionString).ConnectionString, Properties.Settings.Default.KeepSchemaNames);
                case DatabaseType.SQLite:
                    return new SQLiteRepository(databaseInfo.ConnectionString);
                default:
                    return null;
            }
        }

        public static ISqlCeHelper CreateEngineHelper(DatabaseType databaseType)
        {
            switch (databaseType)
            {
                case DatabaseType.SQLCE35:
                    return new SqlCeHelper();
                case DatabaseType.SQLCE40:
                    return new SqlCeHelper4();
                case DatabaseType.SQLServer:
                case DatabaseType.SQLite:
                    return new SqliteHelper();
                default:
                    return null;
            }
        }

        public static IGenerator CreateGenerator(IRepository repository, string outFile, DatabaseType databaseType)
        {
            switch (databaseType)
            {
                case DatabaseType.SQLServer:
                    return new Generator(repository, outFile, false, false);
                case DatabaseType.SQLCE40:
                    return string.IsNullOrEmpty(outFile)
                        ? new Generator4(repository)
                        : new Generator4(repository, outFile);
                case DatabaseType.SQLite:
                    return new Generator(repository, outFile, false, false, true);
                default:
                    return null;
            }
        }

        public static string GetClassBasis(string connectionString, DatabaseType dbType)
        {
            string classBasis;
            if (dbType == DatabaseType.SQLServer)
            {
                var builder = new SqlConnectionStringBuilder(connectionString);
                classBasis = builder.InitialCatalog;

                if (string.IsNullOrEmpty(classBasis) && !string.IsNullOrEmpty(builder.AttachDBFilename))
                {
                    classBasis = Path.GetFileNameWithoutExtension(builder.AttachDBFilename);
                }
            }
            else
            {
                var path = GetFilePath(connectionString, dbType);
                classBasis = Path.GetFileNameWithoutExtension(path);
            }
            return classBasis;
        }

        private static string GetFilePath(string connectionString, DatabaseType dbType)
        {
            var helper = CreateEngineHelper(dbType);

            if (dbType == DatabaseType.SQLServer)
            {
                return helper.PathFromConnectionString(connectionString);
            }
            return helper.PathFromConnectionString(connectionString);
        }

        public static bool IsMissing(DatabaseInfo info)
        {
            if (info.DatabaseType == DatabaseType.SQLServer)
            {
                return false;
            }
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

        internal static class SqlConnectionStringBuilderHelper
        {
            public static SqlConnectionStringBuilder GetBuilder(string connectionString)
            {
                connectionString = ReplaceMdsKeywords(connectionString);
                return new SqlConnectionStringBuilder(connectionString);
            }

            private static string ReplaceMdsKeywords(string connectionString)
            {
                connectionString = connectionString.Replace("Application Intent=", "ApplicationIntent=");
                connectionString = connectionString.Replace("Connect Retry Count=", "ConnectRetryCount=");
                connectionString = connectionString.Replace("Connect Retry Interval=", "ConnectRetryInterval=");
                connectionString = connectionString.Replace("Pool Blocking Period=", "PoolBlockingPeriod=");
                connectionString = connectionString.Replace("Multiple Active Result Sets=", "MultipleActiveResultSets=");
                connectionString = connectionString.Replace("Multi Subnet Failover=", "MultiSubnetFailover=");
                connectionString = connectionString.Replace("Transparent Network IP Resolution=", "TransparentNetworkIPResolution=");
                connectionString = connectionString.Replace("Trust Server Certificate=", "TrustServerCertificate=");
                return connectionString;
            }
        }


        internal static bool IsV40Installed()
        {
            return new SqlCeHelper4().IsV40Installed() != null;
        }

        internal static bool IsV35Installed()
        {
            return new SqlCeHelper().IsV35Installed() != null;
        }

        internal static bool IsV40DbProviderInstalled()
        {
            return new SqlCeHelper4().IsV40DbProviderInstalled();
        }

        internal static bool IsV35DbProviderInstalled()
        {
            return new SqlCeHelper4().IsV35DbProviderInstalled();
        }
    }
}
