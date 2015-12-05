using System.Windows.Input;

namespace ErikEJ.SqlCeToolbox.Commands
{
    class DatabaseMenuCommands
    {
        static DatabaseMenuCommands()
        {
            DatabaseCommand = new RoutedUICommand("Command from table context menu", "DatabaseCommand", typeof(DatabaseMenuCommands));
        }

        public static RoutedUICommand DatabaseCommand
        {
            get;
            private set;
        }
    }
}
