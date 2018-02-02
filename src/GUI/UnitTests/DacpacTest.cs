using System.Linq;
using NUnit.Framework;
using Microsoft.SqlServer.Dac.Extensions.Prototype;
using Microsoft.SqlServer.Dac.Model;

namespace UnitTests
{
    [TestFixture]
    public class DacpacTest
    {
        private TSqlTypedModel model;
        [SetUp]
        public void Setup()
        {
            model = new TSqlTypedModel("Chinook.dacpac");
        }

        [Test]
        public void CanEnumerateTables()
        {
            var tables = model.GetObjects<TSqlTable>(DacQueryScopes.UserDefined)
                .Where(t => t.PrimaryKeyConstraints.Count() > 0)
                .ToList();

            foreach (var item in tables)
            {
                foreach (var col in item.Columns)
                {
                    col.GetProperty<bool?>(Column.IsHidden);
                }
            }
            // Assert
            Assert.AreEqual(11, tables.Count());
        }
    }
}
