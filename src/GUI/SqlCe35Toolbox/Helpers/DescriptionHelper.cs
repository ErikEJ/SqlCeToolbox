using System;
using System.Collections.Generic;
using ErikEJ.SqlCeScripting;
using System.Data;
using System.Linq;

namespace ErikEJ.SqlCeToolbox.Helpers
{
    internal class DescriptionHelper
    {
        private const string TableName = "__ExtendedProperties";

        private const string CreateScript =
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
        private const string InsertScript =
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

        private const string UpdateScript =
@"UPDATE [__ExtendedProperties]
    SET [Value] = '{0}'
    WHERE {1};
GO
";

        private const string SelectScript =
@"SELECT [ObjectName],
         [ParentName],
         [Value]
  FROM [__ExtendedProperties];
GO
";

        private void AddDescription(string description, string parentName, string objectName, DatabaseInfo databaseInfo)
        {
            if (string.IsNullOrWhiteSpace(description))
                return;

            using (IRepository repo = Helpers.RepositoryHelper.CreateRepository(databaseInfo))
            {
                CreateExtPropsTable(repo);
                string sql = string.Format(InsertScript, 
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
            using (IRepository repo = Helpers.RepositoryHelper.CreateRepository(databaseInfo))
            {
                string where = (objectName == null ? "[ObjectName] IS NULL AND " : "[ObjectName] = '" + objectName + "' AND");
                where += (parentName == null ? "[ParentName] IS NULL AND [Type] = 0" : "[ParentName] = '" + parentName + "' AND [Type] = 0");
                repo.ExecuteSql(string.Format(UpdateScript, description, where));
            }

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
            GetDescriptions(databaseInfo);
        }

        public List<DbDescription> GetDescriptions(DatabaseInfo databaseInfo)
        {
            var list = new List<DbDescription>();
            using (IRepository repo = Helpers.RepositoryHelper.CreateRepository(databaseInfo))
            {
                var tlist = repo.GetAllTableNames();
                if (tlist.Contains(TableName))
                {
                    var ds = repo.ExecuteSql(SelectScript);
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
            if (!list.Contains(TableName))
                repo.ExecuteSql(CreateScript);
        }

    }

}
