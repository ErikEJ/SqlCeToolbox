using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.VisualStudio.Shell;

namespace ErikEJ.SqlCe35Toolbox.ToolWindows
{
    [Guid("FABF9319-47EB-497E-B8F6-D9F73FBA5F55")]
    public class SqlEditorWindow : ToolWindowPane
    {
        private FrameworkElement control;

        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        public SqlEditorWindow() : base(null)
        {
            this.Caption = "SQL Editor";
            this.BitmapResourceID = 301;
            this.BitmapIndex = 1;

            control = new SqlEditorControl(this);
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
    }
}