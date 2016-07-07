//------------------------------------------------------------------------------
// <copyright file="ExplorerToolWindow.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace ErikEJ.SqlCeToolbox.ToolWindows
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
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
    }
}
