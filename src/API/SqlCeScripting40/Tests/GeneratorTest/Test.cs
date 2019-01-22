using System;
using System.IO;
using NUnit.Framework;
using ErikEJ.SqlCeScripting;
using System.Collections.Generic;
using ErikEJ.SQLiteScripting;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Text;

namespace Tests.GeneratorTest
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Ce"), TestFixture]
    public class SqlCeScriptingTestFixture
    {
        private enum SchemaType
        {
            NoConstraints,
            FullConstraints,
            FullNoIdentity,
            DataReaderTest
        }

        private static string dbPath = Directory.GetCurrentDirectory();

        private string chinookSQLiteConnectionString = $"Data Source={Path.Combine(dbPath, "chinook.db")}";
        private string dtoConnectionString = $"Data Source={Path.Combine(dbPath, "dto.db")}";
        private string infoConnectionString = $"Data Source={Path.Combine(dbPath, "inf2700_orders-1.db")}";
        private string db21Conn = $"Data Source={Path.Combine(dbPath, "DB21.sqlite")}";
        private string northwindConn = $"Data Source={Path.Combine(dbPath, "Northwind.sdf")}";
        private string umbracoConn = $"Data Source={Path.Combine(dbPath, "UmbracoSqlCe.sdf")}";

        private const string sdfConnectionString = @"Data Source=C:\data\sqlce\test\ams40.sdf;Max Database Size=512";
        private const string sdfConnectionString2 = @"Data Source=C:\data\sqlce\test\PFIZER_DB40.sdf";
        private const string serverConnectionString = @"data source=.;Initial Catalog=AdventureWorksLT2012;Integrated Security=true";
        private const string serverAWConnectionString = @"data source=(localdb)\Mssqllocaldb;Initial Catalog=AdventureWorks2014;Integrated Security=true";
        private const string serverDefConnectionString = @"data source=(localdb)\Mssqllocaldb;Initial Catalog=DefaultValuesTest;Integrated Security=true";
        private const string serverHIDConnectionString = @"data source=.;Initial Catalog=HID;Integrated Security=true";
        private const string serverApiTestConnectionString = @"data source=.;Initial Catalog=SqlCeApiTester;Integrated Security=true";
        private const string BaseballTestConnectionString = @"data source=.;Initial Catalog=BaseballStats;Integrated Security=true";
        private const string E2EConnectionString = @"data source=(localdb)\mssqllocaldb;Initial Catalog=E2E;Integrated Security=true";
        private const string chinookConnectionString = @"Data Source=C:\projects\Chinook\Chinook40.sdf;";
        private const string migrateConnectionString = @"data source=.\SQL2008R2;Initial Catalog=MigrateTest;Integrated Security=true";

        [Test]
        public void TestGetAllColumns()
        {
            var list = new List<Column>();
            using (IRepository repo = new DB4Repository(northwindConn))
            {
                list = repo.GetAllColumns();
            }
            Assert.AreEqual(74, list.Count);
            Assert.AreEqual("int", list[0].DataType);
        }

        [Test]
        public void TestServerMigration()
        {
            string path = @"C:\temp\testChinook40.sqlce";
            using (IRepository sourceRepository = new DB4Repository(chinookConnectionString))
            {
                var generator = new Generator4(sourceRepository, path);
                generator.GenerateAllAndSave(true, false, false, false);
            }
            Assert.IsTrue(System.IO.File.Exists(path));
            using (IRepository serverRepository = new ServerDBRepository4(migrateConnectionString))
            {
                serverRepository.ExecuteSqlFile(path);
            }
        }

        [Test]
        public void TestServerExport()
        {
            string path = @"C:\temp\testAW2012.sqlce";
            using (IRepository sourceRepository = new ServerDBRepository4(serverAWConnectionString))
            {
                var generator = new Generator4(sourceRepository, path);
                generator.ScriptDatabaseToFile(Scope.SchemaData);
            }
        }

        [Test]
        public void TestExportToSqlServer()
        {
            var path = @"C:\temp\testnw.sql";
            using (var sourceRepository = new DB4Repository(northwindConn))
            {
                var generator = new Generator4(sourceRepository, path);
                generator.ScriptDatabaseToFile(Scope.SchemaData);
            }
        }

        [Test]
        public void TestExportToSqlServer2()
        {
            var path = @"C:\temp\testum.sql";
            using (var sourceRepository = new DB4Repository(umbracoConn))
            {
                var generator = new Generator4(sourceRepository, path, false, false, false);
                generator.ExcludeTables(new List<string>());
                generator.ScriptDatabaseToFile(Scope.SchemaData);
            }
        }

        [Test]
        public void TestServerExportToSqlite()
        {
            var path = @"C:\temp\testAW2012.sql";
            using (var sourceRepository = new ServerDBRepository4(serverDefConnectionString, true))
            {
                var generator = new Generator4(sourceRepository, path, false, false, true);
                generator.ExcludeTables(new List<string>());
                generator.ScriptDatabaseToFile(Scope.SchemaDataSQLite);
            }
        }

        [Test]
        public void TestServerExportDotInTable()
        {
            string path = @"C:\temp\API2012.sqlce";
            using (IRepository sourceRepository = new ServerDBRepository4(serverApiTestConnectionString))
            {
                var generator = new Generator4(sourceRepository, path);
                generator.ExcludeTables(new List<string>());
            }
        }

        [Test]
        public void TestServerExportConstrainsIssue()
        {
            string path = @"C:\temp\Baseball.sqlce";
            using (IRepository sourceRepository = new ServerDBRepository4(BaseballTestConnectionString))
            {
                var generator = new Generator4(sourceRepository, path);
                generator.ExcludeTables(new List<string>());
            }
        }

        [Test]
        public void TestServerExportUniqueIssue()
        {
            string path = @"C:\temp\E2E.sqlce";
            using (IRepository sourceRepository = new ServerDBRepository4(E2EConnectionString))
            {
                var generator = new Generator4(sourceRepository, path);
                generator.ExcludeTables(new List<string>());
                generator.GenerateTableScript("OneToOneSeparateFKDependent");
                var test = generator.GeneratedScript;
            }
        }

        [Test]
        public void TestCreateSQLiteTable()
        {
            using (IRepository sourceRepository = new SQLiteRepository(chinookSQLiteConnectionString))
            {
                var generator = new Generator4(sourceRepository, null, false, false, true);
                generator.GenerateTableScript("Album");
                var result = generator.GeneratedScript;
                var lines = Regex.Split(result, "\r\n|\r|\n");
                Assert.AreEqual(10, lines.Length, result);
            }
        }

        [Test]
        public void TestCreateSqliteUneditableTable()
        {
            using (IRepository sourceRepository = new SQLiteRepository(db21Conn))
            {
                var generator = new Generator4(sourceRepository, null, false, false, true);
                var tables = sourceRepository.GetAllTableNames();
                generator.GenerateTableScript("Users");
                var result = generator.GeneratedScript;
                var lines = Regex.Split(result, "\r\n|\r|\n");
                Assert.AreEqual(18, lines.Length, result);
            }
        }

        [Test]
        public void TestSQLiteNetCodeGen()
        {
            using (IRepository sourceRepository = new SQLiteRepository(chinookSQLiteConnectionString))
            {
                var generator = new Generator4(sourceRepository, null, false, false, true);
                generator.GenerateSqliteNetModel("Test");
                var result = generator.GeneratedScript;
                var lines = Regex.Split(result, "\r\n|\r|\n");
                Assert.AreEqual(291, lines.Length, result);
            }
        }

        [Test]
        public void TestSQLiteNetCodeGenWithViews()
        {
            using (IRepository sourceRepository = new SQLiteRepository(infoConnectionString))
            {
                var generator = new Generator4(sourceRepository, null, false, false, true);
                generator.GenerateSqliteNetModel("Test");
                var result = generator.GeneratedScript;
                var lines = Regex.Split(result, "\r\n|\r|\n");
                Assert.AreEqual(264, lines.Length);
            }
        }

        [Test]
        public void ExerciseEngineWithTable()
        {
            using (IRepository sourceRepository = new DB4Repository(sdfConnectionString))
            {
                var generator = new Generator4(sourceRepository);
                using (IRepository targetRepository = new ServerDBRepository4(serverConnectionString))
                {
                    SqlCeDiff.CreateDiffScript(sourceRepository, targetRepository, generator, false);
                }
            }
        }

        [Test]
        public void TestSqlText()
        {
            string sql = @"-- Script Date: 13-03-2012 20:03  - Generated by ExportSqlCe version 3.5.2.7
-- Database information:
-- Locale Identifier: 1030
-- Encryption Mode: 
-- Case Sensitive: False
-- Database: C:\data\sqlce\test\nw40.sdf
-- ServerVersion: 4.0.8854.1
-- DatabaseSize: 1499136
-- Created: 11-07-2010 10:46

-- User Table information:
-- Number of tables: 9
-- Categories: 8 row(s)
-- Customers: 91 row(s)
-- ELMAH: -1 row(s)
-- Employees: 15 row(s)
-- Order Details: 2820 row(s)
-- Orders: 1078 row(s)
-- Products: 77 row(s)
-- Shippers: 15 row(s)
-- Suppliers: 29 row(s)

SET IDENTITY_INSERT [Suppliers] ON;
GO
INSERT INTO [Suppliers] ([Supplier ID],[Company Name],[Contact Name],[Contact Title],[Address],[City],[Region],[Postal Code],[Country],[Phone],[Fax]) VALUES (1,N'Exotic Liquids',N'Charlotte Cooper',N'Purchasing Manager',N'49 Gilbert St.',N'London',null,N'EC1 4SD',N'UK',N'(71) 555-2222',null);
GO
";

            using (IRepository repo = new DB4Repository(chinookConnectionString))
            {
                string showPlan = string.Empty;
                var ds = repo.ExecuteSql(sql, out showPlan);
                Assert.IsTrue(ds.Tables.Count > 0);
                Assert.IsTrue(ds.Tables[0].Rows.Count > 0);
            }

        }

        [Test]
        //https://sqlcetoolbox.codeplex.com/workitem/11165
        public void TestSqlParse1()
        {
            string sql = @"select 
    count(*)
from 
    Album
GO
";

            using (IRepository repo = new DB4Repository(chinookConnectionString))
            {
                string showPlan = string.Empty;
                var ds = repo.ExecuteSql(sql, out showPlan);
            }

        }


        [Test]
        //https://sqlcetoolbox.codeplex.com/workitem/11165
        public void TestSqlParse2()
        {
            string sql = "select\t\r\ncount(*) FROM Album\r\nGO";

            using (IRepository repo = new DB4Repository(chinookConnectionString))
            {
                string showPlan = string.Empty;
                var ds = repo.ExecuteSql(sql, out showPlan);
            }

        }

        [Test]
        public void TestSqlParse3()
        {
            string sql = @"INSERT INTO Databases (Source, FileName, CeVersion) VALUES ('Data Source=C:\Data\SQLCE\Test\ams40.sdf', 'ams40.sdf', 1);
GO
";
            using (IRepository repo = new DB4Repository(chinookConnectionString))
            {
                string showPlan = string.Empty;
                var ds = repo.ExecuteSql(sql, out showPlan);
            }
        }

        [Test]
        public void TestSqlParse4()
        {
            string sql = @"
-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------
GO";
            using (IRepository repo = new DB4Repository(chinookConnectionString))
            {
                string showPlan = string.Empty;
                var ds = repo.ExecuteSql(sql, out showPlan);
            }
        }

        [Test]
        public void TestServerDgml()
        {
            using (IRepository sourceRepository = new ServerDBRepository4(serverAWConnectionString, true))
            {
                var exclusions = new List<string>();
                exclusions.Add("dbo.BuildVersion");
                exclusions.Add("dbo.ErrorLog");
                var generator = new Generator4(sourceRepository, @"C:\temp\test2.dgml", false, false, false);
                generator.GenerateSchemaGraph(serverConnectionString, exclusions);
            }
        }

        [Test]
        public void TestServerHierarchyIdSQLite()
        {
            using (IRepository sourceRepository = new ServerDBRepository4(serverHIDConnectionString))
            {
                var exclusions = new List<string>();
                var generator = new Generator4(sourceRepository, @"C:\code\test2.sql");
                generator.ExcludeTables(exclusions);
                generator.ScriptDatabaseToFile(Scope.SchemaDataSQLite);
            }
        }

        [Test]
        public void TestServerHierarchyIdSqlCe()
        {
            using (IRepository sourceRepository = new ServerDBRepository4(serverHIDConnectionString))
            {
                var exclusions = new List<string>();
                var generator = new Generator4(sourceRepository, @"C:\code\test2.sql");
                generator.ExcludeTables(exclusions);
                generator.ScriptDatabaseToFile(Scope.SchemaData);
            }
        }

        [Test]
        public void TestGraphSort()
        {
            using (IRepository sourceRepository = new DB4Repository(sdfConnectionString))
            {
                var generator = new Generator4(sourceRepository, @"C:\temp\testAMS40.sqlce");
                generator.ExcludeTables(new System.Collections.Generic.List<String>());
            }
        }

        [Test]
        public void TestGraphSortComplex()
        {
            using (IRepository sourceRepository = new DB4Repository(sdfConnectionString2))
            {
                var generator = new Generator4(sourceRepository, @"C:\temp\testPZ.sqlce");
                generator.ExcludeTables(new System.Collections.Generic.List<String>());
            }
        }

        [Test]
        public void TestGraphSortServer()
        {
            using (IRepository sourceRepository = new ServerDBRepository4(serverConnectionString))
            {
                var generator = new Generator4(sourceRepository, @"C:\temp\testAMS40.sqlce");
                generator.ExcludeTables(new System.Collections.Generic.List<String>());
            }
        }

        [Test]
        public void TestCeDgml()
        {
            using (IRepository sourceRepository = new DB4Repository(chinookConnectionString))
            {
                var generator = new Generator4(sourceRepository, @"C:\temp\testChinook40.dgml");
                generator.GenerateSchemaGraph(chinookConnectionString);
            }
        }

        [Test]
        public void TestDiffNullRef()
        {
            string target = @"Data Source=C:\Data\SQLCE\Test\DiffNullRefDatabases\ArtistManager.sdf";
            string source = @"Data Source=C:\Data\SQLCE\Test\DiffNullRefDatabases\ArtistManagerDesignDatabase.sdf";

            using (IRepository sourceRepository = new DB4Repository(source))
            {
                var generator = new Generator4(sourceRepository);
                using (IRepository targetRepository = new DB4Repository(target))
                {
                    SqlCeDiff.CreateDiffScript(sourceRepository, targetRepository, generator, false);
                }
            }
        }

        [Test]
        public void TestDataDiff()
        {
            string source = @"Data Source=C:\projects\ChinookPart2\Chinook40Modified.sdf";
            string target = @"Data Source=C:\projects\ChinookPart2\Chinook40.sdf";

            string modPath = @"C:\projects\ChinookPart2\Chinook40Modified.sdf";
            if (File.Exists(modPath))
                File.Delete(modPath);
            File.Copy(@"C:\projects\ChinookPart2\Chinook40.sdf", modPath);

            using (IRepository sourceRepository = new DB4Repository(source))
            {
                sourceRepository.ExecuteSql("DELETE FROM InvoiceLine WHERE InvoiceLineId = 2;" + System.Environment.NewLine + "GO");
                sourceRepository.ExecuteSql("DELETE FROM InvoiceLine WHERE InvoiceLineId = 3;" + System.Environment.NewLine + "GO");
                sourceRepository.ExecuteSql("DELETE FROM InvoiceLine WHERE InvoiceLineId = 4;" + System.Environment.NewLine + "GO");

                sourceRepository.ExecuteSql("DELETE FROM InvoiceLine WHERE InvoiceLineId = 1000;" + System.Environment.NewLine + "GO");

                sourceRepository.ExecuteSql(@"INSERT INTO [InvoiceLine] ([InvoiceId],[TrackId],[UnitPrice],[Quantity]) 
                            VALUES (100, 500, 10.11, 1)" + System.Environment.NewLine + "GO");
                sourceRepository.ExecuteSql(@"INSERT INTO [InvoiceLine] ([InvoiceId],[TrackId],[UnitPrice],[Quantity]) 
                            VALUES (200, 500, 10.11, 1)" + System.Environment.NewLine + "GO");
                sourceRepository.ExecuteSql(@"INSERT INTO [InvoiceLine] ([InvoiceId],[TrackId],[UnitPrice],[Quantity]) 
                            VALUES (300, 500, 10.11, 1)" + System.Environment.NewLine + "GO");

                sourceRepository.ExecuteSql("UPDATE InvoiceLine SET [UnitPrice]= 99.99 WHERE InvoiceLineId = 20;" + System.Environment.NewLine + "GO");

                using (IRepository targetRepository = new DB4Repository(target))
                {
                    var generator = new Generator4(targetRepository);
                    var script = SqlCeDiff.CreateDataDiffScript(sourceRepository, "InvoiceLine", targetRepository, "InvoiceLine", generator);
                    Assert.IsTrue(script.Contains("DELETE"));
                }
            }
        }

        [Test]
        public void TestDataDirectory()
        {
            string test = @"Data Source=|DataDirectory|\Chinook40.sdf";
            var helper = new SqlCeHelper4();

            string path = helper.PathFromConnectionString(test);
            Assert.IsFalse(path.Contains("DataDirectory"));
        }

        [Test]
        public void TestAttachDbFileName()
        {
            string test = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\ErikEjlskovJensen\AzureStorageEmulatorDb51.mdf;Integrated Security=True;Connect Timeout=30";
            var helper = new SqlServerHelper();

            string path = helper.PathFromConnectionString(test);
            Assert.AreEqual(path, "(LocalDB)\\MSSQLLocalDB.AzureStorageEmulatorDb51.mdf");
        }

        [Test]
        public void TestDataGenWithWhere()
        {
            using (IRepository sourceRepository = new DB4Repository(@"Data Source=c:\tmp\sqlce\Chinook.sdf"))
            {
                var generator = new Generator4(sourceRepository);
                generator.GenerateTableContent("Album", false, false, "AlbumId < 100");
                var script = generator.GeneratedScript;
            }
        }

        [Test]
        public void TestDataGenWithoutWhere()
        {
            using (IRepository sourceRepository = new DB4Repository(@"Data Source=c:\tmp\sqlce\Chinook.sdf"))
            {
                var generator = new Generator4(sourceRepository);
                generator.GenerateTableContent("Album", false);
                var script = generator.GeneratedScript;
            }
        }

        [Test]
        public void TestServerDataGenWithGeography()
        {
            using (IRepository sourceRepository = new ServerDBRepository4(@"Data Source=.\SQL2016DEV;Initial Catalog=AW2014Geo;Integrated Security=true;"))
            {
                var generator = new Generator4(sourceRepository);
                generator.GenerateTableContent("Address", false);
                var script = generator.GeneratedScript;
            }
        }

        [Test]
        public void TestServerTableGenWithGeography()
        {
            using (IRepository sourceRepository = new ServerDBRepository4(@"Data Source=.\SQL2016DEV;Initial Catalog=AW2014Geo;Integrated Security=true;"))
            {
                var generator = new Generator4(sourceRepository);
                generator.GenerateTableCreate("Address");
                var script = generator.GeneratedScript;
            }
        }

        [Test]
        public void TestSQLiteDateQuirk()
        {
            //cloud_service_product_infos
            //;DateFormatString=yyyy-MM-dd HH:mm:ss zzz
            using (IRepository sourceRepository = new SQLiteRepository(@"Data Source=C:\Users\erik\Downloads\spiceworks_prod.db"))
            {
                var generator = new Generator4(sourceRepository, "sw.sql");
                generator.TruncateSQLiteStrings = true;
                generator.ScriptDatabaseToFile(Scope.DataOnly);
            }
        }

        [Test]
        public void TestSQLiteDataTypes()
        {
            //cloud_service_product_infos
            //;DateFormatString=yyyy-MM-dd HH:mm:ss zzz
            using (IRepository sourceRepository = new SQLiteRepository(chinookSQLiteConnectionString))
            {
                var generator = new Generator4(sourceRepository, "sw.sql", false, false, true);
                generator.GenerateTableCreate("Artist");
                Assert.IsTrue(generator.GeneratedScript.Contains("INTEGER"));
            }
        }

        [Test]
        public void Ce4VersionChek()
        {
            var helper = new SqlCeHelper4();

            var result = helper.IsV40Installed();

            Assert.AreEqual(null, result);
        }

        //[Test]
        //public void TestImportBoolean()
        //{
        //    using (IRepository repository = new DB4Repository(@"Data Source=C:\Data\SQLCE\Test\Empty Site40.sdf"))
        //    {
        //        var generator = new Generator4(repository);
        //        using (var reader = new Kent.Boogaart.KBCsv.CsvReader(@"C:\Data\SQLCE\offmst\offmst.txt"))
        //        {
        //            reader.ValueSeparator = ',';
        //            Kent.Boogaart.KBCsv.HeaderRecord hr = reader.ReadHeaderRecord();
        //            if (generator.ValidColumns("offmst", hr.Values))
        //            {
        //                foreach (Kent.Boogaart.KBCsv.DataRecord record in reader.DataRecords)
        //                {
        //                    generator.GenerateTableInsert("offmst", hr.Values, record.Values);
        //                }
        //            }

        //        }
        //    }
        //}


        //C:\Data\SQLCE\ImportNullStringExample\BookNullString.csv

        //[Test]
        //public void TestImportEmptyString()
        //{
        //    using (IRepository repository = new DB4Repository(@"Data Source=C:\Data\SQLCE\Test\Empty Site40.sdf"))
        //    {
        //        var generator = new Generator4(repository);
        //        using (var reader = new Kent.Boogaart.KBCsv.CsvReader(@"C:\Data\SQLCE\ImportNullStringExample\BookNullString.csv"))
        //        {
        //            reader.ValueSeparator = ';';
        //            Kent.Boogaart.KBCsv.HeaderRecord hr = reader.ReadHeaderRecord();
        //            if (generator.ValidColumns("Book", hr.Values))
        //            {
        //                foreach (Kent.Boogaart.KBCsv.DataRecord record in reader.DataRecords)
        //                {
        //                    generator.GenerateTableInsert("Book", hr.Values, record.Values);
        //                }
        //            }

        //        }
        //    }
        //}


        //[Test]
        //public void TestImportValidColUpdate()
        //{
        //    using (IRepository repository = new DB4Repository(@"Data Source=C:\Data\SQLCE\Test\test.sdf"))
        //    {
        //        var generator = new Generator4(repository);
        //        using (var reader = new Kent.Boogaart.KBCsv.CsvReader(@"C:\Data\SQLCE\routes.csv"))
        //        {
        //            reader.ValueSeparator = ',';
        //            Kent.Boogaart.KBCsv.HeaderRecord hr = reader.ReadHeaderRecord();
        //            if (generator.ValidColumns("Routes", hr.Values))
        //            {
        //                foreach (Kent.Boogaart.KBCsv.DataRecord record in reader.DataRecords)
        //                {
        //                    generator.GenerateTableInsert("Routes", hr.Values, record.Values);
        //                }
        //            }

        //        }
        //        Assert.IsTrue(generator.GeneratedScript.Length == 808);
        //    }
        //}

        [Test]
        public void GenerateDatabaseScript_FromMsSqlToSqlite_IncludesFilteredIndex()
        {
            string createTableQuery =
                @"CREATE TABLE [Test]
                (
                     [TestId]  BIGINT         IDENTITY (1, 1) NOT NULL,
                     [Column1] nvarchar(256)  NULL
                );
                CREATE UNIQUE INDEX [Test_UK_Test_Column1] ON [Test] ([Column1] ASC) WHERE ([Column1] IS NOT NULL);";

            string expectedSql = string.Join(Environment.NewLine, new[]
            {
                "CREATE TABLE [Test] (",
                "  [TestId] INTEGER NOT NULL",
                ", [Column1] nvarchar(256) NULL COLLATE NOCASE",
                ");",
                "CREATE UNIQUE INDEX [Test_Test_UK_Test_Column1] ON [Test] ([Column1] ASC) WHERE ([Column1] IS NOT NULL);"
            });

            AssertMsSqlToSqliteGeneratorContains(createTableQuery, "Test", expectedSql);
        }

        [Test]
        public void GenerateDatabaseScript_FromMsSqlToSqlite_IdentityColumn()
        {
            string createTableQuery =
                @"CREATE TABLE [Test]
                (
                        [TestId]  BIGINT         IDENTITY NOT NULL,
                        [Column1] nvarchar(256)  NULL
                );";

            string expectedSql = string.Join(Environment.NewLine, new[]
            {
                "CREATE TABLE [Test] (",
                "  [TestId] INTEGER NOT NULL",
                ", [Column1] nvarchar(256) NULL COLLATE NOCASE",
                ")"
            });

            AssertMsSqlToSqliteGeneratorContains(createTableQuery, "Test", expectedSql);
        }

        [Test]
        public void GenerateDatabaseScript_FromMsSqlToSqlite_IdentityPrimaryKeyColumn()
        {
            string createTableQuery =
                @"CREATE TABLE [Test]
                (
                        [TestId]  BIGINT         IDENTITY NOT NULL,
                        [Column1] nvarchar(256)  NULL,
                        CONSTRAINT [PK_Test] PRIMARY KEY CLUSTERED ([TestId] ASC)
                );";

            string expectedSql = string.Join(Environment.NewLine, new[]
            {
                "CREATE TABLE [Test] (",
                "  [TestId] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL",
                ", [Column1] nvarchar(256) NULL COLLATE NOCASE",
                ")"
            });

            AssertMsSqlToSqliteGeneratorContains(createTableQuery, "Test", expectedSql);
        }

        [Test]
        public void GenerateDatabaseScript_FromMsSqlToSqlite_PrimaryKeyColumn()
        {
            string createTableQuery =
                @"CREATE TABLE [Test]
                (
                        [TestId]  BIGINT         NOT NULL,
                        [Column1] nvarchar(256)  NULL,
                        CONSTRAINT [PK_Test] PRIMARY KEY CLUSTERED ([TestId] ASC)
                );";

            string expectedSql = string.Join(Environment.NewLine, new[]
            {
                "CREATE TABLE [Test] (",
                "  [TestId] bigint NOT NULL",
                ", [Column1] nvarchar(256) NULL COLLATE NOCASE",
                ", CONSTRAINT [PK_Test] PRIMARY KEY ([TestId])",
                ")"
            });

            AssertMsSqlToSqliteGeneratorContains(createTableQuery, "Test", expectedSql);
        }

        private void AssertMsSqlToSqliteGeneratorContains(string createTableQuery, string tableName, string expectedSql)
        {
            // connect to database server
            string connectionString = @"Data Source=.\sqlexpress;Integrated Security=SSPI;";

            // create a new random database name
            string databaseName = this.RandomString(10);
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                while (sqlConnection.DatabaseExists(databaseName))
                {
                    // create a new random database name
                    databaseName = this.RandomString(10);
                }

                using (TempDatabase tempDatabase = new TempDatabase(sqlConnection, databaseName))
                {
                    using (SqlConnection tempSqlConnection = new SqlConnection(tempDatabase.ConnectionString))
                    {
                        tempSqlConnection.Open();

                        tempSqlConnection.Execute(createTableQuery);
                    }

                    string sql = null;

                    using (var repository = new ServerDBRepository4(tempDatabase.ConnectionString, false))
                    {
                        Generator4 generator = new Generator4(
                            repository,
                            null,
                            false,
                            false,
                            true);
                        generator.GenerateTableScript(tableName);
                        sql = generator.GeneratedScript;
                    }

                    Assert.IsTrue(sql.Contains(expectedSql), sql);
                }
            }
        }

        private string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 1; i < size + 1; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }
    }
}