using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlServerCe;
using System.IO;

namespace SqlCeScripter
{
    public partial class ResultsetGrid : UserControl, IDisposable 
    {
        private DataGridViewSearch dgs;
        private ContextMenuStrip imageContext = new ContextMenuStrip();
        private DataGridViewCell selectedCell;
        private SqlCeDataAdapter dAdapter;
        private SqlCeCommandBuilder cb;
        private DataTable dTable;
        private string downloadUri = "http://exportsqlce.codeplex.com";
        private string tableName;
        private bool isView;

        public ResultsetGrid()
        {
            InitializeComponent();
        }

        private void ResultsetGrid_Load(object sender, EventArgs e)
        {
            this.isView = Connect.ViewsSelected;
            this.tableName = Connect.CurrentTable;
            this.btnNewVersion.ForeColor = SystemColors.ControlText;
            this.btnNewVersion.Text = "Check for updates";

            try
            {
                this.dataGridView1.AutoGenerateColumns = true;
                this.dataGridView1.DataError += new DataGridViewDataErrorEventHandler(dataGridView1_DataError);
                if (this.isView)
                {
                    this.toolStripButton1.Enabled = false;
                }
                else
                {
                    imageContext.Items.Add("Import Image", null, new EventHandler(ImportImage));
                    imageContext.Items.Add("Export Image", null, new EventHandler(ExportImage));
                    imageContext.Items.Add("Delete Image", null, new EventHandler(DeleteImage));                
                }
                LoadData();

                this.dataGridView1.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
                this.dataGridView1.AllowUserToOrderColumns = true;
                
                this.dataGridView1.KeyDown += new KeyEventHandler(dataGridView1_KeyDown);
                dgs = new DataGridViewSearch(this.dataGridView1);
                
            }
            catch (System.Data.SqlServerCe.SqlCeException sqlCe)
            {
                Connect.ShowErrors(sqlCe);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        private void LoadData()
        {
            if (this.isView)
            {
                using (SqlCeCommand cmd = new SqlCeCommand())
                {
                    using (SqlCeConnection _conn = new SqlCeConnection(Connect.ConnectionString))
                    {
                        cmd.Connection = _conn;
                        _conn.Open();
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = string.Format(System.Globalization.CultureInfo.InvariantCulture, "SELECT * FROM {0}", this.tableName);
                        // Must use dataset to disable EnforceConstraints
                        DataSet dataSet = new DataSet();
                        dataSet.EnforceConstraints = false;
                        string[] tables = new string[1];
                        tables[0] = "table1";
                        SqlCeDataReader rdr = cmd.ExecuteReader();
                        dataSet.Load(rdr, LoadOption.OverwriteChanges, tables);
                        dataSet.Tables[0].DefaultView.AllowDelete = false;
                        dataSet.Tables[0].DefaultView.AllowEdit = false;
                        dataSet.Tables[0].DefaultView.AllowNew = false;
                        this.bindingSource1.DataSource = dataSet.Tables[0];
                        this.dataGridView1.ReadOnly = true;
                    }
                }
            }
            else
            {
                dAdapter = new SqlCeDataAdapter(string.Format(System.Globalization.CultureInfo.InvariantCulture, "SELECT * FROM [{0}]", this.tableName), Connect.ConnectionString);
                cb = new SqlCeCommandBuilder();
                cb.DataAdapter = dAdapter;
                dTable = new DataTable();
                dAdapter.Fill(dTable);
                this.bindingSource1.DataSource = dTable;

            }
            this.dataGridView1.AutoResizeColumns();
        }

        void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F3)
            {
                this.dgs.ShowSearch();
            }
        }

        private void dataGridView1_MouseUp(object sender, MouseEventArgs e)
        {
            selectedCell = null;
            // Load context menu on right mouse click
            if (e.Button == MouseButtons.Right)
            {
                DataGridView.HitTestInfo hitTestInfo;
                hitTestInfo = dataGridView1.HitTest(e.X, e.Y);
                if (hitTestInfo.Type == DataGridViewHitTestType.Cell)
                {
                    DataGridViewCell cell = dataGridView1[hitTestInfo.ColumnIndex, hitTestInfo.RowIndex];
                    if (cell.FormattedValueType == typeof(System.Drawing.Image))
                    {
                        selectedCell = cell;
                        imageContext.Show(dataGridView1, new Point(e.X, e.Y));
                    }
                }
            }
        }

        void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            // MessageBox.Show(string.Format(System.Globalization.CultureInfo.InvariantCulture, "DataGridView error: {0}, row: {1}, column: {2}", e.Exception.Message, e.RowIndex + 1, e.ColumnIndex + 1)); 
        }


        void ImportImage(object sender, EventArgs e)
        {
            if (selectedCell != null)
            {
                using (OpenFileDialog fd = new OpenFileDialog())
                {
                    fd.Multiselect = false;
                    if (fd.ShowDialog() == DialogResult.OK)
                    {
                        selectedCell.Value = File.ReadAllBytes(fd.FileName);
                    }
                }
            }
        }

        void ExportImage(object sender, EventArgs e)
        {
            if (selectedCell != null && selectedCell.Value != null)
            {
                using (SaveFileDialog fd = new SaveFileDialog())
                {
                    if (fd.ShowDialog() == DialogResult.OK)
                    {
                        File.WriteAllBytes(fd.FileName, (byte[])selectedCell.Value);
                    }
                }
            }
        }

        void DeleteImage(object sender, EventArgs e)
        {
            if (selectedCell != null)
            {
                selectedCell.Value = null;
            }
        }

        // From http://www.codeproject.com/KB/database/DataGridView2Db.aspx

        //tracks for PositionChanged event last row
        private DataRow LastDataRow;

        /// <SUMMARY>
        /// Checks if there is a row with changes and
        /// writes it to the database
        /// </SUMMARY>
        private void UpdateRowToDatabase()
        {
            try
            {
                if (LastDataRow != null)
                {
                    if (LastDataRow.RowState ==
                        DataRowState.Modified)
                    {
                        DataRow[] rows = new DataRow[1];
                        rows[0] = LastDataRow;
                        dAdapter.Update(rows);
                    }
                }
            }
            catch (System.Data.SqlServerCe.SqlCeException sqlCe)
            {
                Connect.ShowErrors(sqlCe);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        private void bindingSource1_PositionChanged(object sender, EventArgs e)
        {
            // if the user moves to a new row, check if the 
            // last row was changed
            BindingSource thisBindingSource =
              (BindingSource)sender;
            DataRow ThisDataRow =
              ((DataRowView)thisBindingSource.Current).Row;
            if (ThisDataRow == LastDataRow)
            {
                // we need to avoid to write a datarow to the 
                // database when it is still processed. Otherwise
                // we get a problem with the event handling of 
                //the DataTable.
                throw new InvalidOperationException("It seems the" +
                  " PositionChanged event was fired twice for" +
                  " the same row");
            }

            UpdateRowToDatabase();
            // track the current row for next 
            // PositionChanged event
            LastDataRow = ThisDataRow;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            try
            {
                dAdapter.Update(dTable);
            }
            catch (System.Data.SqlServerCe.SqlCeException sqlCe)
            {
                Connect.ShowErrors(sqlCe);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            try
            {
                LoadData();
            }
            catch (System.Data.SqlServerCe.SqlCeException sqlCe)
            {
                Connect.ShowErrors(sqlCe);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            using (AboutDlg about = new AboutDlg())
            {
                about.ShowDialog();
            }
        }

        private void btnNewVersion_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.downloadUri))
            {
                System.Diagnostics.Process.Start(this.downloadUri);
            }
        }
       
    }
}
