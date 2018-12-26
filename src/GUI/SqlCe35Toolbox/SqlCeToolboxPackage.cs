using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using EnvDTE;
using ErikEJ.SqlCeToolbox.Helpers;
using ErikEJ.SqlCeToolbox.ToolWindows;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace ErikEJ.SqlCeToolbox
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "4.7", IconResourceID = 400)]
    [SqlCe40ProviderRegistration]
    [SqliteProviderRegistration]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(ExplorerToolWindow), Style = VsDockStyle.Tabbed, Orientation = ToolWindowOrientation.Left, Window = EnvDTE.Constants.vsWindowKindServerExplorer)]
    [ProvideToolWindow(typeof(SqlEditorWindow), Style = VsDockStyle.MDI, MultiInstances = true, Transient = true)]
    [ProvideToolWindow(typeof(DataGridViewWindow), Style = VsDockStyle.MDI, MultiInstances = true, Transient = true)]
    [ProvideToolWindow(typeof(ReportWindow), Style = VsDockStyle.MDI, MultiInstances = true, Transient = true)]
    [ProvideToolWindow(typeof(SubscriptionWindow), Style = VsDockStyle.MDI, MultiInstances = true, Transient = true)]
    [ProvideOptionPage(typeof(OptionsPageGeneral), "SQLite/SQLCE Toolbox", "General", 100, 101, true)]
    [ProvideOptionPage(typeof(OptionsPageAdvanced), "SQLite/SQLCE Toolbox", "Advanced", 100, 102, true)]
    [Guid(GuidList.guidSqlCeToolboxPkgString)]

    public sealed class SqlCeToolboxPackage : Package
    {
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        //public SqlCeToolboxPackage()
        //{
        //    //Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", ToString()));
        //}

        public void SetProgress(string label, uint progress, uint total)
        {
            var statusBar = (IVsStatusbar)GetService(typeof(SVsStatusbar));
            uint cookie = 0;

            if (label == null)
            {
                // Clear the progress bar.
                statusBar.Clear();
                return;
            }
            // Display incremental progress.
            statusBar.Progress(ref cookie, 1, label, progress, total);
        }

        public void SetStatus(string message)
        {
            var statusBar = GetService(typeof(SVsStatusbar)) as IVsStatusbar;
            if (statusBar != null)
            {
                int frozen;
                statusBar.IsFrozen(out frozen);
                if (!Convert.ToBoolean(frozen))
                {
                    if (message == null)
                    {
                        statusBar.Clear();
                    }
                    else
                    {
                        statusBar.SetText(message);
                    }
                }
            }
            // Attempt to fix issue with new VS versions #725
            //if (message != null)
            //    OutputStringInGeneralPane(message);
        }

        private void OutputStringInGeneralPane(string text)
        {
            const int visible = 1;
            const int doNotClearWithSolution = 0;

            // Get the output window
            var outputWindow = GetService(typeof(SVsOutputWindow)) as IVsOutputWindow;

            // The General pane is not created by default. We must force its creation
            if (outputWindow != null)
            {
                var hr = outputWindow.CreatePane(VSConstants.OutputWindowPaneGuid.GeneralPane_guid, "General", visible, doNotClearWithSolution);
                ErrorHandler.ThrowOnFailure(hr);

                // Get the pane
                IVsOutputWindowPane outputWindowPane;
                hr = outputWindow.GetPane(VSConstants.OutputWindowPaneGuid.GeneralPane_guid, out outputWindowPane);
                ErrorHandler.ThrowOnFailure(hr);

                // Output the text
                if (outputWindowPane != null)
                {
                    outputWindowPane.Activate();
                    outputWindowPane.OutputString(text + Environment.NewLine);
                }
            }
        }

        private void ShowToolWindow(object sender, EventArgs e)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            var window = FindToolWindow(typeof(ExplorerToolWindow), 0, true);
            if (window?.Frame == null)
            {
                throw new NotSupportedException(Resources.CanNotCreateWindow);
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        /// <summary>
        /// Support method for finding an existing or creating a new tool window based on type and _id.
        /// </summary>
        /// <typeparam name="T">type of MDI tool window</typeparam>
        /// <param name="windowId"></param>
        /// <returns>the tool window pane</returns>
        public ToolWindowPane CreateWindow<T>(int windowId)
        {
            //find existing tool window based on _id
            var window = FindToolWindow(typeof(T), windowId, false);

            if (window == null)
            {
                //create a new window with explicit tool window _id
                window = FindToolWindow(typeof(T), windowId, true);
                if (window?.Frame == null)
                {
                    throw new NotSupportedException(Resources.CanNotCreateWindow);
                }
            }
            ShowWindow(window);
            return window;

        }

        private int _id;
        /// <summary>
        /// Support method for creating a new tool window based on type.
        /// </summary>
        /// <typeparam name="T">type of MDI tool window</typeparam>
        /// <returns>the tool window pane</returns>
        public ToolWindowPane CreateWindow<T>()
        {
            _id++;
            //create a new window with explicit tool window _id
            var window = FindToolWindow(typeof(T), _id, true);
            if (window?.Frame == null)
            {
                throw new NotSupportedException(Resources.CanNotCreateWindow);
            }
            ShowWindow(window);
            return window;
        }

        public object GetServiceHelper(Type type)
        {
            return GetService(type);
        }

        public void ShowWindow(ToolWindowPane window)
        {
            var windowFrame = (IVsWindowFrame)window.Frame;
            ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        public static Version VisualStudioVersion
        {
            get
            {
                var dte = GetGlobalService(typeof(DTE)) as DTE;
                return dte != null 
                    ? new Version(int.Parse(dte.Version.Split('.')[0], CultureInfo.InvariantCulture), 0) 
                    : new Version(0,0,0,0);
            }
        }

        public Version TelemetryVersion() => VisualStudioVersion;

        public bool VsSupportsDdex40()
        {
            return Properties.Settings.Default.PreferDDEX && DataConnectionHelper.DdexProviderIsInstalled(new Guid(Resources.SqlCompact40Provider));
        }

        public bool VsSupportsDdex35()
        {
            return Properties.Settings.Default.PreferDDEX && DataConnectionHelper.DdexProviderIsInstalled(new Guid(Resources.SqlCompact35Provider));
        }

        public static bool VsSupportsEf6()
        {
            return VisualStudioVersion >= new Version(11, 0);
        }

        public bool VsSupportsSimpleDdex4Provider()
        {
            return ( VisualStudioVersion >= new Version(12, 0))
                && (DataConnectionHelper.DdexProviderIsInstalled(new Guid(Resources.SqlCompact40PrivateProvider)))
                && (Helpers.RepositoryHelper.IsV40Installed())
                && (Helpers.RepositoryHelper.IsV40DbProviderInstalled());
        }

        public static bool IsVsExtension => true;
        /////////////////////////////////////////////////////////////////////////////
        // Overriden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", ToString()));

            // Add our command handlers for menu (commands must exist in the .vsct file)
            var mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                // Create the command for the menu item.
                var menuCommandId = new CommandID(GuidList.guidSqlCeToolboxCmdSet, (int)PkgCmdIDList.cmdidMyCommand);
                var menuItem = new MenuCommand(ShowToolWindow, menuCommandId);
                mcs.AddCommand(menuItem);
                // Create the command for the tool window
                var toolwndCommandId = new CommandID(GuidList.guidSqlCeToolboxCmdSet, (int)PkgCmdIDList.cmdidMyTool);
                var menuToolWin = new MenuCommand(ShowToolWindow, toolwndCommandId);
                mcs.AddCommand(menuToolWin);

                // Server Explorer button 
                var seCommandId = new CommandID(GuidList.guidSEPlusCmdSet, (int)PkgCmdIDList.cmdidSEHello);
                var seItem = new MenuCommand(ShowToolWindow, seCommandId);
                mcs.AddCommand(seItem);
            }
            base.Initialize();
        }
        #endregion
    }
}
