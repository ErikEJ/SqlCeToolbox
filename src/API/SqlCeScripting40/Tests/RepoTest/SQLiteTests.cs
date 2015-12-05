using System;
using NUnit.Framework;
using System.Data;
using System.Collections.Generic;
using ErikEJ.SQLiteScripting;
using ErikEJ.SqlCeScripting;

public class SQLiteScriptingTests
{
    private const string dbPath = @"C:\Users\Erik\SkyDrive\Dokumenter\Code\SQLCE\exportsqlce\SqlCeScripting40\Tests\";

    private string chinookConnectionString = string.Format(
            @"Data Source={0}chinook.db", dbPath);

    private string infoConnectionString = string.Format(
        @"Data Source={0}inf2700_orders-1.db", dbPath);
    
    private string dtoConnectionString = string.Format(
        @"Data Source={0}dto.db", dbPath);

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
        Assert.IsTrue(list.Count == 64);
        Assert.IsTrue(list[0].DataType == "integer");
    }

    [Test]
    public void TestGetAllColumns2()
    {
        var list = new List<Column>();
        using (IRepository repo = new SQLiteRepository(infoConnectionString))
        {
            list = repo.GetAllColumns();
        }
        Assert.IsTrue(list.Count == 68);
        Assert.IsTrue(list[0].DataType == "integer");
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
        Assert.IsTrue(list.Count == 12);
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
        Assert.IsTrue(list.Count == 12);
        Assert.IsTrue(list[0].KeyName == "CUSTOMER219ORDERS");
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
                Assert.IsTrue(reader.GetValue(0).GetType() == typeof(long));
                Assert.IsTrue(reader.GetValue(2).GetType() == typeof(long));
            }
        }
    }

}
