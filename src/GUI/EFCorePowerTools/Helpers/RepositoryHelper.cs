using ErikEJ.SqlCeScripting;
using ErikEJ.SQLiteScripting;

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

        public static string GetFilePath(string connectionString, DatabaseType dbType)
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
