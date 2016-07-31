using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using EnvDTE;
using EnvDTE80;
using ErikEJ.SqlCeToolbox.Helpers;
using ErikEJ.SqlCeToolbox.ToolWindows;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace ErikEJ.SqlCeToolbox
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "4.6.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(ExplorerToolWindow), Style = VsDockStyle.Tabbed, Orientation = ToolWindowOrientation.Left)]
    [ProvideToolWindow(typeof(SqlEditorWindow), Style = VsDockStyle.MDI, MultiInstances = true, Transient = true)]
    [ProvideToolWindow(typeof(DataGridViewWindow), Style = VsDockStyle.MDI, MultiInstances = true, Transient = true)]
    [ProvideToolWindow(typeof(ReportWindow), Style = VsDockStyle.MDI, MultiInstances = true, Transient = true)]
    [ProvideToolWindow(typeof(SubscriptionWindow), Style = VsDockStyle.MDI, MultiInstances = true, Transient = true)]
    [ProvideOptionPage(typeof(OptionsPageGeneral), "SQLCE/SQLite Toolbox", "General", 100, 101, true)]
    [ProvideOptionPage(typeof(OptionsPageAdvanced), "SQLCE/SQLite Toolbox", "Advanced", 100, 102, true)]
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
        public SqlCeToolboxPackage()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", ToString()));
        }

        public void SetStatus(string message)
        {
            int frozen;
            IVsStatusbar statusBar = GetService(typeof(SVsStatusbar)) as IVsStatusbar;
            if (statusBar != null)
            {
                statusBar.IsFrozen(out frozen);
                if (!Convert.ToBoolean(frozen))
                {
                    statusBar.SetText(message);
                }
            }
            OutputStringInGeneralPane(message);
        }

        private void OutputStringInGeneralPane(string text)
        {
            const int visible = 1;
            const int doNotClearWithSolution = 0;

            IVsOutputWindow outputWindow;
            IVsOutputWindowPane outputWindowPane;
            int hr;

            // Get the output window
            outputWindow = GetService(typeof(SVsOutputWindow)) as IVsOutputWindow;

            // The General pane is not created by default. We must force its creation
            if (outputWindow != null)
            {
                hr = outputWindow.CreatePane(VSConstants.OutputWindowPaneGuid.GeneralPane_guid, "General", visible, doNotClearWithSolution);
                ErrorHandler.ThrowOnFailure(hr);

                // Get the pane
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
            ToolWindowPane window = FindToolWindow(typeof(ExplorerToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException(Resources.CanNotCreateWindow);
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            ErrorHandler.ThrowOnFailure(windowFrame.Show());
            //DockWindowIfFloating(windowFrame);
        }

        ///// <summary>
        ///// Docks the specified frame window if it is currently floating.
        ///// </summary>
        ///// <param name="frame">The frame.</param>
        //private static void DockWindowIfFloating(IVsWindowFrame frame)
        //{
        //    // Get the current tool window frame mode.
        //    object currentFrameMode;
        //    frame.GetProperty((int)__VSFPROPID.VSFPROPID_FrameMode, out currentFrameMode);

        //    // If currently floating, switch to dock mode.
        //    if ((VSFRAMEMODE)currentFrameMode == VSFRAMEMODE.VSFM_Float)
        //    {
        //        frame.SetProperty((int)__VSFPROPID.VSFPROPID_FrameMode, VSFRAMEMODE.VSFM_Dock);
        //    }
        //}

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
                if ((null == window) || (null == window.Frame))
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
            if ((null == window) || (null == window.Frame))
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

        public bool VsSupportsSqlPlan()
        {
            return VisualStudioVersion == new Version(10, 0) && (DataConnectionHelper.IsPremiumOrUltimate());
        }

        public bool VsSupportsSimpleDdex4Provider()
        {
            return ( VisualStudioVersion >= new Version(12, 0))
                && (DataConnectionHelper.DdexProviderIsInstalled(new Guid(Resources.SqlCompact40PrivateProvider)))
                && (DataConnectionHelper.IsV40Installed())
                && (DataConnectionHelper.IsV40DbProviderInstalled());
        }

        public bool VsSupportsSimpleDdex35Provider()
        {
            return VsSupportsEf6()
                && (DataConnectionHelper.DdexProviderIsInstalled(new Guid(Resources.SqlCompact35PrivateProvider)))
                && (DataConnectionHelper.IsV35Installed())
                && (DataConnectionHelper.IsV35DbProviderInstalled());
        }

        public static bool IsVsExtension
        {
            get { return true; }
        }
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

            var dte = (DTE2)GetService(typeof(DTE));
            Telemetry.Enabled = Properties.Settings.Default.ParticipateInTelemetry;
            if (Telemetry.Enabled)
            {
                Telemetry.Initialize(dte,
                    Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                    VisualStudioVersion.ToString(),
                    "d4881a82-2247-42c9-9272-f7bc8aa29315");
            }

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                // Create the command for the menu item.
                CommandID menuCommandId = new CommandID(GuidList.guidSqlCeToolboxCmdSet, (int)PkgCmdIDList.cmdidMyCommand);
                MenuCommand menuItem = new MenuCommand(ShowToolWindow, menuCommandId);
                mcs.AddCommand(menuItem);
                // Create the command for the tool window
                CommandID toolwndCommandId = new CommandID(GuidList.guidSqlCeToolboxCmdSet, (int)PkgCmdIDList.cmdidMyTool);
                MenuCommand menuToolWin = new MenuCommand(ShowToolWindow, toolwndCommandId);
                mcs.AddCommand(menuToolWin);

                // Server Explorer button 
                CommandID seCommandId = new CommandID(GuidList.guidSEPlusCmdSet, (int)PkgCmdIDList.cmdidSEHello);
                MenuCommand seItem = new MenuCommand(ShowToolWindow, seCommandId);
                mcs.AddCommand(seItem);
                DataConnectionHelper.LogUsage("Platform: Visual Studio " + VisualStudioVersion.ToString(1));
            }
            DataConnectionHelper.RegisterDdexProviders(false);
            base.Initialize();
        }
        #endregion
    }
}
