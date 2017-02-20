using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace EFCoreReverseEngineer
{
    public class EFCoreReverseEngineer
    {
        public EFCoreReverseEngineerResult GenerateFiles(ReverseEngineerOptions reverseEngineerOptions)
        {
            // Add base services for scaffolding
            var serviceCollection = new ServiceCollection()
            .AddScaffolding()
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
                    var sqliteProvider = new SqlServerDesignTimeServices();
                    sqliteProvider.ConfigureDesignTimeServices(serviceCollection);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var generator = serviceProvider.GetService<ReverseEngineeringGenerator>();

            var tableSelectionSet = reverseEngineerOptions.Tables.Count == 0 
                ? TableSelectionSet.All 
                : new TableSelectionSet(reverseEngineerOptions.Tables);

            var options = new ReverseEngineeringConfiguration
            {
                ConnectionString = reverseEngineerOptions.ConnectionString,
                ProjectPath = reverseEngineerOptions.ProjectPath,
                ProjectRootNamespace = reverseEngineerOptions.ProjectRootNamespace,
                OverwriteFiles = true,
                UseFluentApiOnly = reverseEngineerOptions.UseFluentApiOnly,
                ContextClassName = reverseEngineerOptions.ContextClassName,
                TableSelectionSet = tableSelectionSet
            };

            var model = generator.GetMetadataModel(options);

            var filePaths = generator.GenerateAsync(options).GetAwaiter().GetResult();

            var errors = model.Scaffolding().EntityTypeErrors;

            var result = new EFCoreReverseEngineerResult
            {
                EntityErrors = errors,
                FilePaths = filePaths.EntityTypeFiles,
            };
            result.FilePaths.Add(filePaths.ContextFile);

            return result;
        }
    }
}
