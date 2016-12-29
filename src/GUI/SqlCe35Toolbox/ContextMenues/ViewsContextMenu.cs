using System.Windows.Controls;
using System.Windows.Input;
using ErikEJ.SqlCeToolbox.Commands;
using ErikEJ.SqlCeToolbox.Helpers;
using ErikEJ.SqlCeToolbox.ToolWindows;

namespace ErikEJ.SqlCeToolbox.ContextMenues
{
    public class ViewsContextMenu : ContextMenu
    {
        public ViewsContextMenu(DatabaseMenuCommandParameters menuCommandParameters, ExplorerToolWindow parent)
        {
            var dcmd = new DatabaseMenuCommandsHandler(parent);

            var refreshCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                                                    dcmd.RefreshViews);
            var refreshMenuItem = new MenuItem
            {
                Header = "Refresh",
                Icon = ImageHelper.GetImageFromResource("../resources/refresh_16xLG.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = menuCommandParameters
            };
            refreshMenuItem.CommandBindings.Add(refreshCommandBinding);
            Items.Add(refreshMenuItem);
        }
    }
}