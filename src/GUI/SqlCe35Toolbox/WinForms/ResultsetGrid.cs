using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ErikEJ.SqlCeToolbox.Helpers;
using ErikEJ.SqlCeToolbox.ToolWindows;

namespace ErikEJ.SqlCeToolbox.WinForms
{
    public partial class ResultsetGrid : UserControl 
    {
        private DataGridViewSearch _dgs;
        private readonly ContextMenuStrip _imageContext = new ContextMenuStrip();
        private DataGridViewCell _selectedCell;
        private DbDataAdapter _adapter;
        private DataTable _table;
        private SqlPanel _pnlSql;
        private string _sqlText;

        public ResultsetGrid()
        {
            InitializeComponent();
        }

        public string TableName { get; set; }
        public DatabaseInfo DatabaseInfo { get; set; }
        public bool ReadOnly { get; set; }
        public List<int> ReadOnlyColumns { get; set; }
        public string SqlText 
        {
            private get
            {
                return _sqlText;
            }

            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _sqlText = value.Replace(Environment.NewLine + "      ,[", " ,[");
                }
            } 
        }
        // delegate declaration 
        public delegate void LinkClickedHandler(object sender, LinkArgs e);
        // event declaration 
        //public event LinkClickedHandler LinkClick;

        private void ResultsetGrid_Load(object sender, EventArgs e)
        {
            try
            {
                dataGridView1.AutoGenerateColumns = true;
                dataGridView1.DataError += dataGridView1_DataError;
                dataGridView1.ShowRowErrors = true;
                _imageContext.Items.Add("Import Image", null, ImportImage);
                _imageContext.Items.Add("Export Image", null, ExportImage);
                _imageContext.Items.Add("Delete Image", null, DeleteImage);

                LoadData(_sqlText);
                
                bindingNavigatorAddNewItem.Enabled = !ReadOnly;
                bindingNavigatorDeleteItem.Enabled = !ReadOnly;
                toolStripButton1.Enabled = !ReadOnly;

                dataGridView1.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
                dataGridView1.AllowUserToOrderColumns = true;
                dataGridView1.MultiSelect = false;
                //if (Properties.Settings.Default.ShowNullValuesAsNULL)
                //{
                //    dataGridView1.DefaultCellStyle.NullValue = "NULL";
                //}
                dataGridView1.KeyDown += dataGridView1_KeyDown;
                //dataGridView1.CellContentClick += new DataGridViewCellEventHandler(dataGridView1_CellContentClick);
                _dgs = new DataGridViewSearch(dataGridView1);
                if (ReadOnly)
                {
                    dataGridView1.DefaultCellStyle.ForeColor = SystemColors.GrayText;
                }
            }
            catch (Exception ex)
            {
                AddSqlPanel();
                _pnlSql.SqlText = ex.ToString();
            }
        }

        private void LoadData(string sqlText)
        {
            try
            {
                if (string.IsNullOrEmpty(sqlText))
                    return;

                _adapter = BuildDbDataAdapter(sqlText);
                _table = new DataTable();
                _adapter.Fill(_table);
                bindingSource1.DataSource = _table;

                dataGridView1.DataSource = null;
                dataGridView1.DataSource = bindingSource1;

                dataGridView1.ReadOnly = ReadOnly;
                if (ReadOnlyColumns != null)
                {
                    foreach (int x in ReadOnlyColumns)
                    {
                        dataGridView1.Columns[x].ReadOnly = true;
                        dataGridView1.Columns[x].DefaultCellStyle.ForeColor = SystemColors.GrayText;
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
                    dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
                }

                if (Properties.Settings.Default.MaxColumnWidth > 0)
                {
                    dataGridView1.AutoResizeColumns();
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
                    dataGridView1.AutoResizeColumns();
                }
            }
            catch (Exception ex)
            {
                AddSqlPanel();
                _pnlSql.SqlText = ex.ToString();
            }
        }


        private DbDataAdapter BuildDbDataAdapter(string sqlText)
        {
            if (DatabaseInfo.DatabaseType == DatabaseType.SQLite)
            {
                var conn = new SQLiteConnection();
                conn.ConnectionString = DatabaseInfo.ConnectionString;

                SQLiteCommand command = conn.CreateCommand();
                command.CommandText = sqlText;
                command.Connection = conn;
                
                var sqliteadapter = new SQLiteDataAdapter();
                sqliteadapter.SelectCommand = command;
                // ReSharper disable once UnusedVariable
                var cb = new SQLiteCommandBuilder(sqliteadapter);
                return sqliteadapter;
            }
            else
            {
                string invariantName = Resources.SqlCompact35InvariantName;
                if (DatabaseInfo.DatabaseType == DatabaseType.SQLCE40)
                    invariantName = Resources.SqlCompact40InvariantName;

                var factory = DbProviderFactories.GetFactory(invariantName);

                var conn = factory.CreateConnection();
                if (conn != null)
                {
                    conn.ConnectionString = DatabaseInfo.ConnectionString;
                
                    DbCommand command = factory.CreateCommand();
                    if (command != null)
                    {
                        command.CommandText = sqlText;
                        command.Connection = conn;

                        var adapter = factory.CreateDataAdapter();
                        if (adapter != null)
                        {
                            adapter.SelectCommand = command;
                            var cb = factory.CreateCommandBuilder();
                            if (cb != null) cb.DataAdapter = adapter;
                            return adapter;
                        }
                    }
                }
            }
            return null;
        }
        
        //void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        //{
        //    if (dataGridView1.Columns[e.ColumnIndex] is DataGridViewLinkColumn)
        //    {
        //        string id = dataGridView1[e.ColumnIndex, e.RowIndex].Value.ToString();
        //        string table = dataGridView1.Columns[e.ColumnIndex].HeaderText;
        //        string column = dataGridView1.Columns[e.ColumnIndex].DataPropertyName;
        //        if (LinkClick != null)
        //            LinkClick(this, new LinkArgs(id, table, column));
        //    }
        //}

        void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F3)
            {
                _dgs.ShowSearch();
            }
        }

        private void dataGridView1_MouseUp(object sender, MouseEventArgs e)
        {
            _selectedCell = null;
            // Load context menu on right mouse click
            if (e.Button == MouseButtons.Right)
            {
                DataGridView.HitTestInfo hitTestInfo;
                hitTestInfo = dataGridView1.HitTest(e.X, e.Y);
                if (hitTestInfo.Type == DataGridViewHitTestType.Cell)
                {
                    DataGridViewCell cell = dataGridView1[hitTestInfo.ColumnIndex, hitTestInfo.RowIndex];
                    if (cell.FormattedValueType == typeof(Image))
                    {
                        _selectedCell = cell;
                        bindingSource1.Position = _selectedCell.RowIndex;
                        _imageContext.Show(dataGridView1, new Point(e.X, e.Y));
                    }
                }
            }
        }

        void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            dataGridView1.Rows[e.RowIndex].ErrorText = e.Exception.Message;
        }

        void ImportImage(object sender, EventArgs e)
        {
            if (_selectedCell != null)
            {
                using (OpenFileDialog fd = new OpenFileDialog())
                {
                    fd.Multiselect = false;
                    if (fd.ShowDialog() == DialogResult.OK)
                    {
                        _selectedCell.Value = File.ReadAllBytes(fd.FileName);
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
            if (_selectedCell != null && _selectedCell.Value != null)
            {
                using (SaveFileDialog fd = new SaveFileDialog())
                {
                    if (fd.ShowDialog() == DialogResult.OK)
                    {
                        File.WriteAllBytes(fd.FileName, (byte[])_selectedCell.Value);
                    }
                }
            }
        }

        void DeleteImage(object sender, EventArgs e)
        {
            if (_selectedCell != null && _selectedCell.Value != null)
            {
                _selectedCell.Value = null;
                if (bindingSource1.Position + 1 < bindingSource1.Count)
                    bindingSource1.MoveNext();
                else
                    bindingSource1.MovePrevious();
            }
        }

        // From http://www.codeproject.com/KB/database/DataGridView2Db.aspx

        //tracks for PositionChanged event last row
        private DataRow _lastDataRow;

        /// <SUMMARY>
        /// Checks if there is a row with changes and
        /// writes it to the database
        /// </SUMMARY>
        private void UpdateRowToDatabase()
        {
            try
            {
                if (_lastDataRow != null)
                {
                    if (_lastDataRow.RowState == DataRowState.Modified 
                     || _lastDataRow.RowState == DataRowState.Added
                     || _lastDataRow.RowState == DataRowState.Deleted   )
                    {
                        DataRow[] rows = new DataRow[1];
                        rows[0] = _lastDataRow;
                        _adapter.Update(rows);
                    }
                }
            }
            catch (Exception ex)
            {
                EnvDteHelper.ShowError(ex.ToString());
            }
        }

        private void bindingSource1_PositionChanged(object sender, EventArgs e)
        {
            // if the user moves to a new row, check if the 
            // last row was changed
            var thisBindingSource = sender as BindingSource;

            var current = thisBindingSource?.Current;
            if (current == null) return;
            
            var thisDataRow = ((DataRowView)current).Row;
            if (thisDataRow == _lastDataRow)
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
            _lastDataRow = thisDataRow;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            SaveTable();
        }

        private void SaveTable()
        {
            try
            {
                _adapter.Update(_table);
            }
            catch (Exception ex)
            {
                EnvDteHelper.ShowError(ex.ToString());
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            try
            {
                LoadData(_sqlText);
            }
            catch (Exception ex)
            {
                EnvDteHelper.ShowError(ex.ToString());
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            _dgs.ShowSearch();
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            AddSqlPanel();
        }

        private void AddSqlPanel()
        {
            if (_pnlSql == null)
            {
                _pnlSql = new SqlPanel();
                Controls.Add(_pnlSql);
                _pnlSql.SqlText = _sqlText;
                _pnlSql.SqlChanged += pnlSql_SqlChanged;
            }
            _pnlSql.Show();
            _pnlSql.Focus();
        }

        void pnlSql_SqlChanged(string sqlText)
        {
            _sqlText = sqlText;
            LoadData(sqlText);
        }
    }
}
