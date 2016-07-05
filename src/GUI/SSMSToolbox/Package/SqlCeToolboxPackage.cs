using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

// ReSharper disable once CheckNamespace
namespace ErikEJ.SqlCeToolbox
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "0.1", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(ExplorerToolWindow), Style = VsDockStyle.Tabbed, Orientation = ToolWindowOrientation.Left, Window = "DocumentWell")]
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
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            OtherWindowsCommand.Initialize(this);
            ViewMenuCommand.Initialize(this);
            base.Initialize();            
        }
        #endregion
    }
}
