using ErikEJ.SqlCeToolbox.ToolWindows;

namespace ErikEJ.SqlCeToolbox.Commands
{
    public class DatabaseMenuCommandParameters
    {
        public ExplorerControl ExplorerControl { get; set; }
        public string Connectionstring { get; set; }
        public string Caption { get; set; }
    }
}
