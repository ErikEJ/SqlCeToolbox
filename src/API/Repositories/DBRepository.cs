using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.Text;
using System.Globalization;
using System.Linq;
using DbUp.Support.SqlServer;

namespace ErikEJ.SqlCeScripting
{
    /// <summary>
    /// Implementation of the <see cref="IRepository"/> interface for SQL Server Compact 3.1/3.5
    /// </summary>
#if V40
    // ReSharper disable once InconsistentNaming
    public class DB4Repository : IRepository
#else
    public sealed class DBRepository : IRepository
#endif
    {
        private SqlCeConnection _cn;
        private delegate void AddToListDelegate<T>(ref List<T> list, SqlCeDataReader dr);
        private string _showPlan = string.Empty;
        private bool _schemaHasChanged;

#if V40
        /// <summary>
        /// Initializes a new instance of the <see cref="DB4Repository"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public DB4Repository(string connectionString)
#else
        /// <summary>
        /// Initializes a new instance of the <see cref="DBRepository"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public DBRepository(string connectionString)
#endif
        {
            _cn = new SqlCeConnection(connectionString);
            _cn.Open();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
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

        private static void AddToListString(ref List<string> list, SqlCeDataReader dr)
        {
            list.Add(dr.GetString(0));
        }

        private static void AddToListColumns(ref List<Column> list, SqlCeDataReader dr)
        {
            if (!dr.GetString(0).StartsWith("__sys"))
            {
                list.Add(new Column
                {
                    ColumnName = dr.GetString(0),
                    IsNullable = (YesNoOption)Enum.Parse(typeof(YesNoOption), dr.GetString(1)),
                    DataType = dr.GetString(2),
                    CharacterMaxLength = (dr.IsDBNull(3) ? 0 : dr.GetInt32(3)),
                    NumericPrecision = (dr.IsDBNull(4) ? 0 : Convert.ToInt32(dr[4], CultureInfo.InvariantCulture)),
#if V31
#else
                    AutoIncrementBy = (dr.IsDBNull(5) ? 0 : Convert.ToInt64(dr[5], CultureInfo.InvariantCulture)),
                    AutoIncrementSeed = (dr.IsDBNull(6) ? 0 : Convert.ToInt64(dr[6], CultureInfo.InvariantCulture)),
                    AutoIncrementNext = (dr.IsDBNull(12) ? 0 : Convert.ToInt64(dr[12], CultureInfo.InvariantCulture)),
#endif
                    ColumnHasDefault = (dr.IsDBNull(7) ? false : dr.GetBoolean(7)),
                    ColumnDefault = (dr.IsDBNull(8) ? string.Empty : dr.GetString(8).Trim()),
                    RowGuidCol = (dr.IsDBNull(9) ? false : dr.GetInt32(9) == 378 || dr.GetInt32(9) == 282),
                    NumericScale = (dr.IsDBNull(10) ? 0 : Convert.ToInt32(dr[10], CultureInfo.InvariantCulture)),
                    TableName = dr.GetString(11),
                    Ordinal = dr.GetInt32(13)
                });
            }
        }

        private static void AddToListConstraints(ref List<Constraint> list, SqlCeDataReader dr)
        {
            list.Add(new Constraint
            {
                ConstraintTableName = dr.GetString(0),
                ConstraintName = dr.GetString(1),
                ColumnName = dr.GetString(2),
                UniqueConstraintTableName = dr.GetString(3),
                UniqueConstraintName = dr.GetString(4),
                UniqueColumnName = dr.GetString(5),
                UpdateRule = dr.GetString(6),
                DeleteRule = dr.GetString(7),
                Columns = new ColumnList(),
                UniqueColumns = new ColumnList()
            });
        }

        private void AddToListIndexes(ref List<Index> list, SqlCeDataReader dr)
        {
            list.Add(new Index
            {
                TableName = dr.GetString(0),
                IndexName = dr.GetString(1),
                Unique = dr.GetBoolean(3),
                Clustered = dr.GetBoolean(4),
                OrdinalPosition = dr.GetInt32(5),
                ColumnName = dr.GetString(6),
                SortOrder = (dr.GetInt16(7) == 1 ? SortOrderEnum.ASC : SortOrderEnum.DESC)
            });

        }

        private void AddToListPrimaryKeys(ref List<PrimaryKey> list, SqlCeDataReader dr)
        {
            list.Add(new PrimaryKey
            {
                ColumnName = dr.GetString(0),
                KeyName = dr.GetString(1),
                TableName = dr.GetString(2)
            });
        }

        private List<T> ExecuteReader<T>(string commandText, AddToListDelegate<T> addToListMethod)
        {
            List<T> list = new List<T>();
            using (var cmd = new SqlCeCommand(commandText, _cn))
            {
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                        addToListMethod(ref list, dr);
                }
            }
            return list;
        }

        private IDataReader ExecuteDataReader(string commandText, CommandType commandType)
        {
            using (var cmd = new SqlCeCommand(commandText, _cn))
            {
                cmd.CommandType = commandType;
                return cmd.ExecuteReader();
            }
        }

        private DataTable ExecuteDataTable(string commandText)
        {
            DataTable dt = null;
            try
            {
                dt = new DataTable();
                dt.Locale = CultureInfo.InvariantCulture;
                using (var cmd = new SqlCeCommand(commandText, _cn))
                {
                    using (var da = new SqlCeDataAdapter(cmd))
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
            using (var cmd = new SqlCeCommand(commandText, _cn))
            {
                val = cmd.ExecuteScalar();
            }
            return val;
        }

        private void ExecuteNonQuery(string commandText)
        {
            using (var cmd = new SqlCeCommand(commandText, _cn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private List<KeyValuePair<string, string>> GetSqlCeInfo()
        {
            List<KeyValuePair<string, string>> valueList;
#if V31
            valueList = new List<KeyValuePair<string, string>>();
#else
            valueList = _cn.GetDatabaseInfo();
#endif
            valueList.Add(new KeyValuePair<string, string>("Database", _cn.Database));
            valueList.Add(new KeyValuePair<string, string>("ServerVersion", _cn.ServerVersion));
            if (System.IO.File.Exists(_cn.Database))
            {
                System.IO.FileInfo fi = new System.IO.FileInfo(_cn.Database);
                valueList.Add(new KeyValuePair<string, string>("DatabaseSize", RepositoryHelper.GetSizeReadable(fi.Length)));
                valueList.Add(new KeyValuePair<string, string>("SpaceAvailable", RepositoryHelper.GetSizeReadable(4294967296 - fi.Length)));
                valueList.Add(new KeyValuePair<string, string>("Created", fi.CreationTime.ToShortDateString() + " " + fi.CreationTime.ToShortTimeString()));
            }
            return valueList;
        }

        #region IRepository Members

        /// <summary>
        /// Gets the row version ordinal.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        public Int32 GetRowVersionOrdinal(string tableName)
        {
            object value = ExecuteScalar("SELECT ordinal_position FROM information_schema.columns WHERE TABLE_NAME = '" + tableName + "' AND data_type = 'rowversion'");
            if (value != null)
            {
                return (int)value - 1;
            }
            return -1;
        }

        /// <summary>
        /// Gets the row count.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        public Int64 GetRowCount(string tableName)
        {
            object value = ExecuteScalar("SELECT CARDINALITY FROM INFORMATION_SCHEMA.INDEXES WHERE PRIMARY_KEY = 1 AND TABLE_NAME = N'" + tableName + "'");
            if (value != null)
            {
                return (Int64)value;
            }
            return -1;
        }

        /// <summary>
        /// Determines whether [has identity column] [the specified table name].
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>
        /// 	<c>true</c> if [has identity column] [the specified table name]; otherwise, <c>false</c>.
        /// </returns>
        public bool HasIdentityColumn(string tableName)
        {
            return (GetIdentityOrdinal(tableName) > -1);
        }

        public int GetIdentityOrdinal(string tableName)
        {
            object value = ExecuteScalar("SELECT ordinal_position FROM information_schema.columns WHERE TABLE_NAME = N'" + tableName + "' AND AUTOINC_SEED IS NOT NULL");
            if (value != null)
            {
                return (int)value - 1;
            }
            return -1;
        }

        /// <summary>
        /// Gets all table names.
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllTableNames()
        {
            return ExecuteReader(
                "SELECT table_name FROM information_schema.tables WHERE TABLE_TYPE <> N'SYSTEM TABLE' "
                , new AddToListDelegate<string>(AddToListString));
        }

        public List<string> GetAllTableNamesForExclusion()
        {
            return GetAllTableNames();
        }

        public List<string> GetAllSubscriptionNames()
        {
            object value = ExecuteScalar("SELECT table_name FROM information_schema.tables WHERE TABLE_NAME = '__sysMergeSubscriptions' ");
            if (value == null)
            {
                return new List<string>();
            }
            else
            {
                return ExecuteReader(
                    "SELECT Publisher + ':' + PublisherDatabase + ':' + Publication as Sub FROM __sysMergeSubscriptions ORDER BY Publisher, PublisherDatabase, Publication"
                    , new AddToListDelegate<string>(AddToListString));
            }
        }

        /// <summary>
        /// Gets the database info.
        /// </summary>
        /// <returns></returns>
        public List<KeyValuePair<string, string>> GetDatabaseInfo()
        {
            return GetSqlCeInfo();
        }

        /// <summary>
        /// Gets the columns from table.
        /// </summary>
        /// <returns></returns>
        public List<Column> GetAllColumns()
        {
            var list = ExecuteReader(
                "SELECT     Column_name, is_nullable, data_type, character_maximum_length, numeric_precision, autoinc_increment, autoinc_seed, column_hasdefault, column_default, column_flags, numeric_scale, table_name, autoinc_next, ordinal_position " +
                "FROM         information_schema.columns "
                //+
                //"WHERE      SUBSTRING(COLUMN_NAME, 1,5) <> '__sys'  " 
                //+
                //"ORDER BY ordinal_position ASC "
                , new AddToListDelegate<Column>(AddToListColumns));
            return list.OrderBy(c => c.TableName).ThenBy(c => c.Ordinal).ToList();
        }

        public DataTable GetDataFromTable(string tableName, List<Column> tableColumns)
        {
            return GetDataFromTable(tableName, tableColumns, new List<PrimaryKey>());
        }

        /// <summary>
        /// Gets the data from table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="tableColumns">The columns.</param>
        /// <param name="tablePrimaryKeys"></param>
        /// <returns></returns>
        public DataTable GetDataFromTable(string tableName, List<Column> tableColumns, List<PrimaryKey> tablePrimaryKeys)
        {
            var sb = new StringBuilder(200);
            sb.Append("SELECT ");
            foreach (Column col in tableColumns)
            {
                sb.Append(string.Format(CultureInfo.InvariantCulture, "[{0}], ", col.ColumnName));
            }
            sb.Remove(sb.Length - 2, 2);

            sb.AppendFormat(CultureInfo.InvariantCulture, " From [{0}]", tableName);

            sb.Append(SortSelect(tablePrimaryKeys));
            return ExecuteDataTable(sb.ToString());
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
                sb.Append(string.Format(CultureInfo.InvariantCulture, "[{0}], ", col.ColumnName));
            }
            sb.Remove(sb.Length - 2, 2);

            sb.AppendFormat(CultureInfo.InvariantCulture, " FROM [{0}]", tableName);

            if (!string.IsNullOrEmpty(whereClause))
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, " WHERE {0}", whereClause);
            }

            sb.Append(SortSelect(tablePrimaryKeys));
            return ExecuteDataReader(sb.ToString(), CommandType.Text);
        }

        /// <summary>
        /// Gets the primary keys from table.
        /// </summary>
        /// <returns></returns>
        public List<PrimaryKey> GetAllPrimaryKeys()
        {
            var list = ExecuteReader(
                "SELECT u.COLUMN_NAME, c.CONSTRAINT_NAME, c.TABLE_NAME " +
                "FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS c INNER JOIN " +
                "INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS u ON c.CONSTRAINT_NAME = u.CONSTRAINT_NAME AND u.TABLE_NAME = c.TABLE_NAME " +
                "where c.CONSTRAINT_TYPE = 'PRIMARY KEY' ORDER BY u.TABLE_NAME, c.CONSTRAINT_NAME, u.ORDINAL_POSITION"
                , new AddToListDelegate<PrimaryKey>(AddToListPrimaryKeys));

            return Helper.EnsureUniqueNames(list);
        }

        /// <summary>
        /// Gets all foreign keys.
        /// </summary>
        /// <returns></returns>
        public List<Constraint> GetAllForeignKeys()
        {
            var list = ExecuteReader(
                "SELECT KCU1.TABLE_NAME AS FK_TABLE_NAME,  KCU1.CONSTRAINT_NAME AS FK_CONSTRAINT_NAME, KCU1.COLUMN_NAME AS FK_COLUMN_NAME, " +
                "KCU2.TABLE_NAME AS UQ_TABLE_NAME, KCU2.CONSTRAINT_NAME AS UQ_CONSTRAINT_NAME, KCU2.COLUMN_NAME AS UQ_COLUMN_NAME, RC.UPDATE_RULE, RC.DELETE_RULE, KCU2.ORDINAL_POSITION AS UQ_ORDINAL_POSITION, KCU1.ORDINAL_POSITION AS FK_ORDINAL_POSITION " +
                "FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS RC " +
                "JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE KCU1 ON KCU1.CONSTRAINT_NAME = RC.CONSTRAINT_NAME " +
                "AND KCU1.TABLE_NAME = RC.CONSTRAINT_TABLE_NAME " +
                "JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE KCU2 ON  KCU2.CONSTRAINT_NAME =  RC.UNIQUE_CONSTRAINT_NAME AND KCU2.ORDINAL_POSITION = KCU1.ORDINAL_POSITION AND KCU2.TABLE_NAME = RC.UNIQUE_CONSTRAINT_TABLE_NAME " +
                "ORDER BY FK_TABLE_NAME, FK_CONSTRAINT_NAME, FK_ORDINAL_POSITION"
                , new AddToListDelegate<Constraint>(AddToListConstraints));

            return RepositoryHelper.GetGroupForeignKeys(list, GetAllTableNames());
        }

        ///// <summary>
        ///// Gets all foreign keys.
        ///// </summary>
        ///// <param name="tableName">Name of the table.</param>
        ///// <returns></returns>
        //public List<Constraint> GetAllForeignKeys(string tableName)
        //{
        //    var list = ExecuteReader(
        //        "SELECT KCU1.TABLE_NAME AS FK_TABLE_NAME,  KCU1.CONSTRAINT_NAME AS FK_CONSTRAINT_NAME, KCU1.COLUMN_NAME AS FK_COLUMN_NAME, " +
        //        "KCU2.TABLE_NAME AS UQ_TABLE_NAME, KCU2.CONSTRAINT_NAME AS UQ_CONSTRAINT_NAME, KCU2.COLUMN_NAME AS UQ_COLUMN_NAME, RC.UPDATE_RULE, RC.DELETE_RULE, KCU2.ORDINAL_POSITION AS UQ_ORDINAL_POSITION, KCU1.ORDINAL_POSITION AS FK_ORDINAL_POSITION " +
        //        "FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS RC " +
        //        "JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE KCU1 ON KCU1.CONSTRAINT_NAME = RC.CONSTRAINT_NAME " +
        //        "AND KCU1.TABLE_NAME = RC.CONSTRAINT_TABLE_NAME " +
        //        "JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE KCU2 ON  KCU2.CONSTRAINT_NAME =  RC.UNIQUE_CONSTRAINT_NAME AND KCU2.ORDINAL_POSITION = KCU1.ORDINAL_POSITION AND KCU2.TABLE_NAME = RC.UNIQUE_CONSTRAINT_TABLE_NAME " +
        //        "WHERE KCU1.TABLE_NAME = '" + tableName + "' " +
        //        "ORDER BY FK_TABLE_NAME, FK_CONSTRAINT_NAME, FK_ORDINAL_POSITION"
        //        , new AddToListDelegate<Constraint>(AddToListConstraints));
        //    return Helper.GetGroupForeignKeys(list, GetAllTableNames());
        //}

        /// <summary>
        /// Get the query based on http://msdn.microsoft.com/en-us/library/ms174156.aspx
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        public List<Index> GetIndexesFromTable(string tableName)
        {
            return ExecuteReader(
                "SELECT     TABLE_NAME, INDEX_NAME, PRIMARY_KEY, [UNIQUE], [CLUSTERED], ORDINAL_POSITION, COLUMN_NAME, COLLATION AS SORT_ORDER " + // Weird column name COLLATION FOR SORT_ORDER
                "FROM         Information_Schema.Indexes " +
                "WHERE     (PRIMARY_KEY = 0) " +
                "   AND (TABLE_NAME = '" + tableName + "')  " +
                "   AND (SUBSTRING(COLUMN_NAME, 1,5) <> '__sys')   " +
                "ORDER BY TABLE_NAME, INDEX_NAME, ORDINAL_POSITION"
                , new AddToListDelegate<Index>(AddToListIndexes));
        }

        public List<Index> GetAllIndexes()
        {
            var list = ExecuteReader(
                "SELECT     TABLE_NAME, INDEX_NAME, PRIMARY_KEY, [UNIQUE], [CLUSTERED], ORDINAL_POSITION, COLUMN_NAME, COLLATION AS SORT_ORDER " + // Weird column name COLLATION FOR SORT_ORDER
                "FROM         Information_Schema.Indexes " +
                " WHERE     (PRIMARY_KEY = 0) " +
                " AND (SUBSTRING(COLUMN_NAME, 1,5) <> '__sys') "
                , new AddToListDelegate<Index>(AddToListIndexes));
            return list.OrderBy(i => i.TableName).ThenBy(i => i.IndexName).ThenBy(i => i.OrdinalPosition).ToList();
        }

        public List<View> GetAllViews()
        {
            return new List<View>();
        }

        public List<Trigger> GetAllTriggers()
        {
            return new List<Trigger>();
        }

        /// <summary>
        /// Renames the table.
        /// </summary>
        /// <param name="oldName">The old name.</param>
        /// <param name="newName">The new name.</param>
        public void RenameTable(string oldName, string newName)
        {
            ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, "sp_rename '{0}', '{1}';", oldName, newName));
        }

        /// <summary>
        /// Determines whether this instance is server.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this instance is server; otherwise, <c>false</c>.
        /// </returns>
        public bool IsServer()
        {
            return false;
        }

        public DataSet GetSchemaDataSet(List<string> tables)
        {
            DataSet schemaSet = null;
            try
            {
                schemaSet = new DataSet();
                using (SqlCeDataAdapter adapter = new SqlCeDataAdapter())
                {
                    using (SqlCeCommand cmd = new SqlCeCommand())
                    {
                        cmd.Connection = _cn;
                        foreach (var table in tables)
                        {
                            string strSql = "SELECT * FROM [" + table + "] WHERE 1 = 0";

                            using (SqlCeCommand cmdSet = new SqlCeCommand(strSql, _cn))
                            {
                                using (SqlCeDataAdapter adapter1 = new SqlCeDataAdapter(cmdSet))
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
            catch
            {
                if (schemaSet != null)
                    schemaSet.Dispose();
                throw;
            }
            return schemaSet;
        }

        /// <summary>
        /// Executes the SQL.
        /// </summary>
        /// <param name="script">The script.</param>
        /// <returns></returns>
        public DataSet ExecuteSql(string script)
        {
            DataSet ds = null;
            try
            {
                ds = new DataSet();
                RunCommands(ds, script, false, false);
            }
            finally
            {
                if (ds != null)
                    ds.Dispose();
            }
            return ds;
        }

        public void ExecuteSqlFile(string scriptPath)
        {
            RunCommands(scriptPath);
        }

        public DataSet ExecuteSql(string script, out bool schemaChanged)
        {
            _schemaHasChanged = false;
            DataSet ds = null;
            try
            {
                ds = new DataSet();
                RunCommands(ds, script, false, false);
                schemaChanged = _schemaHasChanged;
            }
            catch
            {
                if (ds != null)
                    ds.Dispose();
                throw;
            }
            return ds;
        }

        public string ParseSql(string script)
        {
            using (DataSet ds = new DataSet())
            {
                RunCommands(ds, script, true, false);
                return _showPlan;
            }
        }

        public DataSet ExecuteSql(string script, out string showPlanString)
        {
            DataSet ds = null;
            try
            {
                ds = new DataSet();
                RunCommands(ds, script, false, true);
                showPlanString = _showPlan;
            }
            catch
            {
                if (ds != null)
                    ds.Dispose();
                throw;
            }
            return ds;
        }

        public DataSet ExecuteSql(string script, out bool schemaChanged, bool ignoreDdlErrors)
        {
            _schemaHasChanged = false;
            DataSet ds = null;
            try
            {
                ds = new DataSet();
                RunCommands(ds, script, false, false, ignoreDdlErrors);
                schemaChanged = _schemaHasChanged;
            }
            catch
            {
                if (ds != null)
                    ds.Dispose();
                throw;
            }
            return ds;
        }

        public Type GetClrTypeFromDataType(string typeName)
        {
            throw new NotImplementedException();
        }

        private void RunCommands(string scriptPath)
        {
            if (!System.IO.File.Exists(scriptPath))
                return;

            using (var sr = System.IO.File.OpenText(scriptPath))
            {
                var sb = new StringBuilder(10000);
                while (!sr.EndOfStream)
                {
                    var readLine = sr.ReadLine();
                    if (readLine != null)
                    {
                        var line = readLine.Trim();
                        if (line.Equals("GO", StringComparison.OrdinalIgnoreCase))
                        {
                            RunCommand(sb.ToString());
                            sb.Clear();
                        }
                        else
                        {
                            sb.Append(line);
                            sb.Append(Environment.NewLine);
                        }
                    }
                }
            }
        }

        private void RunCommand(string sql)
        {
            try
            {
                using (var cmd = new SqlCeCommand())
                {
                    cmd.Connection = _cn;
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlCeException ex)
            {
                throw new Exception(FormatSqlCeException(ex, sql));
            }

        }

        private string FormatSqlCeException(SqlCeException ex, string sql)
        {
            return Helper.ShowErrors(ex) + Environment.NewLine + sql;
        }

        private void RunCommands(DataSet dataset, string script, bool checkSyntax, bool includePlan, bool ignoreDdlErrors = false)
        {
            dataset.EnforceConstraints = false;
            using (SqlCeCommand cmd = new SqlCeCommand())
            {
                cmd.Connection = _cn;
                if (checkSyntax)
                {
                    cmd.CommandText = "SET SHOWPLAN_XML ON;";
                    cmd.ExecuteNonQuery();
                }

                if (includePlan)
                {
                    cmd.CommandText = "SET STATISTICS XML ON;";
                    cmd.ExecuteNonQuery();
                }

                using (SqlCommandReader reader = new SqlCommandReader(script))
                {
                    var commandText = reader.ReadCommand();
                    while (!string.IsNullOrWhiteSpace(commandText))
                    {
                        RunCommand(commandText, dataset, ignoreDdlErrors);
                        commandText = reader.ReadCommand();
                    }
                }

                if (checkSyntax)
                {
                    cmd.CommandText = "SELECT @@SHOWPLAN;";

                    var obj = cmd.ExecuteScalar();
                    var s = obj as string;
                    if (s != null)
                        _showPlan = s;

                    cmd.CommandText = "SET SHOWPLAN_XML OFF;";
                    cmd.ExecuteNonQuery();
                }

                if (includePlan)
                {
                    cmd.CommandText = "SELECT @@SHOWPLAN;";

                    var obj = cmd.ExecuteScalar();
                    var s = obj as string;
                    if (s != null)
                        _showPlan = s;

                    cmd.CommandText = "SET STATISTICS XML OFF;";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void RunCommand(string commandText, DataSet dataSet, bool ignoreDdlErrors)
        {
            using (SqlCeCommand cmd = new SqlCeCommand())
            {
                cmd.CommandText = commandText;
                cmd.Connection = _cn;

                CommandExecute execute = RepositoryHelper.FindExecuteType(commandText);

                if (execute != CommandExecute.Undefined)
                {
                    if (execute == CommandExecute.DataTable)
                    {
                        string[] tables = new string[1];
                        tables[0] = "table" + dataSet.Tables.Count.ToString();
                        using (SqlCeDataReader rdr = cmd.ExecuteReader())
                        {
                            dataSet.Load(rdr, LoadOption.OverwriteChanges, tables);
                            dataSet.Tables[dataSet.Tables.Count - 1].MinimumCapacity = 0;
                            dataSet.Tables[dataSet.Tables.Count - 1].Locale = CultureInfo.InvariantCulture;
                        }
                    }
                    if (execute == CommandExecute.NonQuery || execute == CommandExecute.NonQuerySchemaChanged)
                    {
                        if (execute == CommandExecute.NonQuerySchemaChanged)
                            _schemaHasChanged = true;
                        DataTable table = null;
                        try
                        {
                            int rows = cmd.ExecuteNonQuery();
                            table = new DataTable();
                            table.MinimumCapacity = Math.Max(0, rows);
                            dataSet.Tables.Add(table);
                        }
                        catch
                        {
                            if (table != null)
                                table.Dispose();
                            if (ignoreDdlErrors && execute == CommandExecute.NonQuerySchemaChanged)
                            { }
                            else
                            {
                                throw;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get the local Datetime for last sync
        /// </summary>
        /// <param name="publication"> Publication id: EEJx:Northwind:NwPubl</param>
        /// <returns></returns>
        public DateTime GetLastSuccessfulSyncTime(string publication)
        {
            string[] vals = publication.Split(':');

            if (vals.Length != 3)
                return DateTime.MinValue;

            using (var cmd = _cn.CreateCommand())
            {
                cmd.Connection = _cn;

                cmd.CommandText = "SELECT table_name FROM information_schema.tables WHERE TABLE_NAME = @table";
                cmd.Parameters.Add("@table", SqlDbType.NVarChar, 4000);
                cmd.Parameters["@table"].Value = "__sysMergeSubscriptions";
                object obj = cmd.ExecuteScalar();

                if (obj == null)
                    return DateTime.MinValue;
                cmd.Parameters.Clear();

                cmd.CommandText = "SELECT LastSuccessfulSync FROM __sysMergeSubscriptions " +
                    "WHERE Publisher=@publisher AND PublisherDatabase=@database AND Publication=@publication";

                cmd.Parameters.Add("@publisher", SqlDbType.NVarChar, 4000);
                cmd.Parameters["@publisher"].Value = vals[0];

                cmd.Parameters.Add("@database", SqlDbType.NVarChar, 4000);
                cmd.Parameters["@database"].Value = vals[1];

                cmd.Parameters.Add("@publication", SqlDbType.NVarChar, 4000);
                cmd.Parameters["@publication"].Value = vals[2];

                obj = cmd.ExecuteScalar();
                if (obj == null)
                    return DateTime.MinValue;
                else
                    return ((DateTime)obj);
            }
        }

        public bool KeepSchema()
        {
            return false;
        }
        #endregion
    }
}
