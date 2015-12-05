using System.Windows.Controls;
using System.Windows.Input;
using ErikEJ.SqlCeToolbox.Commands;
using ErikEJ.SqlCeToolbox.Helpers;
using ErikEJ.SqlCeToolbox.ToolWindows;

namespace ErikEJ.SqlCeToolbox.ContextMenues
{
    public class ScopesContextMenu : ContextMenu
    {

        public ScopesContextMenu(MenuCommandParameters scopeMenuCommandParameters, ExplorerToolWindow parent)
        {
            var dcmd = new ScopesMenuCommandsHandler(parent);

            if (scopeMenuCommandParameters.MenuItemType == MenuType.Manage)

            {

                var dropScopeCommandBinding = new CommandBinding(ScopesMenuCommands.ScopeCommand,
                            dcmd.DropScope);

                var dropScopeMenuItem = new MenuItem
                {
                    Header = "Deprovision Scope...",
                    Icon = ImageHelper.GetImageFromResource("../resources/action_Cancel_16xLG.png"),
                    Command = ScopesMenuCommands.ScopeCommand,
                    CommandParameter = scopeMenuCommandParameters,
                };
                dropScopeMenuItem.CommandBindings.Add(dropScopeCommandBinding);
                Items.Add(dropScopeMenuItem);            
            }
            else
            {
            }
        }

    }
}