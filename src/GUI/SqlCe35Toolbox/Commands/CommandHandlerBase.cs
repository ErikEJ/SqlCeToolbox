using System;
using System.Collections.Generic;
using ErikEJ.SqlCeToolbox.ToolWindows;
using System.Diagnostics;

namespace ErikEJ.SqlCeToolbox.Commands
{
    public abstract class CommandHandlerBase
    {
        private ExplorerToolWindow _parentWindow;

        public ExplorerToolWindow ParentWindow
        {
            get
            {
                return _parentWindow;
            }
            set
            {
                _parentWindow = value;
            }
        }

        public void OpenSqlEditorToolWindow(MenuCommandParameters menuInfo, string script, List<string> tables = null, List<SqlCeScripting.Column> columns = null)
        {
            try
            {
                var pkg = _parentWindow.Package as SqlCeToolboxPackage;
                Debug.Assert(pkg != null, "Package property of the Explorere Tool Window should never be null, have you tried to create it manually and not through FindToolWindow()?");

                var sqlEditorWindow = pkg.CreateWindow<SqlEditorWindow>();
                var control = sqlEditorWindow.Content as SqlEditorControl;
                if (control != null)
                {
                    control.DatabaseInfo = menuInfo.DatabaseInfo;
                    control.ExplorerControl = _parentWindow.Content as ExplorerControl;
                    control.SqlText = script;
                }
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, menuInfo.DatabaseInfo.DatabaseType);
            }
        }
    }
}
