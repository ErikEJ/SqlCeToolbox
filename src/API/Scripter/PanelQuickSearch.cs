using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace SqlCeScripter
{
    // Thanks to http://www.codeproject.com/KB/grid/ExtendedDataGridView.aspx

    partial class PanelQuickSearch : UserControl
    {
        public PanelQuickSearch()
        {
            InitializeComponent();
            Dock = DockStyle.Bottom;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            lblQuickFind.Location = new Point(btnClose    .Right, GetY(lblQuickFind));
            txtToFind   .Location = new Point(lblQuickFind.Right, GetY(txtToFind));
            lblOn       .Location = new Point(txtToFind   .Right, GetY(lblOn));
            lblCol      .Location = new Point(lblOn       .Right, GetY(lblCol));
        }

        int GetY(Control control)
        {
            return (Height - control.Height) / 2;
        }

        private void txtToFind_TextChanged(object sender, EventArgs e)
        {
            OnSearchChanged(txtToFind.Text);
        }

        public delegate void SearchHandler(string search);
        public event SearchHandler SearchChanged;

        public void OnSearchChanged(string search)
        {
            if (SearchChanged != null && !string.IsNullOrEmpty(search))
                SearchChanged(search);
        }
        
        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);
            Hide();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            txtToFind.Focus();
        }

        private void txtToFind_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Escape)
                Hide();
        }

        public string Search
        {
            get { return txtToFind.Text; }
            set { txtToFind.Text = value; }
        }

        public string Column
        {
            get { return lblCol.Text; }
            set { lblCol.Text = value; }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Hide();
        }

    }
}
