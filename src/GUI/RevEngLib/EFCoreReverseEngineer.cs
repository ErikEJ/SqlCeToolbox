using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace EFCoreReverseEngineer
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
            .AddLogging();

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
            var generator = serviceProvider.GetService<ModelScaffolder>();

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

            var result = new EfCoreReverseEngineerResult
            {
                EntityErrors = errors,
                EntityWarnings = warnings,
                EntityTypeFilePaths = filePaths.EntityTypeFiles,
                ContextFilePath = filePaths.ContextFile,
            };

            return result;
        }


    }
}
