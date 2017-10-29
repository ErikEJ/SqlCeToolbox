using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Design.Internal;
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
            var serviceCollection = new ServiceCollection()
                .AddScaffolding(reporter)
                .AddSingleton<IOperationReporter, OperationReporter>()
                .AddSingleton<IOperationReportHandler, OperationReportHandler>();

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
                //TODO remove this when bug fix is released in 2.0.1 and adopted
                Fix200Bug(file);
            }
            Fix200Bug(filePaths.ContextFile);

            var result = new EfCoreReverseEngineerResult
            {
                EntityErrors = errors,
                EntityWarnings = warnings,
                EntityTypeFilePaths = filePaths.EntityTypeFiles,
                ContextFilePath = filePaths.ContextFile,
            };

            return result;
        }

        private static void Fix200Bug(string file)
        {
            var text = File.ReadAllText(file);
            text = text.Replace("\"nvarchar\"", "\"nvarchar(4000)\"");
            File.WriteAllText(file, text, Encoding.UTF8);
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
