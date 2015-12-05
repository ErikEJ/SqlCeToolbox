namespace SqlCeScripter
{
    partial class PanelQuickSearch
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtToFind = new System.Windows.Forms.TextBox();
            this.lblCol = new System.Windows.Forms.Label();
            this.lblQuickFind = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblOn = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtToFind
            // 
            this.txtToFind.Location = new System.Drawing.Point(87, 2);
            this.txtToFind.Name = "txtToFind";
            this.txtToFind.Size = new System.Drawing.Size(141, 20);
            this.txtToFind.TabIndex = 0;
            this.txtToFind.TextChanged += new System.EventHandler(this.txtToFind_TextChanged);
            this.txtToFind.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtToFind_KeyDown);
            // 
            // lblCol
            // 
            this.lblCol.AutoSize = true;
            this.lblCol.Location = new System.Drawing.Point(253, 6);
            this.lblCol.Name = "lblCol";
            this.lblCol.Size = new System.Drawing.Size(42, 13);
            this.lblCol.TabIndex = 2;
            this.lblCol.Text = "Column";
            // 
            // lblQuickFind
            // 
            this.lblQuickFind.AutoSize = true;
            this.lblQuickFind.Location = new System.Drawing.Point(23, 6);
            this.lblQuickFind.Name = "lblQuickFind";
            this.lblQuickFind.Size = new System.Drawing.Size(58, 13);
            this.lblQuickFind.TabIndex = 3;
            this.lblQuickFind.Text = "Quick Find";
            this.lblQuickFind.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnClose
            // 
            this.btnClose.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Image = global::SqlCeScripter.Properties.Resources.Critical;
            this.btnClose.Location = new System.Drawing.Point(0, 0);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(17, 25);
            this.btnClose.TabIndex = 4;
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // lblOn
            // 
            this.lblOn.AutoSize = true;
            this.lblOn.Location = new System.Drawing.Point(234, 6);
            this.lblOn.Name = "lblOn";
            this.lblOn.Size = new System.Drawing.Size(19, 13);
            this.lblOn.TabIndex = 5;
            this.lblOn.Text = "on";
            // 
            // PanelQuickSearch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.lblCol);
            this.Controls.Add(this.lblOn);
            this.Controls.Add(this.txtToFind);
            this.Controls.Add(this.lblQuickFind);
            this.Controls.Add(this.btnClose);
            this.Name = "PanelQuickSearch";
            this.Size = new System.Drawing.Size(402, 25);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        

        private System.Windows.Forms.TextBox txtToFind;
        private System.Windows.Forms.Label lblCol;
        private System.Windows.Forms.Label lblQuickFind;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblOn;
    }
}
