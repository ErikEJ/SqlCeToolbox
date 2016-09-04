using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ErikEJ.SqlCeScripting;
using System.Windows.Controls;
using System.Linq;
using ErikEJ.SqlCeToolbox.Helpers;

namespace ErikEJ.SqlCeToolbox.Dialogs
{
    public partial class TableBuilderDialog : Window
    {
        public string TableName { get; set; }
        public List<Column> TableColumns { get; set; }
        public string PkScript { get; set; }

        private ObservableCollection<TableColumn> columns;

        public TableBuilderDialog(string tableDescription)
        {
            InitializeComponent();
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
            TableName = this.txtTableDesc.Text;
            if (string.IsNullOrEmpty(TableName))
            {
                MessageBox.Show("Table name is required");
                return false;
            }
            
            var validation = TableColumn.ValidateColumns(columns.ToList());
            if (!string.IsNullOrEmpty(validation))
            {
                MessageBox.Show(validation);
                return false;
            }
            TableColumns = TableColumn.BuildColumns(columns.ToList(), TableName);
            PkScript = TableColumn.BuildPkScript(columns.ToList(), TableName);
            return true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.txtTableDesc.Focus();
            columns = TableColumn.GetAll;
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
            columns.Add(new TableColumn { DataType = "int", AllowNull = false, Length = 4});
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (dgridCols.SelectedItem != null)
            {
                columns.Remove(dgridCols.SelectedItem as TableColumn);
            }
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