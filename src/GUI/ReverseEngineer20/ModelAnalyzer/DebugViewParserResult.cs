using System.Collections.Generic;

namespace ReverseEngineer20.ModelAnalyzer
{
    public class DebugViewParserResult
    {
        public DebugViewParserResult()
        {
            Nodes = new List<string>();
            Links = new List<string>();
        }

        public List<string> Nodes { get; set; }

        public List<string> Links { get; set; }
    }
}
