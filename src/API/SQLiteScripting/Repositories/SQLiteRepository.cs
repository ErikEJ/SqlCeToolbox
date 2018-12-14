using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ErikEJ.SqlCeScripting;
using System.Data.SQLite;
using System.Data;
using System.IO;
using System.Globalization;
using DbUp.Support.SqlServer;
using System.Text.RegularExpressions;
using System.Data.Common;

namespace ErikEJ.SQLiteScripting
{
    //uses sqlite-netFx40-static-binary-bundle-Win32-2010-1.0.XXX.0.zip
    public class SQLiteRepository : IRepository
    {
        private SQLiteConnection _cn;
        private readonly SQLiteCommand _cmd;
        private readonly string _showPlan = string.Empty;
        private bool _schemaHasChanged;

        public SQLiteRepository(string connectionString)
        {
            _cn = new SQLiteConnection(connectionString);
            _cn.Open();
            _cmd = new SQLiteCommand();
            _cmd.Connection = _cn;
        }

        #region IRepository Members

        public string GetRunTimeVersion()
        {
            return _cn.ServerVersion;
        }

        public List<string> GetAllTableNames()
        {
            var list = new List<string>();
            //Also contains TABLE_DEFINITION!
            var dt = _cn.GetSchema("Tables");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i]["TABLE_TYPE"].ToString() == "table")
                {
                    var definition = dt.Rows[i]["TABLE_DEFINITION"].ToString();
                    list.Add(dt.Rows[i]["TABLE_NAME"].ToString());
                }
            }
            list.Sort();
            return list;
        }

        public List<string> GetAllTableNamesForExclusion()
        {
            return GetAllTableNames();
        }

        public List<View> GetAllViews()
        {
            var list = new List<View>();
            var dt = Schema_Views(_cn);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var view = new View
                {
                    ViewName = dt.Rows[i]["TABLE_NAME"].ToString(),
                    Definition = dt.Rows[i]["VIEW_DEFINITION"].ToString(),
                    Select = dt.Rows[i]["VIEW_SELECT"].ToString()
                };
                list.Add(view);
            }
            return list;
        }

        public List<Column> GetAllViewColumns()
        {
            return GetListOfColumns("ViewColumns");
        }

        /// <summary>
        /// Retrieves view schema information for the database
        /// </summary>
        /// <param name="strCatalog">The catalog (attached database) to retrieve views on</param>
        /// <param name="strView">The view name, can be null</param>
        /// <returns>DataTable</returns>
        private DataTable Schema_Views(SQLiteConnection cn)
        {
            DataTable tbl = 
                new DataTable("Views");
            DataRow row;
            string strSql;
            int nPos;

            tbl.Locale = CultureInfo.InvariantCulture;
            tbl.Columns.Add("TABLE_CATALOG", typeof(string));
            tbl.Columns.Add("TABLE_NAME", typeof(string));
            tbl.Columns.Add("VIEW_DEFINITION", typeof(string));
            tbl.Columns.Add("VIEW_SELECT", typeof(string));
            tbl.Columns.Add("IS_UPDATABLE", typeof(bool));

            tbl.BeginLoadData();

            using (SQLiteCommand cmd = new SQLiteCommand(string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [main].[sqlite_master] WHERE [type] LIKE 'view'")))
            {
                cmd.Connection = cn;
                using (SQLiteDataReader rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        row = tbl.NewRow();

                        row["TABLE_CATALOG"] = "main";
                        row["TABLE_NAME"] = rd.GetString(2);
                        row["IS_UPDATABLE"] = false;
                        row["VIEW_DEFINITION"] = rd.GetString(4);

                        strSql = rd.GetString(4)
                        .Replace(@"\n", @"\n ")
                        .Replace(Environment.NewLine, " " + Environment.NewLine + " ")
                        .Replace('\t', ' ')
                        .Replace(" AS(", " AS (")
                        .Replace(" as(", " as (");
                        nPos = CultureInfo.InvariantCulture.CompareInfo.IndexOf(strSql, " AS ", CompareOptions.IgnoreCase);
                        if (nPos > -1)
                        {
                            strSql = strSql.Substring(nPos + 4).Trim();
                            row["VIEW_SELECT"] = strSql;
                        }

                        tbl.Rows.Add(row);
                    }
                }
            }
            tbl.AcceptChanges();
            tbl.EndLoadData();

            return tbl;
        }

        public List<Trigger> GetAllTriggers()
        {
            var list = new List<Trigger>();
            var dt = _cn.GetSchema("Triggers");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var trigger = new Trigger();
                trigger.TriggerName = dt.Rows[i]["TRIGGER_NAME"].ToString();
                trigger.TableName = dt.Rows[i]["TABLE_NAME"].ToString();
                trigger.Definition = dt.Rows[i]["TRIGGER_DEFINITION"].ToString();
                list.Add(trigger);
            }
            return list;
        }

        public List<Column> GetAllColumns()
        {
            return GetListOfColumns("Columns");
        }

        private List<Column> GetListOfColumns(string schemaView)
        {
            var result = new List<Column>();
            var dt = new DataTable();

            dt = _cn.GetSchema(schemaView);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var col = new Column();

                col.CharacterMaxLength = (int)dt.Rows[i]["CHARACTER_MAXIMUM_LENGTH"];
                col.ColumnHasDefault = (bool)dt.Rows[i]["COLUMN_HASDEFAULT"];
                col.ColumnDefault = dt.Rows[i]["COLUMN_DEFAULT"].GetType() == typeof(DBNull) ? null : dt.Rows[i]["COLUMN_DEFAULT"].ToString();
                if (col.ColumnDefault == null)
                {
                    col.ColumnHasDefault = false;
                }
                col.ColumnName = dt.Rows[i]["COLUMN_NAME"].ToString();
                col.DataType = dt.Rows[i]["DATA_TYPE"].ToString();
                if ((bool)dt.Rows[i]["PRIMARY_KEY"] && col.DataType.ToLowerInvariant() == "integer")
                {
                    col.AutoIncrementBy = 1;
                    col.AutoIncrementSeed = 1;
                }

                if (col.DataType.ToLowerInvariant() == "boolean")
                {
                    col.DataType = "bit";
                }
                if (col.DataType.ToLowerInvariant() == "varchar")
                {
                    col.DataType = "nvarchar";
                }
                if (col.DataType.ToLowerInvariant() == "blob")
                {
                    col.DataType = "image";
                }
                if (col.DataType.ToLowerInvariant() == "integer")
                {
                    col.DataType = "bigint";
                }

                var isNullable = (bool)dt.Rows[i]["IS_NULLABLE"];
                col.IsNullable = isNullable ? YesNoOption.YES : YesNoOption.NO;
                if (dt.Rows[i]["NUMERIC_PRECISION"].GetType() != typeof(DBNull))
                {
                    col.NumericPrecision = (int)dt.Rows[i]["NUMERIC_PRECISION"];
                    if (dt.Rows[i]["NUMERIC_SCALE"].GetType() != typeof(DBNull))
                    {
                        col.NumericScale = (int)dt.Rows[i]["NUMERIC_SCALE"];
                    }
                }
                col.Ordinal = (int)dt.Rows[i]["ORDINAL_POSITION"];
                if (schemaView == "Columns")
                {
                    col.TableName = dt.Rows[i]["TABLE_NAME"].ToString();
                }
                else
                {
                    col.TableName = dt.Rows[i]["VIEW_NAME"].ToString();
                }
                result.Add(col);
            }
            return result;
        }


        public DataTable GetDataFromTable(string tableName, List<Column> columns)
        {
            return GetDataFromTable(tableName, columns, new List<PrimaryKey>());
        }

        public DataTable GetDataFromTable(string tableName, List<Column> tableColumns, List<PrimaryKey> tablePrimaryKeys)
        {
            StringBuilder sb = new StringBuilder(200);
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

        public IDataReader GetDataFromReader(string tableName, List<Column> tableColumns)
        {
            return GetDataFromReader(tableName, tableColumns, new List<PrimaryKey>());
        }

        public IDataReader GetDataFromReader(string tableName, List<Column> tableColumns, List<PrimaryKey> tablePrimaryKeys, string whereClause = null)
        {
            StringBuilder sb = new StringBuilder(200);
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

        private static string SortSelect(List<PrimaryKey> tablePrimaryKeys)
        {
            var sb = new StringBuilder(64);
            if (tablePrimaryKeys.Count > 0)
            {
                sb.Append(" ORDER BY ");
                tablePrimaryKeys.ForEach(delegate(PrimaryKey column)
                {
                    sb.AppendFormat("[{0}],", column.ColumnName);
                });

                // Remove the last comma
                sb.Remove(sb.Length - 1, 1);
            }
            return sb.ToString();
        }

        private IDataReader ExecuteDataReader(string commandText, CommandType commandType)
        {
            using (var cmd = new SQLiteCommand(commandText, _cn))
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
                using (var cmd = new SQLiteCommand(commandText, _cn))
                {
                    using (var da = new SQLiteDataAdapter(cmd))
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

        public List<PrimaryKey> GetAllPrimaryKeys()
        {
            var result = new List<PrimaryKey>();

            var indexes = _cn.GetSchema("Indexes");
            var rows = indexes.AsEnumerable()
                    .Where(row => row.Field<bool>("PRIMARY_KEY"));
            indexes = rows.Any() ? rows.CopyToDataTable() : indexes.Clone();

            var dt = _cn.GetSchema("Columns");
            rows = dt.AsEnumerable()
                .Where(row => row.Field<bool>("PRIMARY_KEY"))
                .OrderBy(row => row.Field<String>("TABLE_NAME")).ThenBy(row => row.Field<int>("ORDINAL_POSITION"));
            dt = rows.Any() ? rows.CopyToDataTable() : dt.Clone();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var pk = new PrimaryKey();
                pk.ColumnName = dt.Rows[i]["COLUMN_NAME"].ToString();

                rows = indexes.AsEnumerable()
                            .Where(row => row.Field<string>("TABLE_NAME") == dt.Rows[i]["TABLE_NAME"].ToString()
                            && row.Field<bool>("PRIMARY_KEY"));

                var pkNameDt = rows.Any() ? rows.CopyToDataTable() : null;
                //SQLite "Indexes" contains PK names by Table (sometimes!)
                if (pkNameDt != null)
                {
                    pk.KeyName = pkNameDt.Rows[0]["INDEX_NAME"].ToString();
                }
                else
                {
                    pk.KeyName = dt.Rows[i]["TABLE_NAME"].ToString();
                }
                pk.TableName = dt.Rows[i]["TABLE_NAME"].ToString();
                result.Add(pk);
            }
            return result;
        }

        public List<SqlCeScripting.Constraint> GetAllForeignKeys()
        {
            var result = new List<SqlCeScripting.Constraint>();
            var dt = _cn.GetSchema("ForeignKeys");
            var previousConstraintName = string.Empty; 
            
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var fk = new SqlCeScripting.Constraint();
                fk.ColumnName = dt.Rows[i]["FKEY_FROM_COLUMN"].ToString();
                var constraintName = dt.Rows[i]["CONSTRAINT_NAME"].ToString();
                var fkOrdinal = int.Parse(dt.Rows[i]["FKEY_FROM_ORDINAL_POSITION"].ToString());
                if (fkOrdinal == 0)
                {
                    previousConstraintName = constraintName;
                }
                fk.ConstraintName = previousConstraintName;
                fk.ConstraintTableName = dt.Rows[i]["TABLE_NAME"].ToString();
                fk.DeleteRule = dt.Rows[i]["FKEY_ON_DELETE"].ToString();
                fk.UniqueColumnName = dt.Rows[i]["FKEY_TO_COLUMN"].ToString();
                fk.UniqueConstraintName = "PK_" + dt.Rows[i]["FKEY_TO_TABLE"];
                fk.UniqueConstraintTableName = dt.Rows[i]["FKEY_TO_TABLE"].ToString();
                fk.UpdateRule = dt.Rows[i]["FKEY_ON_UPDATE"].ToString();
                fk.Columns = new ColumnList();
                fk.UniqueColumns = new ColumnList();
                result.Add(fk);
            }
            return RepositoryHelper.GetGroupForeignKeys(result, GetAllTableNames());
        }

        public List<Index> GetIndexesFromTable(string tableName)
        {
            return GetIndexes(tableName);
        }

        public List<Index> GetAllIndexes()
        {
            return GetIndexes();
        }

        private List<Index> GetIndexes(string tableName = null)
        {
            var result = new List<Index>();
            var columns = _cn.GetSchema("IndexColumns");

            var indexes = _cn.GetSchema("Indexes");
            var rows = indexes.AsEnumerable()
                    .Where(row => row.Field<Boolean>("PRIMARY_KEY") == false);
            indexes = rows.Any() ? rows.CopyToDataTable() : indexes.Clone();

            if (!string.IsNullOrEmpty(tableName))
            {
                rows = indexes.AsEnumerable()
                    .Where(row => row.Field<Boolean>("PRIMARY_KEY") == false
                    && row.Field<string>("TABLE_NAME") == tableName);
                indexes = rows.Any() ? rows.CopyToDataTable() : indexes.Clone();
            }
            for (int x = 0; x < indexes.Rows.Count; x++)
            {
                var ix = new Index();
                var cols = columns.AsEnumerable()
                    .Where(row => row.Field<String>("INDEX_NAME") == indexes.Rows[x]["INDEX_NAME"].ToString());

                var dt = cols.Any() ? cols.CopyToDataTable() : columns.Clone();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ix.Clustered = false;
                    ix.ColumnName = dt.Rows[i]["COLUMN_NAME"].ToString();
                    ix.IndexName = dt.Rows[i]["INDEX_NAME"].ToString();
                    ix.OrdinalPosition = (int)dt.Rows[i]["ORDINAL_POSITION"];
                    ix.SortOrder = dt.Rows[i]["SORT_MODE"].ToString() == "ASC" ? SortOrderEnum.ASC : SortOrderEnum.DESC;
                    ix.TableName = dt.Rows[i]["TABLE_NAME"].ToString();
                    ix.Unique = (bool)indexes.Rows[x]["UNIQUE"];
                    result.Add(ix);
                }
            }
            return result;
        }

        public List<KeyValuePair<string, string>> GetDatabaseInfo()
        {
            List<KeyValuePair<string, string>> valueList = new List<KeyValuePair<string, string>>();
            SQLiteConnectionStringBuilder sb = new SQLiteConnectionStringBuilder(_cn.ConnectionString);

            valueList.Add(new KeyValuePair<string, string>("Database", sb.DataSource));
            valueList.Add(new KeyValuePair<string, string>("ServerVersion", _cn.ServerVersion));
            
            if (File.Exists(sb.DataSource))
            {
                FileInfo fi = new FileInfo(sb.DataSource);
                valueList.Add(new KeyValuePair<string, string>("DatabaseSize", RepositoryHelper.GetSizeReadable(fi.Length)));
                valueList.Add(new KeyValuePair<string, string>("Created", fi.CreationTime.ToShortDateString() + " " + fi.CreationTime.ToShortTimeString()));
            }
            return valueList;
        }

        public bool HasIdentityColumn(string tableName)
        {
            return false;
        }

        public bool IsServer()
        {
            return false;
        }

        public int GetRowVersionOrdinal(string tableName)
        {
            return -1;
        }

        public int GetIdentityOrdinal(string tableName)
        {
            return -1;
        }

        public long GetRowCount(string tableName)
        {
            //Not possible?
            return -1;
        }

        public void RenameTable(string oldName, string newName)
        {
            ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, "ALTER TABLE [{0}] RENAME TO [{1}];", oldName, newName));
        }

        private void ExecuteNonQuery(string commandText)
        {
            using (var cmd = new SQLiteCommand(commandText, _cn))
            {
                cmd.ExecuteNonQuery();
            }
        }

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

        private void RunCommands(string scriptPath)
        {
            if (!File.Exists(scriptPath))
                return;

            using (SqlCommandReaderStreamed sr = new SqlCommandReaderStreamed(scriptPath))
            {
                var commandText = sr.ReadCommand();
                while (!string.IsNullOrWhiteSpace(commandText))
                {
                    RunCommand(commandText);
                    commandText = sr.ReadCommand();
                }
            }
        }

        private void RunCommand(string sql)
        {
            using (SQLiteCommand cmd = new SQLiteCommand())
            {
                cmd.Connection = _cn;
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
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

        internal void RunCommands(DataSet dataset, string script, bool checkSyntax, bool includePlan, bool ignoreDdlErrors = false)
        {
            dataset.EnforceConstraints = false;
            using (SQLiteCommand cmd = new SQLiteCommand())
            {
                cmd.Connection = _cn;
                using (SqlCommandReader reader = new SqlCommandReader(script))
                {
                    var commandText = reader.ReadCommand();
                    while (!string.IsNullOrWhiteSpace(commandText))
                    {
                        if (includePlan)
                        {
                            commandText = "EXPLAIN QUERY PLAN " + commandText;
                        }
                        RunCommand(commandText, dataset, ignoreDdlErrors);
                        commandText = reader.ReadCommand();
                    }
                }
            }
        }

        private void RunCommand(string commandText, DataSet dataSet, bool ignoreDdlErrors)
        {
            using (SQLiteCommand cmd = new SQLiteCommand())
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
                        using (SQLiteDataReader rdr = cmd.ExecuteReader())
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

        public string ParseSql(string script)
        {
            using (DataSet ds = new DataSet())
            {
                StringBuilder plan = new StringBuilder();
                RunCommands(ds, script, false, true);
                for (int i = 0; i < ds.Tables.Count; i++)
                {
                    for (int x = 0; x < ds.Tables[i].Rows.Count; x++)
			        {
			            plan.AppendLine(ds.Tables[i].Rows[x][3].ToString());
			        }
                }
                return plan.ToString();
            }
        }

        public DateTime GetLastSuccessfulSyncTime(string publication)
        {
            return DateTime.MinValue;
        }

        public List<string> GetAllSubscriptionNames()
        {
            return new List<string>();
        }

        public DataSet GetSchemaDataSet(List<string> tables)
        {
            DataSet schemaSet = null;
            try
            {
                schemaSet = new DataSet();
                using (SQLiteDataAdapter adapter = new SQLiteDataAdapter())
                {
                    using (var cmd = new SQLiteCommand())
                    {
                        cmd.Connection = _cn;
                        foreach (var table in tables)
                        {
                            string strSql = "SELECT * FROM [" + table + "] WHERE 1 = 0";

                            using (var cmdSet = new SQLiteCommand(strSql, _cn))
                            {
                                using (var adapter1 = new SQLiteDataAdapter(cmdSet))
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

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (_cmd != null)
            {
                _cmd.Dispose();
            }
            if (_cn != null)
            {
                _cn.Close();
                _cn = null;
            }
        }

        #endregion

        private static SqLiteDbTypeMap _typeNames;
        internal const DbType FallbackDefaultDbType = DbType.Object;

        public Type GetClrTypeFromDataType(string typeName)
        {
            var dbType = TypeNameToDbType(typeName);
            return DbTypeToType(dbType);
        }

        /// <summary>
        /// For a given type name, return a closest-match .NET type
        /// </summary>
        /// <param name="name">The name of the type to match</param>
        /// <returns>The .NET DBType the text evaluates to.</returns>
        internal DbType TypeNameToDbType(
            string name
            )
        {
            if (_typeNames == null)
                _typeNames = GetSqLiteDbTypeMap();

            if (name != null)
            {
                SqLiteDbTypeMapping value;

                if (_typeNames.TryGetValue(name, out value))
                {
                    return value.DataType;
                }
                else
                {
                    int index = name.IndexOf('(');

                    if ((index > 0) &&
                        _typeNames.TryGetValue(name.Substring(0, index).TrimEnd(), out value))
                    {
                        return value.DataType;
                    }
                }
            }
            return FallbackDefaultDbType;
        }

        /// <summary>
        /// Convert a DbType to a Type
        /// </summary>
        /// <param name="typ">The DbType to convert from</param>
        /// <returns>The closest-match .NET type</returns>
        internal Type DbTypeToType(DbType typ)
        {
            return _dbtypeToType[(int)typ];
        }

        private Type[] _dbtypeToType = {
          typeof(string),   // AnsiString (0)
          typeof(byte[]),   // Binary (1)
          typeof(byte),     // Byte (2)
          typeof(bool),     // Boolean (3)
          typeof(decimal),  // Currency (4)
          typeof(DateTime), // Date (5)
          typeof(DateTime), // DateTime (6)
          typeof(decimal),  // Decimal (7)
          typeof(double),   // Double (8)
          typeof(Guid),     // Guid (9)
          typeof(Int16),    // Int16 (10)
          typeof(Int32),    // Int32 (11)
          typeof(Int64),    // Int64 (12)
          typeof(object),   // Object (13)
          typeof(sbyte),    // SByte (14)
          typeof(float),    // Single (15)
          typeof(string),   // String (16)
          typeof(DateTime), // Time (17)
          typeof(UInt16),   // UInt16 (18)
          typeof(UInt32),   // UInt32 (19)
          typeof(UInt64),   // UInt64 (20)
          typeof(double),   // VarNumeric (21)
          typeof(string),   // AnsiStringFixedLength (22)
          typeof(string),   // StringFixedLength (23)
          typeof(string),   // ?? (24)
          typeof(string),   // Xml (25)
        };


        internal sealed class SqLiteDbTypeMap
      : Dictionary<string, SqLiteDbTypeMapping>
        {
            #region Private Data
            private Dictionary<DbType, SqLiteDbTypeMapping> reverse;
            #endregion

            /////////////////////////////////////////////////////////////////////////

            #region Public Constructors
            public SqLiteDbTypeMap()
                : base(new TypeNameStringComparer())
            {
                reverse = new Dictionary<DbType, SqLiteDbTypeMapping>();
            }

            /////////////////////////////////////////////////////////////////////////

            public SqLiteDbTypeMap(
                IEnumerable<SqLiteDbTypeMapping> collection
                )
                : this()
            {
                Add(collection);
            }
            #endregion

            /////////////////////////////////////////////////////////////////////////

            #region System.Collections.Generic.Dictionary "Overrides"
            public new int Clear()
            {
                int result = 0;

                if (reverse != null)
                {
                    result += reverse.Count;
                    reverse.Clear();
                }

                result += Count;
                base.Clear();

                return result;
            }
            #endregion

            /////////////////////////////////////////////////////////////////////////

            #region SQLiteDbTypeMapping Helper Methods
            public void Add(
                IEnumerable<SqLiteDbTypeMapping> collection
                )
            {
                if (collection == null)
                    throw new ArgumentNullException("collection");

                foreach (SqLiteDbTypeMapping item in collection)
                    Add(item);
            }

            /////////////////////////////////////////////////////////////////////////

            public void Add(SqLiteDbTypeMapping item)
            {
                if (item == null)
                    throw new ArgumentNullException("item");

                if (item.TypeName == null)
                    throw new ArgumentException("item type name cannot be null");

                base.Add(item.TypeName, item);

                if (item.Primary)
                    reverse.Add(item.DataType, item);
            }
            #endregion

            /////////////////////////////////////////////////////////////////////////

            #region DbType Helper Methods
            public bool ContainsKey(DbType key)
            {
                if (reverse == null)
                    return false;

                return reverse.ContainsKey(key);
            }

            /////////////////////////////////////////////////////////////////////////

            public bool TryGetValue(DbType key, out SqLiteDbTypeMapping value)
            {
                if (reverse == null)
                {
                    value = null;
                    return false;
                }

                return reverse.TryGetValue(key, out value);
            }

            /////////////////////////////////////////////////////////////////////////

            public bool Remove(DbType key)
            {
                if (reverse == null)
                    return false;

                return reverse.Remove(key);
            }
            #endregion
        }

        /////////////////////////////////////////////////////////////////////////////

        internal sealed class SqLiteDbTypeMapping
        {
            internal SqLiteDbTypeMapping(
                string newTypeName,
                DbType newDataType,
                bool newPrimary
                )
            {
                TypeName = newTypeName;
                DataType = newDataType;
                Primary = newPrimary;
            }

            internal string TypeName;
            internal DbType DataType;
            internal bool Primary;
        }

        internal sealed class TypeNameStringComparer : IEqualityComparer<string>
        {
            #region IEqualityComparer<string> Members
            public bool Equals(
              string left,
              string right
              )
            {
                return String.Equals(left, right, StringComparison.OrdinalIgnoreCase);
            }

            ///////////////////////////////////////////////////////////////////////////

            public int GetHashCode(
              string value
              )
            {
                //
                // NOTE: The only thing that we must guarantee here, according
                //       to the MSDN documentation for IEqualityComparer, is
                //       that for two given strings, if Equals return true then
                //       the two strings must hash to the same value.
                //
                if (value != null)
                    return value.ToLowerInvariant().GetHashCode();
                else
                    throw new ArgumentNullException("value");
            }
            #endregion
        }

        private static SqLiteDbTypeMap GetSqLiteDbTypeMap()
        {
            return new SqLiteDbTypeMap(new[] {
            new SqLiteDbTypeMapping("BIGINT", DbType.Int64, false),
            new SqLiteDbTypeMapping("BIGUINT", DbType.UInt64, false),
            new SqLiteDbTypeMapping("BINARY", DbType.Binary, false),
            new SqLiteDbTypeMapping("BIT", DbType.Boolean, true),
            new SqLiteDbTypeMapping("BLOB", DbType.Binary, true),
            new SqLiteDbTypeMapping("BOOL", DbType.Boolean, false),
            new SqLiteDbTypeMapping("BOOLEAN", DbType.Boolean, false),
            new SqLiteDbTypeMapping("CHAR", DbType.AnsiStringFixedLength, true),
            new SqLiteDbTypeMapping("CLOB", DbType.String, false),
            new SqLiteDbTypeMapping("COUNTER", DbType.Int64, false),
            new SqLiteDbTypeMapping("CURRENCY", DbType.Decimal, false),
            new SqLiteDbTypeMapping("DATE", DbType.DateTime, false),
            new SqLiteDbTypeMapping("DATETIME", DbType.DateTime, true),
            new SqLiteDbTypeMapping("DECIMAL", DbType.Decimal, true),
            new SqLiteDbTypeMapping("DOUBLE", DbType.Double, false),
            new SqLiteDbTypeMapping("FLOAT", DbType.Double, false),
            new SqLiteDbTypeMapping("GENERAL", DbType.Binary, false),
            new SqLiteDbTypeMapping("GUID", DbType.Guid, false),
            new SqLiteDbTypeMapping("IDENTITY", DbType.Int64, false),
            new SqLiteDbTypeMapping("IMAGE", DbType.Binary, false),
            new SqLiteDbTypeMapping("INT", DbType.Int32, true),
            new SqLiteDbTypeMapping("INT8", DbType.SByte, false),
            new SqLiteDbTypeMapping("INT16", DbType.Int16, false),
            new SqLiteDbTypeMapping("INT32", DbType.Int32, false),
            new SqLiteDbTypeMapping("INT64", DbType.Int64, false),
            new SqLiteDbTypeMapping("INTEGER", DbType.Int64, true),
            new SqLiteDbTypeMapping("INTEGER8", DbType.SByte, false),
            new SqLiteDbTypeMapping("INTEGER16", DbType.Int16, false),
            new SqLiteDbTypeMapping("INTEGER32", DbType.Int32, false),
            new SqLiteDbTypeMapping("INTEGER64", DbType.Int64, false),
            new SqLiteDbTypeMapping("LOGICAL", DbType.Boolean, false),
            new SqLiteDbTypeMapping("LONG", DbType.Int64, false),
            new SqLiteDbTypeMapping("LONGCHAR", DbType.String, false),
            new SqLiteDbTypeMapping("LONGTEXT", DbType.String, false),
            new SqLiteDbTypeMapping("LONGVARCHAR", DbType.String, false),
            new SqLiteDbTypeMapping("MEMO", DbType.String, false),
            new SqLiteDbTypeMapping("MONEY", DbType.Decimal, false),
            new SqLiteDbTypeMapping("NCHAR", DbType.StringFixedLength, true),
            new SqLiteDbTypeMapping("NOTE", DbType.String, false),
            new SqLiteDbTypeMapping("NTEXT", DbType.String, false),
            new SqLiteDbTypeMapping("NUMBER", DbType.Decimal, false),
            new SqLiteDbTypeMapping("NUMERIC", DbType.Decimal, false),
            new SqLiteDbTypeMapping("NVARCHAR", DbType.String, true),
            new SqLiteDbTypeMapping("OLEOBJECT", DbType.Binary, false),
            new SqLiteDbTypeMapping("RAW", DbType.Binary, false),
            new SqLiteDbTypeMapping("REAL", DbType.Double, true),
            new SqLiteDbTypeMapping("SINGLE", DbType.Single, true),
            new SqLiteDbTypeMapping("SMALLDATE", DbType.DateTime, false),
            new SqLiteDbTypeMapping("SMALLINT", DbType.Int16, true),
            new SqLiteDbTypeMapping("SMALLUINT", DbType.UInt16, true),
            new SqLiteDbTypeMapping("STRING", DbType.String, false),
            new SqLiteDbTypeMapping("TEXT", DbType.String, false),
            new SqLiteDbTypeMapping("TIME", DbType.DateTime, false),
            new SqLiteDbTypeMapping("TIMESTAMP", DbType.DateTime, false),
            new SqLiteDbTypeMapping("TINYINT", DbType.Byte, true),
            new SqLiteDbTypeMapping("TINYSINT", DbType.SByte, true),
            new SqLiteDbTypeMapping("UINT", DbType.UInt32, true),
            new SqLiteDbTypeMapping("UINT8", DbType.Byte, false),
            new SqLiteDbTypeMapping("UINT16", DbType.UInt16, false),
            new SqLiteDbTypeMapping("UINT32", DbType.UInt32, false),
            new SqLiteDbTypeMapping("UINT64", DbType.UInt64, false),
            new SqLiteDbTypeMapping("ULONG", DbType.UInt64, false),
            new SqLiteDbTypeMapping("UNIQUEIDENTIFIER", DbType.Guid, true),
            new SqLiteDbTypeMapping("UNSIGNEDINTEGER", DbType.UInt64, true),
            new SqLiteDbTypeMapping("UNSIGNEDINTEGER8", DbType.Byte, false),
            new SqLiteDbTypeMapping("UNSIGNEDINTEGER16", DbType.UInt16, false),
            new SqLiteDbTypeMapping("UNSIGNEDINTEGER32", DbType.UInt32, false),
            new SqLiteDbTypeMapping("UNSIGNEDINTEGER64", DbType.UInt64, false),
            new SqLiteDbTypeMapping("VARBINARY", DbType.Binary, false),
            new SqLiteDbTypeMapping("VARCHAR", DbType.AnsiString, true),
            new SqLiteDbTypeMapping("VARCHAR2", DbType.AnsiString, false),
            new SqLiteDbTypeMapping("YESNO", DbType.Boolean, false)
        });
        }

        public bool KeepSchema()
        {
            return false;
        }
    }
}
