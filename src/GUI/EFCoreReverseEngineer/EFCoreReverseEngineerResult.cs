using System.Collections.Generic;

namespace EFCoreReverseEngineer
{
    public class EfCoreReverseEngineerResult
    {
        public IList<string> EntityTypeFilePaths { get; set; }
        public string ContextFilePath { get; set; }
        public IDictionary<string, string> EntityErrors { get; set; }
    }
}
