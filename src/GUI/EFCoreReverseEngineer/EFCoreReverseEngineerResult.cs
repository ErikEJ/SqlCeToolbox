using System.Collections.Generic;

namespace EFCoreReverseEngineer
{
    public class EfCoreReverseEngineerResult
    {
        public IList<string> FilePaths { get; set; }
        public IDictionary<string, string> EntityErrors { get; set; }
    }
}
