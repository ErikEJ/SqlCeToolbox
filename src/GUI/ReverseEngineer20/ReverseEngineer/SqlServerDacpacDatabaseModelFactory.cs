using System;
using System.Collections.Generic;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace ReverseEngineer20
{
    public class SqlServerDacpacDatabaseModelFactory : IDatabaseModelFactory
    {
        private readonly IDiagnosticsLogger<DbLoggerCategory.Scaffolding> _logger;

        public SqlServerDacpacDatabaseModelFactory(IDiagnosticsLogger<DbLoggerCategory.Scaffolding> logger)
        {
            _logger = logger;
        }

        public virtual DatabaseModel Create(string dacpacPath, IEnumerable<string> tables, IEnumerable<string> schemas)
        {
            if (string.IsNullOrEmpty(dacpacPath))
            {
                throw new ArgumentException("invalid path", nameof(dacpacPath));
            }
            if (!System.IO.File.Exists(dacpacPath))
            {
                throw new ArgumentException("Dacpac file not found");
            }
            
            //TODO Return DatabaseModel here!
            return null;
        }

        public DatabaseModel Create(DbConnection connection, IEnumerable<string> tables, IEnumerable<string> schemas)
        {
            throw new System.NotImplementedException();
        }
    }
}