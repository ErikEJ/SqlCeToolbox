using System.Windows.Controls;
using System.Windows.Input;
using ErikEJ.SqlCeToolbox.Commands;
using ErikEJ.SqlCeToolbox.Helpers;
using ErikEJ.SqlCeToolbox.ToolWindows;

namespace ErikEJ.SqlCeToolbox.ContextMenues
{

    public class ColumnContextMenu : ContextMenu
    {
        public ColumnContextMenu(MenuCommandParameters menuCommandParameters, ExplorerControl parent)
        {
            var tcmd = new ColumnMenuCommandsHandler(parent);
            CreateScriptAsCreateMenuItem(tcmd, menuCommandParameters);
            CreateScriptAsDropMenuItem(tcmd, menuCommandParameters);
            CreateScriptAsAlterMenuItem(tcmd, menuCommandParameters);
            Items.Add(new Separator());
            CreateDescriptionMenuItem(tcmd, menuCommandParameters);
        }

        private void CreateScriptAsCreateMenuItem(ColumnMenuCommandsHandler tcmd, MenuCommandParameters menuCommandParameters)
        {
            var scriptCommandBinding = new CommandBinding(TableMenuCommands.TableCommand, tcmd.ScriptAsCreate);
            var scriptMenuItem = new MenuItem
            {
                Header = "Script as ADD",
                Icon = ImageHelper.GetImageFromResource("../resources/sp.png"),
                Command = TableMenuCommands.TableCommand,
                CommandParameter = menuCommandParameters
            };
            scriptMenuItem.CommandBindings.Add(scriptCommandBinding);
            Items.Add(scriptMenuItem);
        }

        private void CreateScriptAsDropMenuItem(ColumnMenuCommandsHandler tcmd, MenuCommandParameters menuCommandParameters)
        {
            var scriptCommandBinding = new CommandBinding(TableMenuCommands.TableCommand, tcmd.ScriptAsDrop);
            var scriptMenuItem = new MenuItem
            {
                Header = "Script as DROP",
                Icon = ImageHelper.GetImageFromResource("../resources/sp.png"),
                Command = TableMenuCommands.TableCommand,
                CommandParameter = menuCommandParameters
            };
            scriptMenuItem.CommandBindings.Add(scriptCommandBinding);
            Items.Add(scriptMenuItem);
        }

        private void CreateScriptAsAlterMenuItem(ColumnMenuCommandsHandler tcmd, MenuCommandParameters menuCommandParameters)
        {
            var scriptCommandBinding = new CommandBinding(TableMenuCommands.TableCommand, tcmd.ScriptAsAlter);
            var scriptMenuItem = new MenuItem
            {
                Header = "Script as ALTER",
                Icon = ImageHelper.GetImageFromResource("../resources/sp.png"),
                Command = TableMenuCommands.TableCommand,
                CommandParameter = menuCommandParameters
            };
            scriptMenuItem.CommandBindings.Add(scriptCommandBinding);
            Items.Add(scriptMenuItem);
        }

        private void CreateDescriptionMenuItem(ColumnMenuCommandsHandler tcmd, MenuCommandParameters menuCommandParameters)
        {
            var addDescriptionCommandBinding = new CommandBinding(DatabaseMenuCommands.DatabaseCommand,
                                                    tcmd.AddDescription);
            var addDescriptionMenuItem = new MenuItem
            {
                Header = "Edit description",
                Icon = ImageHelper.GetImageFromResource("../resources/propes.png"),
                Command = DatabaseMenuCommands.DatabaseCommand,
                CommandParameter = menuCommandParameters
            };
            addDescriptionMenuItem.CommandBindings.Add(addDescriptionCommandBinding);
            Items.Add(addDescriptionMenuItem);
        }


    }
}