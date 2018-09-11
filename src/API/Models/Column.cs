using System;
using System.Collections.Generic;

namespace ErikEJ.SqlCeScripting
{
    public enum DateFormat
    { 
        None,
        DateTime,
        Date,
        DateTime2
    }

    public class Column
    {
        public string ColumnName { get; set; }
        public YesNoOption IsNullable { get; set; }
        public string DataType { get; set; }
        public string ServerDataType { get; set; }
        public int CharacterMaxLength { get; set; }
        public int NumericPrecision { get; set; }
        public int NumericScale { get; set; }
        public Int64 AutoIncrementBy { get; set; }
        public Int64 AutoIncrementSeed { get; set; }
        public Int64 AutoIncrementNext { get; set; }
        public bool ColumnHasDefault { get; set; }
        public string ColumnDefault { get; set; }
        public bool RowGuidCol { get; set; }
        public string TableName { get; set; }
        public DateFormat DateFormat { get; set; }
        public int Width { get; set; }
        public bool PadLeft { get; set; }
        public Int64 Ordinal { get; set; }
        public string ShortType
        {
            get

            {
                if (this.DataType == "nchar" || this.DataType == "nvarchar" || this.DataType == "binary" || this.DataType == "varbinary")
                {
                    return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}({1}),", this.DataType, this.CharacterMaxLength);
                }
                else if (this.DataType == "numeric")
                {
                    return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}({1},{2}),", this.DataType, this.NumericPrecision, this.NumericScale);
                }
                else
                {
                    return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},", this.DataType);
                }
            }
        }
        public bool IsCaseSensitivite { get; set; }
    }

    // Custom comparer for the Column class
    class ColumnComparer : IEqualityComparer<Column>
    {
        // Products are equal if their names and product numbers are equal.
        public bool Equals(Column x, Column y)
        {

            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            //Check whether the products' properties are equal.
            return x.ColumnName == y.ColumnName && x.TableName == y.TableName;
        }

        // If Equals() returns true for a pair of objects 
        // then GetHashCode() must return the same value for these objects.

        public int GetHashCode(Column column)
        {
            //Check whether the object is null
            if (Object.ReferenceEquals(column, null)) return 0;

            //Get hash code for the Name field if it is not null.
            int hashColumnName = column.ColumnName == null ? 0 : column.ColumnName.GetHashCode();

            //Get hash code for the Code field.
            int hashTableName = column.TableName.GetHashCode();

            //Calculate the hash code for the product.
            return hashColumnName ^ hashTableName;
        }

    }

}
