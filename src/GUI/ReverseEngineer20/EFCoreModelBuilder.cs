using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ReverseEngineer20
{
    public class EFCoreModelBuilder
    {
        public Tuple<string, string> GenerateDebugView(string outputPath)
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            var assembly = Load(outputPath);
            if (assembly == null)
            {
                throw new ArgumentException("Unable to load project assembly");
            }

            var reporter = new OperationReporter(
                new OperationReportHandler(
                    m => errors.Add(m),
                    m => warnings.Add(m)));

            DbContextOperations operations = new DbContextOperations(reporter, assembly, assembly);
            var types = operations.GetContextTypes().ToList();

            if (types.Count == 0)
            {
                throw new ArgumentException("No DbContext types found in the project");
            }

            var dbContext = operations.CreateContext(types[0].Name);

            var debugView = dbContext.Model.AsModel().DebugView.View;

            return new Tuple<string, string>(debugView, types[0].Name);
        }

        private Assembly Load(string assemblyPath)
        {
            if (File.Exists(assemblyPath))
            {
                return Assembly.LoadFile(assemblyPath);
            }

            return null;
        }
    }
}
