using ErikEJ.SqlCeScripting;
using ErikEJ.SQLiteScripting;
using System.Data.SqlClient;
using System.IO;

namespace EFCorePowerTools.Helpers
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
                    return new ServerDBRepository(databaseInfo.ConnectionString);
                case DatabaseType.SQLite:
                    return new SQLiteRepository(databaseInfo.ConnectionString);
                default:
                    return null;
            }
        }

        public static string GetClassBasis(string connectionString, DatabaseType dbType)
        {
            var classBasis = "My";
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

        public static string GetFilePath(string connectionString, DatabaseType dbType)
        {
            if (dbType == DatabaseType.SQLServer)
            {
                var helper = new SqlServerHelper();
                return helper.PathFromConnectionString(connectionString);
            }
            var filePath = GetPath(connectionString, dbType);
            return Path.GetFileName(filePath);
        }

        private static string GetPath(string connectionString, DatabaseType dbType)
        {
            var helper = CreateEngineHelper(dbType);
            return helper.PathFromConnectionString(connectionString);
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
    }
}
