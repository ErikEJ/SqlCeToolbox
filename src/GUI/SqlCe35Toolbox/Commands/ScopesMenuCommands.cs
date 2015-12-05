using System.Windows.Input;

namespace ErikEJ.SqlCeToolbox.Commands
{
    class ScopesMenuCommands
    {
        static ScopesMenuCommands()
        {
            ScopeCommand = new RoutedUICommand("Command from subscription context menu", "ScopeCommand", typeof(ScopesMenuCommands));
        }

        public static RoutedUICommand ScopeCommand
        {
            get;
            private set;
        }
    }
}
