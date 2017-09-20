using System.Collections.Generic;

namespace EFCoreReverseEngineer
{
    public class ReverseEngineerOptions
    {
        public DatabaseType DatabaseType { get; set; }
        public string ConnectionString { get; set; }
        public string ProjectPath { get; set; }
        public string OutputPath { get; set; }
        public string ProjectRootNamespace { get; set; }
        public bool UseFluentApiOnly { get; set; }
        public string ContextClassName { get; set; }
        public List<string> Tables { get; set; }
        public bool UseDatabaseNames { get; set; }
    }
}