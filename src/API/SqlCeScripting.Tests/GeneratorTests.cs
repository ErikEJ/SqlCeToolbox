using ErikEJ.SqlCeScripting;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace SqlCeScripting.Tests
{
    public class GeneratorTests
    {
        private static string dbPath = Directory.GetCurrentDirectory();

        private string northwindConn = $"Data Source={Path.Combine(dbPath, "Northwind.sdf")}";

        [Test]
        public void TestGetAllColumns()
        {
            var list = new List<Column>();
            using (IRepository repo = new DBRepository(northwindConn))
            {
                list = repo.GetAllColumns();
            }
            Assert.AreEqual(74, list.Count);
            Assert.AreEqual("int", list[0].DataType);
        }
    }
}
