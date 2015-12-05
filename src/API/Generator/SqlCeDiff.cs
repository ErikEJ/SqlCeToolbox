using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace ErikEJ.SqlCeScripting
{
    public sealed class SqlCeDiff
    {
        private SqlCeDiff()
        {}

        public static void CreateDiffScript(IRepository sourceRepository, IRepository targetRepository,IGenerator generator, bool includeTargetDrops)
        {
            List<string> sourceTables = sourceRepository.GetAllTableNames();
            List<string> targetTables = targetRepository.GetAllTableNames();

            // Script each table not in the target
            foreach (string tableName in sourceTables.Except(targetTables))
            {
                generator.GenerateTableCreate(tableName);
            }
            foreach (string tableName in sourceTables.Except(targetTables))
            {
                generator.GeneratePrimaryKeys(tableName);
            }
            foreach (string tableName in sourceTables.Except(targetTables))
            {
                List<string> tableIndexes = sourceRepository.GetIndexesFromTable(tableName).Select(i => i.IndexName).Distinct().ToList();
                foreach (var index in tableIndexes)
                {
                    generator.GenerateIndexScript(tableName, index);
                }
            }
            foreach (string tableName in sourceTables.Except(targetTables))
            {
                generator.GenerateForeignKeys(tableName);
            }

            // Drop each table in the target but not the source
            if (includeTargetDrops)
            {
                foreach (string tableName in targetTables.Except(sourceTables))
                {
                    generator.GenerateTableDrop(tableName);
                }
            }

            //For each table both in target and source
            foreach (string tableName in sourceTables.Intersect(targetTables))
            {
                // Check columns for the table: Dropped, added or changed ?
                IEnumerable<Column> sourceColumns = from c in sourceRepository.GetAllColumns()
                                    where c.TableName == tableName
                                    select c;
                IEnumerable<Column> targetColumns = from c in targetRepository.GetAllColumns()
                                    where c.TableName == tableName
                                    select c;

                // Added columns
                foreach (var column in sourceColumns.Except(targetColumns, new ColumnComparer()))
                {
                    generator.GenerateColumnAddScript(column);
                }
                // Same columns, check for changes
                foreach (var sourceColumn in sourceColumns.Intersect(targetColumns, new ColumnComparer()))
                {
                    bool altered = false;
                    // Check if they have any differences:
                    var targetColumn = (from c in targetColumns
                                        where c.TableName == sourceColumn.TableName && c.ColumnName == sourceColumn.ColumnName
                                        select c).Single();
                    if (sourceColumn.IsNullable != targetColumn.IsNullable)
                        altered = true;
                    if (sourceColumn.NumericPrecision != targetColumn.NumericPrecision)
                        altered = true;
                    if (sourceColumn.NumericScale != targetColumn.NumericScale)
                        altered = true;
                    if (sourceColumn.AutoIncrementBy != targetColumn.AutoIncrementBy)
                        altered = true;
                    if (sourceColumn.CharacterMaxLength != targetColumn.CharacterMaxLength)
                        altered = true;
                    if (sourceColumn.DataType != targetColumn.DataType)
                        altered = true;

                    if (altered)
                        generator.GenerateColumnAlterScript(sourceColumn);

                    // Changed defaults is special case
                    if (!targetColumn.ColumnHasDefault && sourceColumn.ColumnHasDefault)
                    {
                        generator.GenerateColumnSetDefaultScript(sourceColumn);
                    }
                    if (!sourceColumn.ColumnHasDefault && targetColumn.ColumnHasDefault)
                    {
                        generator.GenerateColumnDropDefaultScript(sourceColumn);
                    }
                    // If both columns have defaults, but they are different
                    if ((sourceColumn.ColumnHasDefault && targetColumn.ColumnHasDefault) && (sourceColumn.ColumnDefault != targetColumn.ColumnDefault))
                    {
                        generator.GenerateColumnSetDefaultScript(sourceColumn);
                    }
                }

                //Check primary keys
                List<PrimaryKey> sourcePK = sourceRepository.GetAllPrimaryKeys().Where(p => p.TableName == tableName).ToList();
                List<PrimaryKey> targetPK = targetRepository.GetAllPrimaryKeys().Where(p => p.TableName == tableName).ToList();

                // Add the PK
                if (targetPK.Count == 0 && sourcePK.Count > 0)
                {
                    generator.GeneratePrimaryKeys(tableName);
                }

                // Do we have the same columns, if not, drop and create.
                if (sourcePK.Count > 0 && targetPK.Count > 0)
                {
                    if (sourcePK.Count == targetPK.Count)
                    {
                        //Compare columns
                        for (int i = 0; i < sourcePK.Count; i++)
                        {
                            if (sourcePK[i].ColumnName != targetPK[i].ColumnName)
                            {
                                generator.GeneratePrimaryKeyDrop(sourcePK[i], tableName);
                                generator.GeneratePrimaryKeys(tableName);
                                break;
                            }
                        }
                    }
                    // Not same column count, just drop and create
                    else
                    {
                        generator.GeneratePrimaryKeyDrop(sourcePK[0], tableName);
                        generator.GeneratePrimaryKeys(tableName);
                    }
                }

                // Check indexes
                List<Index> sourceIXs = sourceRepository.GetIndexesFromTable(tableName);
                List<Index> targetIXs = targetRepository.GetIndexesFromTable(tableName);

                // Check added indexes (by name only)
                foreach (var index in sourceIXs)
                {
                    var targetIX = targetIXs.Where(s => s.IndexName == index.IndexName);
                    if (targetIX.Count() == 0)
                    {
                        generator.GenerateIndexScript(index.TableName, index.IndexName);
                    }
                }

                // Check foreign keys
                List<Constraint> sourceFKs = sourceRepository.GetAllForeignKeys().Where(fk => fk.ConstraintTableName == tableName).ToList();
                List<Constraint> targetFKs = targetRepository.GetAllForeignKeys().Where(fk => fk.ConstraintTableName == tableName).ToList();

                // Check added foreign keys (by name only)
                foreach (var fk in sourceFKs)
                {
                    Constraint targetFK = targetFKs.Where(s => s.ConstraintName == fk.ConstraintName).SingleOrDefault();
                    if (targetFK == null)
                    {
                        generator.GenerateForeignKey(fk);
                    }
                }
                // Check deleted FKs (by name only)
                foreach (var fk in targetFKs)
                {
                    Constraint sourceFK = sourceFKs.Where(s => s.ConstraintName == fk.ConstraintName).SingleOrDefault();
                    if (sourceFK == null)
                    {
                        generator.GenerateForeignKeyDrop(fk);
                    }
                }

                // Check deleted indexes (by name only)
                foreach (var index in targetIXs)
                {
                    var sourceIX = sourceIXs.Where(s => s.IndexName == index.IndexName);
                    if (sourceIX.Count() == 0)
                    {
                        generator.GenerateIndexOnlyDrop(index.TableName, index.IndexName);
                    }
                }

                // Dropped columns
                foreach (var column in targetColumns.Except(sourceColumns, new ColumnComparer()))
                {
                    generator.GenerateColumnDropScript(column);
                }

            }
        }

        public static string AssemblyFileVersion
        {
            get
            {
                object[] attributes = System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(System.Reflection.AssemblyFileVersionAttribute), false);
                if (attributes.Length == 0)
                    return "";
                return ((System.Reflection.AssemblyFileVersionAttribute)attributes[0]).Version;
            }
        }

        public static string CreateDataDiffScript(IRepository sourceRepository, string sourceTable, IRepository targetRepository, string targetTable, IGenerator generator)
        {
            //more advanced would be a field-mapping to be able to transfer the data between different structures
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("-- Script Date: {0} {1}  - ErikEJ.SqlCeScripting version {2}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), AssemblyFileVersion);
            sb.Append(Environment.NewLine);
            sb.AppendLine("-- Data Diff script:");
            List<Column> sourceColumns = (from c in sourceRepository.GetAllColumns()
                                                where c.TableName == sourceTable
                                                select c).ToList();
            List<Column> targetColumns = (from c in targetRepository.GetAllColumns()
                                                where c.TableName == targetTable
                                                select c).ToList();

            var sourcePkList = sourceRepository.GetAllPrimaryKeys().Where(pk => pk.TableName == sourceTable).Select(pk => pk.ColumnName).ToList();
            if (sourcePkList.Count < 1)
            {
                throw new ArgumentException("Source does not have a primary key, this is required");
            }

            var targetPkList = targetRepository.GetAllPrimaryKeys().Where(pk => pk.TableName == targetTable).Select(pk => pk.ColumnName).ToList();
            if (targetPkList.Count < 1)
            {
                throw new ArgumentException("Target does not have a primary key, this is required");
            }

            if (sourcePkList.Count != targetPkList.Count)
            {
                throw new ArgumentException("Source and Target primary key are not comparable, this is required");
            }

            if (sourceColumns.Count() != targetColumns.Count())
            {
                throw new ArgumentException("Source and target does not have same number of columns");
            }

            for (int i = 0; i < sourceColumns.Count(); i++)
            {
                if (sourceColumns[i].ShortType != targetColumns[i].ShortType)
                {
                    throw new ArgumentException(string.Format("The columm {0} does not have the expected type {1} in the target table", sourceColumns[i].ColumnName, sourceColumns[i].ShortType));
                }
            }

            string sourcePkSort = string.Empty;
            string targetPkSort = string.Empty;
            
            for(int i = 0; i < sourceColumns.Count(); i++)
            {
                if(sourcePkList.Contains(sourceColumns[i].ColumnName))
                {
                    string prefix = (sourcePkSort == string.Empty) ? "" : ", ";
                    sourcePkSort += prefix + sourceColumns[i].ColumnName;
                    targetPkSort += prefix + targetColumns[i].ColumnName;
                }
            }

            //two arrays in the same order, now just compare them
            DataRow[] targetRows = targetRepository.GetDataFromTable(targetTable, targetColumns).Select(null, targetPkSort);
            int targetRow = 0;
            foreach(DataRow sourceRow in sourceRepository.GetDataFromTable(sourceTable, sourceColumns).Select(null, sourcePkSort))
            {
                //compare
                int pkCompare = 0;

                string whereClause = string.Empty;
                if (targetRow < targetRows.Count())
                {
                    for (int i = 0; i < sourcePkList.Count; i++)
                    {
                        if (whereClause.Length > 0)
                            whereClause += " AND ";
                        whereClause += String.Format(" [{0}] = {1}", targetPkList[i], generator.SqlFormatValue(targetTable, sourcePkList[i], targetRows[targetRow][sourcePkList[i]].ToString()));
                    }
                    if (whereClause.Length > 0)
                        whereClause += ";";
                }
                while (targetRow < targetRows.Count()
                    && (pkCompare = CompareDataRows(sourceRow, sourcePkList, targetRows[targetRow], targetPkList)) > 0)
                {
                    sb.AppendLine(String.Format("DELETE FROM [{0}] WHERE {1}", targetTable, whereClause));
                    sb.AppendLine("GO");
                    targetRow++;
                    whereClause = string.Empty;
                    for (int i = 0; i < sourcePkList.Count; i++)
                    {
                        if (whereClause.Length > 0)
                            whereClause += " AND ";
                        whereClause += String.Format(" [{0}] = {1}", targetPkList[i], generator.SqlFormatValue(targetTable, sourcePkList[i], targetRows[targetRow][sourcePkList[i]].ToString()));
                    }
                    if (whereClause.Length > 0)
                        whereClause += ";";
                }
                if (targetRow >= targetRows.Count() || pkCompare < 0)
                {
                    sb.AppendLine(generator.GenerateInsertFromDataRow(targetTable, sourceRow));
                    targetRow++;
                }
                else if (CompareDataRows(sourceRow, null, targetRows[targetRow], null) != 0)
                {
                    sb.AppendLine(String.Format("UPDATE [{0}] SET {1} WHERE {2}", targetTable, generator.GenerateUpdateFromDataRow(targetTable, sourceRow), whereClause));
                    sb.AppendLine("GO");
                }
                targetRow++;
                
            }
            return sb.ToString();
        }

        private static int CompareDataRows(DataRow a, IList<string> aFields, DataRow b, IList<string> bFields)
        {
            if (aFields != null && bFields != null)
            {
                if ((aFields != bFields && a.ItemArray.Count() != b.ItemArray.Count()) ||
                    aFields.Count != bFields.Count)
                    throw new ArgumentException("The field count has to be the same in order to compare the DataRows.");
            }
            if (aFields != null && bFields != null)
            {
                for (int i = 0; i < aFields.Count; i++)
                {
                    int comparisonResult = CompareObject(a[aFields[i]], b[bFields[i]]);
                    if (comparisonResult != 0)
                        return comparisonResult;
                    continue;
                }
                return 0;
            }
            else//compare all fields
            {
                object[] aArr = a.ItemArray, bArr = b.ItemArray;
                for (int i = 0; i < aArr.Count(); i++)

                {
                    int comparisonResult = CompareObject(aArr[i], bArr[i]);
                    if (comparisonResult != 0)
                        return comparisonResult;
                    continue;
                }
                return 0;
            }
        }

        private static int CompareObject(object aO, object bO)
        {
            if (aO is DBNull && bO is DBNull)
                return 0;
            if (aO is DBNull)
                return +1;
            if (bO is DBNull)
                return -1;

            if (aO is IComparable && bO is IComparable)
            {
                if (!aO.GetType().IsAssignableFrom(bO.GetType()))
                    throw new ArgumentException("The field types do not match.");
                return ((IComparable)aO).CompareTo(bO);
            }
            throw new ArgumentException("Not all fields implement IComparable.");
        }


    }
}
