using System.Windows.Controls;
using System.Windows.Input;
using ErikEJ.SqlCeToolbox.ToolWindows;
using ErikEJ.SqlCeScripting;
using FabTab;

namespace ErikEJ.SqlCeToolbox.Commands
{
    public class IndexMenuCommandsHandler
    {
        private readonly ExplorerControl _parentWindow;

        public IndexMenuCommandsHandler(ExplorerControl parent)
        {
            _parentWindow = parent;
        }

        public void ScriptAsCreate(object sender, ExecutedRoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                var menuInfo = menuItem.CommandParameter as MenuCommandParameters;

                if (menuInfo != null)
                {
                    using (IRepository repository = RepoHelper.CreateRepository(menuInfo.Connectionstring))
                    {
                        var generator = RepoHelper.CreateGenerator(repository, null);
                        generator.GenerateIndexScript(menuInfo.Name, menuInfo.Caption);
                        OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);
                    }
                }
            }
        }

        public void ScriptAsDrop(object sender, ExecutedRoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                var menuInfo = menuItem.CommandParameter as MenuCommandParameters;

                if (menuInfo != null)
                {
                    using (IRepository repository = RepoHelper.CreateRepository(menuInfo.Connectionstring))
                    {
                        var generator = RepoHelper.CreateGenerator(repository, null);
                        generator.GenerateIndexDrop(menuInfo.Name, menuInfo.Caption);
                        OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);
                    }
                }
            }
        }

        public void ScriptAsStatistics(object sender, ExecutedRoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                var menuInfo = menuItem.CommandParameter as MenuCommandParameters;

                if (menuInfo != null)
                {
                    using (IRepository repository = RepoHelper.CreateRepository(menuInfo.Connectionstring))
                    {
                        var generator = RepoHelper.CreateGenerator(repository, null);
                        generator.GenerateIndexStatistics(menuInfo.Name, menuInfo.Caption);
                        OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);
                    }
                }
            }
        }

        private void OpenSqlEditorToolWindow(MenuCommandParameters menuInfo, string script)
        {
            SqlEditorControl editor = new SqlEditorControl();
            editor.Database = menuInfo.Connectionstring;
            editor.SqlText = script;
            FabTabItem tab = new FabTabItem();
            tab.Content = editor;
            tab.Header = menuInfo.Caption;
            _parentWindow.FabTab.Items.Add(tab);
            _parentWindow.FabTab.SelectedIndex = _parentWindow.FabTab.Items.Count - 1; 
            return;
        }




    }
}