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
        private string[] _sample;
        private readonly DebugViewParser _parser = new DebugViewParser();

        [SetUp]
        public void Setup()
        {
            _sample = File.ReadAllLines("Aw2014Person.txt");
        }

        [Test]
        public void ParseDebugViewSample1()
        {
            // Act
            var result = _parser.Parse(_sample, "Test");

            // Assert
            Assert.AreEqual(83, result.Nodes.Count);
            Assert.AreEqual(159, result.Links.Count);

            Assert.AreEqual(18, result.Links.Count(n => n.Contains("IsUnique=\"True\"")));
        }

        [Test]
        public void BuildSample1()
        {
            // Act
            var builder = new DgmlBuilder();
            var result = builder.Build(File.ReadAllText("Aw2014Person.txt"), "test");

            // Assert
            Assert.AreNotEqual(result, null);

            File.WriteAllText(@"C:\temp\testing.dgml", result, Encoding.UTF8);
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
