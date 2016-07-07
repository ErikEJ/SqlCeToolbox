using System;
using System.Windows.Forms;

namespace ErikEJ.SqlCeToolbox.ToolWindows
{
    public partial class SqlPanel : UserControl
    {
        public SqlPanel()
        {
            InitializeComponent();
            Dock = DockStyle.Bottom;
        }

        public string SqlText 
        {
            get { return txtSql.Text; }
            set { txtSql.Text = value; }
        }

        public delegate void SqlHandler(string search);
        public event SqlHandler SqlChanged;

        public void OnSqlChanged(string sql)
        {
            if (SqlChanged != null && !string.IsNullOrEmpty(sql))
                SqlChanged(sql);
        }

        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);
            Hide();

        }
        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            txtSql.Focus();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OnSqlChanged(txtSql.Text);
            Hide();
        }

    }
}
