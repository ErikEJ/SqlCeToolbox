using EntityFrameworkCore.Scaffolding.Handlebars;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.Extensions.DependencyInjection;
using ReverseEngineer20.ReverseEngineer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ReverseEngineer20
{
    public class EfCoreReverseEngineer
    {
        public EfCoreReverseEngineerResult GenerateFiles(ReverseEngineerOptions reverseEngineerOptions)
        {
            var errors = new List<string>();
            var warnings = new List<string>();
            var reporter = new OperationReporter(
                new OperationReportHandler(
                    m => errors.Add(m),
                    m => warnings.Add(m)));

            // Add base services for scaffolding
            var serviceCollection = new ServiceCollection();

            serviceCollection
                .AddScaffolding(reporter)
                .AddSingleton<IOperationReporter, OperationReporter>()
                .AddSingleton<IOperationReportHandler, OperationReportHandler>();

            if (reverseEngineerOptions.UseHandleBars)
            {
                serviceCollection.AddHandlebarsScaffolding(reverseEngineerOptions.ProjectPath);
            }

            if (reverseEngineerOptions.UseInflector)
            {
                serviceCollection.AddSingleton<IPluralizer, InflectorPluralizer>();
            }

            // Add database provider services
            switch (reverseEngineerOptions.DatabaseType)
            {
                case DatabaseType.SQLCE35:
                    throw new NotImplementedException();
                case DatabaseType.SQLCE40:
                    var sqlCeProvider = new SqlCeDesignTimeServices();
                    sqlCeProvider.ConfigureDesignTimeServices(serviceCollection);
                    break;
                case DatabaseType.SQLServer:
                    var provider = new SqlServerDesignTimeServices();
                    provider.ConfigureDesignTimeServices(serviceCollection);
                    break;
                case DatabaseType.SQLite:
                    var sqliteProvider = new SqliteDesignTimeServices();
                    sqliteProvider.ConfigureDesignTimeServices(serviceCollection);
                    serviceCollection.AddSingleton<IDatabaseModelFactory, SqliteDatabaseModelFactory>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var generator = serviceProvider.GetService<IModelScaffolder>();

            var filePaths = generator.Generate(
                reverseEngineerOptions.ConnectionString,
                reverseEngineerOptions.Tables,
                new List<string>(), 
                reverseEngineerOptions.ProjectPath,
                reverseEngineerOptions.OutputPath,
                reverseEngineerOptions.ProjectRootNamespace,
                reverseEngineerOptions.ContextClassName,
                !reverseEngineerOptions.UseFluentApiOnly,
                overwriteFiles: true,
                useDatabaseNames: reverseEngineerOptions.UseDatabaseNames);
                // Explanation: Use table and column names directly from the database.

            foreach (var file in filePaths.EntityTypeFiles)
            {
                PostProcess(file, reverseEngineerOptions.IdReplace);
            }
            PostProcess(filePaths.ContextFile, reverseEngineerOptions.IdReplace);

            if (!reverseEngineerOptions.IncludeConnectionString)
            {
                PostProcessContext(filePaths.ContextFile);
            }

            var result = new EfCoreReverseEngineerResult
            {
                EntityErrors = errors,
                EntityWarnings = warnings,
                EntityTypeFilePaths = filePaths.EntityTypeFiles,
                ContextFilePath = filePaths.ContextFile,
            };

            return result;
        }

        private void PostProcessContext(string contextFile)
        {
            var finalLines = new List<string>();
            var lines = File.ReadAllLines(contextFile);

            foreach (var line in lines)
            {
                if (line.Trim().StartsWith("#warning To protect"))
                    continue;

                if (line.Trim().StartsWith("optionsBuilder.Use"))
                    continue;

                finalLines.Add(line);
            }
            File.WriteAllLines(contextFile, finalLines, Encoding.UTF8);
        }

        private void PostProcess(string file, bool idReplace)
        {
            if (idReplace)
            {
                var text = File.ReadAllText(file);
                text = text.Replace("Id, ", "ID, ");
                text = text.Replace("Id }", "ID }");
                text = text.Replace("Id }", "ID }");
                text = text.Replace("Id)", "ID)");
                text = text.Replace("Id { get; set; }", "ID { get; set; }");
                File.WriteAllText(file, text, Encoding.UTF8);
            }
        }

        public string GenerateClassName(string value)
        {
            var className = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value);
            var isValid = System.CodeDom.Compiler.CodeDomProvider.CreateProvider("C#").IsValidIdentifier(className);

            if (!isValid)
            {
                // File name contains invalid chars, remove them
                var regex = new Regex(@"[^\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Nd}\p{Nl}\p{Mn}\p{Mc}\p{Cf}\p{Pc}\p{Lm}]", RegexOptions.None, TimeSpan.FromSeconds(5));
                className = regex.Replace(className, "");

                // Class name doesn't begin with a letter, insert an underscore
                if (!char.IsLetter(className, 0))
                {
                    className = className.Insert(0, "_");
                }
            }

            return className.Replace(" ", string.Empty);
        }
    }
}
