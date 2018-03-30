using System;
using NUnit.Framework;
using System.Data;
using System.Collections.Generic;
using ErikEJ.SQLiteScripting;
using ErikEJ.SqlCeScripting;

public class SQLiteScriptingTests
{
    private const string dbPath = @"C:\Code\SqlCeToolbox\src\API\SqlCeScripting40\Tests\";

    private string chinookConnectionString = string.Format(
        @"Data Source={0}chinook.db", dbPath);

    private string infoConnectionString = string.Format(
        @"Data Source={0}inf2700_orders-1.db", dbPath);
    
    private string fkConnectionString = string.Format(
        @"Data Source={0}FkMultiKey.db", dbPath);

    private string viewsConnectionString = string.Format(
        @"Data Source={0}views.db", dbPath);

    private string noRowIdConnectionString = string.Format(
        @"Data Source={0}norowid.db", dbPath);

    private string viewCommentConnectionString = string.Format(
        @"Data Source={0}new3.db", dbPath);

    private string viewComputedColConnectionString = string.Format(
    @"Data Source={0}new4.db", dbPath);

    private string viewColBugConnectionString = string.Format(
    @"Data Source={0}SampleToEric.db", dbPath);

    private string testSchemaBugConnectionString = string.Format(
        @"Data Source={0}Test.db", dbPath);

    [Test]
    public void TestGetAllTableNames()
    {
        var list = new List<string>();
        using (IRepository repo = new SQLiteRepository(chinookConnectionString))
        {
            list = repo.GetAllTableNames();
        }
        Assert.IsTrue(list.Count == 11);
        Assert.IsTrue(list[0] == "Album");
    }

    [Test]
    public void TestGetAllTablesSchemaBug()
    {
        var list = new List<string>();
        using (IRepository repo = new SQLiteRepository(testSchemaBugConnectionString))
        {
            //var views = repo.GetAllViews();
            var cols = repo.GetAllViewColumns();
        }
        Assert.IsTrue(list.Count == 1);
    }


    [Test]
    public void TestGetAllTableNamesViewComments()
    {
        var list = new List<string>();
        using (IRepository repo = new SQLiteRepository(viewCommentConnectionString))
        {
            list = repo.GetAllTableNames();
        }
        Assert.IsTrue(list.Count == 1);
        Assert.IsTrue(list[0] == "MyTable");
    }

    [Test]
    public void TestGetAllTableNames2()
    {
        var list = new List<string>();
        using (IRepository repo = new SQLiteRepository(infoConnectionString))
        {
            list = repo.GetAllTableNames();
        }
        Assert.IsTrue(list.Count == 8);
    }

    [Test]
    public void TestGetAllColumns()
    {
        var list = new List<Column>();
        using (IRepository repo = new SQLiteRepository(chinookConnectionString))
        {
            list = repo.GetAllColumns();
        }
        Assert.AreEqual(67, list.Count);
        Assert.AreEqual("bigint", list[0].DataType);
    }

    //[Test]
    //public void TestGetAllColumnsWithDefault()
    //{
    //    var list = new List<Column>();
    //    using (IRepository repo = new SQLiteRepository(@"data source = C:\temp\schemaless.db"))
    //    {
    //        list = repo.GetAllColumns();
    //    }
    //    Assert.IsTrue(list.Count == 64);
    //    Assert.IsTrue(list[0].DataType == "integer");
    //}


    [Test]
    public void TestGetAllColumns2()
    {
        var list = new List<Column>();
        using (IRepository repo = new SQLiteRepository(infoConnectionString))
        {
            list = repo.GetAllColumns();
        }
        Assert.AreEqual(68, list.Count);
        Assert.AreEqual("bigint", list[0].DataType);
    }

    [Test]
    public void TestGetAllViews()
    {
        var list = new List<View>();
        using (IRepository repo = new SQLiteRepository(infoConnectionString))
        {
            list = repo.GetAllViews();
        }
        Assert.IsTrue(list.Count == 3);
    }

    [Test]
    public void TestGetAllViewsViewComments()
    {
        var list = new List<View>();
        using (IRepository repo = new SQLiteRepository(viewCommentConnectionString))
        {
            list = repo.GetAllViews();
        }
        Assert.IsTrue(list.Count == 1);
    }

    [Test]
    public void TestGetAllViewColumnsViewComments()
    {
        var list = new List<Column>();
        using (IRepository repo = new SQLiteRepository(viewCommentConnectionString))
        {
            list = repo.GetAllViewColumns();
        }
        Assert.IsTrue(list.Count == 1);
    }

    [Test]
    public void TestGetAllViewColumnsComputed()
    {
        var list = new List<Column>();
        using (IRepository repo = new SQLiteRepository(viewComputedColConnectionString))
        {
            list = repo.GetAllViewColumns();
        }
        Assert.IsTrue(list.Count == 2);
    }

    [Test]
    public void TestGetAllViewColumnsBug()
    {
        var list = new List<Column>();
        using (IRepository repo = new SQLiteRepository(viewColBugConnectionString))
        {
            list = repo.GetAllViewColumns();
        }
        Assert.AreEqual(5, list.Count);
    }

    [Test]
    public void TestGetView()
    {
        var list = new List<View>();
        using (var repo = new SQLiteRepository(viewsConnectionString))
        {
            list = repo.GetAllViews();
        }
        Assert.IsTrue(list.Count == 1);
    }

