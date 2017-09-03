using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using EFCorePowerTools.Handlers;
using EnvDTE;
using EnvDTE80;
using ErikEJ.SqlCeToolbox.Helpers;
using Microsoft.DbContextPackage;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio;

namespace EFCorePowerTools
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [SqlCe40ProviderRegistration]
    [SqliteProviderRegistration]
    [InstalledProductRegistration("#110", "#112", "0.1", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(GuidList.guidDbContextPackagePkgString)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    // ReSharper disable once InconsistentNaming
    public sealed class EFCorePowerToolsPackage : Package
    {
        private readonly ReverseEngineerCodeFirstHandler _reverseEngineerCodeFirstHandler;
        private DTE2 _dte2;

        public EFCorePowerToolsPackage()
        {
            _reverseEngineerCodeFirstHandler = new ReverseEngineerCodeFirstHandler(this);
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
                var menuCommandId5 = new CommandID(GuidList.guidDbContextPackageCmdSet,
                    (int) PkgCmdIDList.cmdidReverseEngineerCodeFirst);
                var menuItem5 = new OleMenuCommand(OnProjectContextMenuInvokeHandler, null,
                    OnProjectMenuBeforeQueryStatus, menuCommandId5);

                oleMenuCommandService.AddCommand(menuItem5);
            }
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
                _reverseEngineerCodeFirstHandler.ReverseEngineerCodeFirst(project);
            }
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

            //TODO ErikEJ Extend list!
            menuCommand.Visible =
                project.Kind == "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}" ||
                project.Kind == "{9A19103F-16F7-4668-BE54-9A1E7A4F7556}"; // csproj
        }

        internal void LogError(List<string> statusMessages, Exception exception)
        {
            _dte2.StatusBar.Text = "An error occurred while reverse engineering Code First. See the Output window for details.";

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
