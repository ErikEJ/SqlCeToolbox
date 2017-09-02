using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using EFCorePowerTools.Handlers;
using EnvDTE;
using EnvDTE80;
using ErikEJ.SqlCeToolbox.Helpers;
using Microsoft.VisualStudio.Shell;

namespace EFCorePowerTools
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [SqlCe40ProviderRegistration]
    [SqliteProviderRegistration]
    [InstalledProductRegistration("#110", "#112", "0.1", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(EFCorePowerToolsPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class EFCorePowerToolsPackage : Package
    {
        public const string PackageGuidString = "864565dd-e43c-450b-bc33-95dd357140ee";

        private readonly ReverseEngineerCodeFirstHandler _reverseEngineerCodeFirstHandler;
        private DTE2 _dte2;

        public EFCorePowerToolsPackage()
        {
            _reverseEngineerCodeFirstHandler = new ReverseEngineerCodeFirstHandler(this);
        }

        internal DTE2 DTE2 => _dte2;

        protected override void Initialize()
        {
            base.Initialize();

            _dte2 = GetService(typeof(DTE)) as DTE2;

            if (_dte2 == null)
            {
                return;
            }

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