    [Test]
    public void TestGetIndexesNoRowId()
    {
        var list = new List<Index>();
        using (var repo = new SQLiteRepository(noRowIdConnectionString))
        {
            list = repo.GetAllIndexes();
        }
        Assert.IsTrue(list.Count == 0);
    }

    [Test]
    public void TestGetAllViewColumns()
    {
        var list = new List<Column>();
        using (IRepository repo = new SQLiteRepository(infoConnectionString))
        {
            list = repo.GetAllViewColumns();
        }
        Assert.IsTrue(list.Count == 11);
    }

    [Test]
    public void TestGetAllTriggers()
    {
        var list = new List<Trigger>();
        using (IRepository repo = new SQLiteRepository(infoConnectionString))
        {
            list = repo.GetAllTriggers();
        }
        Assert.IsTrue(list.Count == 0);
    }

    [Test]
    public void TestGetAllPrimaryKeys()
    {
        var list = new List<PrimaryKey>();
        using (IRepository repo = new SQLiteRepository(chinookConnectionString))
        {
            list = repo.GetAllPrimaryKeys();
        }
        Assert.AreEqual(13, list.Count);
        Assert.IsTrue(list[0].KeyName == "sqlite_master_PK_Album");
    }

    [Test]
    public void TestGetAllPrimaryKeys2()
    {
        var list = new List<PrimaryKey>();
        using (IRepository repo = new SQLiteRepository(infoConnectionString))
        {
            list = repo.GetAllPrimaryKeys();
        }
        Assert.AreEqual(11, list.Count);
        Assert.AreEqual("CUSTOMER219ORDERS", list[0].KeyName);
    }

    [Test]
    public void TestGetAllForeignKeys()
    {
        var list = new List<ErikEJ.SqlCeScripting.Constraint>();
        using (IRepository repo = new SQLiteRepository(chinookConnectionString))
        {
            list = repo.GetAllForeignKeys();
        }
        Assert.IsTrue(list.Count == 11);
        Assert.IsTrue(list[0].ConstraintName == "FK_Album_0_0");
    }

    [Test]
    public void TestGetAllForeignKeysMultiColumnKey()
    {
        var list = new List<ErikEJ.SqlCeScripting.Constraint>();
        using (IRepository repo = new SQLiteRepository(fkConnectionString))
        {
            list = repo.GetAllForeignKeys();
        }
        Assert.IsTrue(list.Count == 1);
        Assert.IsTrue(list[0].ConstraintName == "FK_BEVERAGE_DIRECTORY_0_0");
    }

    [Test]
    public void TestGetAllIndexes()
    {
        var list = new List<Index>();
        using (IRepository repo = new SQLiteRepository(chinookConnectionString))
        {
            list = repo.GetAllIndexes();
        }
        Assert.IsTrue(list.Count == 22);
        Assert.IsTrue(list[0].IndexName == "IFK_AlbumArtistId");
    }

    [Test]
    public void TestGetAllIndexes2()
    {
        var list = new List<Index>();
        using (IRepository repo = new SQLiteRepository(infoConnectionString))
        {
            list = repo.GetAllIndexes();
        }
        Assert.IsTrue(list.Count == 0);
    }

    [Test]
    public void TestGetIndexesFromTable()
    {
        var list = new List<Index>();
        using (IRepository repo = new SQLiteRepository(chinookConnectionString))
        {
            list = repo.GetIndexesFromTable("Album");
        }
        Assert.IsTrue(list.Count == 2);
        Assert.IsTrue(list[1].IndexName == "IPK_Album");
    }

    [Test]
    public void TestDatabaseInfo()
    {
        var values = new List<KeyValuePair<string, string>>();
        using (IRepository repo = new SQLiteRepository(chinookConnectionString))
        {
            values = repo.GetDatabaseInfo();
        }
        Assert.IsTrue(values.Count == 4);
    }

    [Test]
    public void TestParse()
    {
        var sql = "SELECT * FROM Album;" + Environment.NewLine + "GO";
        var result = string.Empty;
        using (IRepository repo = new SQLiteRepository(chinookConnectionString))
        {
            result = repo.ParseSql(sql);
        }
        Assert.IsTrue(result.StartsWith("SCAN "));

        sql = "SELECT * FROM Album WHERE AlbumId = 1;" + Environment.NewLine + "GO";
        using (IRepository repo = new SQLiteRepository(chinookConnectionString))
        {
            result = repo.ParseSql(sql);
        }
        Assert.IsTrue(result.StartsWith("SEARCH TABLE "));

    }

    [Test]
    public void TestPragma()
    {
        var sql = "pragma table_info(Album);" + Environment.NewLine + "GO";
        DataSet result = null;
        using (IRepository repo = new SQLiteRepository(chinookConnectionString))
        {
            result = repo.ExecuteSql(sql);
        }
        Assert.IsTrue(result.Tables.Count == 1);
    }

    [Test]
    public void TestGetDataFromReader()
    {
        var columns = new List<Column> 
        {   new Column { ColumnName = "AlbumId"},
            new Column { ColumnName = "Title"},
            new Column { ColumnName = "ArtistId"},
        };       

        IDataReader reader = null;

        using (IRepository repo = new SQLiteRepository(chinookConnectionString))
        {
            reader = repo.GetDataFromReader("Album", columns);
            while (reader.Read())
            {
                Assert.IsTrue(reader.GetValue(0) is long);
                Assert.IsTrue(reader.GetValue(2) is long);
            }
        }
    }

}
