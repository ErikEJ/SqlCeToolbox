﻿using System.Collections.Generic;

namespace EFCoreReverseEngineer
{
    public class EfCoreReverseEngineerResult
    {
        public IList<string> EntityTypeFilePaths { get; set; }
        public string ContextFilePath { get; set; }
        public List<string> EntityErrors { get; set; }
        public List<string> EntityWarnings { get; set; }
    }
}
