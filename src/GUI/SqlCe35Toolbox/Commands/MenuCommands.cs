using System.Windows.Input;

namespace ErikEJ.SqlCeToolbox.Commands
{
    public sealed class TableMenuCommands
    {
        private TableMenuCommands()
        { }

        static TableMenuCommands()
        {
            TableCommand = new RoutedUICommand("Command from table context menu", "TableCommand", typeof(TableMenuCommands));
        }

        public static RoutedUICommand TableCommand
        {
            get;
            private set;
        }
    }
}