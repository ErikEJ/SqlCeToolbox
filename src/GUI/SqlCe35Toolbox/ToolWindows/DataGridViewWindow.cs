using System.Runtime.InteropServices;
using System.Windows;
using ErikEJ.SqlCeToolbox.Helpers;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace ErikEJ.SqlCeToolbox.ToolWindows
{
    [Guid("BF84EA66-D821-4DBA-B1B1-2777D8574775")]
    public class DataGridViewWindow : ToolWindowPane, IVsWindowFrameNotify3
    {
        private FrameworkElement control;

        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        public DataGridViewWindow()
            : base(null)
        {
            this.Caption = "Data Editor";
            this.BitmapResourceID = 301;
            this.BitmapIndex = 1;
            Telemetry.TrackPageView(nameof(DataGridViewWindow));
            control = new DataEditControl(this);
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

        public int OnClose(ref uint pgrfSaveOptions)
        {
            var editControl = control as DataEditControl;
            if (editControl != null)
            {
                if (editControl.ResultsetGrid != null)
                {
                    editControl.ResultsetGrid.Dispose();
                }
            }
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int OnDockableChange(int fDockable, int x, int y, int w, int h)
        {
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int OnMove(int x, int y, int w, int h)
        {
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int OnShow(int fShow)
        {
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int OnSize(int x, int y, int w, int h)
        {
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }        
    }
}