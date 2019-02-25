using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using QuickGraph.Algorithms;
using QuickGraph.Data;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace ErikEJ.SqlCeScripting
{
    /// <summary>
    /// Class for generating scripts
    /// Use the GeneratedScript property to get the resulting script
    /// </summary>
#if V40
    public class Generator4 : IGenerator
#else
    public class Generator : IGenerator
#endif
    {
        private String _outFile;
        private IRepository _repository;
        private StringBuilder _sbScript;
        private String _sep = "GO" + Environment.NewLine;
        private List<string> _tableNames;
        private List<string> _whereClauses; // must be of the same length as _tableNames
        private Int32 _fileCounter = -1;
        private List<Column> _allColumns;
        private List<Constraint> _allForeignKeys;
        private List<PrimaryKey> _allPrimaryKeys;
        private List<Index> _allIndexes;
        private List<View> _allViews;
        private List<Trigger> _allTriggers;
        private bool _batchForAzure;
        private bool _sqlite;
        private bool _preserveDateAndDateTime2;
        private bool _truncateSqLiteStrings;

#if V40
        /// <summary>
        /// Initializes a new instance of the <see cref="Generator4"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="outFile">The out file.</param>
        public Generator4(IRepository repository, string outFile)
#else
        /// <summary>
        /// Initializes a new instance of the <see cref="Generator"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="outFile">The out file.</param>
        public Generator(IRepository repository, string outFile)
#endif
        {
            if (string.IsNullOrEmpty(outFile))
            {
                throw new ArgumentNullException(nameof(outFile));
            }
            Init(repository, outFile);
        }

#if V40
        public Generator4(IRepository repository)
#else
        public Generator(IRepository repository)
#endif
        {
            Init(repository, null);
        }

#if V40
        public Generator4(IRepository repository, string outFile, bool azure, bool preserveSqlDates, bool sqlite = false)
#else

        public Generator(IRepository repository, string outFile, bool azure, bool preserveSqlDates, bool sqlite = false)
#endif
        {
            _batchForAzure = azure;
            _sqlite = sqlite;
            if (sqlite)
                _sep = string.Empty;
            _preserveDateAndDateTime2 = preserveSqlDates;
            Init(repository, outFile);
        }

        private void Init(IRepository repository, string outFile)
        {
            _outFile = outFile;
            _repository = repository;
            _sbScript = new StringBuilder(10485760);
            _tableNames = _repository.GetAllTableNames();
            _allColumns = _repository.GetAllColumns();
            _allForeignKeys = repository.GetAllForeignKeys();
            _allPrimaryKeys = repository.GetAllPrimaryKeys();
            _allTriggers = repository.GetAllTriggers();
            _allViews = repository.GetAllViews();
            if (!repository.IsServer())
                _allIndexes = repository.GetAllIndexes();

            string scriptEngineBuild = AssemblyFileVersion;

            if (_repository.IsServer())
            {
                // Check if datatypes are supported when exporting from server
                // Either they can be converted, are supported, or an exception is thrown (if not supported)
                // Currently only sql_variant is not supported
                foreach (Column col in _allColumns)
                {
                    col.CharacterMaxLength = Helper.CheckDateColumnLength(col.DataType, col);
                    col.DateFormat = Helper.CheckDateFormat(col.DataType);

                    // Check if the current column points to a unique identity column,
                    // as the two columns' datatypes must match
                    bool refToIdentity = false;
                    Dictionary<string, Constraint> columnForeignKeys = new Dictionary<string, Constraint>();

                    // Fix for multiple constraints with same columns
                    var tableKeys = _allForeignKeys.Where(c => c.ConstraintTableName == col.TableName);
                    foreach (var constraint in tableKeys)
                    {
                        if (!columnForeignKeys.ContainsKey(constraint.Columns.ToString()))
                        {
                            columnForeignKeys.Add(constraint.Columns.ToString(), constraint);
                        }
                    }

                    if (columnForeignKeys.ContainsKey(string.Format("[{0}]", col.ColumnName)))
                    {
                        var refCol = _allColumns.Where(c => c.TableName == columnForeignKeys[string.Format("[{0}]", col.ColumnName)].UniqueConstraintTableName
                            && string.Format("[{0}]", c.ColumnName) == columnForeignKeys[string.Format("[{0}]", col.ColumnName)].UniqueColumnName).FirstOrDefault();
                        if (refCol != null && refCol.AutoIncrementBy > 0)
                        {
                            refToIdentity = true;
                        }
                    }
                    col.ServerDataType = col.DataType;
                    // This modifies the datatype to be SQL Compact compatible
                    col.DataType = Helper.CheckDataType(col.DataType, col, refToIdentity, _preserveDateAndDateTime2);
                }
            }
            _sbScript.AppendFormat("-- Script Date: {0} {1}  - ErikEJ.SqlCeScripting version {2}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), scriptEngineBuild);
            _sbScript.AppendLine();
            if (!string.IsNullOrEmpty(_outFile) && !_repository.IsServer())
            {
                GenerateDatabaseInfo();
            }
        }

        public void ExcludeTables(IList<string> tablesToExclude)
        {
            var allTables = _repository.GetAllTableNamesForExclusion();
            foreach (string tableToExclude in tablesToExclude)
            {
                allTables.Remove(tableToExclude);
            }
            FinalizeTableNames(allTables, Enumerable.Empty<TableParameter>());
        }

        public void IncludeTables(IList<string> tablesToInclude, IList<string> whereClauses)
        {
            if (tablesToInclude.Count != whereClauses.Count)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Numbers of elements in {0} and {1} do not match", nameof(tablesToInclude), nameof(whereClauses)));
            }
            var tablesAndWhereClauses = tablesToInclude
                .Zip(whereClauses, (table, whereClause) => new TableParameter(table, whereClause))
                .ToArray();

            var allTables = _repository.GetAllTableNamesForExclusion(); // Probably need to add another method for inclusion or rename the existing one
            allTables = allTables.Where(tableName => tablesToInclude.Contains(tableName)).ToList();
            FinalizeTableNames(allTables, tablesAndWhereClauses);
        }

        private void FinalizeTableNames(IList<string> tablesNamesToAssign, IEnumerable<TableParameter> tableParameters)
        {
            var finalTables = new List<string>();
            _whereClauses = new List<string>();
            foreach (string table in tablesNamesToAssign)
            {
                var localName = GetLocalName(table);
                finalTables.Add(localName);
                var tableParam = tableParameters.FirstOrDefault(x => x.TableName == table);
                if (tableParam != null)
                {
                    tableParam.TableName = localName;
                    _whereClauses.Add(tableParam.WhereClause);
                }
                else
                {
                    _whereClauses.Add(null);
                }
            }
            _tableNames = finalTables;
            try
            {
                var sortedTables = new List<string>();
                var whereClauses = new List<string>();
                var g = FillSchemaDataSet(finalTables).ToGraph();
                foreach (var table in g.TopologicalSort())
                {
                    sortedTables.Add(table.TableName);
                    var tableParam = tableParameters.FirstOrDefault(x => x.TableName == table.TableName);
                    if (tableParam != null)
                    {
                        whereClauses.Add(tableParam.WhereClause);
                    }
                    else
                    {
                        whereClauses.Add(null);
                    }
                }
                _tableNames = sortedTables;
                _whereClauses = whereClauses;
            }
            catch (QuickGraph.NonAcyclicGraphException)
            {
                _sbScript.AppendLine("-- Warning - circular reference preventing proper sorting of tables");
            }
        }

        /// <summary>
        /// SQLite allows strings longer than the defined size to be stored. Enabling this option will truncate strings that are longer than the defined size, and log any truncations to: %temp%\SQLiteTruncates.log
        /// </summary>
        public bool TruncateSQLiteStrings
        {
            get { return _truncateSqLiteStrings; }

            set
            {
                _truncateSqLiteStrings = value;
                if (_truncateSqLiteStrings)
                {
                    if (File.Exists(TruncateLogFileName()))
                    {
                        File.Delete(TruncateLogFileName());
                    }
                }
            }
        }

        public string ScriptDatabaseToFile(Scope scope)
        {
            Helper.FinalFiles = _outFile;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            switch (scope)
            {
                case Scope.Schema:
                    GenerateAllAndSave(false, false, false, false);
                    break;
                case Scope.SchemaData:
                    GenerateAllAndSave(true, false, false, false);
                    break;
                case Scope.SchemaDataBlobs:
                    GenerateAllAndSave(true, true, false, false);
                    break;
                case Scope.SchemaDataAzure:
                    _batchForAzure = true;
                    GenerateAllAndSave(true, false, false, false);
                    break;
                case Scope.DataOnly:
                    GenerateAllAndSave(true, false, true, false);
                    break;
                case Scope.DataOnlyForSqlServer:
                    GenerateAllAndSave(true, false, true, true);
                    break;
                case Scope.DataOnlyForSqlServerIgnoreIdentity:
                    GenerateAllAndSave(true, false, true, true, true);
                    break;
                case Scope.SchemaDataSQLite:
                    _sqlite = true;
                    _sep = string.Empty;
                    GenerateAllAndSave(true, false, false, false);
                    break;
                case Scope.SchemaSQLite:
                    _sqlite = true;
                    _sep = string.Empty;
                    GenerateAllAndSave(false, false, false, false);
                    break;
            }
            sw.Stop();
            return string.Format(CultureInfo.InvariantCulture, "Sent script to output file(s) : {0} in {1} ms", Helper.FinalFiles, (sw.ElapsedMilliseconds).ToString(CultureInfo.CurrentCulture));
        }

        /// <summary>
        /// Generates the table script.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        public void GenerateTableScript(string tableName)
        {
            GenerateTableCreate(tableName, false);
            if (!_sqlite)
            {
                GeneratePrimaryKeys(tableName);
            }
            GenerateIndex(tableName);
            if (!_sqlite)
            {
                GenerateForeignKeys(tableName);
            }
            if (_sqlite)
            {
                var triggers = _allTriggers.Where(t => t.TableName == tableName).ToList();
                foreach (var trigger in triggers)
                {
                    GenerateTrigger(trigger);
                }
            }
        }

        /// <summary>
        /// Generates the table data.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="saveImageFiles">if set to <c>true</c> [save image files].</param>
        /// <returns></returns>
        public string GenerateTableData(string tableName, bool saveImageFiles)
        {
            GenerateTableContent(tableName, saveImageFiles);
            return GeneratedScript;
        }

        /// <summary>
        /// Gets the generated script, and clears what has been generated so far
        /// </summary>
        /// <value>The generated script.</value>
        public string GeneratedScript
        {
            get
            {
                var script = _sbScript.ToString();
                _sbScript.Clear();
                return script;
            }
        }

        /// <summary>
        /// Generates the content of the table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="saveImageFiles">if set to <c>true</c> [save image files].</param>
        /// <param name="ignoreIdentity"></param>
        /// <param name="whereClause"></param>
        public void GenerateTableContent(string tableName, bool saveImageFiles, bool ignoreIdentity = false, string whereClause = null)
        {
            if (saveImageFiles && string.IsNullOrEmpty(_outFile))
            {
                throw new ArgumentNullException(nameof(saveImageFiles), "outFile must be specified in the Generator constructor when using saveImageFiles");
            }

            var identityOrdinal = _repository.GetIdentityOrdinal(tableName);
            var hasIdentity = (identityOrdinal > -1);
            if (ignoreIdentity)
            {
                hasIdentity = false;
            }
            var unicodePrefix = "N";
            if (_sqlite)
                unicodePrefix = string.Empty;
            // Skip rowversion column
            var rowVersionOrdinal = _repository.GetRowVersionOrdinal(tableName);
            if (_sqlite) rowVersionOrdinal = -1;
            var columns = _allColumns.Where(c => c.TableName == tableName).OrderBy(c => c.Ordinal).ToList();
            var nvarcharSizes = columns.Where(x => x.DataType == "nvarchar" && x.CharacterMaxLength > 0)
                .ToDictionary(x => x.ColumnName, x => x.CharacterMaxLength);
            using (var rdr = _repository.GetDataFromReader(tableName, columns, _allPrimaryKeys.Where(p => p.TableName == tableName).ToList(), whereClause))
            {
                var firstRun = true;
                var rowCount = 0;
                var fields = new List<string>();
                for (var iColumn = 0; iColumn < rdr.FieldCount; iColumn++)
                {
                    fields.Add(rdr.GetName(iColumn));
                }
                var scriptPrefix = GetInsertScriptPrefix(tableName, fields, rowVersionOrdinal, identityOrdinal, ignoreIdentity);

                var prefix = "{ts '";
                var postfix = "'}";
                if (_sqlite)
                {
                    prefix = "'";
                    postfix = "'";
                }

                while (rdr.Read())
                {
                    if (firstRun)
                    {
#if V31
#else
                        if (hasIdentity && !_sqlite)
                        {
                            _sbScript.Append(string.Format(CultureInfo.InvariantCulture, "SET IDENTITY_INSERT [{0}] ON;", tableName));
                            _sbScript.Append(Environment.NewLine);
                            _sbScript.Append(_sep);
                            firstRun = false;
                        }
#endif
                    }
                    _sbScript.Append(scriptPrefix);
                    _sbScript.Append(Environment.NewLine);

                    for (var iColumn = 0; iColumn < rdr.FieldCount; iColumn++)
                    {
                        var fieldType = rdr.GetFieldType(iColumn);
                        //Skip rowversion column
                        if (rowVersionOrdinal == iColumn
                            || rdr.GetName(iColumn).StartsWith("__sys", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                        if (ignoreIdentity && (identityOrdinal == iColumn))
                        {
                            continue;
                        }
                        if (rdr.IsDBNull(iColumn))
                        {
                            _sbScript.Append("NULL");
                        }
                        else if (fieldType == typeof(string))
                        {
                            var stringValue = rdr.GetString(iColumn).Replace("'", "''");
                            if (TruncateSQLiteStrings && !string.IsNullOrEmpty(stringValue))
                            {
                                int size;
                                string colName = rdr.GetName(iColumn);
                                if (nvarcharSizes.TryGetValue(colName, out size))
                                {
                                    if (stringValue.Length > size)
                                    {
                                        using (var sw = File.AppendText(TruncateLogFileName()))
                                        {
                                            sw.Write(DateTime.Now.ToString("O"));
                                            sw.Write(";");
                                            sw.Write(tableName);
                                            sw.Write(";");
                                            sw.Write(colName);
                                            sw.Write(";");
                                            sw.WriteLine(stringValue);
                                        }
                                        stringValue = stringValue.Substring(0, size);
                                    }
                                }
                            }
                            _sbScript.AppendFormat("{0}'{1}'", unicodePrefix, stringValue);
                        }
                        else if (fieldType == typeof(DateTime))
                        {
                            // see http://msdn.microsoft.com/en-us/library/ms180878.aspx#BackwardCompatibilityforDownlevelClients
                            var column = _allColumns.Single(c => c.TableName == tableName && c.ColumnName == rdr.GetName(iColumn));
                            //Being careful here due to issues with the sqlite ADO.NET provider
                            DateTime? date;
                            try
                            {
                                date = rdr.GetDateTime(iColumn);
                            }
                            catch (FormatException ex)
                            {
                                throw new Exception(ex.Message + " - Value: '" + rdr.GetString(iColumn) + "' in " + tableName + ":" + column.ColumnName);
                            }
                            var format = column.DateFormat;
                            //Work item: 17681
                            if (!_preserveDateAndDateTime2)
                                format = DateFormat.DateTime;
                            switch (format)
                            {
                                case DateFormat.None:
                                    //Datetime globalization - ODBC escape: {ts '2004-03-29 19:21:00'}
                                    _sbScript.Append(prefix);
                                    _sbScript.Append(date.Value.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture));
                                    _sbScript.Append(postfix);
                                    break;
                                case DateFormat.DateTime:
                                    // sqlite: '2007-01-01 00:00:00'
                                    //Datetime globalization - ODBC escape: {ts '2004-03-29 19:21:00'}
                                    _sbScript.Append(prefix);
                                    _sbScript.Append(date.Value.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture));
                                    _sbScript.Append(postfix);
                                    break;
                                case DateFormat.Date:
                                    _sbScript.Append(unicodePrefix + "'");
                                    _sbScript.Append(date.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                                    _sbScript.Append("'");
                                    break;
                                case DateFormat.DateTime2:
                                    _sbScript.Append(unicodePrefix + "'");
                                    _sbScript.Append(date.Value.ToString("yyyy-MM-dd HH:mm:ss.fffffff", CultureInfo.InvariantCulture));
                                    _sbScript.Append("'");
                                    break;
                            }
                        }
                        else if (fieldType == typeof(DateTimeOffset))
                        {
                            var dto = (DateTimeOffset)rdr.GetValue(iColumn);
                            if (_preserveDateAndDateTime2)
                            {
                                _sbScript.Append(unicodePrefix + "'");
                                _sbScript.Append(dto.ToString("yyyy-MM-dd HH:mm:ss.fffffff zzz", CultureInfo.InvariantCulture));
                                _sbScript.Append("'");
                            }
                            else
                            {
                                _sbScript.Append(prefix);
                                _sbScript.Append(dto.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture));
                                _sbScript.Append(postfix);
                            }
                        }
                        else if (fieldType == typeof(TimeSpan))
                        {
                            var ts = (TimeSpan)rdr.GetValue(iColumn);
                            _sbScript.Append(unicodePrefix + "'");
                            _sbScript.Append(ts.ToString());
                            _sbScript.Append("'");
                        }
                        else if (fieldType == typeof(byte[]))
                        {
                            var buffer = (byte[])rdr.GetValue(iColumn);
                            if (saveImageFiles)
                            {
                                var id = Guid.NewGuid().ToString("N") + ".blob";
                                _sbScript.AppendFormat("SqlCeCmd_LoadImage({0})", id);
                                FileStream fs = null;
                                BinaryWriter bw = null;
                                try
                                {
                                    fs = File.Open(Path.Combine(Path.GetDirectoryName(_outFile), id), FileMode.Create);
                                    bw = new BinaryWriter(fs);
                                    fs = null;
                                    bw.Write(buffer, 0, buffer.Length);
                                }
                                finally
                                {
                                    bw?.Close();
                                    fs?.Close();
                                }
                            }
                            else
                            {
                                _sbScript.Append(_sqlite ? "X'" : "0x");
                                for (var i = 0; i < buffer.Length; i++)
                                {
                                    _sbScript.Append(buffer[i].ToString("X2", CultureInfo.InvariantCulture));
                                }
                                if (_sqlite)
                                {
                                    _sbScript.Append("'");
                                }
                            }
                        }
                        else if (fieldType == typeof(double))
                        {
                            var intString = rdr.GetDouble(iColumn).ToString("R", CultureInfo.InvariantCulture);
                            _sbScript.Append(intString);
                        }
                        else if (fieldType == typeof(float))
                        {
                            var intString = rdr.GetFloat(iColumn).ToString("R", CultureInfo.InvariantCulture);
                            _sbScript.Append(intString);
                        }
                        else if (fieldType == typeof(byte) || fieldType == typeof(short) || fieldType == typeof(int) ||
                            fieldType == typeof(long) || fieldType == typeof(decimal))
                        {
                            var intString = Convert.ToString(rdr.GetValue(iColumn), CultureInfo.InvariantCulture);
                            _sbScript.Append(intString);
                        }
                        else if (fieldType == typeof(bool))
                        {
                            var boolVal = (bool)rdr.GetValue(iColumn);
                            if (boolVal)
                            { _sbScript.Append("1"); }
                            else
                            { _sbScript.Append("0"); }
                        }
                        else
                        {
                            //Decimal point globalization
                            var value = Convert.ToString(rdr.GetValue(iColumn), CultureInfo.InvariantCulture);
                            _sbScript.AppendFormat("'{0}'", value.Replace("'", "''"));
                        }
                        _sbScript.Append(",");
                    }
                    // remove trailing comma
                    _sbScript.Remove(_sbScript.Length - 1, 1);

                    _sbScript.Append(");");
                    _sbScript.Append(Environment.NewLine);
                    if (_batchForAzure && ((rowCount + 1) % 1000) == 0)
                    {
                        _sbScript.Append(_sep);
                    }
                    else if (!_batchForAzure)
                    {
                        _sbScript.Append(_sep);
                    }
                    // Split large output!
                    if (_sbScript.Length > 9485760 && !string.IsNullOrEmpty(_outFile))
                    {
                        if (_batchForAzure)
                        {
                            _sbScript.Append(_sep);
                        }
#if V31
#else
                        if (hasIdentity && !_sqlite)
                        {
                            _sbScript.Append(string.Format(CultureInfo.InvariantCulture, "SET IDENTITY_INSERT [{0}] OFF;", tableName));
                            _sbScript.Append(Environment.NewLine);
                            _sbScript.Append(_sep);
                        }
                        if (_sqlite)
                        {
                            GenerateSqliteSuffix();
                        }
#endif
                        _fileCounter++;
                        Helper.WriteIntoFile(_sbScript.ToString(), _outFile, _fileCounter, _sqlite);
                        _sbScript.Remove(0, _sbScript.Length);
#if V31
#else
                        if (hasIdentity && !_sqlite)
                        {
                            _sbScript.Append(string.Format(CultureInfo.InvariantCulture, "SET IDENTITY_INSERT [{0}] ON;", tableName));
                            _sbScript.Append(Environment.NewLine);
                            _sbScript.Append(_sep);
                        }
                        if (_sqlite)
                        {
                            GenerateSqlitePrefix();
                        }
#endif
                    }
                    rowCount++;
                }
                if (_batchForAzure)
                {
                    _sbScript.Append(_sep);
                }
#if V31
#else
                if (hasIdentity && !_sqlite)
                {
                    _sbScript.Append(string.Format(CultureInfo.InvariantCulture, "SET IDENTITY_INSERT [{0}] OFF;", tableName));
                    _sbScript.Append(Environment.NewLine);
                    _sbScript.Append(_sep);
                }
#endif
            }
        }

        public string GenerateInsertFromDataRow(string tableName, DataRow row)
        {
            return GenerateInserOrUpdate(tableName, true, row);
        }

        public string GenerateUpdateFromDataRow(string tableName, DataRow row)
        {
            return GenerateInserOrUpdate(tableName, false, row);
        }

        public void GenerateTableSelect(string tableName)
        {
            GenerateTableSelect(tableName, false);
        }

        /// <summary>
        /// Generates the table select statement.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="editableInSqlite"></param>
        public void GenerateTableSelect(string tableName, bool editableInSqlite)
        {
            List<Column> columns = _allColumns.Where(c => c.TableName == tableName).ToList();
            if (columns.Count > 0)
            {
                _sbScript.Append("SELECT ");

                columns.ForEach(delegate (Column col)
                {
                    if (_sqlite && col.DataType == "datetime" && editableInSqlite)
                    {
                        _sbScript.AppendFormat(CultureInfo.InvariantCulture,
                        "datetime([{0}]) AS [{0}]{1}      ,"
                        , col.ColumnName
                        , Environment.NewLine);
                    }
                    else
                    {
                        _sbScript.AppendFormat(CultureInfo.InvariantCulture,
                            "[{0}]{1}      ,"
                            , col.ColumnName
                            , Environment.NewLine);
                    }
                });

                // Remove the last comma and spaces
                _sbScript.Remove(_sbScript.Length - 7, 7);
                _sbScript.AppendFormat("  FROM [{0}];{1}", tableName, Environment.NewLine);
                _sbScript.Append(_sep);
            }
        }

        /// <summary>
        /// Generates the view select statement.
        /// </summary>
        /// <param name="viewName">Name of the view.</param>
        public void GenerateViewSelect(string viewName)
        {
            View view = _allViews.Where(c => c.ViewName == viewName).SingleOrDefault();
            if (view.ViewName != null)
            {
                _sbScript.Append(view.Select);
                _sbScript.Append(Environment.NewLine);
                _sbScript.Append(_sep);
            }
        }

        /// <summary>
        /// Generates the table insert statement.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        public void GenerateTableInsert(string tableName)
        {
            List<Column> columns = _allColumns.Where(c => c.TableName == tableName).ToList();
            if (columns.Count > 0)
            {
                _sbScript.AppendFormat(CultureInfo.InvariantCulture, "INSERT INTO [{0}]", tableName);
                _sbScript.AppendFormat(CultureInfo.InvariantCulture, Environment.NewLine);
                _sbScript.Append("           (");

                columns.ForEach(delegate (Column col)
                {
                    _sbScript.AppendFormat(CultureInfo.InvariantCulture,
                        "[{0}]{1}           ,"
                        , col.ColumnName
                        , Environment.NewLine);
                });

                // Remove the last comma
                _sbScript.Remove(_sbScript.Length - 14, 14);
                _sbScript.AppendFormat("){0}     VALUES{1}           (", Environment.NewLine, Environment.NewLine);
                columns.ForEach(delegate (Column col)
                {
                    _sbScript.AppendFormat(CultureInfo.InvariantCulture,
                        "<{0}, {1}>{2}           ,"
                        , col.ColumnName
                        , col.ShortType
                        , Environment.NewLine);
                });
                // Remove the last comma
                _sbScript.Remove(_sbScript.Length - 14, 14);
                _sbScript.AppendFormat(");{0}", Environment.NewLine);
                _sbScript.Append(_sep);
            }
        }

        /// <summary>
        /// Generates the table insert.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="fields">The fields.</param>
        /// <param name="values">The values.</param>
        /// <param name="lineNumber"></param>
        public void GenerateTableInsert(string tableName, IList<string> fields, IList<string> values, int lineNumber)
        {
            if (fields.Count != values.Count)
            {
                StringBuilder valueString = new StringBuilder();
                valueString.Append("Values:");
                valueString.Append(Environment.NewLine);
                foreach (string val in values)
                {
                    valueString.Append(val);
                    valueString.Append(Environment.NewLine);
                }
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Error on line {0} in the csv file. The number of values ({1}) and fields ({2}) do not match - {3}", lineNumber, values.Count, fields.Count, valueString));
            }
            List<Column> columns = _allColumns.Where(c => c.TableName == tableName).ToList();
            if (columns.Count > 0)
            {
                _sbScript.AppendFormat("INSERT INTO [{0}] (", tableName);
                foreach (string field in fields)
                {
                    _sbScript.AppendFormat(CultureInfo.InvariantCulture,
                        "[{0}],", field);
                }
                // Remove the last comma
                _sbScript.Remove(_sbScript.Length - 1, 1);
                _sbScript.Append(") ");
                _sbScript.Append(Environment.NewLine);
                _sbScript.Append("VALUES (");
                int i = 0;
                foreach (string value in values)
                {
                    _sbScript.Append(SqlFormatValue(tableName, fields[i], value));
                    _sbScript.Append(",");
                    i++;
                }
                // Remove the last comma
                _sbScript.Remove(_sbScript.Length - 1, 1);
                _sbScript.AppendFormat(");{0}", Environment.NewLine);
                _sbScript.Append(_sep);
            }
        }

        public string SqlFormatValue(string tableName, string fieldName, string value)
        {
            StringBuilder sbScript = new StringBuilder();
            string unicodePrefix = "N";
            if (_sqlite)
                unicodePrefix = string.Empty;
            Column column = _allColumns.Where(c => c.TableName == tableName && c.ColumnName.ToUpperInvariant() == fieldName.ToUpperInvariant()).SingleOrDefault();
            if (column == null)
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Could not find column {0} in table {1}", fieldName.ToLowerInvariant(), tableName));
            if (string.IsNullOrEmpty(value))
            {
                sbScript.Append("NULL");
            }
            //else if (value == string.Empty)
            //{
            //    _sbScript.Append("''");
            //}
            else if (column.DataType == "nchar" || column.DataType == "nvarchar" || column.DataType == "ntext")
            {
                sbScript.AppendFormat(unicodePrefix + "'{0}'", value.Replace("'", "''"));
            }
            else if (column.DataType == "datetime")
            {
                DateTime date = DateTime.Parse(value);
                //Datetime globalization - ODBC escape: {ts '2004-03-29 19:21:00'}
                if (!_sqlite) sbScript.Append("{ts ");
                sbScript.Append("'");
                sbScript.Append(date.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
                sbScript.Append("'");
                if (!_sqlite) sbScript.Append("}");
            }
            else if (column.DataType == "bigint"
                || column.DataType == "int"
                || column.DataType == "float"
                || column.DataType == "money"
                || column.DataType == "real"
                || column.DataType == "tinyint"
                || column.DataType == "numeric"
                || column.DataType == "tinyint"
                || column.DataType == "smallint")
            {
                string val = Convert.ToString(value, CultureInfo.InvariantCulture);
                sbScript.Append(val);
            }
            else if (column.DataType == "bit")
            {
                if (value == "0" || value == "1")
                {
                    sbScript.Append(value);
                }
                else
                {
                    bool boolVal = Boolean.Parse(value);
                    if (boolVal)
                    { sbScript.Append("1"); }
                    else
                    { sbScript.Append("0"); }
                }
            }
            else
            {
                string val = Convert.ToString(value, CultureInfo.InvariantCulture);
                sbScript.AppendFormat("'{0}'", val.Replace("'", "''"));
            }
            return sbScript.ToString();
        }

        public void AddIdentityInsert(string tableName)
        {
            bool hasIdentity = _repository.HasIdentityColumn(tableName);
            if (hasIdentity && !_sqlite)
            {
                _sbScript.Append(string.Format(CultureInfo.InvariantCulture, "SET IDENTITY_INSERT [{0}] ON;", tableName));
                _sbScript.Append(Environment.NewLine);
                _sbScript.Append(_sep);
            }
        }

        /// <summary>
        /// Validates the columns.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="columns">The columns.</param>
        /// <returns></returns>
        public bool ValidColumns(string tableName, IList<string> columns)
        {
            string wrongColumns = string.Empty;

            List<string> cols = (from a in _allColumns
                                 where a.TableName == tableName
                                 select a.ColumnName.ToUpperInvariant()).ToList();

            var upperColumns = columns.Select(i => i.ToUpperInvariant()).ToList();

            foreach (var name in upperColumns)
            {
                if (!cols.Contains(name))
                {
                    wrongColumns += name + " ";
                }
            }
            if (string.IsNullOrEmpty(wrongColumns))
            {
                return true;
            }
            else
            {
                _sbScript.Append("-- Cannot create script, one or more field names on first line are invalid:");
                _sbScript.Append("-- Wrong column names: " + wrongColumns);
                _sbScript.Append(Environment.NewLine);
                _sbScript.Append("-- Also check that correct separator is chosen");
                return false;
            }
        }

        /// <summary>
        /// Generates the table update statement.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        public void GenerateTableUpdate(string tableName)
        {
            List<Column> columns = _allColumns.Where(c => c.TableName == tableName).ToList();
            if (columns.Count > 0)
            {
                _sbScript.AppendFormat(CultureInfo.InvariantCulture, "UPDATE [{0}] ", tableName);
                _sbScript.AppendFormat(CultureInfo.InvariantCulture, Environment.NewLine);
                _sbScript.Append("   SET ");

                columns.ForEach(delegate (Column col)
                {
                    _sbScript.AppendFormat(CultureInfo.InvariantCulture,
                        "[{0}] = <{1}, {2}>{3}      ,"
                        , col.ColumnName
                        , col.ColumnName
                        , col.ShortType
                        , Environment.NewLine);
                });

                // Remove the last comma
                _sbScript.Remove(_sbScript.Length - 7, 7);
                _sbScript.AppendFormat(" WHERE <Search Conditions,,>;{0}", Environment.NewLine);
                _sbScript.Append(_sep);
            }
        }

        /// <summary>
        /// Generates the table delete statement.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        public void GenerateTableDelete(string tableName)
        {
            _sbScript.AppendFormat("DELETE FROM [{0}]{1}", tableName, Environment.NewLine);
            _sbScript.AppendFormat("WHERE <Search Conditions,,>;{0}", Environment.NewLine);
            _sbScript.Append(_sep);
        }

        /// <summary>
        /// Generates the table drop statement.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        public void GenerateTableDrop(string tableName)
        {
            _sbScript.AppendFormat("DROP TABLE [{0}];{1}", tableName, Environment.NewLine);
            _sbScript.Append(_sep);
        }

        public void GenerateSchemaGraph(string connectionString)
        {
            GenerateSchemaGraph(connectionString, true, true);
        }

        public void GenerateSchemaGraph(string connectionString, bool includeSystemTables, bool generateScripts)
        {
            GenerateSchemaGraph(connectionString, includeSystemTables, generateScripts, new List<string>());
        }

        public void GenerateSchemaGraph(string connectionString, IList<string> tablesToExclude)
        {
            GenerateSchemaGraph(connectionString, false, false, tablesToExclude);
        }

        /// <summary>
        /// Generates the schema graph.
        /// </summary>
        private void GenerateSchemaGraph(string connectionString, bool includeSystemTables, bool generateScripts, IList<string> tablesToExclude)
        {
            string dgmlFile = _outFile;
            using (var dgmlHelper = new DgmlHelper(dgmlFile))
            {
                string scriptExt = ".dgml.sqlce";
                if (_repository.IsServer())
                {
                    scriptExt = ".dgml.sql";
                }

                var descriptionHelper = new DescriptionHelper();

                List<DbDescription> descriptionCache = descriptionHelper.GetDescriptions(_repository);

                dgmlHelper.BeginElement("Nodes");
                var dbdesc = descriptionCache.Where(dc => dc.Parent == null && dc.Object == null).Select(dc => dc.Description).SingleOrDefault();
                dgmlHelper.WriteNode("Database", connectionString, null, "Database", "Expanded", dbdesc);

                var serverTableNames = _repository.GetAllTableNamesForExclusion();
                List<string> schemas = new List<string>();
                if (_repository.IsServer())
                {
                    foreach (string tableToExclude in tablesToExclude)
                    {
                        serverTableNames.Remove(tableToExclude);
                    }
                    _tableNames = serverTableNames;
                    foreach (var table in serverTableNames)
                    {
                        string[] split = table.Split('.');
                        if (!schemas.Contains(split[0]))
                            schemas.Add(split[0]);
                    }
                    foreach (var schema in schemas)
                    {
                        dgmlHelper.WriteNode(schema, schema, null, "Schema", "Expanded", null);
                    }
                }

                foreach (string table in _tableNames)
                {
                    if (!includeSystemTables && table.StartsWith("__"))
                        continue;
                    //Create individual scripts per table
                    if (generateScripts)
                    {
                        _sbScript.Remove(0, _sbScript.Length);
                        GenerateTableScript(table);
                        string tableScriptPath = Path.Combine(Path.GetDirectoryName(dgmlFile), table + scriptExt);
                        File.WriteAllText(tableScriptPath, GeneratedScript);
                    }
                    // Create Nodes
                    var desc = descriptionCache.Where(dc => dc.Parent == null && dc.Object == table).Select(dc => dc.Description).SingleOrDefault();
                    if (generateScripts)
                    {
                        dgmlHelper.WriteNode(table, table, table + scriptExt, "Table", "Collapsed", desc);
                    }
                    else
                    {
                        dgmlHelper.WriteNode(table, table, null, "Table", "Collapsed", desc);
                    }
                    List<Column> columns = _allColumns.Where(c => c.TableName == table).ToList();
                    foreach (Column col in columns)
                    {

                        string shortType = col.ShortType.Remove(col.ShortType.Length - 1);

                        string category = "Field";
                        if (col.IsNullable == YesNoOption.YES)
                            category = "Field Optional";

                        // Fix for multiple constraints with same columns
                        Dictionary<string, Constraint> columnForeignKeys = new Dictionary<string, Constraint>();

                        var tableKeys = _allForeignKeys.Where(c => c.ConstraintTableName == col.TableName);
                        foreach (var constraint in tableKeys)
                        {
                            if (!columnForeignKeys.ContainsKey(constraint.Columns.ToString()))
                            {
                                columnForeignKeys.Add(constraint.Columns.ToString(), constraint);
                            }
                        }

                        if (columnForeignKeys.ContainsKey(string.Format("[{0}]", col.ColumnName)))
                        {
                            category = "Field Foreign";
                        }

                        List<PrimaryKey> primaryKeys = _allPrimaryKeys.Where(p => p.TableName == table).ToList();
                        if (primaryKeys.Count > 0)
                        {
                            var keys = (from k in primaryKeys
                                        where k.ColumnName == col.ColumnName
                                        select k.ColumnName).SingleOrDefault();
                            if (!string.IsNullOrEmpty(keys))
                                category = "Field Primary";

                        }
                        var colDesc = descriptionCache.Where(dc => dc.Parent == table && dc.Object == col.ColumnName).Select(dc => dc.Description).SingleOrDefault();
                        if (!string.IsNullOrEmpty(colDesc))
                            shortType = shortType + Environment.NewLine + colDesc;
                        dgmlHelper.WriteNode(string.Format("{0}_{1}", table, col.ColumnName), col.ColumnName, null, category, null, shortType);
                    }
                }
                dgmlHelper.EndElement();

                dgmlHelper.BeginElement("Links");
                foreach (var schema in schemas)
                {
                    dgmlHelper.WriteLink("Database", schema, null, "Contains");
                }
                foreach (string table in _tableNames)
                {
                    if (!includeSystemTables && table.StartsWith("__"))
                        continue;

                    if (_repository.IsServer())
                    {
                        var split = table.Split('.');
                        dgmlHelper.WriteLink(split[0], table, null, "Contains");
                    }
                    else
                    {
                        dgmlHelper.WriteLink("Database", table, null, "Contains");
                    }

                    List<Column> columns = _allColumns.Where(c => c.TableName == table).ToList();
                    foreach (Column col in columns)
                    {
                        dgmlHelper.WriteLink(table, string.Format("{0}_{1}", table, col.ColumnName),
                            null, "Contains");
                    }

                    List<Constraint> foreignKeys = _allForeignKeys.Where(c => c.ConstraintTableName == table).ToList();
                    foreach (Constraint key in foreignKeys)
                    {
                        var col = key.Columns[0];
                        col = RemoveBrackets(col);
                        var uniqueCol = key.UniqueColumns[0];
                        uniqueCol = RemoveBrackets(uniqueCol);
                        string source = string.Format("{0}_{1}", table, col);
                        string target = string.Format("{0}_{1}", key.UniqueConstraintTableName, uniqueCol);
                        dgmlHelper.WriteLink(source, target, key.ConstraintName, "Foreign Key");
                    }
                }
                dgmlHelper.EndElement();

                //Close the DGML document
                dgmlHelper.Close();
            }
        }

        public void GeneratePrimaryKeys()
        {
            foreach (string tableName in _tableNames)
            {
                GeneratePrimaryKeys(tableName);
            }
        }

        /// <summary>
        /// Generates the primary keys.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        public void GeneratePrimaryKeys(string tableName)
        {
            List<PrimaryKey> primaryKeys = _allPrimaryKeys.Where(p => p.TableName == tableName).ToList();

            //_repository.GetPrimaryKeysFromTable(tableName);

            if (primaryKeys.Count > 0)
            {
                if (_sqlite)
                {
                    if (primaryKeys.Count == 1)
                    {
                        Column column = _allColumns.Single(c => c.TableName == tableName && c.ColumnName == primaryKeys[0].ColumnName);
                        if (column.DataType == "INTEGER PRIMARY KEY AUTOINCREMENT")
                        {
                            // primary key already defined in the column line
                            return;
                        }
                    }

                    _sbScript.AppendFormat("{0}, CONSTRAINT [{1}] PRIMARY KEY (", Environment.NewLine, primaryKeys[0].KeyName);
                }
                else
                {
                    _sbScript.AppendFormat("ALTER TABLE [{0}] ADD CONSTRAINT [{1}] PRIMARY KEY (", tableName, primaryKeys[0].KeyName);
                }
                primaryKeys.ForEach(delegate (PrimaryKey column)
                {
                    _sbScript.AppendFormat("[{0}]", column.ColumnName);
                    _sbScript.Append(",");
                });

                // Remove the last comma
                _sbScript.Remove(_sbScript.Length - 1, 1);
                if (!_sqlite)
                {
                    _sbScript.AppendFormat(");{0}", Environment.NewLine);
                    _sbScript.Append(_sep);
                }
                else
                {
                    _sbScript.Append(")");
                }
            }
            else if (_batchForAzure)
            {
                _sbScript.AppendFormat("PRINT N'** Warning: Table [{0}] does not have a primary clustered key - it cannot be migrated to SQL Azure';{1}", tableName, Environment.NewLine);
            }
        }

        internal void GenerateForeignKeys()
        {
            foreach (string tableName in _tableNames)
            {
                GenerateForeignKeys(tableName);
            }
        }

        /// <summary>
        /// Generates the foreign keys.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        public void GenerateForeignKeys(string tableName)
        {
            List<Constraint> foreignKeys = _allForeignKeys.Where(fk => fk.ConstraintTableName == tableName).ToList();

            foreach (Constraint constraint in foreignKeys)
            {
                if (_sqlite)
                {
                    _sbScript.AppendFormat(CultureInfo.InvariantCulture, "{0}, CONSTRAINT [{1}] FOREIGN KEY ({2}) REFERENCES [{3}] ({4}) ON DELETE {5} ON UPDATE {6}"
                        , Environment.NewLine
                        , constraint.ConstraintName
                        , constraint.Columns
                        , constraint.UniqueConstraintTableName
                        , constraint.UniqueColumns
                        , constraint.DeleteRule
                        , constraint.UpdateRule);
                }
                else
                {
                    _sbScript.AppendFormat(CultureInfo.InvariantCulture, "ALTER TABLE [{0}] ADD CONSTRAINT [{1}] FOREIGN KEY ({2}) REFERENCES [{3}]({4}) ON DELETE {5} ON UPDATE {6};{7}"
                        , constraint.ConstraintTableName
                        , constraint.ConstraintName
                        , constraint.Columns
                        , constraint.UniqueConstraintTableName
                        , constraint.UniqueColumns
                        , constraint.DeleteRule
                        , constraint.UpdateRule
                        , Environment.NewLine);
                    _sbScript.Append(_sep);
                }
            }
        }

        /// <summary>
        /// Generate index create statement for each user table
        /// </summary>
        /// <returns></returns>
        internal void GenerateIndex()
        {
            foreach (string tableName in _tableNames)
            {
                GenerateIndex(tableName);
            }
        }

        /// <summary>
        /// Generates the index script.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="indexName">Name of the index.</param>
        public void GenerateIndexScript(string tableName, string indexName)
        {
            GenerateSingleIndex(tableName, indexName);
        }

        /// <summary>
        /// Generates the index drop statement.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="indexName">Name of the index.</param>
        public void GenerateIndexDrop(string tableName, string indexName)
        {
            var tableIndexes = _repository.IsServer()
                ? _repository.GetIndexesFromTable(tableName)
                : _allIndexes.Where(i => i.TableName == tableName).ToList();

            var indexesByName = tableIndexes
                .Where(i => i.IndexName == indexName)
                .OrderBy(i => i.OrdinalPosition);

            if (indexesByName.Any())
            {
                if (_sqlite)
                {
                    _sbScript.AppendFormat("DROP INDEX [{0}];{1}", indexName, Environment.NewLine);
                }
                else
                {
                    _sbScript.AppendFormat("DROP INDEX [{0}].[{1}];{2}", tableName, indexName, Environment.NewLine);
                }
                _sbScript.Append(_sep);
            }
            else
            {
                _sbScript.AppendFormat("ALTER TABLE [{0}] DROP CONSTRAINT [{1}];{2}", tableName, indexName, Environment.NewLine);
                _sbScript.Append(_sep);
            }
        }

        public void GenerateIndexOnlyDrop(string tableName, string indexName)
        {
            _sbScript.AppendFormat("DROP INDEX [{0}].[{1}];{2}", tableName, indexName, Environment.NewLine);
            _sbScript.Append(_sep);
        }

        /// <summary>
        /// Generates the index statistics.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="indexName">Name of the index.</param>
        public void GenerateIndexStatistics(string tableName, string indexName)
        {
            _sbScript.AppendFormat("sp_show_statistics '{0}', '{1}';{2}", tableName, indexName, Environment.NewLine);
            _sbScript.Append(_sep);
            _sbScript.AppendFormat("sp_show_statistics_columns '{0}', '{1}';{2}", tableName, indexName, Environment.NewLine);
            _sbScript.Append(_sep);
            _sbScript.AppendFormat("sp_show_statistics_steps '{0}', '{1}';{2}", tableName, indexName, Environment.NewLine);
            _sbScript.Append(_sep);
        }

        /// <summary>
        /// Generates the index create statement.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        private void GenerateIndex(string tableName)
        {
            List<Index> tableIndexes;
            if (_repository.IsServer())
            {
                tableIndexes = _repository.GetIndexesFromTable(tableName);
            }
            else
            {
                tableIndexes = _allIndexes.Where(i => i.TableName == tableName).ToList();
            }

            if (tableIndexes.Count > 0)
            {
                IEnumerable<string> uniqueIndexNameList = tableIndexes.Select(i => i.IndexName).Distinct();

                foreach (string uniqueIndexName in uniqueIndexNameList)
                {
                    GenerateSingleIndex(tableName, uniqueIndexName);
                }
            }
        }


        /// <summary>
        /// Generates the table columns.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        public List<string> GenerateTableColumns(string tableName)
        {
            return (from a in _allColumns
                    where a.TableName == tableName
                    select a.ColumnName).ToList();
        }

        public void GenerateColumnAddScript(Column column)
        {
            if (column.IsNullable == YesNoOption.NO)
            {
                _sbScript.AppendLine("-- Adding as column with NOT NULL is not allowed, set a default value or allow NULL");
            }
            if (_sqlite)
            {
                _sbScript.Append(string.Format("ALTER TABLE [{0}] ADD COLUMN {1};{2}", column.TableName, GenerateColumLine(false, column, _batchForAzure), Environment.NewLine));
            }
            else
            {
                _sbScript.Append(string.Format("ALTER TABLE [{0}] ADD {1};{2}", column.TableName, GenerateColumLine(false, column, _batchForAzure), Environment.NewLine));
            }
            _sbScript.Append(_sep);
        }

        public void GenerateColumnDropScript(Column column)
        {
            _sbScript.Append(string.Format("ALTER TABLE [{0}] DROP COLUMN [{1}];{2}", column.TableName, column.ColumnName, Environment.NewLine));
            _sbScript.Append(_sep);
        }

        public void GenerateColumnAlterScript(Column column)
        {
            _sbScript.Append(string.Format("ALTER TABLE [{0}] ALTER COLUMN {1};{2}", column.TableName, GenerateColumLine(false, column, _batchForAzure), Environment.NewLine));
            _sbScript.Append(_sep);
        }

        public void GenerateColumnSetDefaultScript(Column column)
        {
            // ALTER TABLE MyCustomers ALTER COLUMN CompanyName SET DEFAULT 'A. Datum Corporation'
            _sbScript.Append(string.Format("ALTER TABLE [{0}] ALTER COLUMN [{1}] SET DEFAULT {2};{3}", column.TableName, column.ColumnName, column.ColumnDefault, Environment.NewLine));
            _sbScript.Append(_sep);
        }

        public void GenerateColumnDropDefaultScript(Column column)
        {
            //ALTER TABLE MyCustomers ALTER COLUMN CompanyName DROP DEFAULT
            _sbScript.Append(string.Format("ALTER TABLE [{0}] ALTER COLUMN [{1}] DROP DEFAULT;{2}", column.TableName, column.ColumnName, Environment.NewLine));
            _sbScript.Append(_sep);
        }

        public void GeneratePrimaryKeyDrop(PrimaryKey primaryKey, string tableName)
        {
            //ALTER TABLE xx DROP CONSTRAINT yy
            _sbScript.Append(string.Format("ALTER TABLE [{0}] DROP CONSTRAINT [{1}];{2}", tableName, primaryKey.KeyName, Environment.NewLine));
            _sbScript.Append(_sep);
        }

        public void GenerateForeignKey(Constraint constraint)
        {
            _sbScript.AppendFormat(CultureInfo.InvariantCulture, "ALTER TABLE [{0}] ADD CONSTRAINT [{1}] FOREIGN KEY ({2}) REFERENCES [{3}]({4}) ON DELETE {5} ON UPDATE {6};{7}"
                , constraint.ConstraintTableName
                , constraint.ConstraintName
                , constraint.Columns
                , constraint.UniqueConstraintTableName
                , constraint.UniqueColumns
                , constraint.DeleteRule
                , constraint.UpdateRule
                , Environment.NewLine);
            _sbScript.Append(_sep);
        }

        public void GenerateForeignKey(string tableName, string keyName)
        {
            var key = _allForeignKeys.Where(c => c.ConstraintTableName == tableName && c.ConstraintName.StartsWith(keyName)).FirstOrDefault();
            if (key != null)
            {
                GenerateForeignKey(key);
            }
        }

        public void GenerateForeignKeyDrop(Constraint constraint)
        {
            _sbScript.Append(string.Format("ALTER TABLE [{0}] DROP CONSTRAINT [{1}];{2}", constraint.ConstraintTableName, constraint.ConstraintName, Environment.NewLine));
            _sbScript.Append(_sep);
        }

        public void GenerateForeignKeyDrop(string tableName, string keyName)
        {
            var key = _allForeignKeys.Where(c => c.ConstraintTableName == tableName && c.ConstraintName.StartsWith(keyName)).FirstOrDefault();
            if (key != null)
            {
                GenerateForeignKeyDrop(key);
            }
        }

        internal void GenerateIdentityResets(bool forServer = false)
        {
            foreach (var tableName in _tableNames)
            {
                GenerateIdentityReset(tableName, forServer);
            }
        }

        public void GenerateIdentityReset(string tableName, bool forServer)
        {
            int identityOrdinal = _repository.GetIdentityOrdinal(tableName);
            if (identityOrdinal > -1)
            {
                var col = _allColumns.Where(c => c.TableName == tableName && c.AutoIncrementBy > 0).SingleOrDefault();
                if (col != null)
                {
                    if (forServer)
                    {
                        _sbScript.AppendLine(string.Format(CultureInfo.InvariantCulture,
                            "DBCC CHECKIDENT ('{0}', RESEED, {1});", tableName, col.AutoIncrementNext));
                    }
                    else
                    {
                        _sbScript.AppendLine(string.Format(CultureInfo.InvariantCulture, "ALTER TABLE [{0}] ALTER COLUMN [{1}] IDENTITY ({2},{3});", tableName, col.ColumnName, col.AutoIncrementNext, col.AutoIncrementBy));
                    }
                    _sbScript.Append(_sep);
                }
            }
        }

        public void GenerateSqlitePrefix()
        {
            _sbScript.AppendLine("SELECT 1;");
            _sbScript.AppendLine("PRAGMA foreign_keys=OFF;");
            _sbScript.AppendLine("BEGIN TRANSACTION;");
        }

        public void GenerateSqliteSuffix()
        {
            _sbScript.AppendLine("COMMIT;");
        }

        public void GenerateAllAndSave(bool includeData, bool saveImages, bool dataOnly, bool forServer, bool ignoreIdentity = false)
        {
            if (_sqlite)
            {
                GenerateSqlitePrefix();
                if (dataOnly)
                {
                    GenerateTableContent(false);
                }
                else
                {
                    GenerateTable(true);
                    if (includeData)
                    {
                        GenerateTableContent(false);
                    }
                    GenerateIndex();
                    GenerateTriggers(_allTriggers);
                    GenerateTriggersForForeignKeys();
                    GenerateViews();
                }
                GenerateSqliteSuffix();
            }
            else
            {
                if (dataOnly)
                {
                    GenerateTableContent(false, ignoreIdentity);
                    if (!ignoreIdentity)
                    {
                        GenerateIdentityResets(forServer);
                    }
                }
                else
                {
                    GenerateTable(includeData);
                    if (_batchForAzure)
                    {
                        GeneratePrimaryKeys();
                    }
                    if (includeData)
                    {
                        GenerateTableContent(saveImages);
                    }
                    if (!_batchForAzure)
                    {
                        GeneratePrimaryKeys();
                    }
                    GenerateIndex();
                    GenerateForeignKeys();
                }
            }
            Helper.WriteIntoFile(GeneratedScript, _outFile, FileCounter, _sqlite);
        }

        private void GenerateTriggersForForeignKeys()
        {
            foreach (string tableName in _tableNames)
            {
                GenerateTriggersForForeignKeys(tableName);
            }
        }

        private void GenerateTriggersForForeignKeys(string tableName)
        {
            List<Constraint> foreignKeys = _allForeignKeys.Where(fk => fk.ConstraintTableName == tableName).ToList();

            foreach (Constraint constraint in foreignKeys)
            {
                GenerateInsertTriggerForForeignKey(constraint);
                GenerateUpdateTriggerForForeignKey(constraint);
            }
        }

        private void GenerateInsertTriggerForForeignKey(Constraint constraint)
        {
            GenerateTriggerForForeignKey("fki", TriggerType.Insert, constraint);
        }

        private void GenerateUpdateTriggerForForeignKey(Constraint constraint)
        {
            GenerateTriggerForForeignKey("fku", TriggerType.Update, constraint);
        }

        private void GenerateTriggerForForeignKey(string prefix, string triggerType, Constraint constraint)
        {
            string constraintName = constraint.ConstraintName;
            string tableName = constraint.ConstraintTableName;
            string foreignTableName = constraint.UniqueConstraintTableName;

            string columnName = constraint.Columns[0];
            Column column = _allColumns.Single(c => c.TableName == tableName && c.ColumnName == RemoveBrackets(columnName));

            string foreignColumnName = constraint.UniqueColumns[0];
            Column foreignColumn = _allColumns.Single(c => c.TableName == foreignTableName && c.ColumnName == RemoveBrackets(foreignColumnName));

            string triggerName = prefix + "_" + tableName + "_" + RemoveBrackets(columnName) + "_" + foreignTableName + "_" + RemoveBrackets(foreignColumnName);

            _sbScript.Append(
                $"CREATE TRIGGER [{triggerName}] BEFORE {triggerType} ON [{tableName}] FOR EACH ROW BEGIN" +
                $" SELECT RAISE(ROLLBACK, '{triggerType} on table {tableName} violates foreign key constraint {constraintName}')" +
                " WHERE ");

            if (column.IsNullable == YesNoOption.YES)
            {
                _sbScript.Append(string.Join(" ", constraint.Columns.Select(x => $"NEW.{x} IS NOT NULL AND").ToArray()));
            }

            _sbScript.Append($"(SELECT {string.Join(", ", constraint.UniqueColumns.ToArray())} FROM {foreignTableName} WHERE ");

            for (int i = 0; i < constraint.Columns.Count; i++)
            {
                int j = i;
                if (j > constraint.UniqueColumns.Count - 1)
                {
                    // different foreign keys are using the same columns from the same table
                    // re-use the last foreign column name
                    j = constraint.UniqueColumns.Count - 1;
                }

                _sbScript.Append($" {constraint.UniqueColumns[j]} = NEW.{constraint.Columns[i]}");

                if (i < constraint.Columns.Count - 1)
                {
                    _sbScript.Append(" AND");
                }
            }

            _sbScript.Append(") IS NULL;");
            _sbScript.AppendLine(" END;");
        }

        private class TriggerType
        {
            public const string Insert = "Insert";

            public const string Update = "Update";
        }

        public IList<string> GeneratedFiles
        {
            get
            {
                return Helper.FinalFiles.Replace(", ", ",").Split(',');
            }
        }

        public int FileCounter
        {
            get
            {
                if (_fileCounter > -1)
                    _fileCounter++;
                return _fileCounter;
            }
        }

        public string AssemblyFileVersion
        {
            get
            {
                object[] attributes = System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(System.Reflection.AssemblyFileVersionAttribute), false);
                if (attributes.Length == 0)
                    return "";
                return ((System.Reflection.AssemblyFileVersionAttribute)attributes[0]).Version;
            }
        }


        public void GenerateDatabaseInfo()
        {
            _sbScript.Append("-- Database information:");
            _sbScript.AppendLine();

            foreach (var kv in _repository.GetDatabaseInfo())
            {
                _sbScript.Append("-- ");
                _sbScript.Append(kv.Key);
                _sbScript.Append(": ");
                _sbScript.Append(kv.Value);
                _sbScript.AppendLine();
            }
            _sbScript.AppendLine();

            // Populate all tablenames
            _sbScript.Append("-- User Table information:");
            _sbScript.AppendLine();
            _sbScript.Append("-- ");
            _sbScript.Append("Number of tables: ");
            _sbScript.Append(_tableNames.Count);
            _sbScript.AppendLine();

            foreach (string tableName in _tableNames)
            {
                Int64 rowCount = _repository.GetRowCount(tableName);
                _sbScript.Append("-- ");
                _sbScript.Append(tableName);
                _sbScript.Append(": ");
                _sbScript.Append(rowCount);
                _sbScript.Append(" row(s)");
                _sbScript.AppendLine();
            }
            _sbScript.AppendLine();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="includeData"></param>
        internal void GenerateTable(bool includeData)
        {
            foreach (string tableName in _tableNames)
                GenerateTableCreate(tableName, includeData);

        }

        public void GenerateTableCreate(string tableName)
        {
            GenerateTableCreate(tableName, false);
        }

        public void GenerateTableCreate(string tableName, List<Column> columns)
        {
            GenerateTableCreate(tableName, false, columns);
        }

        internal void GenerateTableCreate(string tableName, bool includeData)
        {
            List<Column> columns = _allColumns.Where(c => c.TableName == tableName).ToList();
            GenerateTableCreate(tableName, includeData, columns);
        }

        internal void GenerateTableCreate(string tableName, bool includeData, List<Column> columns)
        {
            if (columns.Count > 0)
            {
                _sbScript.AppendFormat("CREATE TABLE [{0}] ({1}  ", tableName, Environment.NewLine);

                foreach (Column col in columns)
                {
                    string line = GenerateColumLine(includeData, col, _batchForAzure);
                    if (!string.IsNullOrEmpty(line))
                        _sbScript.AppendFormat("{0}{1}, ", line.Trim(), Environment.NewLine);
                }
                // Remove the last comma
                _sbScript.Remove(_sbScript.Length - 2, 2);
                if (!_sqlite)
                {
                    _sbScript.AppendFormat(");{0}", Environment.NewLine);
                    _sbScript.Append(_sep);
                }
                else
                {
                    _sbScript.Remove(_sbScript.Length - 2, 2);
                    GeneratePrimaryKeys(tableName);
                    GenerateForeignKeys(tableName);
                    _sbScript.AppendFormat("{0});{1}", Environment.NewLine, Environment.NewLine);
                }
            }
        }

        private static string RemoveBrackets(string columnName)
        {
            if (columnName.StartsWith("["))
                columnName = columnName.Substring(1);
            if (columnName.EndsWith("]"))
                columnName = columnName.Remove(columnName.Length - 1);
            return columnName;
        }

        private string GetLocalName(string table)
        {
            if (!_repository.IsServer() || _repository.KeepSchema())
                return table;

            int index = table.IndexOf('.');
            if (index >= 0)
                return (table.Substring(index + 1));
            return (table);
        }

        private string GenerateColumLine(bool includeData, Column col, bool azure)
        {
            string line;

            string colDefault = col.ColumnHasDefault ? " DEFAULT (" + col.ColumnDefault + ")" : string.Empty;
            if (_sqlite && col.ColumnHasDefault)
            {
                if (col.ColumnDefault.ToLowerInvariant().Contains("newid()"))
                {
                    colDefault = string.Empty;
                }
                if (col.ColumnDefault.ToLowerInvariant().StartsWith("n'"))
                {
                    colDefault = " DEFAULT " + col.ColumnDefault.Remove(0, 1);
                }
                if (col.ColumnDefault.ToLowerInvariant().Contains("getdate()"))
                {
                    colDefault = " DEFAULT current_timestamp";
                }
            }
            string colNull = col.IsNullable == YesNoOption.YES ? " NULL" : " NOT NULL";
            string collate = string.Empty;
            switch (col.DataType)
            {
                case "nvarchar":
                case "nchar":
                case "binary":
                case "varbinary":
                    if (_sqlite)
                    {
                        collate = col.IsCaseSensitivite ? string.Empty : " COLLATE NOCASE";
                    }

                    line = string.Format(CultureInfo.InvariantCulture,
                        "[{0}] {1}({2}){3}{4}{5}"
                        , col.ColumnName
                        , col.DataType
                        , col.CharacterMaxLength == -1 ? 4000 : col.CharacterMaxLength
                        , colDefault
                        , colNull
                        , collate
                        );
                    break;
                case "numeric":
                    line = string.Format(CultureInfo.InvariantCulture,
                        "[{0}] {1}({2},{3}){4}{5}"
                        , col.ColumnName
                        , col.DataType
                        , col.NumericPrecision
                        , col.NumericScale
                        , colDefault
                        , colNull
                        );
                    break;
                case "uniqueidentifier":
                    string rowGuidCol = string.Empty;
                    if (col.RowGuidCol && !azure && !_sqlite)
                    {
                        rowGuidCol = " ROWGUIDCOL";
                    }
                    line = string.Format(CultureInfo.InvariantCulture,
                        "[{0}] {1}{2}{3}{4}"
                        , col.ColumnName
                        , col.DataType
                        , colDefault
                        , rowGuidCol
                        , colNull
                        );
                    break;
                case "int":
                case "bigint":

                    string colIdentity = string.Empty;
                    if (col.AutoIncrementBy > 0)
                    {
                        if (_sqlite)
                        {
                            // http://www.sqlite.org/lang_createtable.html#rowid
                            col.DataType = "INTEGER";

                            List<PrimaryKey> primaryKeys = _allPrimaryKeys.Where(p => p.TableName == col.TableName).ToList();
                            if ((primaryKeys.Count == 1) && (primaryKeys.Select(x => x.ColumnName).Contains(col.ColumnName)))
                            {
                                col.DataType += " PRIMARY KEY AUTOINCREMENT";
                            }
                        }
                        else
                        {
                            if (includeData)
                            {
                                colIdentity = string.Format(CultureInfo.InvariantCulture, " IDENTITY ({0},{1})", col.AutoIncrementNext, col.AutoIncrementBy);
                            }
                            else
                            {
                                colIdentity = string.Format(CultureInfo.InvariantCulture, " IDENTITY ({0},{1})", col.AutoIncrementSeed, col.AutoIncrementBy);
                            }
                        }
                    }

                    line = string.Format(CultureInfo.InvariantCulture,
                        "[{0}] {1}{2}{3}{4}"
                        , col.ColumnName
                        , col.DataType
                        , colDefault
                        , colIdentity
                        , colNull
                        );
                    break;
                default:
                    line = string.Format(CultureInfo.InvariantCulture,
                        "[{0}] {1}{2}{3}"
                        , col.ColumnName
                        , col.DataType
                        , colDefault
                        , colNull
                        );
                    break;
            }
            return line;
        }

        public void GenerateTableContent(bool saveImageFiles, bool ignoreIdentity = false)
        {
            if (_whereClauses != null && _tableNames.Count != _whereClauses.Count)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Numbers of elements in {0} and {1} do not match", nameof(_tableNames), nameof(_whereClauses)));
            }
            int index = 0;
            foreach (var tableName in _tableNames)
            {
                if (_whereClauses != null)
                {
                    GenerateTableContent(tableName, saveImageFiles, ignoreIdentity, _whereClauses[index]);
                }
                else
                {
                    GenerateTableContent(tableName, saveImageFiles, ignoreIdentity);
                }
                index++;
            }
        }


        public void GenerateSqliteNetModel(string nameSpace)
        {
            _sbScript.Clear();
            var baseTextWriter = new StringWriter();
            var indentWriter = new IndentedTextWriter(baseTextWriter);

            indentWriter.Indent = 0;
            indentWriter.WriteLine("using SQLite;");
            indentWriter.WriteLine("using System;");
            indentWriter.WriteLine(string.Empty);
            indentWriter.WriteLine("namespace " + nameSpace);
            indentWriter.WriteLine("{");
            indentWriter.Indent = 1;

            indentWriter.WriteLine("public class SQLiteDb");
            indentWriter.WriteLine("{");
            indentWriter.Indent = 2;
            indentWriter.WriteLine("string _path;");
            indentWriter.WriteLine("public SQLiteDb(string path)");
            indentWriter.WriteLine("{");
            indentWriter.Indent = 3;
            indentWriter.WriteLine("_path = path;");
            indentWriter.Indent = 2;
            indentWriter.WriteLine("}");
            indentWriter.WriteLine("");

            indentWriter.WriteLine(" public void Create()");
            indentWriter.WriteLine("{");
            indentWriter.Indent = 3;
            indentWriter.WriteLine("using (SQLiteConnection db = new SQLiteConnection(_path))");
            indentWriter.WriteLine("{");
            indentWriter.Indent = 4;

            foreach (var tableName in _tableNames)
            {
                indentWriter.WriteLine(@"db.CreateTable<{0}>();", tableName);
            }
            indentWriter.Indent = 3;
            indentWriter.WriteLine("}");
            indentWriter.Indent = 2;
            indentWriter.WriteLine("}");
            indentWriter.Indent = 1;
            indentWriter.WriteLine("}");
            var viewNames = _repository.GetAllViews().Select(v => v.ViewName).ToList();
            foreach (var tableName in _tableNames.Concat(viewNames).ToList())
            {
                indentWriter.WriteLine("public partial class " + tableName);
                indentWriter.WriteLine("{");
                indentWriter.Indent = 2;
                var columns = _allColumns.Where(c => c.TableName == tableName).ToList();
                var primaryKeys = _allPrimaryKeys.Where(p => p.TableName == tableName).ToList();
                var indexCols = _allIndexes.Where(i => i.TableName == tableName).ToList();
                foreach (var column in columns)
                {
                    Type clrType = _repository.GetClrTypeFromDataType(column.DataType);

                    var pkAttribute = string.Empty;
                    var maxAttribute = string.Empty;
                    var notNullAttribute = string.Empty;
                    var indexAttributes = new List<string>();

                    //Only support single column primary keys
                    //until support for multiple are added to sqlite-net
                    if (primaryKeys.Count == 1)
                    {
                        var pk =
                            primaryKeys.Where(p => p.ColumnName == column.ColumnName)
                                .Select(p => p.KeyName)
                                .SingleOrDefault();
                        if (pk != null)
                        {
                            pkAttribute += "PrimaryKey";
                        }
                        if (column.AutoIncrementBy > 0 && !string.IsNullOrEmpty(pkAttribute))
                        {
                            pkAttribute += ", AutoIncrement";
                        }
                    }

                    var ixCols = indexCols.Where(i => i.ColumnName == column.ColumnName).ToList();
                    foreach (var ixCol in ixCols)
                    {
                        if (ixCol.Unique)
                        {
                            indexAttributes.Add("Unique(Name = \"" + ixCol.IndexName + "\", Order = " + ixCol.OrdinalPosition.ToString() + ")");
                        }
                        else
                        {
                            indexAttributes.Add("Indexed(Name = \"" + ixCol.IndexName + "\", Order = " + ixCol.OrdinalPosition.ToString() + ")");
                        }
                    }

                    if (clrType.FullName == "System.String" && column.CharacterMaxLength > 0 && column.CharacterMaxLength < int.MaxValue)
                    {
                        maxAttribute = "MaxLength(" + column.CharacterMaxLength.ToString(CultureInfo.InvariantCulture) + ")";
                    }

                    if (column.IsNullable == YesNoOption.NO && string.IsNullOrEmpty(pkAttribute))
                    {
                        notNullAttribute = "NotNull";
                    }

                    if (!string.IsNullOrEmpty(pkAttribute))
                    {
                        indentWriter.WriteLine("[" + pkAttribute + "]");
                    }
                    foreach (var item in indexAttributes)
                    {
                        indentWriter.WriteLine("[" + item + "]");
                    }
                    if (!string.IsNullOrEmpty(maxAttribute))
                    {
                        indentWriter.WriteLine("[" + maxAttribute + "]");
                    }
                    if (!string.IsNullOrEmpty(notNullAttribute))
                    {
                        indentWriter.WriteLine("[" + notNullAttribute + "]");
                    }
                    var isNullable = string.Empty;
                    if (column.IsNullable == YesNoOption.YES && clrType.IsValueType)
                    {
                        isNullable = "?";
                    }
                    indentWriter.WriteLine("public " + clrType.Name + isNullable + " " + column.ColumnName + " { get; set; }");
                    indentWriter.WriteLine("");
                }
                indentWriter.Indent = 1;
                indentWriter.WriteLine("}");
                indentWriter.WriteLine("");
            }
            indentWriter.Indent = 0;
            indentWriter.WriteLine("}");

            _sbScript.Append(baseTextWriter);
        }

        #region private methods
        private string TruncateLogFileName()
        {
            return Path.Combine(Path.GetTempPath(), "SQLiteTruncates.log");
        }

        private DataSet FillSchemaDataSet(List<string> tables)
        {
            DataSet schemaDataSet = _repository.GetSchemaDataSet(tables);
            foreach (Constraint fk in _allForeignKeys)
            {
                //Only add relation if both tables are included!
                if (tables.Contains(fk.ConstraintTableName) && tables.Contains(fk.UniqueConstraintTableName))
                {
                    // No self references supported 
                    if (fk.ConstraintTableName != fk.UniqueConstraintTableName)
                    {
                        DataColumn[] fkColumns = new DataColumn[fk.Columns.Count];
                        DataColumn[] uniqueColumns = new DataColumn[fk.Columns.Count];
                        for (int i = 0; i < fk.Columns.Count; i++)
                        {
                            fk.Columns[i] = RemoveBrackets(fk.Columns[i]);
                            fk.UniqueColumns[i] = RemoveBrackets(fk.UniqueColumns[i]);
                            fkColumns[i] = schemaDataSet.Tables[fk.ConstraintTableName].Columns[fk.Columns[i]];
                            uniqueColumns[i] = schemaDataSet.Tables[fk.UniqueConstraintTableName].Columns[fk.UniqueColumns[i]];
                        }

                        if (!schemaDataSet.Relations.Contains(fk.ConstraintName))
                        {
                            try
                            {
                                schemaDataSet.Relations.Add(fk.ConstraintName,
                                    uniqueColumns,
                                    fkColumns);
                            }
                            //"Handle" duplicated Server foreign keys
                            catch (ArgumentException ex1)
                            {
                                _sbScript.AppendLine("-- Warning - constraint: " + fk.ConstraintTableName + " " + ex1.Message);
                            }
                            catch (InvalidConstraintException ex2)
                            {
                                _sbScript.AppendLine("-- Warning - constraint: " + fk.ConstraintTableName + " " + ex2.Message);
                            }
                        }
                    }
                }
            }
            return schemaDataSet;
        }

        private string GenerateInserOrUpdate(string tableName, bool createInsert, DataRow row)
        {
            StringBuilder sb = new StringBuilder();
            int identityOrdinal = _repository.GetIdentityOrdinal(tableName);
            bool hasIdentity = (identityOrdinal > -1);
            string unicodePrefix = "N";
            if (_sqlite)
                unicodePrefix = string.Empty;
            // Skip rowversion column
            Int32 rowVersionOrdinal = _repository.GetRowVersionOrdinal(tableName);
            List<Column> columns = _allColumns.Where(c => c.TableName == tableName).ToList();
            var fields = columns.Select(c => c.ColumnName).ToList();
            var scriptPrefix = string.Empty;
            if (createInsert)
                scriptPrefix = GetInsertScriptPrefix(tableName, fields, rowVersionOrdinal, identityOrdinal, false);

            if (createInsert && hasIdentity && !_sqlite)
            {
                sb.Append(string.Format(CultureInfo.InvariantCulture, "SET IDENTITY_INSERT [{0}] ON;", tableName));
                sb.Append(Environment.NewLine);
                sb.Append(_sep);
            }
            sb.Append(scriptPrefix);
            for (int iColumn = 0; iColumn < row.ItemArray.Count(); iColumn++)
            {
                var fieldType = row[iColumn].GetType();
                //Skip rowversion column
                if (rowVersionOrdinal == iColumn || row.Table.Columns[iColumn].ColumnName.StartsWith("__sys", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                //ignore identity for updates
                if (!createInsert && (identityOrdinal == iColumn))
                {
                    continue;
                }
                if (!createInsert)
                    sb.Append(string.Format(" [{0}] = ", row.Table.Columns[iColumn].ColumnName));
                if (row.IsNull(iColumn))
                {
                    sb.Append("NULL");
                }
                else if (fieldType == typeof(String))
                {
                    sb.AppendFormat("{0}'{1}'", unicodePrefix, row[iColumn].ToString().Replace("'", "''"));
                }
                else if (fieldType == typeof(DateTime))
                {
                    //Datetime globalization - ODBC escape: {ts '2004-03-29 19:21:00'}
                    sb.Append("{ts '");
                    sb.Append(((DateTime)row[iColumn]).ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture));
                    sb.Append("'}");
                }
                else if (fieldType == typeof(Byte[]))
                {
                    Byte[] buffer = (Byte[])row[iColumn];
                    sb.Append("0x");
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        sb.Append(buffer[i].ToString("X2", CultureInfo.InvariantCulture));
                    }
                }
                else if (fieldType == typeof(Double) || fieldType == typeof(Single))
                {
                    string intString = Convert.ToDouble(row[iColumn]).ToString("R", CultureInfo.InvariantCulture);
                    sb.Append(intString);
                }
                else if (fieldType == typeof(Byte) || fieldType == typeof(Int16) || fieldType == typeof(Int32) ||
                    fieldType == typeof(Int64) || fieldType == typeof(Decimal))
                {
                    string intString = Convert.ToString(row[iColumn], CultureInfo.InvariantCulture);
                    sb.Append(intString);
                }
                else if (fieldType == typeof(Boolean))
                {
                    bool boolVal = (Boolean)row[iColumn];
                    if (boolVal)
                    { sb.Append("1"); }
                    else
                    { sb.Append("0"); }
                }
                else
                {
                    string value = Convert.ToString(row[iColumn], CultureInfo.InvariantCulture);
                    sb.AppendFormat("'{0}'", value.Replace("'", "''"));
                }
                sb.Append(",");
            }
            // remove trailing comma
            sb.Remove(sb.Length - 1, 1);
            if (createInsert)
            {
                sb.Append(");");
                sb.Append(Environment.NewLine);
                sb.Append(_sep);
                if (hasIdentity && !_sqlite)
                {
                    sb.Append(string.Format(CultureInfo.InvariantCulture, "SET IDENTITY_INSERT [{0}] OFF;", tableName));
                    sb.Append(Environment.NewLine);
                    sb.Append(_sep);
                }
            }
            return sb.ToString();
        }

        private void GenerateSingleIndex(string tableName, string uniqueIndexName)
        {
            List<Index> tableIndexes;
            if (_repository.IsServer())
            {
                tableIndexes = _repository.GetIndexesFromTable(tableName);
            }
            else
            {
                tableIndexes = _allIndexes.Where(i => i.TableName == tableName).ToList();
            }

            IOrderedEnumerable<Index> indexesByName = from i in tableIndexes
                                                      where i.IndexName == uniqueIndexName
                                                      orderby i.OrdinalPosition
                                                      select i;
            if (indexesByName.Any())
            {
                var idx = indexesByName.First();

                if (idx.Unique && !_sqlite)
                {
                    _sbScript.AppendFormat("ALTER TABLE [{0}] ADD CONSTRAINT [{1}] UNIQUE (", idx.TableName, idx.IndexName);
                    foreach (Index col in indexesByName)
                    {
                        _sbScript.AppendFormat("[{0}],", col.ColumnName);
                    }
                }
                else
                {
                    _sbScript.Append("CREATE ");
                    // Just get the first one to decide whether it's unique and/or clustered index
                    if (idx.Unique)
                        _sbScript.Append("UNIQUE ");
                    if (idx.Clustered)
                        _sbScript.Append("CLUSTERED ");

                    var indexName = idx.IndexName;
                    if (_sqlite)
                        indexName = idx.TableName + "_" + idx.IndexName;
                    _sbScript.AppendFormat("INDEX [{0}] ON [{1}] (", indexName, idx.TableName);
                    foreach (Index col in indexesByName)
                    {
                        _sbScript.AppendFormat("[{0}] {1},", col.ColumnName, col.SortOrder.ToString());
                    }
                }
                // Remove the last comma
                _sbScript.Remove(_sbScript.Length - 1, 1);
                _sbScript.Append(")");

                if (!string.IsNullOrEmpty(idx.Filter))
                {
                    _sbScript.Append($" WHERE {idx.Filter}");
                }

                _sbScript.Append(";");
                _sbScript.AppendLine();
                _sbScript.Append(_sep);
            }
            else
            {
                GeneratePrimaryKeys(tableName);
            }
        }

        private static string GetInsertScriptPrefix(string tableName, List<string> fieldNames, int rowVersionOrdinal, int identityOrdinal, bool ignoreIdentity)
        {
            if (!ignoreIdentity)
                identityOrdinal = -1;

            StringBuilder sbScriptTemplate = new StringBuilder(1000);
            sbScriptTemplate.AppendFormat("INSERT INTO [{0}] (", tableName);

            StringBuilder columnNames = new StringBuilder();
            // Generate the field names first
            for (int iColumn = 0; iColumn < fieldNames.Count; iColumn++)
            {
                if (iColumn != rowVersionOrdinal && iColumn != identityOrdinal && !fieldNames[iColumn].StartsWith("__sys", StringComparison.OrdinalIgnoreCase))
                {
                    columnNames.AppendFormat("[{0}]{1}", fieldNames[iColumn], ",");
                }
            }
            columnNames.Remove(columnNames.Length - 1, 1);
            sbScriptTemplate.Append(columnNames);
            sbScriptTemplate.Append(") VALUES (");
            return sbScriptTemplate.ToString();
        }

        private void GenerateViews()
        {
            foreach (var view in _allViews)
            {
                _sbScript.AppendFormat(view.Definition);
                _sbScript.AppendFormat(";" + Environment.NewLine);
                _sbScript.Append(_sep);
            }
        }

        private void GenerateTriggers(List<Trigger> triggers)
        {
            foreach (var trigger in triggers)
            {
                if (!_tableNames.Contains(trigger.TableName)) continue;
                GenerateTrigger(trigger);
            }
        }

        private void GenerateTrigger(Trigger trigger)
        {
            _sbScript.Append(trigger.Definition);
            _sbScript.Append(";" + Environment.NewLine);
            _sbScript.Append(_sep);
        }
        #endregion
    }
}