using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace SqlCeScripter
{
    // Thanks to http://www.codeproject.com/KB/grid/ExtendedDataGridView.aspx
    internal class DataGridViewSearch : IDisposable 
    {
        PanelQuickSearch m_pnlQuickSearch;

        DataGridView dgv;

        int sortedColumn;

        internal DataGridViewSearch(DataGridView dgv)
        {
            this.dgv = dgv;
        }

        internal void ShowSearch()
        {
            if (dgv.SortedColumn != null)            
            {
                sortedColumn = dgv.SortedColumn.Index;
                AddControl();

                if (dgv.SelectedRows.Count > 0)
                    m_pnlQuickSearch.Search = dgv.SelectedRows[0].Cells[dgv.SortedColumn.Index].Value.ToString();
                m_pnlQuickSearch.Column = dgv.SortedColumn.HeaderText;

                ShowControl();
            }
            else if (dgv.SelectedCells.Count > 0)
            {
                sortedColumn = dgv.SelectedCells[0].ColumnIndex;
                AddControl();
                m_pnlQuickSearch.Search = dgv.SelectedCells[0].Value.ToString();
                m_pnlQuickSearch.Column = dgv.Columns[sortedColumn].HeaderText;
                ShowControl();            
            }

        }

        private void ShowControl()
        {
            m_pnlQuickSearch.Show();
            m_pnlQuickSearch.Focus();
        }

        private void AddControl()
        {
            if (m_pnlQuickSearch == null)
            {
                m_pnlQuickSearch = new PanelQuickSearch();
                dgv.Controls.Add(m_pnlQuickSearch);
                m_pnlQuickSearch.SearchChanged += m_pnlQuickSearch_SearchChanged;
            }
        }

        void m_pnlQuickSearch_SearchChanged(string search)
        {
            try
            {
                foreach (DataGridViewRow row in dgv.SelectedRows)
                    row.Selected = false;

                if (dgv.SortedColumn != null)
                {
                    if (dgv.SortOrder == SortOrder.Ascending)
                        dgv.Rows[BinarySearchAsc(search)].Selected = true;
                    else
                        dgv.Rows[BinarySearchDesc(search)].Selected = true;
                }
                else
                {
                    dgv.Rows[SequentialSearch(search)].Selected = true;
                }
                dgv.FirstDisplayedScrollingRowIndex = dgv.SelectedRows[0].Index;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        private int SequentialSearch(string search)
        {

            int pos = 0;
            foreach (DataGridViewRow row in dgv.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.ColumnIndex == sortedColumn)
                    {
                        if (cell.Value != null)
                        {
                            if (cell.Value.ToString().StartsWith(search, StringComparison.OrdinalIgnoreCase))
                            {
                                return cell.RowIndex;
                            }
                        }
                    }
                }
            }
            return pos;
        }

        int BinarySearchAsc(string value)
        {
            int max     = dgv.Rows.Count - 1,
                min     = 0,
                current,
                compare;

            while (max >= min)
            {
                current = (max - min) / 2 + min;

                compare = dgv[sortedColumn, current].Value.ToString().CompareTo(value);

                if (compare > 0)
                    max = current - 1;
                else if (compare < 0)
                    min = current + 1;
                else
                    return current;
            }

            if (min >= dgv.Rows.Count)
                return dgv.Rows.Count - 1;

            return min;
        }

        int BinarySearchDesc(string value)
        {
            int max     = dgv.Rows.Count - 1,
                min     = 0,
                current,
                compare;

            while (max >= min)
            {
                current = (max - min) / 2 + min;

                compare = dgv[sortedColumn, current].Value.ToString().CompareTo(value);

                if (compare < 0)
                    max = current - 1;
                else if (compare > 0)
                    min = current + 1;
                else
                    return current;
            }

            if (max < 0)
                return 0;

            return max;
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (this.m_pnlQuickSearch != null)
            {
                m_pnlQuickSearch.Dispose();
            }
        }

        #endregion
    }
}
