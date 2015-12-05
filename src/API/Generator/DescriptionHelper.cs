using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Reflection;
using ErikEJ.SqlCeScripting;
using Microsoft.Win32;
using System.Data;
using System.Linq;

namespace ErikEJ.SqlCeScripting
{
    public class DescriptionHelper
    {
        private const string tableName = "__ExtendedProperties";

        private const string selectScript =
@"SELECT [ObjectName],
         [ParentName],
         [Value]
  FROM [__ExtendedProperties];
GO
";

        public List<DbDescription> GetDescriptions(IRepository repository)
        {
            var list = new List<DbDescription>();
            string res = string.Empty;
            var tlist = repository.GetAllTableNames();
            if (tlist.Contains(tableName))
            {
                var ds = repository.ExecuteSql(selectScript);
                if (ds.Tables.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        var dbDesc = new DbDescription();
                        dbDesc.Object = row[0] == DBNull.Value ? null : row[0].ToString();
                        dbDesc.Parent = row[1] == DBNull.Value ? null : row[1].ToString();
                        dbDesc.Description = row[2] == DBNull.Value ? null : row[2].ToString();
                        list.Add(dbDesc);
                    }
                }
            }
            return list;
        }

    }

}
