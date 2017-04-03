using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ErikEJ.SqlCeScripting;
using ErikEJ.SqlCeToolbox.Helpers;

namespace ErikEJ.SqlCeToolbox.Dialogs
{
    public partial class TableBuilderDialog
    {
        public string TableName { get; set; }
        public List<Column> TableColumns { get; set; }
        public string PkScript { get; set; }

        public int Mode { get; set; } // 1 = add, 2 = edit

        private ObservableCollection<TableColumn> _columns;
        private readonly DatabaseType _dbType; 

        public TableBuilderDialog(string tableDescription, DatabaseType dbType)
        {
            Telemetry.TrackPageView(nameof(TableBuilderDialog));
            InitializeComponent();
            _dbType = dbType;
            Background = VsThemes.GetWindowBackground();
            if (!string.IsNullOrWhiteSpace(tableDescription))
            {
                txtTableDesc.Text = tableDescription;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SaveSettings())
            {
                DialogResult = true;
                Close();
            }
        }

        private bool SaveSettings()
        {
            try
            {
                TableName = txtTableDesc.Text;
                if (string.IsNullOrEmpty(TableName))
                {
                    EnvDteHelper.ShowError("Table name is required");
                    return false;
                }

                var validation = TableColumn.ValidateColumns(_columns.ToList());
                if (!string.IsNullOrEmpty(validation))
                {
                    EnvDteHelper.ShowError(validation);
                    return false;
                }
                TableColumns = TableColumn.BuildColumns(_columns.ToList(), TableName);
                PkScript = TableColumn.BuildPkScript(_columns.ToList(), TableName);
                PkScript = _dbType == DatabaseType.SQLite ? BuildSqLitePkScript(_columns.ToList(), TableName) : TableColumn.BuildPkScript(_columns.ToList(), TableName);
                return true;
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, DatabaseType.SQLServer);
            }
            return false;
        }

        private static string BuildSqLitePkScript(List<TableColumn> columns, string tableName)
        {
            var script = string.Empty;
            var pkCols = columns.Where(x => x.PrimaryKey).ToList();
            if (pkCols.Count > 0)
            {
                string cols = pkCols.Aggregate(string.Empty, (current, col) => current + string.Format("[{0}],", col.Name));
                cols = cols.Substring(0, cols.Length - 1);
                script = string.Format("{0}, CONSTRAINT [PK_{1}] PRIMARY KEY ({2})", Environment.NewLine, tableName, cols);
            }
            return script;
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtTableDesc.Focus();
            if (TableColumns != null)
            {
                _columns = new ObservableCollection<TableColumn>(TableColumn.BuildTableColumns(TableColumns, TableName));
            }
            else
            {
                if (Mode == 1)
                {
                    _columns = _dbType == DatabaseType.SQLite ? TableColumn.GetNewSqlite : TableColumn.GetNew;
                    
                }
                else
                {
                    _columns = _dbType == DatabaseType.SQLite ? TableColumn.GetAllSqlite : TableColumn.GetAll;
                }
            }
            if (Mode == 1 || Mode == 2)
            {
                AddButton.Visibility = Visibility.Collapsed;
                DeleteButton.Visibility = Visibility.Collapsed;
                dgridCols.CanUserAddRows = false;
                dgridCols.CanUserDeleteRows = false;
                lblTable.Content = "Table Name";
                txtTableDesc.IsEnabled = false;
            }
            if (Mode == 1)
            {
                Title = "Add column";
            }
            if (Mode == 2)
            {
                colPrimary.Visibility = Visibility.Collapsed;
                colName.IsReadOnly = true;
                Title = "Edit column";
            }
            dgridCols.ItemsSource = _columns;
            var items = TableDataType.GetAll.Where(i => !"REAL.INTEGER.TEXT.BLOB.NUMERIC".Contains(i.Key)).ToDictionary(i => i.Key, i => i.Value);
            if (_dbType == DatabaseType.SQLite)
                items = TableDataType.GetAll.Where(i => "REAL.INTEGER.TEXT.BLOB.NUMERIC".Contains(i.Key)).ToDictionary(i => i.Key, i => i.Value);
            colDataType.ItemsSource = items;
        }

        private void DataTypeFieldChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = dgridCols.SelectedItem as TableColumn;
            if (item != null)
            {
                item.Length = TableDataType.GetDefaultLength(item.DataType);
                item.Precision = TableDataType.GetDefaultPrecision(item.DataType);
                item.Scale = TableDataType.GetDefaultScale(item.DataType);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            //Move Up
            //var item = dgridCols.SelectedItem as TableColumn;
            //if (item != null)
            //{
            //    int from = _columns.IndexOf(item);
            //    if (from < _columns.Count - 1)
            //        _columns.Move(from, from + 1);
            //}
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            //Move Down
            //var item = dgridCols.SelectedItem as TableColumn;
            //if (item != null)
            //{
            //    int from = _columns.IndexOf(item);
            //    if (from > 0)
            //        _columns.Move(from, from - 1);
            //}
        }

        private void PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            var c = e.Row.DataContext as TableColumn;
            if (e.Column.DisplayIndex == 2)
            {
                if (c != null && TableDataType.IsLengthFixed(c.DataType))
                {
                    var textBox = (e.EditingElement as TextBox);
                    if (textBox != null) textBox.IsReadOnly = true;
                }
                else
                {
                    var textBox = (e.EditingElement as TextBox);
                    if (textBox != null) textBox.IsReadOnly = false;
                }
            }
            if (e.Column.DisplayIndex != 3) return;
            if (c != null && TableDataType.IsNullFixed(c.DataType))
            {
                var checkBox = (e.EditingElement as CheckBox);
                if (checkBox != null)
                {
                    checkBox.IsEnabled = false;
                    checkBox.IsChecked = false;
                }
            }
            else
            {
                var checkBox = (e.EditingElement as CheckBox);
                if (checkBox != null)
                {
                    checkBox.IsEnabled = true;
                    checkBox.IsChecked = false;
                }
            }
        }
    }
}