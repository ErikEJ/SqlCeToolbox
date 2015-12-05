using System.Windows.Input;

namespace ErikEJ.SqlCeToolbox.Commands
{
    class SubscriptionsMenuCommands
    {
        static SubscriptionsMenuCommands()
        {
            SubscriptionCommand = new RoutedUICommand("Command from subscription context menu", "SubscriptionCommand", typeof(SubscriptionsMenuCommands));
        }

        public static RoutedUICommand SubscriptionCommand
        {
            get;
            private set;
        }
    }
}
