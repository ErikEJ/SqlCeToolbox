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

namespace ErikEJ.SqlCeToolbox.ToolWindows
{
    public partial class ResultsetGrid : UserControl, IDisposable 
    {
        private DataGridViewSearch dgs;
        private ContextMenuStrip imageContext = new ContextMenuStrip();
        private DataGridViewCell selectedCell;
        private SqlCeDataAdapter dAdapter;
        private SqlCeCommandBuilder cb;
        private DataTable dTable;
        private string tableName;

        public ResultsetGrid()
        {
            InitializeComponent();
        }

        public string TableName { get; set; }

        public string ConnectionString { get; set; }

        public bool ReadOnly { get; set; }

        public List<int> ReadOnlyColumns { get; set; }

        private void ResultsetGrid_Load(object sender, EventArgs e)
        {
            this.tableName = TableName;

            try
            {
                this.dataGridView1.AutoGenerateColumns = true;
                this.dataGridView1.DataError += new DataGridViewDataErrorEventHandler(dataGridView1_DataError);
                imageContext.Items.Add("Import Image", null, new EventHandler(ImportImage));
                imageContext.Items.Add("Export Image", null, new EventHandler(ExportImage));
                imageContext.Items.Add("Delete Image", null, new EventHandler(DeleteImage));                
                LoadData();
                
                this.dataGridView1.ReadOnly = ReadOnly;
                if (this.ReadOnlyColumns != null)
                {
                    foreach (int x in ReadOnlyColumns)
                    {
                        this.dataGridView1.Columns[x].ReadOnly = true;
                        this.dataGridView1.Columns[x].DefaultCellStyle.ForeColor = SystemColors.GrayText;
                    }
                }
                if (Properties.Settings.Default.MultiLineTextEntry)
                {
                    foreach (DataGridViewColumn col in dataGridView1.Columns)
                    {
                        if (col is DataGridViewTextBoxColumn)
                        {
                            col.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                        }
                    }
                }
                this.dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

                this.bindingNavigatorAddNewItem.Enabled = !ReadOnly;
                this.bindingNavigatorDeleteItem.Enabled = !ReadOnly;
                this.toolStripButton1.Enabled = !ReadOnly;

                this.dataGridView1.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
                this.dataGridView1.AllowUserToOrderColumns = true;
                this.dataGridView1.MultiSelect = false;
                
                this.dataGridView1.KeyDown += new KeyEventHandler(dataGridView1_KeyDown);
                dgs = new DataGridViewSearch(this.dataGridView1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Helpers.DataConnectionHelper.ShowErrors(ex));
            }
        }

        private void LoadData()
        {
            dAdapter = new SqlCeDataAdapter(this.tableName, this.ConnectionString);
            cb = new SqlCeCommandBuilder();
            cb.DataAdapter = dAdapter;
            dTable = new DataTable();
            dAdapter.Fill(dTable);
            this.bindingSource1.DataSource = dTable;
            if (Properties.Settings.Default.MaxColumnWidth > 0)
            {
                this.dataGridView1.AutoResizeColumns();
                for (int i = 0; i < dataGridView1.Columns.Count; i++)
                {
                    if (dataGridView1.Columns[i].Width > Properties.Settings.Default.MaxColumnWidth)
                    {
                        dataGridView1.Columns[i].Width = Properties.Settings.Default.MaxColumnWidth;
                    }
                }
            }
            else
            {
                this.dataGridView1.AutoResizeColumns();
            }
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
                        bindingSource1.Position = selectedCell.RowIndex;
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
                        if (bindingSource1.Position + 1 < bindingSource1.Count)
                            bindingSource1.MoveNext();
                        else
                            bindingSource1.MovePrevious();
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
            if (selectedCell != null && selectedCell.Value != null)
            {
                selectedCell.Value = null;
                if (bindingSource1.Position + 1 < bindingSource1.Count)
                    bindingSource1.MoveNext();
                else
                    bindingSource1.MovePrevious();
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
                    if (LastDataRow.RowState == DataRowState.Modified 
                     || LastDataRow.RowState == DataRowState.Added
                     || LastDataRow.RowState == DataRowState.Deleted   )
                    {
                        DataRow[] rows = new DataRow[1];
                        rows[0] = LastDataRow;
                        dAdapter.Update(rows);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Helpers.DataConnectionHelper.ShowErrors(ex));
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
            SaveTable();

        }

        private void SaveTable()
        {
            try
            {
                int rows = dAdapter.Update(dTable);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Helpers.DataConnectionHelper.ShowErrors(ex));
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            try
            {
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Helpers.DataConnectionHelper.ShowErrors(ex));
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            this.dgs.ShowSearch();
        }

       
    }
}
