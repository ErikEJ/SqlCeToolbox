using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using DbUp.Support.SqlServer;
using System.IO;

namespace ErikEJ.SqlCeScripting
{
#if V40
    // ReSharper disable once InconsistentNaming
    public class ServerDBRepository4 : IRepository
#else
    public sealed class ServerDBRepository : IRepository
#endif
    {
        private SqlConnection _cn;
        private readonly bool _keepSchemaName;

        private readonly List<string> _sqlCeFunctions = new List<string>()
        {
          "ABS(",
          "ACOS(",
          "ASIN(",
          "ATAN(",
          "ATN2(",
          "CEILING(",
          "CHARINDEX(",
          "CAST(",
          "COS(",
          "COT(",
          "DATEADD(",
          "DATEDIFF(",
          "DATENAME(",
          "DATEPART(",
          "DATALENGTH(",
          "DEGREES(",
          "EXP(",
          "FLOOR(",
          "GETDATE(",
          "LEN(",
          "LOG(",
          "LOG10(",
          "LOWER(",
          "LTRIM(",
          "NCHAR(",
          "NEWID(",
          "PATINDEX(",
          "PI(",
          "POWER(",
          "RADIANS(",
          "RAND(",
          "REPLACE(",
          "REPLICATE(",
          "RTRIM(",
          "SIGN(",
          "SIN(",
          "SPACE(",
          "SQRT(",
          "STR(",
          "STUFF(",
          "SUBSTRING(",
          "TAN(",
          "UNICODE(",
          "UPPER("
        };


        private delegate void AddToListDelegate<T>(ref List<T> list, SqlDataReader dr);
#if V40
        public ServerDBRepository4(string connectionString, bool keepSchemaName = false)
#else
        public ServerDBRepository(string connectionString, bool keepSchemaName = false)
#endif
        {
            _keepSchemaName = keepSchemaName;
            _cn = new SqlConnection(connectionString);
            _cn.Open();
        }

        public void Dispose()
        {
            if (_cn != null)
            {
                _cn.Close();
                _cn = null;
            }
        }

        public string GetRunTimeVersion()
        {
            return _cn.ServerVersion;
        }

        private static void AddToListString(ref List<string> list, SqlDataReader dr)
        {
            list.Add(dr.GetString(0));
        }

        private void AddToListColumns(ref List<Column> list, SqlDataReader dr)
        {
            //0  COLUMN_NAME
            //1  IS_NULLABLE
            //2  DATA_TYPE
            //3  CHARACTER_MAXIMUM_LENGTH
            //4  NUMERIC_PRECISION
            //5  AUTOINC_INCREMENT
            //6  AUTOINC_SEED
            //7  COLUMN_HASDEFAULT
            //8  COLUMN_FLAGS
            //9  NUMERIC_SCALE
            //10 TABLE_NAME
            //11 AUTOINC_NEXT
            //12 TABLE_SCHEMA
            //13 ORDINAL_POSITION
            //14 CASE_SENSITIVE
            string defValue = string.Empty;
            bool hasDefault = false;
            if (!dr.IsDBNull(8))
            {
                var t = dr.GetString(8);
                if (t.ToUpperInvariant().Contains("GETUTCDATE()"))
                {
                    t = "(GETDATE())";
                }
                if (t.ToUpperInvariant().Contains("NEWSEQUENTIALID()"))
                {
                    t = "(NEWID())";
                }
                if (t.ToUpperInvariant().ContainsAny(_sqlCeFunctions.ToArray()))
                {
                    defValue = t;
                }
                t = t.Replace("(", string.Empty);
                t = t.Replace(")", string.Empty);
                if (t.Length > 0)
                {
                    var arr = t.ToCharArray();
                    if (Char.IsNumber(arr[0]))
                    {
                        defValue = t;
                    }
                    if (arr[0] == '\'')
                    {
                        defValue = t;
                    }
                    if (t.StartsWith("N'"))
                    {
                        defValue = t;
                    }
                    if (t.StartsWith("-"))
                    {
                        defValue = t;
                    }
                }
                if (!string.IsNullOrEmpty(defValue))
                    hasDefault = true;
            }
            string table = dr.GetString(11);
            if (_keepSchemaName)
                table = dr.GetString(13) + "." + table;
            list.Add(new Column
            {
                ColumnName = dr.GetString(0),
                IsNullable = (YesNoOption)Enum.Parse(typeof(YesNoOption), dr.GetString(1)),
                DataType = dr.GetString(2),
                CharacterMaxLength = (dr.IsDBNull(3) ? 0 : dr.GetInt32(3)),
                NumericPrecision = (dr.IsDBNull(4) ? 0 : Convert.ToInt32(dr[4], System.Globalization.CultureInfo.InvariantCulture)),
                AutoIncrementBy = (dr.IsDBNull(5) ? 0 : Convert.ToInt64(dr[5], System.Globalization.CultureInfo.InvariantCulture)),
                AutoIncrementSeed = (dr.IsDBNull(6) ? 0 : Convert.ToInt64(dr[6], System.Globalization.CultureInfo.InvariantCulture)),
                AutoIncrementNext = (dr.IsDBNull(12) ? 0 : Convert.ToInt64(dr[12], System.Globalization.CultureInfo.InvariantCulture)),
                ColumnHasDefault = hasDefault,
                ColumnDefault = defValue,
                RowGuidCol = (dr.IsDBNull(9) ? false : dr.GetInt32(9) == 378 || dr.GetInt32(9) == 282),
                NumericScale = (dr.IsDBNull(10) ? 0 : Convert.ToInt32(dr[10], System.Globalization.CultureInfo.InvariantCulture)),
                TableName = table,
                IsCaseSensitivite = dr.IsDBNull(14) ? false : dr.GetInt32(14) == 1
            });
        }

        private void AddToListConstraints(ref List<Constraint> list, SqlDataReader dr)
        {
            string table = dr.GetString(0);
            string uniqueTable = dr.GetString(3);
            if (_keepSchemaName)
            {
                table = dr.GetString(10) + "." + table;
                uniqueTable = dr.GetString(10) + "." + uniqueTable;
            }

            list.Add(new Constraint
            {
                ConstraintTableName = table,
                ConstraintName = dr.GetString(1),
                ColumnName = string.Format(System.Globalization.CultureInfo.InvariantCulture, "[{0}]", dr.GetString(2)),
                UniqueConstraintTableName = uniqueTable,
                UniqueConstraintName = dr.GetString(4),
                UniqueColumnName = string.Format(System.Globalization.CultureInfo.InvariantCulture, "[{0}]", dr.GetString(5)),
                UpdateRule = dr.GetString(6),
                DeleteRule = dr.GetString(7),
                Columns = new ColumnList(),
                UniqueColumns = new ColumnList()
            });
        }

        private void AddToListIndexes(ref List<Index> list, SqlDataReader dr)
        {
            string table = dr.GetString(0);
            if (_keepSchemaName)
            {
                table = dr.GetString(8);
            }
            list.Add(new Index
            {
                TableName = table,
                IndexName = dr.GetString(1),
                Unique = dr.GetBoolean(3),
                Filter = dr.GetBoolean(4) ? dr.GetString(5) : null,
                Clustered = dr.GetBoolean(6),
                OrdinalPosition = dr.GetInt32(7),
                ColumnName = dr.GetString(8),
                SortOrder = (dr.GetBoolean(9) ? SortOrderEnum.DESC : SortOrderEnum.ASC)
            });

        }

        private void AddToListPrimaryKeys(ref List<PrimaryKey> list, SqlDataReader dr)
        {
            string table = dr.GetString(2);
            if (_keepSchemaName)
                table = dr.GetString(3) + "." + table;
            list.Add(new PrimaryKey
            {
                ColumnName = dr.GetString(0),
                KeyName = dr.GetString(1),
                TableName = table
            });
        }

        private List<T> ExecuteReader<T>(string commandText, AddToListDelegate<T> addToListMethod)
        {
            List<T> list = new List<T>();
            using (var cmd = new SqlCommand(commandText, _cn))
            {
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                        addToListMethod(ref list, dr);
                }
            }
            return list;
        }

