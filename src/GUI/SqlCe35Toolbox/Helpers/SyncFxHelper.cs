using System.Collections.Generic;
using System.Linq;
using ErikEJ.SqlCeScripting;
using SyncFxContrib;

namespace ErikEJ.SqlCeToolbox.Helpers
{
    internal class SyncFxHelper
    {
        public Dictionary<string, string> GenerateCodeForScope(string sqlConnectionString, string sqlCeConnectionString, string source, string classPrefix, List<Column> columns, string defaultNamespace)
        {
            var syncFxContrib = new SqlCeToolBoxSyncFxLib();
            var classCode = syncFxContrib.GenerateProvisioningCode(sqlConnectionString, sqlCeConnectionString, source, classPrefix, columns, defaultNamespace);

            var returnValue = new Dictionary<string, string> { { classPrefix, classCode } };

            return returnValue;
        }

        public void ProvisionScope(string connectionString, string classPrefix, List<Column> columns)
        {
            var syncFxContrib = new SqlCeToolBoxSyncFxLib();
            syncFxContrib.ProvisionSqlCeScope(connectionString, classPrefix, columns);
        }

        public bool DeprovisionDatabase(string connectionString)
        {
            var syncFxContrib = new SqlCeToolBoxSyncFxLib();
            syncFxContrib.DeprovisionSqlCeStore(connectionString);
            return true;
        }

        internal static IEnumerable<string> GetSqlCeScopes(string connectionString)
        {
            var syncFxContrib = new SqlCeToolBoxSyncFxLib();
            return syncFxContrib.GetSqlCeScopes(connectionString);
        }

        internal static List<Column> GetSqlCeScopeDefinition(string connectionString, string scopeName)
        {
            var syncFxContrib = new SqlCeToolBoxSyncFxLib();
            return syncFxContrib.GetSqlCeScopeDefinition(connectionString, scopeName);
        }

        internal static void DeprovisionSqlCeScope(string connectionString, string scopeName)
        {
            var syncFxContrib = new SqlCeToolBoxSyncFxLib();
            syncFxContrib.DeprovisionSqlCeScope(connectionString,scopeName);
        }

        internal static void GenerateSnapshot(string connectionString, string fileName)
        {
            var syncFxContrib = new SqlCeToolBoxSyncFxLib();
            syncFxContrib.GenerateSnapshot(connectionString, fileName);
        }

        internal static bool IsProvisioned(DatabaseInfo databaseInfo)
        {
            bool isProvisioned = false;
            try
            {
                using (var repository = DataConnectionHelper.CreateRepository(databaseInfo))
                {
                    isProvisioned = repository.GetAllTableNamesForExclusion().Any(
                             t => t == "scope_info" || t == "scope_config" || t == "schema_info");

                }
            }
            catch
            {
                // ignored
            }
            return isProvisioned;
        }

        internal static bool SqlCeScopeExists(string connectionString, string scopeName)
        {
            var syncFxContrib = new SqlCeToolBoxSyncFxLib();
            return syncFxContrib.SqlCeScopeExists(connectionString,scopeName);
        }

    }
}
