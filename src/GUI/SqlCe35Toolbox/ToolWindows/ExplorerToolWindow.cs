using System;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.VisualStudio.Shell;

namespace ErikEJ.SqlCeToolbox.ToolWindows
{

    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    ///
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane, 
    /// usually implemented by the package implementer.
    ///
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its 
    /// implementation of the IVsUIElementPane interface.
    /// </summary>
    [Guid("c5bb427c-36fe-45e9-ac41-f1895991c277")]
    public class ExplorerToolWindow : ToolWindowPane
    {
        // This is the user control hosted by the tool window; it is exposed to the base class 
        // using the Content property. Note that, even if this class implements IDispose, we are
        // not calling Dispose on this object. This is because ToolWindowPane calls Dispose on 
        // the object returned by the Content property.
        private FrameworkElement control;

        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        public ExplorerToolWindow() :
            base(null)
        {
            // Set the window title reading it from the resources.
            Caption = Resources.ToolWindowTitle;
            // Set the image that will appear on the tab of the window frame
            // when docked with an other window
            // The resource ID correspond to the one defined in the resx file
            // while the Index is the offset in the bitmap strip. Each image in
            // the strip being 16x16.
            BitmapResourceID = 301;
            BitmapIndex = 1;

            control = new ExplorerControl(this);
        }

        /// <summary>
        /// This property returns the control that should be hosted in the Tool Window.
        /// It can be either a FrameworkElement (for easy creation of toolwindows hosting WPF content), 
        /// or it can be an object implementing one of the IVsUIWPFElement or IVsUIWin32Element interfaces.
        /// </summary>
        override public object Content 
        {
            get
            {
                return this.control;
            }
        }

        public object GetServiceHelper(Type serviceType)
        {
            return base.GetService(serviceType);
        }

    }
}
