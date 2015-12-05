using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ErikEJ.SqlCeToolbox
{
    public class TableDataType
    {
        //Name, FixedLength (bool), MinLength, Maxlength, DefaultLength,
        //FixedAllowNulls, FixedIdentity

        public string Name { get; set; }
        public bool FixedLength { get; set; }
        public short MinLength { get; set; }
        public short MaxLength { get; set; }
        public short DefaultLength { get; set; }
        public bool FixedAllowNulls { get; set; }
        public bool FixedIdentity { get; set; }

        private static Dictionary<string, TableDataType> getAll;
        static TableDataType()
        {
            getAll = new Dictionary<string, TableDataType>()
                {
{ "bigint", new TableDataType { Name = "bigint", DefaultLength = 8, FixedLength = true, FixedAllowNulls = false, FixedIdentity = false } },
{ "binary", new TableDataType { Name = "binary", DefaultLength = 100, FixedLength = false, MinLength = 1, MaxLength = 8000, FixedAllowNulls = false, FixedIdentity = true } },
{ "bit", new TableDataType { Name = "bit", DefaultLength = 1, FixedLength = true, FixedAllowNulls = false, FixedIdentity = true } },
{ "datetime", new TableDataType { Name = "datetime", DefaultLength = 8, FixedLength = true, FixedAllowNulls = false, FixedIdentity = true } },
{ "float", new TableDataType { Name = "float", DefaultLength = 8, FixedLength = true, FixedAllowNulls = false, FixedIdentity = true } },
{ "image", new TableDataType { Name = "image", DefaultLength = 16, FixedLength = true, FixedAllowNulls = false, FixedIdentity = true } },
{ "int", new TableDataType { Name = "int", DefaultLength = 4, FixedLength = true, FixedAllowNulls = false, FixedIdentity = false } },
{ "money", new TableDataType { Name = "money", DefaultLength = 8, FixedLength = true, FixedAllowNulls = false, FixedIdentity = true } },
{ "nchar", new TableDataType { Name = "nchar", DefaultLength = 100, FixedLength = false, MinLength = 1, MaxLength = 4000, FixedAllowNulls = false, FixedIdentity = true } },
{ "ntext", new TableDataType { Name = "ntext", DefaultLength = 16, FixedLength = true, FixedAllowNulls = false, FixedIdentity = true } },
{ "numeric", new TableDataType { Name = "numeric", DefaultLength = 19, FixedLength = true, FixedAllowNulls = false, FixedIdentity = true } },
{ "nvarchar", new TableDataType { Name = "nvarchar", DefaultLength = 100, FixedLength = false, MinLength = 1, MaxLength = 4000, FixedAllowNulls = false, FixedIdentity = true } },
{ "real", new TableDataType { Name = "real", DefaultLength = 4, FixedLength = true, FixedAllowNulls = false, FixedIdentity = true } },
{ "rowversion", new TableDataType { Name = "rowversion", DefaultLength = 8, FixedLength = true, FixedAllowNulls = true, FixedIdentity = true } },
{ "smallint", new TableDataType { Name = "smallint", DefaultLength = 2, FixedLength = true, FixedAllowNulls = false, FixedIdentity = true } },
{ "tinyint", new TableDataType { Name = "tinyint", DefaultLength = 1, FixedLength = true, FixedAllowNulls = false, FixedIdentity = true } },
{ "uniqueidentifier", new TableDataType { Name = "uniqueidentifier", DefaultLength = 16, FixedLength = true, FixedAllowNulls = false, FixedIdentity = true } },
{ "varbinary", new TableDataType { Name = "varbinary", DefaultLength = 100, FixedLength = false, MinLength = 1, MaxLength = 8000, FixedAllowNulls = false, FixedIdentity = true } }
                };
        }

        public static Dictionary<string, TableDataType> GetAll
        {
            get
            {
                return getAll;
            }
        }

        public static bool IsLengthFixed(string dataType)
        {
            if (string.IsNullOrEmpty(dataType))
                return false;
            if (GetAll.ContainsKey(dataType))
                return GetAll[dataType].FixedLength;
            return false;
        }

        public static bool IsNullFixed(string dataType)
        {
            if (string.IsNullOrEmpty(dataType))
                return false;
            if (GetAll.ContainsKey(dataType))
                return GetAll[dataType].FixedAllowNulls;
            return false;
        }

        public static short GetDefaultLength(string dataType)
        {
            if (string.IsNullOrEmpty(dataType))
                return 1;
            if (GetAll.ContainsKey(dataType))
                return GetAll[dataType].DefaultLength;
            return 1;
        }

        public static short GetMaxLength(string dataType)
        {
            if (string.IsNullOrEmpty(dataType))
                return 1;
            if (GetAll.ContainsKey(dataType))
                return GetAll[dataType].MaxLength;
            return 1;
        }

        public static short GetMinLength(string dataType)
        {
            if (string.IsNullOrEmpty(dataType))
                return 1;
            if (GetAll.ContainsKey(dataType))
                return GetAll[dataType].MinLength;
            return 1;
        }

    }
}
