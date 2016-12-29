using System.Windows.Controls;
using System.Windows.Input;
using ErikEJ.SqlCeToolbox.Commands;
using ErikEJ.SqlCeToolbox.Helpers;
using ErikEJ.SqlCeToolbox.ToolWindows;

namespace ErikEJ.SqlCeToolbox.ContextMenues
{
    public class TablesContextMenu : ContextMenu
    {
        public TablesContextMenu(DatabaseMenuCommandParameters menuCommandParameters, ExplorerToolWindow parent)
        {
            var dcmd = new DatabaseMenuCommandsHandler(parent);

            var createTableCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                            dcmd.BuildTable);
            var createTableMenuItem = new MenuItem
            {
                Header = "Build Table (beta)...",
                Icon = ImageHelper.GetImageFromResource("../resources/AddTable_5632.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = menuCommandParameters
            };
            createTableMenuItem.CommandBindings.Add(createTableCommandBinding);
            Items.Add(createTableMenuItem);
            
            Items.Add(new Separator());

            var refreshCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                                                    dcmd.RefreshTables);
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