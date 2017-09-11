using System.IO;
using System.Linq;
using System.Text;
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

            Assert.AreEqual(18, result.Links.Where(n => n.Contains("IsUnique=\"True\"")).Count());
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
    }
}
