using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using ErikEJ.SqlCeScripting;

namespace ErikEJ.SqlCeToolbox
{
    public class TableColumn : BindableBase
    {
        private string _name;
        public string Name
        {
            get { return this._name; }
            set { this.SetProperty(ref this._name, value, "Name"); }
        }

        private string _dataType;
        public string DataType
        {
            get { return this._dataType; }
            set { this.SetProperty(ref this._dataType, value, "DataType"); }
        }

        private string _defaultValue;
        public string DefaultValue
        {
            get { return this._defaultValue; }
            set { this.SetProperty(ref this._defaultValue, value, "DefaultValue"); }
        }

        private short _length;
        public short Length
        {
            get { return this._length; }
            set { this.SetProperty(ref this._length, value, "Length"); }
        }

        private bool _allowNull;
        public bool AllowNull
        {
            get { return this._allowNull; }
            set { this.SetProperty(ref this._allowNull, value, "AllowNull"); }
        }

        private bool _identity;
        public bool Identity
        {
            get { return this._identity; }
            set { this.SetProperty(ref this._identity, value, "Identity"); }
        }

        private bool _primaryKey;
        public bool PrimaryKey
        {
            get { return this._primaryKey; }
            set { this.SetProperty(ref this._primaryKey, value, "PrimaryKey"); }
        }

        private byte _precision;
        public byte Precision
        {
            get { return this._precision; }
            set { this.SetProperty(ref this._precision, value, "Precision"); }
        }

        private byte _scale;
        public byte Scale
        {
            get { return this._scale; }
            set { this.SetProperty(ref this._scale, value, "Scale"); }
        }

        public static ObservableCollection<TableColumn> GetAll
        {
            get
            {
                return new ObservableCollection<TableColumn>()
                {
                  new TableColumn { Name = "Id", DataType = "int", Length = 4, AllowNull = false, PrimaryKey = true, Identity = true }
                };
            }
        }

        public static ObservableCollection<TableColumn> GetNew
        {
            get
            {
                return new ObservableCollection<TableColumn>()
                {
                  new TableColumn { Name = "Col", DataType = "nvarchar", Length = 25, AllowNull = true, PrimaryKey = false, Identity = false }
                };
            }
        }

        public static List<TableColumn> BuildTableColumns(List<Column> columns, string tableName)
        {
            var list = new List<TableColumn>();
            foreach (var item in columns)
            {
                var col = new TableColumn();
                col.AllowNull = item.IsNullable == YesNoOption.YES;
                col.DataType = item.DataType;
                col.DefaultValue = item.ColumnDefault;
                col.Identity = item.AutoIncrementBy > 1;
                Int16 colLength = 0;
                if (Int16.TryParse(item.CharacterMaxLength.ToString(), out colLength))
                    col.Length = colLength; 
                if (TableDataType.IsLengthFixed(item.DataType))
                {
                    col.Length = TableDataType.GetDefaultLength(item.DataType);
                }
                col.Name = item.ColumnName;
                col.Precision = Byte.Parse(item.NumericPrecision.ToString());
                //TODO Detect!
                col.PrimaryKey = false;
                col.Scale = Byte.Parse(item.NumericScale.ToString());
                list.Add(col);
            }
            return list;            
        }


        public static List<Column> BuildColumns(List<TableColumn> columns, string tableName)
        {
            var list = new List<Column>();
            foreach (var item in columns)
            {
                var col = new Column();
                col.AutoIncrementBy = item.Identity ? 1 : 0;
                col.AutoIncrementSeed = item.Identity ? 1 : 0;
                col.CharacterMaxLength = item.Length;
                col.ColumnHasDefault = item.DefaultValue != null && !item.Identity;
                col.ColumnDefault = item.DefaultValue;
                col.ColumnName = item.Name;
                col.DataType = item.DataType;
                col.DateFormat = DateFormat.DateTime;
                col.IsNullable = item.AllowNull ? YesNoOption.YES : YesNoOption.NO;
                col.NumericPrecision = item.Precision;
                col.NumericScale = item.Scale;
                col.TableName = tableName;
                list.Add(col);
            }
            return list;
        }

        public static string BuildPkScript(List<TableColumn> columns, string TableName)
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
                script = string.Format("ALTER TABLE [{0}] ADD CONSTRAINT [PK_{1}] PRIMARY KEY ({2});", TableName, TableName, cols);
                script += string.Format("{0}GO", System.Environment.NewLine);
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
