using System;
using System.Data;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;
using ErikEJ.SqlCeToolbox.Helpers;
using ErikEJ.SqlCeToolbox.ToolWindows;

namespace ErikEJ.SqlCeToolbox.Commands
{
    public class ViewMenuCommandsHandler :CommandHandlerBase
    {
        private string separator = ";" + Environment.NewLine;

        public ViewMenuCommandsHandler(ExplorerToolWindow parent)
        {
            ParentWindow = parent;
        }

        public void ScriptAsCreate(object sender, ExecutedRoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var menuInfo = menuItem?.CommandParameter as MenuCommandParameters;
            if (menuInfo == null) return;
            try
            {
                OpenSqlEditorToolWindow(menuInfo,  menuInfo.Description + separator);
                DataConnectionHelper.LogUsage("ViewScriptAsCreate");
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void ScriptAsDrop(object sender, ExecutedRoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var menuInfo = menuItem?.CommandParameter as MenuCommandParameters;
            if (menuInfo == null) return;
            try
            {
                OpenSqlEditorToolWindow(menuInfo, string.Format("DROP VIEW [{0}]" + separator, menuInfo.Name));
                DataConnectionHelper.LogUsage("ViewScriptAsDrop");
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void ScriptAsDropAndCreate(object sender, ExecutedRoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var menuInfo = menuItem?.CommandParameter as MenuCommandParameters;
            if (menuInfo == null) return;
            try
            {
                var sql = string.Format("DROP VIEW [{0}]" + separator, menuInfo.Name);
                sql += string.Format(menuInfo.Description + separator);
                OpenSqlEditorToolWindow(menuInfo, sql);
                DataConnectionHelper.LogUsage("ViewScriptAsDropAndCreate");
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void ScriptAsSelect(object sender, ExecutedRoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var menuInfo = menuItem?.CommandParameter as MenuCommandParameters;
            if (menuInfo == null) return;
            try
            {
                using (var repository = Helpers.RepositoryHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    var generator = DataConnectionHelper.CreateGenerator(repository, menuInfo.DatabaseInfo.DatabaseType);
                    generator.GenerateViewSelect(menuInfo.Name);
                    var script = generator.GeneratedScript;
                    OpenSqlEditorToolWindow(menuInfo, script);
                    DataConnectionHelper.LogUsage("ViewScriptAsSelect");
                }
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void SpawnSqlEditorWindow(object sender, ExecutedRoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var menuInfo = menuItem?.CommandParameter as MenuCommandParameters;
            if (menuInfo == null) return;
            OpenSqlEditorToolWindow(menuInfo, string.Empty);
        }

        public void ReportTableData(object sender, ExecutedRoutedEventArgs e)
        {
            string sqlText;
            var menuItem = sender as MenuItem;
            var ds = new DataSet();
            var menuInfo = menuItem?.CommandParameter as MenuCommandParameters;
            if (menuInfo == null) return;
            try
            {
                using (var repository = Helpers.RepositoryHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    var generator = DataConnectionHelper.CreateGenerator(repository, menuInfo.DatabaseInfo.DatabaseType);
                    generator.GenerateViewSelect(menuInfo.Name);
                    OpenSqlEditorToolWindow(menuInfo, generator.GeneratedScript);

                    sqlText = Environment.NewLine + generator.GeneratedScript
                        + Environment.NewLine + "GO";
                    ds = repository.ExecuteSql(sqlText);
                }
                var pkg = ParentWindow.Package as SqlCeToolboxPackage;
                Debug.Assert(pkg != null, "Package property of the Explorere Tool Window should never be null, have you tried to create it manually and not through FindToolWindow()?");

                string dbName = System.IO.Path.GetFileNameWithoutExtension(menuInfo.DatabaseInfo.Caption);
                if (dbName != null)
                {
                    var window = pkg.CreateWindow<ReportWindow>(Math.Abs(menuInfo.Name.GetHashCode() - dbName.GetHashCode()));
                    if (window == null) return;
                    window.Caption = menuInfo.Name + " (" + dbName + ")";
                    pkg.ShowWindow(window);

                    var control = window.Content as ReportControl;
                    if (control != null)
                    {
                        control.DatabaseInfo = menuInfo.DatabaseInfo;
                        control.TableName = menuInfo.Name;
                        control.DataSet = ds;
                        control.ShowReport();
                    }
                }
                DataConnectionHelper.LogUsage("ViewReport");
            }
            catch (System.IO.FileNotFoundException)
            {
                EnvDteHelper.ShowError("Microsoft Report Viewer 2010 not installed, please download and install to use this feature  http://www.microsoft.com/en-us/download/details.aspx?id=6442");
                return;
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
            ds.Dispose();
        }   
    }
}