using System.Runtime.InteropServices;
using System.Windows;
using ErikEJ.SqlCeToolbox.Helpers;
using Microsoft.VisualStudio.Shell;

namespace ErikEJ.SqlCeToolbox.ToolWindows
{
    [Guid("683B7FA4-6A84-40C1-A43B-1803DC159F14")]
    public class ReportWindow : ToolWindowPane
    {
        private FrameworkElement control;

        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        public ReportWindow()
            : base(null)
        {
            this.Caption = "Report";
            this.BitmapResourceID = 301;
            this.BitmapIndex = 1;
            Telemetry.TrackPageView(nameof(ReportWindow));
            control = new ReportControl(this);
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