using System.Runtime.InteropServices;
using System.Windows;
using ErikEJ.SqlCeToolbox.Helpers;
using Microsoft.VisualStudio.Shell;

namespace ErikEJ.SqlCeToolbox.ToolWindows
{
    [Guid("FABF9319-47EB-497E-B8F6-D9F73FBA5FEE")]
    public class SubscriptionWindow : ToolWindowPane
    {
        private readonly FrameworkElement _control;

        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        public SubscriptionWindow() : base(null)
        {
            Caption = "Manage Merge Replication Subscription";
            BitmapResourceID = 301;
            BitmapIndex = 1;
            Telemetry.TrackPageView(nameof(SubscriptionWindow));
            _control = new SubscriptionControl(this);
        }

        /// <summary>
        /// This property returns the _control that should be hosted in the Tool Window.
        /// It can be either a FrameworkElement (for easy creation of toolwindows hosting WPF content), 
        /// or it can be an object implementing one of the IVsUIWPFElement or IVsUIWin32Element interfaces.
        /// </summary>
        public override object Content 
        {
            get
            {
                return _control;
            }
        }
    }
}