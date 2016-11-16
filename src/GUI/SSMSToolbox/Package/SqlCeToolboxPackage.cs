using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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

// ReSharper disable once CheckNamespace
namespace ErikEJ.SqlCeToolbox
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "4.6", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(ExplorerToolWindow), Style = VsDockStyle.Tabbed, Orientation = ToolWindowOrientation.Left, Window = "DocumentWell")]
    [ProvideToolWindow(typeof(SqlEditorWindow), Style = VsDockStyle.MDI, MultiInstances = true, Transient = true)]
    [ProvideToolWindow(typeof(DataGridViewWindow), Style = VsDockStyle.MDI, MultiInstances = true, Transient = true)]
    [ProvideToolWindow(typeof(ReportWindow), Style = VsDockStyle.MDI, MultiInstances = true, Transient = true)]
    [ProvideToolWindow(typeof(SubscriptionWindow), Style = VsDockStyle.MDI, MultiInstances = true, Transient = true)]
    [ProvideOptionPage(typeof(OptionsPageGeneral), "SQLCE/SQLite Toolbox", "General", 100, 101, true)]
    [ProvideOptionPage(typeof(OptionsPageAdvanced), "SQLCE/SQLite Toolbox", "Advanced", 100, 102, true)]
    [Guid(PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class SqlCeToolboxPackage : Package
    {
        /// <summary>
        /// ExplorerToolWindowPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "9b80f327-181a-496f-93d9-dcf03d56a792";

        /// <summary>
        /// Initializes a new instance of the <see cref="ExplorerToolWindow"/> class.
        /// </summary>
        public SqlCeToolboxPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", ToString()));
        }

        public object GetServiceHelper(Type type)
        {
            return GetService(type);
        }

        public bool VsSupportsSimpleDdex35Provider()
        {
            return false;
        }

        public bool VsSupportsSimpleDdex4Provider()
        {
            return false;
        }

        public static bool VsSupportsEf6()
        {
            return false;
        }

        public bool VsSupportsDdex40()
        {
            return false;
        }

        public bool VsSupportsDdex35()
        {
            return false;
        }

        public static Version VisualStudioVersion
        {
            get
            {
                return  new Version(0, 0, 0, 0);
            }
        }

        public static bool IsVsExtension
        {
            get { return false; }
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

        public void ShowWindow(ToolWindowPane window)
        {
            var windowFrame = (IVsWindowFrame)window.Frame;
            ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            var dte = (DTE2)GetService(typeof(DTE));
            Telemetry.Enabled = Properties.Settings.Default.ParticipateInTelemetry;
            if (Telemetry.Enabled)
            {
                Telemetry.Initialize(dte,
                    Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                    //TODO Make this dynamic!
                    "130",
                    "d4881a82-2247-42c9-9272-f7bc8aa29315");
            }
            DataConnectionHelper.LogUsage("Platform: SSMS 130");
            OtherWindowsCommand.Initialize(this);
            ViewMenuCommand.Initialize(this);
            base.Initialize();            
        }
        #endregion
    }
}
