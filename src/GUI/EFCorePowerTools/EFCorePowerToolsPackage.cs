using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using EFCorePowerTools.Extensions;
using EFCorePowerTools.Handlers;
using EnvDTE;
using EnvDTE80;
using ErikEJ.SqlCeToolbox.Helpers;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio;

namespace EFCorePowerTools
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [SqlCe40ProviderRegistration]
    [SqliteProviderRegistration]
    [InstalledProductRegistration("#110", "#112", "0.9", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(GuidList.guidDbContextPackagePkgString)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    // ReSharper disable once InconsistentNaming
    public sealed class EFCorePowerToolsPackage : Package
    {
        private readonly ReverseEngineerHandler _reverseEngineerHandler;
        private readonly ModelAnalyzerHandler _modelAnalyzerHandler;
        private readonly AboutHandler _aboutHandler;
        private readonly DgmlNugetHandler _dgmlNugetHandler;
        private readonly ServerDgmlHandler _serverDgmlHandler;
        private DTE2 _dte2;

        public EFCorePowerToolsPackage()
        {
            _reverseEngineerHandler = new ReverseEngineerHandler(this);
            _modelAnalyzerHandler = new ModelAnalyzerHandler(this);
            _aboutHandler = new AboutHandler(this);
            _dgmlNugetHandler = new DgmlNugetHandler(this);
            _serverDgmlHandler = new ServerDgmlHandler(this);
        }

        internal DTE2 Dte2 => _dte2;

        protected override void Initialize()
        {
            base.Initialize();

            _dte2 = GetService(typeof(DTE)) as DTE2;

            if (_dte2 == null)
            {
                return;
            }

            var oleMenuCommandService
                = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;

            if (oleMenuCommandService != null)
            {
                var menuCommandId3 = new CommandID(GuidList.guidDbContextPackageCmdSet,
                    (int)PkgCmdIDList.cmdidDgmlBuild);
                var menuItem3 = new OleMenuCommand(OnProjectContextMenuInvokeHandler, null,
                    OnProjectMenuBeforeQueryStatus, menuCommandId3);
                oleMenuCommandService.AddCommand(menuItem3);

                var menuCommandId4 = new CommandID(GuidList.guidDbContextPackageCmdSet,
                    (int)PkgCmdIDList.cmdidReverseEngineerDgml);
                var menuItem4 = new OleMenuCommand(OnProjectContextMenuInvokeHandler, null,
                    OnProjectMenuBeforeQueryStatus, menuCommandId4);
                oleMenuCommandService.AddCommand(menuItem4);

                var menuCommandId5 = new CommandID(GuidList.guidDbContextPackageCmdSet,
                    (int) PkgCmdIDList.cmdidReverseEngineerCodeFirst);
                var menuItem5 = new OleMenuCommand(OnProjectContextMenuInvokeHandler, null,
                    OnProjectMenuBeforeQueryStatus, menuCommandId5);
                oleMenuCommandService.AddCommand(menuItem5);

                var menuCommandId7 = new CommandID(GuidList.guidDbContextPackageCmdSet,
                    (int)PkgCmdIDList.cmdidAbout);
                var menuItem7 = new OleMenuCommand(OnProjectContextMenuInvokeHandler, null,
                    OnProjectMenuBeforeQueryStatus, menuCommandId7);
                oleMenuCommandService.AddCommand(menuItem7);

                var menuCommandId8 = new CommandID(GuidList.guidDbContextPackageCmdSet,
                    (int)PkgCmdIDList.cmdidDgmlNuget);
                var menuItem8 = new OleMenuCommand(OnProjectContextMenuInvokeHandler, null,
                    OnProjectMenuBeforeQueryStatus, menuCommandId8);
                oleMenuCommandService.AddCommand(menuItem8);

                var menuCommandId9 = new CommandID(GuidList.guidDbContextPackageCmdSet,
                   (int)PkgCmdIDList.cmdidSqlBuild);
                var menuItem9 = new OleMenuCommand(OnProjectContextMenuInvokeHandler, null,
                    OnProjectMenuBeforeQueryStatus, menuCommandId9);
                oleMenuCommandService.AddCommand(menuItem9);
            }

            //Boot Telemetry
            Telemetry.Enabled = true;
            if (Telemetry.Enabled)
            {
                Telemetry.Initialize(Dte2,
                    Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                    VisualStudioVersion.ToString(),
                    "d4881a82-2247-42c9-9272-f7bc8aa29315");
            }
            Telemetry.TrackEvent("Platform: Visual Studio " + VisualStudioVersion.ToString(1));

            // AssemblyBindingRedirectHelper.ConfigureBindingRedirects();
        }

        private void OnProjectMenuBeforeQueryStatus(object sender, EventArgs e)
        {
            var menuCommand = sender as MenuCommand;

            if (menuCommand == null)
            {
                return;
            }

            if (_dte2.SelectedItems.Count != 1)
            {
                return;
            }

            var project = _dte2.SelectedItems.Item(1).Project;

            if (project == null)
            {
                return;
            }

            menuCommand.Visible =
                project.Kind == "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}" ||
                project.Kind == "{9A19103F-16F7-4668-BE54-9A1E7A4F7556}"; // csproj
        }

        private void OnProjectContextMenuInvokeHandler(object sender, EventArgs e)
        {
            var menuCommand = sender as MenuCommand;
            if (menuCommand == null || _dte2.SelectedItems.Count != 1)
            {
                return;
            }

            var project = _dte2.SelectedItems.Item(1).Project;
            if (project == null)
            {
                return;
            }

            if (menuCommand.CommandID.ID == PkgCmdIDList.cmdidReverseEngineerCodeFirst)
            {
                _reverseEngineerHandler.ReverseEngineerCodeFirst(project);
            }
            else if (menuCommand.CommandID.ID == PkgCmdIDList.cmdidReverseEngineerDgml)
            {
                _serverDgmlHandler.GenerateServerDgmlFiles();
            }
            else if (menuCommand.CommandID.ID == PkgCmdIDList.cmdidDgmlNuget)
            {
                _dgmlNugetHandler.InstallDgmlNuget(project);
            }
            else if (menuCommand.CommandID.ID == PkgCmdIDList.cmdidDgmlBuild)
            {
                var path = LocateProjectAssemblyPath(project);
                if (path != null)
                {
                    _modelAnalyzerHandler.Generate(path, project);
                }
            }
            else if (menuCommand.CommandID.ID == PkgCmdIDList.cmdidSqlBuild)
            {
                var path = LocateProjectAssemblyPath(project);
                if (path != null)
                {
                    _modelAnalyzerHandler.Generate(path, project, generateDdl: true);
                }
            }
            else if (menuCommand.CommandID.ID == PkgCmdIDList.cmdidAbout)
            {
                _aboutHandler.ShowDialog();
            }
        }

        private string LocateProjectAssemblyPath(Project project)
        {
            if (!project.TryBuild())
            {
                _dte2.StatusBar.Text = "Build failed. Unable to discover a DbContext class.";

                return null;
            }

            var path = project.GetOutPutAssemblyPath();
            if (path != null)
            {
                return path;
            }

            _dte2.StatusBar.Text = "Unable to locate project assembly.";

            return null;
        }

        internal void LogError(List<string> statusMessages, Exception exception)
        {
            _dte2.StatusBar.Text = "An error occurred. See the Output window for details.";

            var buildOutputWindow = _dte2.ToolWindows.OutputWindow.OutputWindowPanes.Item("Build");
            buildOutputWindow.OutputString(Environment.NewLine);

            foreach (var error in statusMessages)
            {
                buildOutputWindow.OutputString(error + Environment.NewLine);
            }
            if (exception != null)
            {
                buildOutputWindow.OutputString(exception + Environment.NewLine);
            }

            buildOutputWindow.Activate();
        }

        private Version VisualStudioVersion => new Version(int.Parse(_dte2.Version.Split('.')[0], CultureInfo.InvariantCulture), 0);

        internal T GetService<T>()
            where T : class
        {
            return (T)GetService(typeof(T));
        }

        internal TResult GetService<TService, TResult>()
            where TService : class
            where TResult : class
        {
            return (TResult)GetService(typeof(TService));
        }
    }
}
