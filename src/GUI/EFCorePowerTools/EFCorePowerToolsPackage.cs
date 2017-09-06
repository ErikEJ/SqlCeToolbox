using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using EFCorePowerTools.Extensions;
using EFCorePowerTools.Handlers;
using EnvDTE;
using EnvDTE80;
using ErikEJ.SqlCeToolbox.Helpers;
using Microsoft.DbContextPackage;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Design;
using Microsoft.VisualStudio.Shell.Interop;

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
        private readonly ModelAnalyzerHandler _modelAnalyzerHandler;
        private readonly AboutHandler _aboutHandler;
        private DTE2 _dte2;

        public EFCorePowerToolsPackage()
        {
            _reverseEngineerCodeFirstHandler = new ReverseEngineerCodeFirstHandler(this);
            _modelAnalyzerHandler = new ModelAnalyzerHandler(this);
            _aboutHandler = new AboutHandler(this);
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

                var menuCommandId6 = new CommandID(GuidList.guidDbContextPackageCmdSet,
                    (int)PkgCmdIDList.cmdidDebugView);
                var menuItem6 = new OleMenuCommand(OnItemContextMenuInvokeHandler, null,
                    OnItemMenuBeforeQueryStatus, menuCommandId6);
                oleMenuCommandService.AddCommand(menuItem6);

                var menuCommandId7 = new CommandID(GuidList.guidDbContextPackageCmdSet,
                    (int)PkgCmdIDList.cmdidAbout);
                var menuItem7 = new OleMenuCommand(OnProjectContextMenuInvokeHandler, null,
                    OnProjectMenuBeforeQueryStatus, menuCommandId7);
                oleMenuCommandService.AddCommand(menuItem7);
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
            else if (menuCommand.CommandID.ID == PkgCmdIDList.cmdidAbout)
            {
                _aboutHandler.ShowDialog();
            }
        }

        private void OnItemContextMenuInvokeHandler(object sender, EventArgs e)
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

            try
            {
                var contextType = DiscoverUserContextType();

                if (contextType != null)
                {
                    if (menuCommand.CommandID.ID == PkgCmdIDList.cmdidDebugView)
                    {
                        _modelAnalyzerHandler.GenerateDebugView(contextType);
                    }
                }
            }
            catch (TargetInvocationException ex)
            {
                var innerException = ex.InnerException;

                var remoteStackTraceString =
                    typeof(Exception).GetField("_remoteStackTraceString", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?? typeof(Exception).GetField("remote_stack_trace", BindingFlags.Instance | BindingFlags.NonPublic);
                remoteStackTraceString.SetValue(innerException, innerException.StackTrace + "$$RethrowMarker$$");

                throw innerException;
            }
        }


        private dynamic DiscoverUserContextType()
        {
            var project = _dte2.SelectedItems.Item(1).ProjectItem.ContainingProject;

            if (!project.TryBuild())
            {
                _dte2.StatusBar.Text = "Build failed. Unable to discover a DbContext class.";

                return null;
            }

            DynamicTypeService typeService;
            IVsSolution solution;
            using (var serviceProvider = new ServiceProvider((Microsoft.VisualStudio.OLE.Interop.IServiceProvider)_dte2.DTE))
            {
                typeService = (DynamicTypeService)serviceProvider.GetService(typeof(DynamicTypeService));
                solution = (IVsSolution)serviceProvider.GetService(typeof(SVsSolution));
            }

            IVsHierarchy vsHierarchy;
            var hr = solution.GetProjectOfUniqueName(_dte2.SelectedItems.Item(1).ProjectItem.ContainingProject.UniqueName, out vsHierarchy);

            if (hr != ProjectExtensions.S_OK)
            {
                throw Marshal.GetExceptionForHR(hr);
            }

            var resolver = typeService.GetTypeResolutionService(vsHierarchy);

            var codeElements = FindClassesInCodeModel(_dte2.SelectedItems.Item(1).ProjectItem.FileCodeModel.CodeElements);

            if (codeElements.Any())
            {
                foreach (var codeElement in codeElements)
                {
                    var userContextType = resolver.GetType(codeElement.FullName);

                    if (userContextType != null && IsContextType(userContextType))
                    {
                        return userContextType;
                    }
                }
            }

            _dte2.StatusBar.Text = "A type deriving from DbContext could not be found in the selected project.";

            return null;
        }

        private static IEnumerable<CodeElement> FindClassesInCodeModel(CodeElements codeElements)
        {
            foreach (CodeElement codeElement in codeElements)
            {
                if (codeElement.Kind == vsCMElement.vsCMElementClass)
                {
                    yield return codeElement;
                }

                foreach (var element in FindClassesInCodeModel(codeElement.Children))
                {
                    yield return element;
                }
            }
        }

        private static bool IsContextType(Type userContextType)
        {
            var systemContextType = GetBaseTypes(userContextType).FirstOrDefault(
                t => t.FullName == "Microsoft.EntityFrameworkCore.DbContext" 
                && t.Assembly.GetName().Name == "Microsoft.EntityFrameworkCore");

            return systemContextType != null;
        }

        private static IEnumerable<Type> GetBaseTypes(Type type)
        {
            while (type != typeof(object))
            {
                yield return type.BaseType;

                type = type.BaseType;
            }
        }

        private void OnItemMenuBeforeQueryStatus(object sender, EventArgs e)
        {
            OnItemMenuBeforeQueryStatus(
                sender,
                new[] { ".cs" });
        }

        private void OnItemMenuBeforeQueryStatus(object sender, IEnumerable<string> supportedExtensions)
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

            var extensionValue = GetSelectedItemExtension();
            menuCommand.Visible = supportedExtensions.Contains(extensionValue);
        }

        private string GetSelectedItemExtension()
        {
            var selectedItem = _dte2.SelectedItems.Item(1);

            if ((selectedItem.ProjectItem == null)
                || (selectedItem.ProjectItem.Properties == null))
            {
                return null;
            }

            var extension = selectedItem.ProjectItem.Properties.Item("Extension");

            if (extension == null)
            {
                return null;
            }

            return (string)extension.Value;
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
