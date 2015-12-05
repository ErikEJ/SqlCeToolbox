using System.Windows.Input;

namespace ErikEJ.SqlCeToolbox.Commands
{
    class IndexMenuCommands
    {
        static IndexMenuCommands()
        {
            IndexCommand = new RoutedUICommand("Command from index context menu", "IndexCommand", typeof(IndexMenuCommands));
        }

        public static RoutedUICommand IndexCommand
        {
            get;
            private set;
        }
    }
}
