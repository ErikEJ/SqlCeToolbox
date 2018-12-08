using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ErikEJ.SqlCeScripting
{
    public class TableParameter
    {
        public string TableName { get; set; }
        public string WhereClause { get; set; }

        public TableParameter(string tableName, string whereClause = null)
        {
            TableName = tableName;
            WhereClause = whereClause;
        }
    }
}
