using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ReverseEngineer20.ModelAnalyzer;

namespace UnitTests
{
    [TestFixture]
    public class DgmlTest
    {
        private readonly DebugViewParser _parser = new DebugViewParser();

        [Test]
        public void ParseDebugViewSample1()
        {
            // Arrange
            var _debugView = File.ReadAllLines("Aw2014Person.txt");

            // Act
            var result = _parser.Parse(_debugView, "Test");

            // Assert
            Assert.AreEqual(84, result.Nodes.Count);
            Assert.AreEqual(160, result.Links.Count);

            Assert.AreEqual(18, result.Links.Count(n => n.Contains("IsUnique=\"True\"")));
        }

        [Test]
        public void ParseDebugViewFkBug()
        {
            // Arrange
            var _debugView = File.ReadAllLines("Northwind.txt");

            // Act
            var result = _parser.Parse(_debugView, "Test");

            // Assert
            Assert.AreEqual(103, result.Nodes.Count);
            Assert.AreEqual(203, result.Links.Count);

            Assert.AreEqual(0, result.Links.Count(n => n.Contains("IsUnique=\"True\"")));
        }

        [Test]
        public void ParseDebugViewMultiColFk()
        {
            // Arrange
            var _debugView = File.ReadAllLines("Pfizer.txt");

            // Act
            var result = _parser.Parse(_debugView, "Test");

            // Assert
            Assert.AreEqual(134, result.Nodes.Count);
            Assert.AreEqual(211, result.Links.Count);
        }

        [Test]
        public void BuildSample1()
        {
            // Act
            var builder = new DgmlBuilder();
            var result = builder.Build(File.ReadAllText("Aw2014Person.txt"), "test");

            // Assert
            Assert.AreNotEqual(result, null);

            File.WriteAllText(@"C:\temp\Aw2014Person.dgml", result, Encoding.UTF8);
        }

        [Test]
        public void BuildNorthwind()
        {
            // Act
            var builder = new DgmlBuilder();
            var result = builder.Build(File.ReadAllText("Northwind.txt"), "test");

            // Assert
            Assert.AreNotEqual(result, null);

            File.WriteAllText(@"C:\temp\northwind.dgml", result, Encoding.UTF8);
        }

        [Test]
        public void BuildPfizer()
        {
            // Act
            var builder = new DgmlBuilder();
            var result = builder.Build(File.ReadAllText("Pfizer.txt"), "test");

            // Assert
            Assert.AreNotEqual(result, null);

            File.WriteAllText(@"C:\temp\pfizer.dgml", result, Encoding.UTF8);
        }

        //[Test]
        //public void CanBuildDgml()
        //{
        //    using (var myContext = new MyDbContext())
        //    {
        //        var dgml = myContext.AsDgmlView();
        //        var path = Path.GetTempFileName() + ".dgml";
        //        File.WriteAllText(path, dgml, Encoding.UTF8);
        //        Process.Start(path);
        //    }
        //}

        public class Samurai
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class MyDbContext : DbContext
        {
            public DbSet<Samurai> Samurais { get; set; }
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=EFCoreSamuraiConsole;Integrated Security=True;";
                optionsBuilder.UseInMemoryDatabase("dgml");
            }
        }

    }
}
