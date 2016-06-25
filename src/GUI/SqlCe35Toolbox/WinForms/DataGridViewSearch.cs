using System;
using System.Windows.Forms;

namespace ErikEJ.SqlCeToolbox.WinForms
{
    // Thanks to http://www.codeproject.com/KB/grid/ExtendedDataGridView.aspx
    internal class DataGridViewSearch : IDisposable 
    {
        PanelQuickSearch _mPnlQuickSearch;

        readonly DataGridView _dgv;

        int _sortedColumn;

        internal DataGridViewSearch(DataGridView dgv)
        {
            _dgv = dgv;
        }

        internal void ShowSearch()
        {
            if (_dgv.SortedColumn != null)            
            {
                _sortedColumn = _dgv.SortedColumn.Index;
                AddControl();

                if (_dgv.SelectedRows.Count > 0)
                    _mPnlQuickSearch.Search = _dgv.SelectedRows[0].Cells[_dgv.SortedColumn.Index].Value.ToString();
                _mPnlQuickSearch.Column = _dgv.SortedColumn.HeaderText;

                ShowControl();
            }
            else if (_dgv.SelectedCells.Count > 0)
            {
                _sortedColumn = _dgv.SelectedCells[0].ColumnIndex;
                AddControl();
                _mPnlQuickSearch.Search = _dgv.SelectedCells[0].Value.ToString();
                _mPnlQuickSearch.Column = _dgv.Columns[_sortedColumn].HeaderText;
                ShowControl();            
            }

        }

        private void ShowControl()
        {
            _mPnlQuickSearch.Show();
            _mPnlQuickSearch.Focus();
        }

        private void AddControl()
        {
            if (_mPnlQuickSearch == null)
            {
                _mPnlQuickSearch = new PanelQuickSearch();
                _dgv.Controls.Add(_mPnlQuickSearch);
                _mPnlQuickSearch.SearchChanged += m_pnlQuickSearch_SearchChanged;
            }
        }

        void m_pnlQuickSearch_SearchChanged(string search)
        {
            try
            {
                foreach (DataGridViewRow row in _dgv.SelectedRows)
                    row.Selected = false;

                if (_dgv.SortedColumn != null)
                {
                    if (_dgv.SortOrder == SortOrder.Ascending)
                        _dgv.Rows[BinarySearchAsc(search)].Selected = true;
                    else
                        _dgv.Rows[BinarySearchDesc(search)].Selected = true;
                }
                else
                {
                    _dgv.Rows[SequentialSearch(search)].Selected = true;
                }
                _dgv.FirstDisplayedScrollingRowIndex = _dgv.SelectedRows[0].Index;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        private int SequentialSearch(string search)
        {
            int pos = 0;
            foreach (DataGridViewRow row in _dgv.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.ColumnIndex == _sortedColumn)
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
            int max     = _dgv.Rows.Count - 1,
                min     = 0,
                current,
                compare;

            while (max >= min)
            {
                current = (max - min) / 2 + min;

                compare = string.Compare(_dgv[_sortedColumn, current].Value.ToString(), value, StringComparison.Ordinal);

                if (compare > 0)
                    max = current - 1;
                else if (compare < 0)
                    min = current + 1;
                else
                    return current;
            }

            if (min >= _dgv.Rows.Count)
                return _dgv.Rows.Count - 1;

            return min;
        }

        int BinarySearchDesc(string value)
        {
            int max     = _dgv.Rows.Count - 1,
                min     = 0,
                current,
                compare;

            while (max >= min)
            {
                current = (max - min) / 2 + min;

                compare = string.Compare(_dgv[_sortedColumn, current].Value.ToString(), value, StringComparison.Ordinal);

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
            if (_mPnlQuickSearch != null)
            {
                _mPnlQuickSearch.Dispose();
            }
        }

        #endregion
    }
}
