using ErikEJ.SqlCeScripting;
using ErikEJ.SqlCeToolbox.Dialogs;
using ErikEJ.SqlCeToolbox.ToolWindows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ErikEJ.SqlCeToolbox.Commands
{
    public class ColumnMenuCommandsHandler
    {
        private readonly ExplorerControl _parentWindow;
        private BaseCommandHandler _handler;

        public ColumnMenuCommandsHandler(ExplorerControl parent)
        {
            _parentWindow = parent;
            _handler = new BaseCommandHandler(parent);
        }

        public void ScriptAsCreate(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var menuInfo = ValidateMenuInfo(sender);
                if (menuInfo != null)
                {
                    using (IRepository repository = RepoHelper.CreateRepository(menuInfo.Connectionstring))
                    {
                        var generator = RepoHelper.CreateGenerator(repository, null);
                        List<Column> columns = repository.GetAllColumns();
                        var col = columns.Where(c => c.TableName == menuInfo.Description && c.ColumnName == menuInfo.Name).SingleOrDefault();
                        if (col == null)
                        {
                            MessageBox.Show("Could not find the column in the table, has it been dropped?");
                        }
                        else
                        {
                            generator.GenerateColumnAddScript(col);
                        }
                        _handler.OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Helpers.DataConnectionHelper.ShowErrors(ex));
            }
        }

        public void ScriptAsDrop(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var menuInfo = ValidateMenuInfo(sender);
                if (menuInfo != null)
                {
                    using (IRepository repository = RepoHelper.CreateRepository(menuInfo.Connectionstring))
                    {
                        var generator = RepoHelper.CreateGenerator(repository, null);
                        List<Column> columns = repository.GetAllColumns();
                        var col = columns.Where(c => c.TableName == menuInfo.Description && c.ColumnName == menuInfo.Name).SingleOrDefault();
                        if (col == null)
                        {
                            MessageBox.Show("Could not find the column in the table, has it been dropped?");
                        }
                        else
                        {
                            generator.GenerateColumnDropScript(col);
                        }
                        _handler.OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Helpers.DataConnectionHelper.ShowErrors(ex));
            }
        }

        public void ScriptAsAlter(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var menuInfo = ValidateMenuInfo(sender);
                if (menuInfo != null)
                {
                    using (IRepository repository = RepoHelper.CreateRepository(menuInfo.Connectionstring))
                    {
                        var generator = RepoHelper.CreateGenerator(repository, null);
                        List<Column> columns = repository.GetAllColumns();
                        var col = columns.Where(c => c.TableName == menuInfo.Description && c.ColumnName == menuInfo.Name).SingleOrDefault();
                        if (col == null)
                        {
                            MessageBox.Show("Could not find the column in the table, has it been dropped?");
                        }
                        else
                        {
                            generator.GenerateColumnAlterScript(col);
                        }
                        _handler.OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Helpers.DataConnectionHelper.ShowErrors(ex));
            }
        }

        public void AddDescription(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var menuInfo = ValidateMenuInfo(sender);
                if (menuInfo != null)
                {
                    using (IRepository repository = RepoHelper.CreateRepository(menuInfo.Connectionstring))
                    {
                        var desc = ExplorerControl.DescriptionCache.Where(d => d.Object == menuInfo.Name && d.Parent == menuInfo.Description).Select(d => d.Description).SingleOrDefault();
                        DescriptionDialog ro = new DescriptionDialog(desc);
                        ro.Owner = Application.Current.MainWindow;
                        ro.ShowDialog();
                        if (ro.DialogResult.HasValue && ro.DialogResult.Value == true && !string.IsNullOrWhiteSpace(ro.Description) && ro.Description != desc)
                        {
                            new Helpers.DescriptionHelper().SaveDescription(menuInfo.Connectionstring, ExplorerControl.DescriptionCache, ro.Description, menuInfo.Description, menuInfo.Name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Helpers.DataConnectionHelper.ShowErrors(ex));
            }
        }

        private static MenuCommandParameters ValidateMenuInfo(object sender)
        {
            var menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                return menuItem.CommandParameter as MenuCommandParameters;
            }
            else
            {
                return null;
            }
        }

    }
}