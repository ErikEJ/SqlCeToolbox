using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ErikEJ.SqlCeScripting;

namespace ErikEJ.SqlCeToolbox.Helpers
{
    public class TableColumn : BindableBase
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value, "Name"); }
        }

        private string _dataType;
        public string DataType
        {
            get { return _dataType; }
            set { SetProperty(ref _dataType, value, "DataType"); }
        }

        private string _defaultValue;
        public string DefaultValue
        {
            get { return _defaultValue; }
            set { SetProperty(ref _defaultValue, value, "DefaultValue"); }
        }

        private short _length;
        public short Length
        {
            get { return _length; }
            set { SetProperty(ref _length, value, "Length"); }
        }

        private bool _allowNull;
        public bool AllowNull
        {
            get { return _allowNull; }
            set { SetProperty(ref _allowNull, value, "AllowNull"); }
        }

        private bool _identity;
        public bool Identity
        {
            get { return _identity; }
            set { SetProperty(ref _identity, value, "Identity"); }
        }

        private bool _primaryKey;
        public bool PrimaryKey
        {
            get { return _primaryKey; }
            set { SetProperty(ref _primaryKey, value, "PrimaryKey"); }
        }

        private byte _precision;
        public byte Precision
        {
            get { return _precision; }
            set { SetProperty(ref _precision, value, "Precision"); }
        }

        private byte _scale;
        public byte Scale
        {
            get { return _scale; }
            set { SetProperty(ref _scale, value, "Scale"); }
        }

        public static ObservableCollection<TableColumn> GetAll => new ObservableCollection<TableColumn>()
        {
            new TableColumn { Name = "Id", DataType = "int", Length = 4, AllowNull = false, PrimaryKey = true, Identity = true }
        };

        public static ObservableCollection<TableColumn> GetNew => new ObservableCollection<TableColumn>()
        {
            new TableColumn { Name = "Col", DataType = "nvarchar", Length = 25, AllowNull = true, PrimaryKey = false, Identity = false }
        };

        public static ObservableCollection<TableColumn> GetAllSqlite => new ObservableCollection<TableColumn>()
        {
            new TableColumn { Name = "Id", DataType = "INTEGER", Length = 8, AllowNull = false, PrimaryKey = true, Identity = true }
        };

        public static ObservableCollection<TableColumn> GetNewSqlite => new ObservableCollection<TableColumn>()
        {
            new TableColumn { Name = "Col", DataType = "TEXT", Length = 25, AllowNull = true, PrimaryKey = false, Identity = false }
        };

        public static List<TableColumn> BuildTableColumns(List<Column> columns, string tableName)
        {
            var list = new List<TableColumn>();
            foreach (var item in columns)
            {
                var col = new TableColumn
                {
                    AllowNull = item.IsNullable == YesNoOption.YES,
                    DataType = item.DataType,
                    DefaultValue = item.ColumnDefault,
                    Identity = item.AutoIncrementBy > 1,
                    Name = item.ColumnName,
                    Precision = byte.Parse(item.NumericPrecision.ToString()),
                    //TODO Detect!
                    PrimaryKey = false,
                    Scale = byte.Parse(item.NumericScale.ToString())
                };
                short colLength;
                if (short.TryParse(item.CharacterMaxLength.ToString(), out colLength))
                    col.Length = colLength;
                if (TableDataType.IsLengthFixed(item.DataType))
                {
                    col.Length = TableDataType.GetDefaultLength(item.DataType);
                }
                list.Add(col);
            }
            return list;            
        }


        public static List<Column> BuildColumns(List<TableColumn> columns, string tableName)
        {
            var list = new List<Column>();
            foreach (var item in columns)
            {
                var col = new Column
                {
                    AutoIncrementBy = item.Identity ? 1 : 0,
                    AutoIncrementSeed = item.Identity ? 1 : 0,
                    CharacterMaxLength = item.Length,
                    ColumnHasDefault = !string.IsNullOrEmpty(item.DefaultValue) && !item.Identity,
                    ColumnDefault = item.DefaultValue,
                    ColumnName = item.Name,
                    DataType = item.DataType,
                    DateFormat = DateFormat.DateTime,
                    IsNullable = item.AllowNull ? YesNoOption.YES : YesNoOption.NO,
                    NumericPrecision = item.Precision,
                    NumericScale = item.Scale,
                    TableName = tableName
                };
                list.Add(col);
            }
            return list;
        }

        public static string BuildPkScript(List<TableColumn> columns, string tableName)
        {
            var script = string.Empty;
            var pkCols = columns.Where(x => x.PrimaryKey).ToList();
            if (pkCols.Count > 0)
            {
                string cols = string.Empty;
                foreach (var col in pkCols)
                {
                    cols += string.Format("[{0}],", col.Name);
                }
                cols = cols.Substring(0, cols.Length - 1);
                script = string.Format("ALTER TABLE [{0}] ADD CONSTRAINT [PK_{1}] PRIMARY KEY ({2});", tableName, tableName, cols);
                script += string.Format("{0}GO", Environment.NewLine);
            }
            return script;
        }

        public static string ValidateColumns(List<TableColumn> columns)
        {
            foreach (var item in columns)
            {
                if (string.IsNullOrEmpty(item.DataType))
                {
                    return "Data type is required";
                }
                if (string.IsNullOrEmpty(item.Name))
                {
                    return "Column name is required";
                }
                if (!TableDataType.IsLengthFixed(item.DataType))
                {
                    if (item.Length < TableDataType.GetMinLength(item.DataType)
                        || item.Length > TableDataType.GetMaxLength(item.DataType))
                    {
                        return string.Format("Column length of colum {0} must be between {1} and {2}",
                            item.Name, TableDataType.GetMinLength(item.DataType), TableDataType.GetMaxLength(item.DataType));
                    }
                }
                //Check for duplicates
                if (columns.Select(x => x.Name).ToList()
                    .GroupBy(x => x)
                    .Where(x => x.Count() > 1)
                    .Select(x => x.Key).ToList().Count > 0)
                {
                    return "Duplicate column names not allowed";
                }
                //Check for only 1 identity column
                var identityCols = columns.Where(x => x.Identity).ToList();
                if (identityCols.Count > 1)
                {
                    return "Only a single Identity column allowed";
                }
            }
            return string.Empty;
        }
    }
}