        private IDataReader ExecuteDataReader(string commandText)
        {
            using (var cmd = new SqlCommand(commandText, _cn))
            {
                return cmd.ExecuteReader();
            }
        }

        private DataTable ExecuteDataTable(string commandText)
        {
            DataTable dt = null;
            try
            {
                dt = new DataTable();
                dt.Locale = System.Globalization.CultureInfo.InvariantCulture;
                using (var cmd = new SqlCommand(commandText, _cn))
                {
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            catch
            {
                if (dt != null)
                    dt.Dispose();
                throw;
            }
            return dt;
        }

        private object ExecuteScalar(string commandText)
        {
            object val;
            using (var cmd = new SqlCommand(commandText, _cn))
            {
                val = cmd.ExecuteScalar();
            }
            return val;
        }

        private void ExecuteNonQuery(string commandText)
        {
            using (var cmd = new SqlCommand(commandText, _cn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        #region IRepository Members

        public Int32 GetRowVersionOrdinal(string tableName)
        {
            //TODO This could probably be improved
            if (_keepSchemaName)
            {
                var parts = tableName.Split('.');
                tableName = parts[1];
            }
            object value = ExecuteScalar("SELECT ordinal_position FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + tableName + "' AND data_type = 'timestamp'");
            if (value != null)
            {
                int offsetOrdinal = 0;
                object offset = ExecuteScalar("SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + tableName + "' AND (DATA_TYPE = 'sql_variant') AND ORDINAL_POSITION < " + value + ";");
                if (offset != null)
                    offsetOrdinal = (int)offset;
                return (int)value - 1 - offsetOrdinal;
            }
            return -1;
        }

        public Int64 GetRowCount(string tableName)
        {
            return -1;
        }

        public bool HasIdentityColumn(string tableName)
        {
            return (ExecuteScalar("SELECT is_identity FROM sys.columns INNER JOIN sys.objects ON sys.columns.object_id = sys.objects.object_id WHERE sys.objects.name = '" + tableName + "' AND sys.objects.type = 'U' AND sys.columns.is_identity = 1") != null);
        }

        public List<string> GetAllTableNames()
        {
            return ExecuteReader(
                "select [name] from sys.tables WHERE type = 'U' AND is_ms_shipped = 0 ORDER BY [name];"
                , new AddToListDelegate<string>(AddToListString));
        }

        public List<string> GetAllTableNamesForExclusion()
        {
            return ExecuteReader(
                "SELECT S.name + '.' + T.name  from sys.tables T INNER JOIN sys.schemas S ON T.schema_id = S.schema_id WHERE [type] = 'U' AND is_ms_shipped = 0 ORDER BY S.name, T.[name];"
                , new AddToListDelegate<string>(AddToListString));
        }

        public List<string> GetAllSubscriptionNames()
        {
            return new List<string>();
        }

        public List<View> GetAllViews()
        {
            return new List<View>();
        }

        public List<Trigger> GetAllTriggers()
        {
            return new List<Trigger>();
        }

        public List<KeyValuePair<string, string>> GetDatabaseInfo()
        {
            return new List<KeyValuePair<string, string>>();
        }

        public List<Column> GetAllColumns()
        {
            return ExecuteReader(@"
                SELECT COLUMN_NAME, col.IS_NULLABLE, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, NUMERIC_PRECISION, 
                AUTOINC_INCREMENT =  CASE cols.is_identity  WHEN 0 THEN 0 WHEN 1 THEN IDENT_INCR('[' + col.TABLE_SCHEMA + '].[' + col.TABLE_NAME + ']')  END, 
                AUTOINC_SEED =     CASE cols.is_identity WHEN 0 THEN 0 WHEN 1 THEN IDENT_SEED('[' + col.TABLE_SCHEMA + '].[' + col.TABLE_NAME + ']')  END, 
                COLUMN_HASDEFAULT =  CASE WHEN col.COLUMN_DEFAULT IS NULL THEN CAST(0 AS bit) ELSE CAST (1 AS bit) END, COLUMN_DEFAULT, 
                COLUMN_FLAGS = CASE cols.is_rowguidcol WHEN 0 THEN 0 ELSE 378 END,
                NUMERIC_SCALE, col.TABLE_NAME, 
                AUTOINC_NEXT = CASE cols.is_identity WHEN 0 THEN 0 WHEN 1 THEN IDENT_CURRENT('[' + col.TABLE_SCHEMA + '].[' + col.TABLE_NAME + ']') + IDENT_INCR('[' + col.TABLE_SCHEMA + '].[' + col.TABLE_NAME + ']') END, 
                col.TABLE_SCHEMA, col.ORDINAL_POSITION,
                CASE_SENSITIVE = CASE WHEN collations.description like '%case-sensitive%' THEN 1 WHEN collations.description like '%case-insensitive%' THEN 0 END
                FROM INFORMATION_SCHEMA.COLUMNS col  
                JOIN sys.columns cols on col.COLUMN_NAME = cols.name 
                AND cols.object_id = OBJECT_ID('[' + col.TABLE_SCHEMA + '].[' + col.TABLE_NAME + ']')  
                JOIN sys.schemas schms on schms.name = col.TABLE_SCHEMA                
                JOIN sys.tables tab ON col.TABLE_NAME = tab.name and tab.schema_id = schms.schema_id 
                LEFT JOIN sys.fn_helpcollations() collations ON collations.name = col.COLLATION_NAME COLLATE SQL_Latin1_General_CP1_CI_AS
                WHERE SUBSTRING(COLUMN_NAME, 1,5) <> '__sys' 
                AND tab.type = 'U' AND is_ms_shipped = 0 
                AND (cols.is_computed = 0)
                AND DATA_TYPE <> 'sql_variant' 
				UNION 
                SELECT COLUMN_NAME, col.IS_NULLABLE, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, NUMERIC_PRECISION, 
                AUTOINC_INCREMENT =  CASE cols.is_identity  WHEN 0 THEN 0 WHEN 1 THEN IDENT_INCR('[' + col.TABLE_SCHEMA + '].[' + col.TABLE_NAME + ']')  END, 
                AUTOINC_SEED =     CASE cols.is_identity WHEN 0 THEN 0 WHEN 1 THEN IDENT_SEED('[' + col.TABLE_SCHEMA + '].[' + col.TABLE_NAME + ']')  END, 
                COLUMN_HASDEFAULT =  CASE WHEN col.COLUMN_DEFAULT IS NULL THEN CAST(0 AS bit) ELSE CAST (1 AS bit) END, COLUMN_DEFAULT, 
                COLUMN_FLAGS = CASE cols.is_rowguidcol WHEN 0 THEN 0 ELSE 378 END,
                NUMERIC_SCALE, col.TABLE_NAME, 
                AUTOINC_NEXT = CASE cols.is_identity WHEN 0 THEN 0 WHEN 1 THEN IDENT_CURRENT('[' + col.TABLE_SCHEMA + '].[' + col.TABLE_NAME + ']') + IDENT_INCR('[' + col.TABLE_SCHEMA + '].[' + col.TABLE_NAME + ']') END, 
                col.TABLE_SCHEMA, col.ORDINAL_POSITION,
                CASE_SENSITIVE = CASE WHEN collations.description like '%case-sensitive%' THEN 1 WHEN collations.description like '%case-insensitive%' THEN 0 END
                FROM INFORMATION_SCHEMA.COLUMNS col  
                JOIN sys.columns cols on col.COLUMN_NAME = cols.name 
                AND cols.object_id = OBJECT_ID('[' + col.TABLE_SCHEMA + '].[' + col.TABLE_NAME + ']')  
                JOIN sys.schemas schms on schms.name = col.TABLE_SCHEMA                
                JOIN sys.tables tab ON col.TABLE_NAME = tab.name and tab.schema_id = schms.schema_id 
                LEFT JOIN sys.fn_helpcollations() collations ON collations.name = col.COLLATION_NAME COLLATE SQL_Latin1_General_CP1_CI_AS
			    JOIN sys.computed_columns cc on cc.object_id = cols.object_id 
                WHERE SUBSTRING(COLUMN_NAME, 1,5) <> '__sys' 
                AND tab.type = 'U' AND is_ms_shipped = 0 
                AND (cc.is_computed = 1 AND cc.is_persisted = 1)
                AND DATA_TYPE <> 'sql_variant' 
				ORDER BY col.TABLE_NAME, col.ORDINAL_POSITION ASC
                OPTION (MERGE JOIN)
"
                , new AddToListDelegate<Column>(AddToListColumns));
        }

        public DataTable GetDataFromTable(string tableName, List<Column> tableColumns)
        {
            return GetDataFromTable(tableName, tableColumns, new List<PrimaryKey>());
        }

        public DataTable GetDataFromTable(string tableName, List<Column> tableColumns, List<PrimaryKey> tablePrimaryKeys)
        {
            // Include the schema name, may not always be dbo!
            var sb = new StringBuilder(200);
            sb.Append("SELECT ");
            foreach (Column col in tableColumns)
            {
                sb.Append(string.Format(System.Globalization.CultureInfo.InvariantCulture, "[{0}], ", col.ColumnName));
            }
            sb.Remove(sb.Length - 2, 2);

            sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, " From [{0}]", GetSchemaAndTableName(tableName));

            sb.Append(SortSelect(tablePrimaryKeys));
            return ExecuteDataTable(sb.ToString());
        }

        public IDataReader GetDataFromReader(string tableName, List<Column> tableColumns)
        {
            return GetDataFromReader(tableName, tableColumns, new List<PrimaryKey>());
        }

        public IDataReader GetDataFromReader(string tableName, List<Column> tableColumns, List<PrimaryKey> tablePrimaryKeys, string whereClause = null)
        {
            var sb = new StringBuilder(200);
            sb.Append("SELECT ");
            foreach (Column col in tableColumns)
            {
                if (col.ServerDataType == "hierarchyid")
                {
                    sb.Append(string.Format(System.Globalization.CultureInfo.InvariantCulture, "CAST([{0}] AS varbinary(892)) AS [{0}], ", col.ColumnName));
                }
                else
                {
                    sb.Append(string.Format(System.Globalization.CultureInfo.InvariantCulture, "[{0}], ", col.ColumnName));
                }
            }
            sb.Remove(sb.Length - 2, 2);

            sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, " From [{0}]", GetSchemaAndTableName(tableName));

            if (!string.IsNullOrEmpty(whereClause))
            {
                sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, " WHERE {0}", whereClause);
            }

            sb.Append(SortSelect(tablePrimaryKeys));
            return ExecuteDataReader(sb.ToString());
        }

        private static string SortSelect(List<PrimaryKey> tablePrimaryKeys)
        {
            var sb = new StringBuilder(64);
            if (tablePrimaryKeys.Count > 0)
            {
                sb.Append(" ORDER BY ");
                tablePrimaryKeys.ForEach(delegate (PrimaryKey column)
                {
                    sb.AppendFormat("[{0}],", column.ColumnName);
                });

                // Remove the last comma
                sb.Remove(sb.Length - 1, 1);
            }
            return sb.ToString();
        }

        public List<PrimaryKey> GetAllPrimaryKeys()
        {
            return ExecuteReader(
                "SELECT u.COLUMN_NAME, c.CONSTRAINT_NAME, c.TABLE_NAME, c.CONSTRAINT_SCHEMA " +
                "FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS c INNER JOIN " +
                "INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS u ON c.CONSTRAINT_NAME = u.CONSTRAINT_NAME AND u.TABLE_NAME = c.TABLE_NAME " +
                "where c.CONSTRAINT_TYPE = 'PRIMARY KEY' ORDER BY u.TABLE_NAME, c.CONSTRAINT_NAME, u.ORDINAL_POSITION"
                , new AddToListDelegate<PrimaryKey>(AddToListPrimaryKeys));
        }

        public List<Constraint> GetAllForeignKeys()
        {
            var list = ExecuteReader(
                "SELECT OBJECT_NAME(f.parent_object_id) AS FK_TABLE_NAME, f.name AS FK_CONSTRAINT_NAME, " +
                "COL_NAME(fc.parent_object_id, fc.parent_column_id) AS FK_COLUMN_NAME, OBJECT_NAME(f.referenced_object_id) AS UQ_TABLE_NAME,  " +
                "'' AS UQ_CONSTRAINT_NAME, COL_NAME(fc.referenced_object_id, fc.referenced_column_id) AS UQ_COLUMN_NAME,  " +
                "REPLACE(f.update_referential_action_desc,'_',' ') AS UPDATE_RULE, REPLACE(f.delete_referential_action_desc,'_',' ') AS DELETE_RULE, 1, 1, " +
                " OBJECT_SCHEMA_NAME(f.referenced_object_id) " +
                "FROM sys.foreign_keys AS f INNER JOIN sys.foreign_key_columns AS fc ON f.OBJECT_ID = fc.constraint_object_id  " +
                "JOIN sys.schemas schms on schms.name = OBJECT_SCHEMA_NAME(f.referenced_object_id) " +
                "JOIN sys.tables tab ON OBJECT_NAME(f.referenced_object_id) = tab.name and tab.schema_id = schms.schema_id " +
                "WHERE is_disabled = 0  " +
                "AND tab.is_ms_shipped = 0 AND tab.type = 'U' " +
                "ORDER BY FK_TABLE_NAME, FK_CONSTRAINT_NAME, fc.constraint_column_id"
                , new AddToListDelegate<Constraint>(AddToListConstraints));
            return RepositoryHelper.GetGroupForeignKeys(list, GetAllTableNames());
        }

        /// <summary>
        /// Get the indexes for the table
        /// </summary>
        /// <returns></returns>
        public List<Index> GetIndexesFromTable(string tableName)
        {
            return ExecuteReader(
                "select top 4096	OBJECT_NAME(i.object_id) AS TABLE_NAME, i.name AS INDEX_NAME, 0 AS PRIMARY_KEY, " +
                "i.is_unique AS [UNIQUE], i.has_filter AS [HAS_FILTER], i.filter_definition AS [FILTER], CAST(0 AS bit) AS [CLUSTERED], CAST(ic.key_ordinal AS int) AS ORDINAL_POSITION, c.name AS COLUMN_NAME, ic.is_descending_key AS SORT_ORDER, '" + tableName + "' AS original " +
                "from sys.indexes i left outer join     sys.index_columns ic on i.object_id = ic.object_id and i.index_id = ic.index_id " +
                "left outer join sys.columns c on c.object_id = ic.object_id and c.column_id = ic.column_id " +
                "where  i.is_disabled = 0 AND i.is_hypothetical = 0 AND i.object_id = object_id('[" + GetSchemaAndTableName(tableName) + "]') AND i.name IS NOT NULL AND i.is_primary_key = 0  AND ic.is_included_column  = 0 " +
                "AND i.type IN (1,2) AND c.is_computed = 0 " +
                "order by i.name, case key_ordinal when 0 then 256 else ic.key_ordinal end"
                , new AddToListDelegate<Index>(AddToListIndexes));
        }


        public void RenameTable(string oldName, string newName)
        {
            ExecuteNonQuery(string.Format(System.Globalization.CultureInfo.InvariantCulture, "sp_rename '{0}', '{1}';", oldName, newName));
        }

        public bool IsServer()
        {
            return true;
        }

        public DataSet ExecuteSql(string script)
        {
            return new DataSet();
        }

        public DataSet GetSchemaDataSet(List<string> tables)
        {
            DataSet schemaSet = null;
            try
            {
                schemaSet = new DataSet();
                using (SqlDataAdapter adapter = new SqlDataAdapter())
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = _cn;
                        foreach (var table in tables)
                        {
                            var strSql = string.Format(System.Globalization.CultureInfo.InvariantCulture, "SELECT * FROM [{0}] WHERE 0 = 1", GetSchemaAndTableName(table));

                            using (SqlCommand command = new SqlCommand(strSql, _cn))
                            {
                                using (SqlDataAdapter adapter1 = new SqlDataAdapter(command))
                                {
                                    adapter1.FillSchema(schemaSet, SchemaType.Source, table);

                                    //Fill the table in the dataset 
                                    cmd.CommandText = strSql;
                                    adapter.SelectCommand = cmd;
                                    adapter.Fill(schemaSet, table);
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                if (schemaSet != null)
                    schemaSet.Dispose();
            }
            return schemaSet;
        }

        private string GetSchemaAndTableName(string table)
        {
            if (_keepSchemaName)
            {
                var parts = table.Split('.');
                if (parts.Length == 2)
                {
                    return parts[0] + "].[" + parts[1];
                }
                return table;
            }
            else
            {
                return (string)ExecuteScalar(string.Format(System.Globalization.CultureInfo.InvariantCulture, "SELECT TABLE_SCHEMA FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}'", table)) + "].[" + table;
            }
        }

        public DataSet ExecuteSql(string script, out bool schemaChanged)
        {
            schemaChanged = false;
            return new DataSet();
        }

        public DataSet ExecuteSql(string script, out string showPlanString)
        {
            showPlanString = string.Empty;
            return new DataSet();
        }

        public DataSet ExecuteSql(string script, out bool schemaChanged, bool ignoreDdlErrors)
        {
            schemaChanged = false;
            return new DataSet();
        }

        public void ExecuteSqlFile(string scriptPath)
        {
            RunCommands(scriptPath);
        }

        private void RunCommands(string scriptPath)
        {
            if (!File.Exists(scriptPath))
                return;

            bool isConsoleApp = Console.OpenStandardInput(1) != Stream.Null;

            using (SqlCommandReaderStreamed sr = new SqlCommandReaderStreamed(scriptPath))
            {
                var commandText = sr.ReadCommand();
                int i = 1;
                if (isConsoleApp) Console.WriteLine("Running script commands " + i);
                while (!string.IsNullOrWhiteSpace(commandText))
                {
                    RunCommand(commandText);
                    if (isConsoleApp) Console.SetCursorPosition(0, Console.CursorTop - 1);
                    i++;
                    if (isConsoleApp) Console.WriteLine("Running script commands " + i);
                    commandText = sr.ReadCommand();
                }
            }
        }

        private void RunCommand(string sql)
        {
            try
            {
                using (var cmd = new SqlCommand())
                {
                    cmd.Connection = _cn;
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception(FormatSqlException(ex, sql));
            }
        }

        private string FormatSqlException(SqlException ex, string sql)
        {
            return Helper.ShowErrors(ex) + Environment.NewLine + sql;
        }

        public string ParseSql(string script)
        {
            return string.Empty;
        }

        /// <summary>
        /// Get the local Datetime for last sync
        /// </summary>
        /// <param name="publication"> Publication id: EEJx:Northwind:NwPubl</param>
        /// <returns></returns>
        public DateTime GetLastSuccessfulSyncTime(string publication)
        {
            return DateTime.MinValue;
        }

        public int GetIdentityOrdinal(string tableName)
        {
            //TODO This could probably be improved
            if (_keepSchemaName)
            {
                var parts = tableName.Split('.');
                if (parts.Length > 1)
                {
                    tableName = parts[1];
                }
            }
            object value = ExecuteScalar("SELECT TOP(1) ic.ORDINAL_POSITION FROM sys.columns INNER JOIN sys.objects ON sys.columns.object_id = sys.objects.object_id  INNER JOIN INFORMATION_SCHEMA.COLUMNS ic ON ic.TABLE_NAME = '" + tableName + "' WHERE sys.objects.name = '" + tableName + "' AND sys.objects.type = 'U' AND sys.columns.is_identity = 1");
            if (value != null)
            {
                int offsetOrdinal = 0;
                object offset = ExecuteScalar("SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + tableName + "' AND (DATA_TYPE = 'sql_variant') AND ORDINAL_POSITION < " + value + ";");
                if (offset != null)
                    offsetOrdinal = (int)offset;
                return (int)value - 1 - offsetOrdinal;
            }
            return -1;
        }

        public List<Index> GetAllIndexes()
        {
            throw new NotImplementedException();
        }

        public Type GetClrTypeFromDataType(string typeName)
        {
            throw new NotImplementedException();
        }

        public bool KeepSchema()
        {
            return _keepSchemaName;
        }
        #endregion
    }

    public static class StringExtensions
    {
        public static bool ContainsAll(this string str, params string[] values)
        {
            if (string.IsNullOrEmpty(str) && values.Length <= 0) return false;
            foreach (var value in values)
            {
                if (str != null && !str.Contains(value))
                    return false;
            }
            return true;
        }

        public static bool ContainsAny(this string str, params string[] values)
        {
            if (!string.IsNullOrEmpty(str) || values.Length > 0)
            {
                foreach (string value in values)
                {
                    if (str != null && str.Contains(value))
                        return true;
                }
            }
            return false;
        }
    }
}
