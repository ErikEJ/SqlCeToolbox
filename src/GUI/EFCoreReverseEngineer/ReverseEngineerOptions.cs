namespace EFCoreReverseEngineer
{
    public class ReverseEngineerOptions
    {
        public DatabaseType DatabaseType { get; set; }
        public string ConnectionString { get; set; }
        public string ProjectPath { get; set; }
        public string ProjectRootNamespace { get; set; }
        public bool UseFluentApiOnly { get; set; }
        public  string ContextClassName { get; set; }

        //TODO Table selection!
    }
}
