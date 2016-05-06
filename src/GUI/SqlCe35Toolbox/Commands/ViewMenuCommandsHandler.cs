using System;
using System.Data;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;
using ErikEJ.SqlCeScripting;
using ErikEJ.SqlCeToolbox.Helpers;
using ErikEJ.SqlCeToolbox.ToolWindows;

namespace ErikEJ.SqlCeToolbox.Commands
{
    public class ViewMenuCommandsHandler :CommandHandlerBase
    {
        private string separator = ";" + Environment.NewLine + "GO" + Environment.NewLine;

        public ViewMenuCommandsHandler(ExplorerToolWindow parent)
        {
            ParentWindow = parent;
        }

        public void ScriptAsCreate(object sender, ExecutedRoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null) return;
            var menuInfo = menuItem.CommandParameter as MenuCommandParameters;
            if (menuInfo == null) return;
            try
            {
                using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    OpenSqlEditorToolWindow(menuInfo, string.Format("CREATE VIEW [{0}] AS {1}{2}" + separator, menuInfo.Name, Environment.NewLine, menuInfo.Description));
                    Helpers.DataConnectionHelper.LogUsage("ViewScriptAsCreate");
                }
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void ScriptAsDrop(object sender, ExecutedRoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null) return;
            var menuInfo = menuItem.CommandParameter as MenuCommandParameters;
            if (menuInfo == null) return;
            try
            {
                using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    OpenSqlEditorToolWindow(menuInfo, string.Format("DROP VIEW [{0}]" + separator, menuInfo.Name));
                    Helpers.DataConnectionHelper.LogUsage("ViewScriptAsDrop");
                }
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType, false);
            }
        }

        public void ReportTableData(object sender, ExecutedRoutedEventArgs e)
        {
            string sqlText = null;
            var menuItem = sender as MenuItem;
            var ds = new DataSet();
            if (menuItem == null) return;
            var menuInfo = menuItem.CommandParameter as MenuCommandParameters;
            if (menuInfo == null) return;
            try
            {
                using (IRepository repository = Helpers.DataConnectionHelper.CreateRepository(menuInfo.DatabaseInfo))
                {
                    sqlText = string.Format(Environment.NewLine + "SELECT * FROM [{0}]", menuInfo.Name)
                        + Environment.NewLine + "GO";
                    ds = repository.ExecuteSql(sqlText);
                }
                var pkg = ParentWindow.Package as SqlCeToolboxPackage;
                Debug.Assert(pkg != null, "Package property of the Explorere Tool Window should never be null, have you tried to create it manually and not through FindToolWindow()?");

                string dbName = System.IO.Path.GetFileNameWithoutExtension(menuInfo.DatabaseInfo.Caption);
                var window = pkg.CreateWindow<ReportWindow>(Math.Abs(menuInfo.Name.GetHashCode() - dbName.GetHashCode()));
                window.Caption = menuInfo.Name + " (" + dbName + ")";
                pkg.ShowWindow(window);

                var control = window.Content as ReportControl;
                control.DatabaseInfo = menuInfo.DatabaseInfo;
                control.TableName = menuInfo.Name;
                control.DataSet = ds;
                control.ShowReport();
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