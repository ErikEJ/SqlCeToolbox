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
        public List<Tuple<string, string>> GenerateDebugView(string outputPath)
        {
            var result = new List<Tuple<string, string>>();

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

            foreach (var type in types)
            {
                var dbContext = operations.CreateContext(types[0].Name);
                var debugView = dbContext.Model.AsModel().DebugView.View;
                result.Add(new Tuple<string, string>(type.Name, debugView));
            }

            return result;
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
