using System.Windows.Input;

namespace ErikEJ.SqlCeToolbox.Commands
{
    class ColumnMenuCommands
    {
        static ColumnMenuCommands()
        {
            ColumnCommand = new RoutedUICommand("Command from index context menu", "IndexCommand", typeof(ColumnMenuCommands));
        }

        public static RoutedUICommand ColumnCommand
        {
            get;
            private set;
        }
    }
}
