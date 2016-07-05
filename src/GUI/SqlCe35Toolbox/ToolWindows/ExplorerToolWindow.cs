using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace ErikEJ.SqlCeToolbox.ToolWindows
{
    [Guid("f37f5141-198e-4181-9582-7b026ee8f915")]
    public class ExplorerToolWindow : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExplorerToolWindow"/> class.
        /// </summary>
        public ExplorerToolWindow() : base(null)
        {
            this.Caption = "ExplorerToolWindow";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new ExplorerControl();
        }

        public object GetServiceHelper(Type serviceType)
        {
            return base.GetService(serviceType);
        }
    }

}
