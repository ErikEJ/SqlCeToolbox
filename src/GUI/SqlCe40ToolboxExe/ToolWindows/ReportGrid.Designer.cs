namespace ErikEJ.SqlCeToolbox.ToolWindows
{
    partial class ReportGrid
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.reportView = new Microsoft.Reporting.WinForms.ReportViewer();
            this.SuspendLayout();
            // 
            // reportView
            // 
            this.reportView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.reportView.Location = new System.Drawing.Point(0, 0);
            this.reportView.Name = "reportView";
            this.reportView.Size = new System.Drawing.Size(790, 470);
            this.reportView.TabIndex = 0;
            // 
            // ReportGrid
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.reportView);
            this.Name = "ReportGrid";
            this.Size = new System.Drawing.Size(790, 470);
            this.Load += new System.EventHandler(this.ReportGrid_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.Reporting.WinForms.ReportViewer reportView;
    }
}
