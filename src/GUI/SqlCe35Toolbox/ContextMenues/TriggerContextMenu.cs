using System.Windows.Controls;
using System.Windows.Input;
using ErikEJ.SqlCeToolbox.Commands;
using ErikEJ.SqlCeToolbox.Helpers;
using ErikEJ.SqlCeToolbox.ToolWindows;

namespace ErikEJ.SqlCeToolbox.ContextMenues
{
    public class TriggerContextMenu : ContextMenu
    {
        public TriggerContextMenu(MenuCommandParameters menuCommandParameters, ExplorerToolWindow parent)
        {
            var tcmd = new TriggerMenuCommandsHandler(parent);
            CreateScriptAsCreateMenuItem(tcmd, menuCommandParameters);
            CreateScriptAsDropMenuItem(tcmd, menuCommandParameters);
            CreateScriptAsDropAndCreateMenuItem(tcmd, menuCommandParameters);
        }

        private void CreateScriptAsCreateMenuItem(TriggerMenuCommandsHandler tcmd, MenuCommandParameters menuCommandParameters)
        {
            var scriptCommandBinding = new CommandBinding(IndexMenuCommands.IndexCommand, tcmd.ScriptAsCreate);
            var scriptMenuItem = new MenuItem
            {
                Header = "Script as CREATE",
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
                Command = IndexMenuCommands.IndexCommand,
                CommandParameter = menuCommandParameters
            };
            scriptMenuItem.CommandBindings.Add(scriptCommandBinding);
            Items.Add(scriptMenuItem);
        }

        private void CreateScriptAsDropMenuItem(TriggerMenuCommandsHandler tcmd, MenuCommandParameters menuCommandParameters)
        {
            var scriptCommandBinding = new CommandBinding(IndexMenuCommands.IndexCommand, tcmd.ScriptAsDrop);
            var scriptMenuItem = new MenuItem
            {
                Header = "Script as DROP",
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
                Command = IndexMenuCommands.IndexCommand,
                CommandParameter = menuCommandParameters
            };
            scriptMenuItem.CommandBindings.Add(scriptCommandBinding);
            Items.Add(scriptMenuItem);
        }

        private void CreateScriptAsDropAndCreateMenuItem(TriggerMenuCommandsHandler tcmd, MenuCommandParameters menuCommandParameters)
        {
            var scriptCommandBinding = new CommandBinding(IndexMenuCommands.IndexCommand, tcmd.ScriptAsDropAndCreate);
            var scriptMenuItem = new MenuItem
            {
                Header = "Script as DROP and CREATE",
                Icon = ImageHelper.GetImageFromResource("../resources/script_16xLG.png"),
                Command = IndexMenuCommands.IndexCommand,
                CommandParameter = menuCommandParameters
            };
            scriptMenuItem.CommandBindings.Add(scriptCommandBinding);
            Items.Add(scriptMenuItem);
        }
    }
}