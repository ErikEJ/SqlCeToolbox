using System;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Shell;

namespace ErikEJ.SqlCeToolbox.ToolWindows
{
    [Guid("f37f5141-198e-4181-9582-7b026ee8f915")]
    public class ExplorerToolWindow : ToolWindowPane
    {
        private readonly FrameworkElement _control;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExplorerToolWindow"/> class.
        /// </summary>
        public ExplorerToolWindow() : base(null)
        {
            Caption = Resources.App;

            //BitmapImageMoniker = Microsoft.VisualStudio.Imaging.KnownMonikers.ConnectToDatabase;

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            _control = new ExplorerControl(this);
        }

        public override object Content
        {
            get
            {
                return _control;
            }
        }

        public object GetServiceHelper(Type serviceType)
        {
            return GetService(serviceType);
        }
    }
}
