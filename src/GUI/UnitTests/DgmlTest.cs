using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using DgmlBuilder;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class DgmlTest
    {
        private readonly DebugViewParser _parser = new DebugViewParser();
        private string template;

        [SetUp]
        public void Setuup()
        {
            template = GetTemplate();
        }


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
            var builder = new DgmlBuilder.DgmlBuilder();
            var result = builder.Build(File.ReadAllText("Aw2014Person.txt"), "test", template);

            // Assert
            Assert.AreNotEqual(result, null);

            File.WriteAllText(@"C:\temp\Aw2014Person.dgml", result, Encoding.UTF8);
        }

        [Test]
        public void BuildNorthwind()
        {
            // Act
            var builder = new DgmlBuilder.DgmlBuilder();
            var result = builder.Build(File.ReadAllText("Northwind.txt"), "test", template);

            // Assert
            Assert.AreNotEqual(result, null);

            File.WriteAllText(@"C:\temp\northwind.dgml", result, Encoding.UTF8);
        }

        [Test]
        public void BuildPfizer()
        {
            // Act
            var builder = new DgmlBuilder.DgmlBuilder();
            var result = builder.Build(File.ReadAllText("Pfizer.txt"), "test", template);

            // Assert
            Assert.AreNotEqual(result, null);

            File.WriteAllText(@"C:\temp\pfizer.dgml", result, Encoding.UTF8);
        }

        [Test]
        public void BuildBNoFk()
        {
            // Act
            var builder = new DgmlBuilder.DgmlBuilder();
            var result = builder.Build(File.ReadAllText("NoFk.txt"), "test", template);

            // Assert
            Assert.AreNotEqual(result, null);

            File.WriteAllText(@"C:\temp\nofk.dgml", result, Encoding.UTF8);
        }

        [Test]
        public void BuildSingleNav()
        {
            // Act
            var builder = new DgmlBuilder.DgmlBuilder();
            var result = builder.Build(File.ReadAllText("SingleNav.txt"), "test", template);

            // Assert
            Assert.AreNotEqual(result, null);

            File.WriteAllText(@"C:\temp\singlenav.dgml", result, Encoding.UTF8);
        }

        private static string GetTemplate()
        {
            var resourceName = "UnitTests.template.dgml";

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                using (var reader = new StreamReader(stream ?? throw new InvalidOperationException()))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        //[Test]
        //public void CanBuildDgml()
        //{
        //    using (var myContext = new MyDbContext())
        //    {
        //        var dgml = myContext.AsDgml();
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
                optionsBuilder.UseInMemoryDatabase("dgml");
            }
        }

    }
}
