using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace EFCoreReverseEngineer
{
    public class EfCoreReverseEngineer
    {
        public EfCoreReverseEngineerResult GenerateFiles(ReverseEngineerOptions reverseEngineerOptions)
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
                    var sqliteProvider = new SqliteDesignTimeServices();
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
                OutputPath = reverseEngineerOptions.OutputPath,
                ProjectRootNamespace = reverseEngineerOptions.ProjectRootNamespace,
                OverwriteFiles = true,
                UseFluentApiOnly = reverseEngineerOptions.UseFluentApiOnly,
                ContextClassName = reverseEngineerOptions.ContextClassName,
                TableSelectionSet = tableSelectionSet
            };

            var model = generator.GetMetadataModel(options);

            var filePaths = generator.GenerateAsync(options).GetAwaiter().GetResult();

            var errors = model.Scaffolding().EntityTypeErrors;

            var result = new EfCoreReverseEngineerResult
            {
                EntityErrors = errors,
                FilePaths = filePaths.EntityTypeFiles,
            };
            result.FilePaths.Add(filePaths.ContextFile);

            return result;
        }

        public string GenerateClassName(string value)
        {
            var className = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value);
            var isValid = System.CodeDom.Compiler.CodeDomProvider.CreateProvider("C#").IsValidIdentifier(className);

            if (!isValid)
            {
                // File name contains invalid chars, remove them
                Regex regex = new Regex(@"[^\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Nd}\p{Nl}\p{Mn}\p{Mc}\p{Cf}\p{Pc}\p{Lm}]");
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
