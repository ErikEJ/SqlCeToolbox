using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace ErikEJ.SqlCeScripting
{
    public interface IGenerator
    {
        void ExcludeTables(IList<string> tablesToExclude);
        string ScriptDatabaseToFile(Scope scope);
        void GenerateTableScript(string tableName);
        string GenerateTableData(string tableName, bool saveImageFiles);
        void GenerateTableContent(string tableName, bool saveImageFiles, bool ignoreIdentity = false, string whereClause = null);
        string GeneratedScript { get; }
        IList<string> GeneratedFiles { get; }
        void GenerateTableSelect(string tableName);
        void GenerateViewSelect(string tableName);
        void GenerateTableSelect(string tableName, bool editableInSqlite);
        void GenerateTableInsert(string tableName);
        void GenerateTableUpdate(string tableName);
        void GenerateTableDelete(string tableName);
        void GenerateTableDrop(string tableName);
        void GenerateTableCreate(string tabelName);
        void GenerateTableCreate(string tableName, List<Column> columns);
        void GenerateTableInsert(string tableName, IList<string> fields, IList<string> values, int lineNumber);
        bool ValidColumns(string tableName, IList<string> columns);
        void GenerateSchemaGraph(string connectionString);
        void GenerateSchemaGraph(string connectionString, bool includeSystemTables, bool generateScripts);
        void GenerateSchemaGraph(string connectionString, IList<string> tablesToExclude);
        void GeneratePrimaryKeys(string tableName);
        void GenerateForeignKeys(string tableName);
        void GenerateIndexScript(string tableName, string indexName);
        void GenerateIndexDrop(string tableName, string indexName);
        void GenerateIndexOnlyDrop(string tableName, string indexName); 
        void GenerateIndexStatistics(string tableName, string indexName);
        void GenerateIdentityReset(string tableName, bool forServer);
        List<string> GenerateTableColumns(string tableName);

        void GenerateColumnAddScript(Column column);
        void GenerateColumnDropScript(Column column);
        void GenerateColumnAlterScript(Column column);
        void GenerateColumnSetDefaultScript(Column column);
        void GenerateColumnDropDefaultScript(Column column);

        void GeneratePrimaryKeyDrop(PrimaryKey primaryKey, string tableName);
        void GenerateForeignKey(Constraint constraint);
        void GenerateForeignKeyDrop(Constraint constraint);
        void GenerateForeignKeyDrop(string tableName, string keyName);
        void GenerateForeignKey(string tableName, string keyName);
        string GenerateInsertFromDataRow(string tableName, DataRow row);
        string GenerateUpdateFromDataRow(string tableName, DataRow row);
        string SqlFormatValue(string tableName, string fieldName, string value);
        void GenerateDatabaseInfo();
        void GenerateSqliteNetModel(string nameSpace);
        /// <summary>
        /// SQLite allows strings longer than the defined size to be stored. Enabling this option will truncate strings that are longer than the defined size, and log any truncations to: %temp%\SQLiteTruncates.log
        /// </summary>
        bool TruncateSQLiteStrings { get; set; }
    }
}
