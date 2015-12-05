using System.Windows.Controls;
using System.Windows.Input;
using ErikEJ.SqlCeToolbox.Commands;
using ErikEJ.SqlCeToolbox.Helpers;
using ErikEJ.SqlCeToolbox.ToolWindows;

namespace ErikEJ.SqlCeToolbox.ContextMenues
{
    public class TablesContextMenu : ContextMenu
    {
        //public TablesContextMenu(CreateNewCommandParameters parameters, ExplorerToolWindow parent)
        //{
        //    var dcmd = new CreateNewMenuCommandsHandler(parent);

        //    var commandBinding = new CommandBinding(CreateNewMenuCommands.Command, dcmd.CreateNewTable);
        //    var menuItem = new MenuItem
        //                                 {
        //                                     Header = "Create New Table",
        //                                     //Icon = ImageHelper.GetImageFromResource("../resources/user.png"),
        //                                     Command = CreateNewMenuCommands.Command,
        //                                     CommandParameter = parameters
        //                                 };
        //    menuItem.CommandBindings.Add(commandBinding);
        //    Items.Add(menuItem);
        //}
    }
}