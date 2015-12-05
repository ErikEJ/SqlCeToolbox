using System.Windows.Controls;
using System.Windows.Input;
using ErikEJ.SqlCeToolbox.Commands;
using ErikEJ.SqlCeToolbox.Helpers;
using ErikEJ.SqlCeToolbox.ToolWindows;

namespace ErikEJ.SqlCeToolbox.ContextMenues
{
    public class SubscriptionsContextMenu : ContextMenu
    {

         public SubscriptionsContextMenu(MenuCommandParameters subsMenuCommandParameters, ExplorerToolWindow parent)
        {
            var dcmd = new SubscriptionsMenuCommandsHandler(parent);

            if (subsMenuCommandParameters.MenuItemType == MenuType.Manage)

            {
                var newSubsCommandBinding = new CommandBinding(SubscriptionsMenuCommands.SubscriptionCommand,
                                            dcmd.NewSubscription);

                var newSubsMenuItem = new MenuItem
                {
                    Header = "Manage Subscription...",
                    Icon = ImageHelper.GetImageFromResource("../resources/arrow_Sync_16xLG.png"),
                    Command = SubscriptionsMenuCommands.SubscriptionCommand,
                    CommandParameter = subsMenuCommandParameters,
                };
                newSubsMenuItem.CommandBindings.Add(newSubsCommandBinding);
                Items.Add(newSubsMenuItem);


                var dropSubsCommandBinding = new CommandBinding(SubscriptionsMenuCommands.SubscriptionCommand,
                            dcmd.DropSubscription);

                var dropSubsMenuItem = new MenuItem
                {
                    Header = "Drop Subscription...",
                    Icon = ImageHelper.GetImageFromResource("../resources/arrow_Sync_16xLG.png"),
                    Command = SubscriptionsMenuCommands.SubscriptionCommand,
                    CommandParameter = subsMenuCommandParameters,
                };
                dropSubsMenuItem.CommandBindings.Add(dropSubsCommandBinding);
                Items.Add(dropSubsMenuItem);            
            }
            else
            {
                var newSubsCommandBinding = new CommandBinding(SubscriptionsMenuCommands.SubscriptionCommand,
                                            dcmd.NewSubscription);

                var newSubsMenuItem = new MenuItem
                {
                    Header = "New Subscription...",
                    Icon = ImageHelper.GetImageFromResource("../resources/arrow_Sync_16xLG.png"),
                    Command = SubscriptionsMenuCommands.SubscriptionCommand,
                    CommandParameter = subsMenuCommandParameters,
                };
                newSubsMenuItem.CommandBindings.Add(newSubsCommandBinding);
                Items.Add(newSubsMenuItem);
            }
        }

    }
}