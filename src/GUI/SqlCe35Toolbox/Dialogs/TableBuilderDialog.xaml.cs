using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ErikEJ.SqlCeScripting;
using ErikEJ.SqlCeToolbox.Helpers;
using Microsoft.VisualStudio.PlatformUI;

namespace ErikEJ.SqlCeToolbox.Dialogs
{
    public partial class TableBuilderDialog : DialogWindow
    {
        public string TableName { get; set; }
        public List<Column> TableColumns { get; set; }
        public string PkScript { get; set; }

        public int Mode { get; set; } // 1 = add, 2 = edit

        private ObservableCollection<TableColumn> columns;
        private DatabaseType _dbType; 

        public TableBuilderDialog(string tableDescription, DatabaseType dbType)
        {
            Telemetry.TrackPageView(nameof(TableBuilderDialog));
            InitializeComponent();
            _dbType = dbType;
            this.Background = Helpers.VSThemes.GetWindowBackground();
            if (!string.IsNullOrWhiteSpace(tableDescription))
            {
                this.txtTableDesc.Text = tableDescription;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SaveSettings())
            {
                this.DialogResult = true;
                Close();
            }
            else
            {
                return;
            }
        }

        private bool SaveSettings()
        {
            try
            {
                TableName = this.txtTableDesc.Text;
                if (string.IsNullOrEmpty(TableName))
                {
                    EnvDTEHelper.ShowError("Table name is required");
                    return false;
                }

                var validation = TableColumn.ValidateColumns(columns.ToList());
                if (!string.IsNullOrEmpty(validation))
                {
                    EnvDTEHelper.ShowError(validation);
                    return false;
                }
                TableColumns = TableColumn.BuildColumns(columns.ToList(), TableName);
                PkScript = TableColumn.BuildPkScript(columns.ToList(), TableName);
                if (_dbType == DatabaseType.SQLite)
                {
                    PkScript = BuildSQLitePkScript(columns.ToList(), TableName);
                }
                else
                {
                    PkScript = TableColumn.BuildPkScript(columns.ToList(), TableName);
                }
                return true;
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, DatabaseType.SQLServer);
            }
            return false;
        }

        private static string BuildSQLitePkScript(List<TableColumn> columns, string TableName)
        {
            var script = string.Empty;
            var pkCols = columns.Where(x => x.PrimaryKey).ToList();
            if (pkCols.Count > 0)
            {
                string cols = string.Empty;
                foreach (var col in pkCols)
                {
                    cols += string.Format("[{0}],", col.Name);
                }
                cols = cols.Substring(0, cols.Length - 1);
                script = string.Format("{0}, CONSTRAINT [PK_{1}] PRIMARY KEY ({2})", Environment.NewLine, TableName, cols);
            }
            return script;
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.txtTableDesc.Focus();
            if (TableColumns != null)
            {
                columns = new ObservableCollection<TableColumn>(TableColumn.BuildTableColumns(TableColumns, TableName));
            }
            else
            {
                if (Mode == 1)
                {
                    columns = TableColumn.GetNew;
                }
                else
                {
                    columns = TableColumn.GetAll;
                }
            }
            if (Mode == 1 || Mode == 2)
            {
                AddButton.Visibility = System.Windows.Visibility.Collapsed;
                DeleteButton.Visibility = System.Windows.Visibility.Collapsed;
                dgridCols.CanUserAddRows = false;
                dgridCols.CanUserDeleteRows = false;
                lblTable.Content = "Table Name";
                txtTableDesc.IsEnabled = false;
            }
            if (Mode == 1)
            {
                this.Title = "Add column";
            }
            if (Mode == 2)
            {
                colPrimary.Visibility = System.Windows.Visibility.Collapsed;
                colName.IsReadOnly = true;
                this.Title = "Edit column";
            }
            this.dgridCols.ItemsSource = columns;
            this.colDataType.ItemsSource = TableDataType.GetAll;
        }

        private void DataTypeFieldChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var item = dgridCols.SelectedItem as TableColumn;
            if (item != null)
            {
                item.Length = TableDataType.GetDefaultLength(item.DataType);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            //Move Up
            //var item = dgridCols.SelectedItem as TableColumn;
            //if (item != null)
            //{
            //    int from = columns.IndexOf(item);
            //    if (from < columns.Count - 1)
            //        columns.Move(from, from + 1);
            //}
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            //Move Down
            //var item = dgridCols.SelectedItem as TableColumn;
            //if (item != null)
            //{
            //    int from = columns.IndexOf(item);
            //    if (from > 0)
            //        columns.Move(from, from - 1);
            //}
        }

        void PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            TableColumn c = e.Row.DataContext as TableColumn;
            if (e.Column.DisplayIndex == 2)
            {
                if (TableDataType.IsLengthFixed(c.DataType))
                {
                    TextBox textBox = (e.EditingElement as TextBox);
                    textBox.IsReadOnly = true;
                }
                else
                {
                    TextBox textBox = (e.EditingElement as TextBox);
                    textBox.IsReadOnly = false;
                }
            }
            if (e.Column.DisplayIndex == 3)
            {
                if (TableDataType.IsNullFixed(c.DataType))
                {
                    CheckBox checkBox = (e.EditingElement as CheckBox);
                    checkBox.IsEnabled = false;
                    checkBox.IsChecked = false;
                }
                else
                {
                    CheckBox checkBox = (e.EditingElement as CheckBox);
                    checkBox.IsEnabled = true;
                    checkBox.IsChecked = false;
                }
            }
        }


    }
}