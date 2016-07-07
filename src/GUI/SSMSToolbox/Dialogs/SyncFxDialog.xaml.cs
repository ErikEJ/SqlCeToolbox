using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ErikEJ.SqlCeScripting;
using ErikEJ.SqlCeToolbox.Helpers;

namespace ErikEJ.SqlCeToolbox.Dialogs
{
    public partial class SyncFxDialog
    {
        public SyncFxDialog()
        {
            Telemetry.TrackPageView(nameof(SyncFxDialog));
            InitializeComponent();
            Background = VsThemes.GetWindowBackground();
        }

        private readonly List<CheckListItem> _items = new List<CheckListItem>();
        private List<Column> _allColumns = new List<Column>();
        private readonly List<Column> _selectedColumns = new List<Column>();
        private List<PrimaryKey> _pkColumns = new List<PrimaryKey>();

        public List<string> Tables { get; set; }

        public List<Column> Columns
        {
            get { return _selectedColumns; }
            set { _allColumns = value; }
        }

        public List<PrimaryKey> PrimaryKeyColumns
        {
            get { return _pkColumns; }
            set { _pkColumns = value; }
        }

        public bool Advanced
        {
            get { return chkAdvanced.IsChecked != null && chkAdvanced.IsChecked.Value; }
        }

        public string ModelName
        {
            get { return txtModel.Text; }
            set { txtModel.Text = value; }
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _selectedColumns.AddRange(_allColumns);
            foreach (string table in Tables)
            {
                _items.Add(new CheckListItem {IsChecked = false, Label = table});
            }
            chkTables.ItemsSource = _items;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Tables.Clear();
            foreach (object item in chkTables.Items)
            {
                var checkItem = (CheckListItem) item;
                if (checkItem.IsChecked)
                {
                    Tables.Add(checkItem.Label);
                }
            }
            Close();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void chkClear_Click(object sender, RoutedEventArgs e)
        {
            if (chkClear.IsChecked != null && chkClear.IsChecked.Value)
            {
                foreach (CheckListItem item in _items)
                {
                    if (!item.IsChecked)
                    {
                        item.IsChecked = true;
                    }
                }
            }
            else
            {
                foreach (CheckListItem item in _items)
                {
                    if (item.IsChecked)
                    {
                        item.IsChecked = false;
                    }
                }
            }
            chkTables.ItemsSource = null;
            chkTables.ItemsSource = _items;
        }


        private void PopulateColumns(string table)
        {
            List<CheckListItem> columns = new List<CheckListItem>();
            var columnList = _allColumns.Where(ac => ac.TableName == table).ToList();
            foreach (var column in columnList)
            {
                var isChecked = true;
                var col =
                    _selectedColumns
                        .SingleOrDefault(sc => sc != null && sc.ColumnName == column.ColumnName && sc.TableName == column.TableName);
                if (col == null)
                    isChecked = false;
                // var isPK = pkColumns.Any(c => c.TableName == column.TableName && c.ColumnName == column.ColumnName);
                columns.Add(new CheckListItem {IsChecked = isChecked, Label = column.ColumnName, Tag = table});
            }
            chkColumns.ItemsSource = null;
            chkColumns.ItemsSource = columns;
        }

        private void chkTables_ItemSelectionChanged(object sender,
            Xceed.Wpf.Toolkit.Primitives.ItemSelectionChangedEventArgs e)
        {
            var item = e.Item as CheckListItem;
            if (item != null)
            {
                PopulateColumns(item.Label);
            }
        }

        private void chkColumns_ItemSelectionChanged(object sender,
            Xceed.Wpf.Toolkit.Primitives.ItemSelectionChangedEventArgs e)
        {
            var item = e.Item as CheckListItem;
            if (item != null)
            {
                //remove or add to/from selected columns list
                var col =
                    _selectedColumns
                        .SingleOrDefault(sc => sc != null && sc.ColumnName == item.Label && sc.TableName == item.Tag);
                if (col != null)
                {
                    if (!item.IsChecked)
                    {
                        _selectedColumns.Remove(col);
                    }
                    _selectedColumns.Add(col);
                }
            }
        }
    }
}

