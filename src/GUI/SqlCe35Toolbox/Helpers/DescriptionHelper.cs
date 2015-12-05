using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Reflection;
using ErikEJ.SqlCeScripting;
using Microsoft.VisualStudio.Data;
using Microsoft.VisualStudio.Data.AdoDotNet;
using Microsoft.VisualStudio.Data.Interop;
using Microsoft.Win32;
using EnvDTE;
using System.Data;
using System.Linq;

namespace ErikEJ.SqlCeToolbox.Helpers
{
    internal class DescriptionHelper
    {
        private const string tableName = "__ExtendedProperties";

        private const string createScript =
@"CREATE TABLE __ExtendedProperties
(
[Id] [int] NOT NULL IDENTITY,
[Type] [int] NOT NULL DEFAULT 0,
[ParentName] nvarchar(128) NULL,
[ObjectName] nvarchar(128) NULL,
[Value] nvarchar(4000) NOT NULL
);
GO
CREATE INDEX [__ExtendedProperties_ObjectName_ParentName] ON [__ExtendedProperties] ([ObjectName], [ParentName]);
GO
";
        private const string insertScript =
@"INSERT INTO [__ExtendedProperties]
           ([Type]
           ,[ParentName]
           ,[ObjectName]
           ,[Value])
     VALUES
           (0
           ,{0}
           ,{1}
           ,'{2}');
GO
";

        private const string updateScript =
@"UPDATE [__ExtendedProperties]
    SET [Value] = '{0}'
    WHERE {1};
GO
";

        private const string selectScript =
@"SELECT [ObjectName],
         [ParentName],
         [Value]
  FROM [__ExtendedProperties];
GO
";


        private const string selectSingleScript =
@"SELECT [Value]
  FROM [__ExtendedProperties]
  WHERE {0};
GO
";

        private void AddDescription(string description, string parentName, string objectName, DatabaseInfo databaseInfo)
        {
            if (string.IsNullOrWhiteSpace(description))
                return;

            using (IRepository repo = Helpers.DataConnectionHelper.CreateRepository(databaseInfo))
            {
                CreateExtPropsTable(repo);
                string sql = string.Format(insertScript, 
                    (parentName == null ? "NULL" : "'" + parentName + "'"),
                    (objectName == null ? "NULL" : "'" + objectName + "'"),
                    description.Replace("'", "''"));
                repo.ExecuteSql(sql);
            }
        
        }

        private void UpdateDescription(string description, string parentName, string objectName, DatabaseInfo databaseInfo)
        {
            if (string.IsNullOrWhiteSpace(description))
                return;

            description = description.Replace("'", "''");
            using (IRepository repo = Helpers.DataConnectionHelper.CreateRepository(databaseInfo))
            {
                string where = (objectName == null ? "[ObjectName] IS NULL AND " : "[ObjectName] = '" + objectName + "' AND");
                where += (parentName == null ? "[ParentName] IS NULL AND [Type] = 0" : "[ParentName] = '" + parentName + "' AND [Type] = 0");
                var ds = repo.ExecuteSql(string.Format(updateScript, description, where));
            }

        }
    
        /// <summary>
        /// This will only be called if the caller (the tree list) knows that the table exists
        /// </summary>
        /// <param name="databaseInfo"></param>
        /// <returns></returns>
        private string GetDescription(string parentName, string objectName, DatabaseInfo databaseInfo)
        {
            string res = string.Empty;
            using (IRepository repo = Helpers.DataConnectionHelper.CreateRepository(databaseInfo))
            {
                string where = (objectName == null ? "[ObjectName] IS NULL" : "[ObjectName] = '" + objectName + "' AND");
                where += (parentName == null ? "[ParentName] IS NULL" : "[ParentName] = '" + parentName + "' AND [Type] = 0");
                var ds = repo.ExecuteSql(string.Format(selectSingleScript, where));
                if (ds.Tables.Count > 0)
                {
                    return ds.Tables[0].Rows[0][0].ToString();
                }
            }
            return res;
        }

        public void SaveDescription(DatabaseInfo databaseInfo,List<DbDescription> cache, string description, string parentName, string objectName)
        {
            DbDescription desc = null;
            if (cache != null)
                desc = cache.Where(c => c.Object == objectName && c.Parent == parentName).SingleOrDefault();
            if (desc != null)
            {
                UpdateDescription(description, parentName, objectName, databaseInfo);
            }
            else
            {
                AddDescription(description, parentName, objectName, databaseInfo);
            }
            cache = GetDescriptions(databaseInfo);
        }

        public List<DbDescription> GetDescriptions(DatabaseInfo databaseInfo)
        {
            var list = new List<DbDescription>();
            string res = string.Empty;
            using (IRepository repo = Helpers.DataConnectionHelper.CreateRepository(databaseInfo))
            {
                var tlist = repo.GetAllTableNames();
                if (tlist.Contains(tableName))
                {
                    var ds = repo.ExecuteSql(selectScript);
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
            }
            return list;
        }


        private static void CreateExtPropsTable(IRepository repo)
        {
            var list = repo.GetAllTableNames();
            if (!list.Contains(tableName))
                repo.ExecuteSql(createScript);
        }

    }

}
