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

namespace ErikEJ.SQLiteScripting
{
    //uses sqlite-netFx40-static-binary-bundle-Win32-2010-1.0.XXX.0.zip
    public class SQLiteRepository : IRepository
    {
        private SQLiteConnection cn;
        private SQLiteCommand cmd;
        private delegate void AddToListDelegate<T>(ref List<T> list, SQLiteDataReader dr);
        private string showPlan = string.Empty;
        private bool schemaHasChanged = false;

        public SQLiteRepository(string connectionString)
        {
            cn = new SQLiteConnection(connectionString);
            cn.Open();
            cmd = new SQLiteCommand();
            cmd.Connection = cn;
        }

        #region IRepository Members

        public string GetRunTimeVersion()
        {
            return cn.ServerVersion;
        }

        public List<string> GetAllTableNames()
        {
            var list = new List<string>();
            //Also contains TABLE_DEFINITION!
            var dt = cn.GetSchema("Tables");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i]["TABLE_TYPE"].ToString() == "table")
                {
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
            var dt = cn.GetSchema("Views");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var view = new View();
                view.ViewName = dt.Rows[i]["TABLE_NAME"].ToString();
                view.Definition = dt.Rows[i]["VIEW_DEFINITION"].ToString();
                list.Add(view);
            }
            return list;
        }

        public List<Column> GetAllViewColumns()
        {
            return GetListOfColumns("ViewColumns");
        }

        public List<Trigger> GetAllTriggers()
        {
            var list = new List<Trigger>();
            var dt = cn.GetSchema("Triggers");
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


//GetSchema returns a DataTable that contains information about the tables, columns, or whatever you specify. 
//Valid GetSchema arguments for SQLite include:
//•DataTypes
//•ReservedWords

        public List<Column> GetAllColumns()
        {
            return GetListOfColumns("Columns");
        }

        private List<Column> GetListOfColumns(string schemaView)
        {
            var result = new List<Column>();
            var dt = cn.GetSchema(schemaView);

            //var tables = cn.GetSchema("Tables");
            //for (int i = 0; i < dt.Columns.Count; i++)
            //{
            //    System.Diagnostics.Debug.WriteLine(dt.Columns[i].ColumnName);
            //}

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var col = new Column();

                col.CharacterMaxLength = (int)dt.Rows[i]["CHARACTER_MAXIMUM_LENGTH"];
                col.ColumnHasDefault = (bool)dt.Rows[i]["COLUMN_HASDEFAULT"];
                col.ColumnDefault = dt.Rows[i]["COLUMN_DEFAULT"].GetType() != typeof(System.DBNull) ? null : dt.Rows[i]["COLUMN_DEFAULT"].ToString();
                if (col.ColumnDefault == null)
                {
                    col.ColumnHasDefault = false;
                }
                col.ColumnName = dt.Rows[i]["COLUMN_NAME"].ToString();
                col.DataType = dt.Rows[i]["DATA_TYPE"].ToString();
                if ((bool)dt.Rows[i]["PRIMARY_KEY"] && col.DataType == "INTEGER")
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
                col.IsNullable = (bool)isNullable ? YesNoOption.YES : YesNoOption.NO;
                if (dt.Rows[i]["NUMERIC_PRECISION"].GetType() != typeof(System.DBNull))
                {
                    col.NumericPrecision = (int)dt.Rows[i]["NUMERIC_PRECISION"];
                    if (dt.Rows[i]["NUMERIC_SCALE"].GetType() != typeof(System.DBNull))
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

        public System.Data.DataTable GetDataFromTable(string tableName, List<Column> columns)
        {
            return GetDataFromTable(tableName, columns, new List<PrimaryKey>());
        }

        public System.Data.DataTable GetDataFromTable(string tableName, List<Column> tableColumns, List<PrimaryKey> tablePrimaryKeys)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(200);
            sb.Append("SELECT ");
            foreach (Column col in tableColumns)
            {
                sb.Append(string.Format(System.Globalization.CultureInfo.InvariantCulture, "[{0}], ", col.ColumnName));
            }
            sb.Remove(sb.Length - 2, 2);

            sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, " From [{0}]", tableName);

            sb.Append(SortSelect(tablePrimaryKeys));
            return ExecuteDataTable(sb.ToString());
        }

        public System.Data.IDataReader GetDataFromReader(string tableName, List<Column> tableColumns)
        {
            return GetDataFromReader(tableName, tableColumns, new List<PrimaryKey>());
        }

        public System.Data.IDataReader GetDataFromReader(string tableName, List<Column> tableColumns, List<PrimaryKey> tablePrimaryKeys, string whereClause = null)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(200);
            sb.Append("SELECT ");
            foreach (Column col in tableColumns)
            {
                sb.Append(string.Format(System.Globalization.CultureInfo.InvariantCulture, "[{0}], ", col.ColumnName));
            }
            sb.Remove(sb.Length - 2, 2);

            sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, " FROM [{0}]", tableName);

            if (!string.IsNullOrEmpty(whereClause))
            {
                sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, " WHERE {0}", whereClause);
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
            using (var cmd = new SQLiteCommand(commandText, cn))
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
                dt.Locale = System.Globalization.CultureInfo.InvariantCulture;
                using (var cmd = new SQLiteCommand(commandText, cn))
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

            var indexes = cn.GetSchema("Indexes");
            var rows = indexes.AsEnumerable()
                    .Where(row => row.Field<Boolean>("PRIMARY_KEY") == true);
            indexes = rows.Any() ? rows.CopyToDataTable() : indexes.Clone();

            var dt = cn.GetSchema("Columns");
            rows = dt.AsEnumerable()
                .Where(row => row.Field<Boolean>("PRIMARY_KEY") == true)
                .OrderBy(row => row.Field<String>("TABLE_NAME")).ThenBy(row => row.Field<int>("ORDINAL_POSITION"));
            dt = rows.Any() ? rows.CopyToDataTable() : indexes.Clone();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var pk = new PrimaryKey();
                pk.ColumnName = dt.Rows[i]["COLUMN_NAME"].ToString();

                rows = indexes.AsEnumerable()
                            .Where(row => row.Field<String>("TABLE_NAME") == dt.Rows[i]["TABLE_NAME"].ToString()
                            && row.Field<bool>("PRIMARY_KEY") == true);

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

        public List<ErikEJ.SqlCeScripting.Constraint> GetAllForeignKeys()
        {
            var result = new List<ErikEJ.SqlCeScripting.Constraint>();
            var dt = cn.GetSchema("ForeignKeys");

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var fk = new ErikEJ.SqlCeScripting.Constraint();
                fk.ColumnName = dt.Rows[i]["FKEY_FROM_COLUMN"].ToString();
                fk.ConstraintName = dt.Rows[i]["CONSTRAINT_NAME"].ToString();
                fk.ConstraintTableName = dt.Rows[i]["TABLE_NAME"].ToString();
                fk.DeleteRule = dt.Rows[i]["FKEY_ON_DELETE"].ToString();
                fk.UniqueColumnName = dt.Rows[i]["FKEY_TO_COLUMN"].ToString();
                fk.UniqueConstraintName = "PK_" + dt.Rows[i]["FKEY_TO_TABLE"].ToString();
                fk.UniqueConstraintTableName = dt.Rows[i]["FKEY_TO_TABLE"].ToString();
                fk.UpdateRule = dt.Rows[i]["FKEY_ON_UPDATE"].ToString();
                fk.Columns = new ColumnList();
                fk.UniqueColumns = new ColumnList();
                result.Add(fk);
            }
            return RepositoryHelper.GetGroupForeingKeys(result, GetAllTableNames());
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
            var columns = cn.GetSchema("IndexColumns");

            var indexes = cn.GetSchema("Indexes");
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
                var dt = columns.AsEnumerable()
                    .Where(row => row.Field<String>("INDEX_NAME") == indexes.Rows[x]["INDEX_NAME"].ToString())
                    .CopyToDataTable();

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
            SQLiteConnectionStringBuilder sb = new SQLiteConnectionStringBuilder(cn.ConnectionString);

            valueList.Add(new KeyValuePair<string, string>("Database", sb.DataSource));
            valueList.Add(new KeyValuePair<string, string>("ServerVersion", cn.ServerVersion));
            
            if (System.IO.File.Exists(sb.DataSource))
            {
                System.IO.FileInfo fi = new System.IO.FileInfo(sb.DataSource);
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
            ExecuteNonQuery(string.Format(System.Globalization.CultureInfo.InvariantCulture, "ALTER TABLE [{0}] RENAME TO [{1}];", oldName, newName));
        }

        private void ExecuteNonQuery(string commandText)
        {
            using (var cmd = new SQLiteCommand(commandText, cn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        public System.Data.DataSet ExecuteSql(string script)
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
            if (!System.IO.File.Exists(scriptPath))
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
                cmd.Connection = cn;
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
        }

        public System.Data.DataSet ExecuteSql(string script, out string showPlanString)
        {
            DataSet ds = null;
            try
            {
                ds = new DataSet();
                RunCommands(ds, script, false, true);
                showPlanString = showPlan;
            }
            catch
            {
                if (ds != null)
                    ds.Dispose();
                throw;
            }
            return ds;
        }

        public System.Data.DataSet ExecuteSql(string script, out bool schemaChanged)
        {
            schemaHasChanged = false;
            DataSet ds = null;
            try
            {
                ds = new DataSet();
                RunCommands(ds, script, false, false);
                schemaChanged = schemaHasChanged;
            }
            catch
            {
                if (ds != null)
                    ds.Dispose();
                throw;
            }
            return ds;
        }

        public System.Data.DataSet ExecuteSql(string script, out bool schemaChanged, bool ignoreDDLErrors)
        {
            schemaHasChanged = false;
            DataSet ds = null;
            try
            {
                ds = new DataSet();
                RunCommands(ds, script, false, false, ignoreDDLErrors);
                schemaChanged = schemaHasChanged;
            }
            catch
            {
                if (ds != null)
                    ds.Dispose();
                throw;
            }
            return ds;
        }

        internal void RunCommands(DataSet dataset, string script, bool checkSyntax, bool includePlan, bool ignoreDDLErrors = false)
        {
            dataset.EnforceConstraints = false;
            using (SQLiteCommand cmd = new SQLiteCommand())
            {
                cmd.Connection = cn;
                using (SqlCommandReader reader = new SqlCommandReader(script))
                {
                    var commandText = reader.ReadCommand();
                    while (!string.IsNullOrWhiteSpace(commandText))
                    {
                        if (includePlan)
                        {
                            commandText = "EXPLAIN QUERY PLAN " + commandText;
                        }
                        RunCommand(commandText, dataset, ignoreDDLErrors);
                        commandText = reader.ReadCommand();
                    }
                }
            }
        }

        private void RunCommand(string commandText, DataSet dataSet, bool ignoreDDLErrors)
        {
            using (SQLiteCommand cmd = new SQLiteCommand())
            {
                cmd.CommandText = commandText;
                cmd.Connection = cn;

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
                            schemaHasChanged = true;
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
                            if (ignoreDDLErrors && execute == CommandExecute.NonQuerySchemaChanged)
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

        public System.Data.DataSet GetSchemaDataSet(List<string> tables)
        {
            DataSet schemaSet = null;
            try
            {
                schemaSet = new DataSet();
                using (SQLiteDataAdapter adapter = new SQLiteDataAdapter())
                {
                    using (var cmd = new SQLiteCommand())
                    {
                        cmd.Connection = cn;
                        foreach (var table in tables)
                        {
                            string strSQL = "SELECT * FROM [" + table + "] WHERE 1 = 0";

                            using (var cmdSet = new SQLiteCommand(strSQL, cn))
                            {
                                using (var adapter1 = new SQLiteDataAdapter(cmdSet))
                                {
                                    adapter1.FillSchema(schemaSet, SchemaType.Source, table);

                                    //Fill the table in the dataset 
                                    cmd.CommandText = strSQL;
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
            if (cmd != null)
            {
                cmd.Dispose();
            }
            if (cn != null)
            {
                cn.Close();
                cn = null;
            }
        }

        #endregion

        private static SqLiteDbTypeMap _typeNames = null;
        internal const DbType FallbackDefaultDbType = DbType.Object;

        public Type GetClrTypeFromDataType(string typeName)
        {
            var dbType = TypeNameToDbType(typeName);
            return DbTypeToType(dbType);
        }

        /// <summary>
        /// For a given type name, return a closest-match .NET type
        /// </summary>
        /// <param name="connection">The connection context for custom type mappings, if any.</param>
        /// <param name="name">The name of the type to match</param>
        /// <param name="flags">The flags associated with the parent connection object.</param>
        /// <returns>The .NET DBType the text evaluates to.</returns>
        internal DbType TypeNameToDbType(
            string name
            )
        {
            if (_typeNames == null)
                _typeNames = GetSQLiteDbTypeMap();

            if (name != null)
            {
                SqLiteDbTypeMapping value;

                if (_typeNames.TryGetValue(name, out value))
                {
                    return value.dataType;
                }
                else
                {
                    int index = name.IndexOf('(');

                    if ((index > 0) &&
                        _typeNames.TryGetValue(name.Substring(0, index).TrimEnd(), out value))
                    {
                        return value.dataType;
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

                result += base.Count;
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

                if (item.typeName == null)
                    throw new ArgumentException("item type name cannot be null");

                base.Add(item.typeName, item);

                if (item.primary)
                    reverse.Add(item.dataType, item);
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
                typeName = newTypeName;
                dataType = newDataType;
                primary = newPrimary;
            }

            internal string typeName;
            internal DbType dataType;
            internal bool primary;
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

        private static SqLiteDbTypeMap GetSQLiteDbTypeMap()
        {
            return new SqLiteDbTypeMap(new SqLiteDbTypeMapping[] {
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
    }
}
