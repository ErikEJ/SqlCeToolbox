using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class DgmlTest
    {

        [Test]
        public void ParseDebugViewSample1()
        {
            var sample = File.ReadAllText("Vax.txt");
        }
    }
}
