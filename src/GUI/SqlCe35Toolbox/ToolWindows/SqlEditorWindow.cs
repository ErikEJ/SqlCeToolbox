using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Windows.Forms;
using ErikEJ.SqlCeToolbox.Helpers;
using System;

namespace ErikEJ.SqlCeToolbox.ToolWindows
{
    [Guid("FABF9319-47EB-497E-B8F6-D9F73FBA5F55")]
    public class SqlEditorWindow : ToolWindowPane, IVsWindowFrameNotify3
    {
        private FrameworkElement control;
        private bool mustClose;
        private SqlEditorControl editor;

        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        public SqlEditorWindow() : base(null)
        {
            this.Caption = "SQL Editor";
            this.BitmapResourceID = 301;
            this.BitmapIndex = 1;
            Telemetry.TrackPageView(nameof(SqlEditorWindow));
            editor = new SqlEditorControl(this);
            control = editor;
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

        private bool isControlKeyDepressed = false;
        private bool isOtherKeyDepressed = false;
        private bool isCommandCombinationDepressed = false;

        protected override bool PreProcessMessage(ref Message msg)
        {
            if (Properties.Settings.Default.DisableEditorKeyboardShortcuts)
                return base.PreProcessMessage(ref msg);

            if (msg.Msg == 256)
            {
                if (msg.WParam == (IntPtr)17)
                {
                    isControlKeyDepressed = true;
                    isOtherKeyDepressed = false;
                }
              else
                {
                    if (isOtherKeyDepressed == true)
                    {
                        isControlKeyDepressed = false;
                    }
                    isOtherKeyDepressed = true;
                    if (isControlKeyDepressed == true)
                    {
                        switch (msg.WParam.ToInt64())
                        {
                            case 69: // Ctrl+E command
                                editor.ExecuteScript();
                                isCommandCombinationDepressed = true;
                                break;
                            case 78: // Ctrl+N command
                                editor.OpenSqlEditorToolWindow();
                                isCommandCombinationDepressed = true;
                                break;
                            case 79: // Ctrl+O command
                                editor.OpenScript();
                                isCommandCombinationDepressed = true;
                                break;
                            case 83: // Ctrl+S command
                                editor.SaveScript(false);
                                isCommandCombinationDepressed = true;
                                break;
                            default:
                                isCommandCombinationDepressed = false;
                                break;
                        }
                    }
                    else
                    {
                        isCommandCombinationDepressed = false;
                    }
                }

                if (isCommandCombinationDepressed == true)
                {
                    msg.Result = (IntPtr)1;
                    return true;
                }
            }
            return base.PreProcessMessage(ref msg);
        }

        #region IVsWindowFrameNotify3 Members

        public int OnClose(ref uint pgrfSaveOptions)
        {
            if (!Properties.Settings.Default.PromptToSaveChangedScript)
            {
                return Microsoft.VisualStudio.VSConstants.S_OK;
            }
            // Check if your content is dirty here, then
            var window = this.control as SqlEditorControl;
            if (window != null && window.IsDirty && !mustClose)
            {
                // Prompt a dialog
                DialogResult res = EnvDTEHelper.ShowMessageBox("This script has been modified. Do you want to save the changes ?", OLEMSGBUTTON.OLEMSGBUTTON_YESNOCANCEL, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST, OLEMSGICON.OLEMSGICON_QUERY);
                // If the users wants to save
                if (res == DialogResult.Yes)
                {
                    window.SaveScript(true);
                }
                if (res == DialogResult.Cancel)
                {
                    // If "cancel" is clicked, abort the close
                    return Microsoft.VisualStudio.VSConstants.E_ABORT;
                }
                if (res == DialogResult.No)
                {
                    mustClose = true;
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

        #endregion
    }
}