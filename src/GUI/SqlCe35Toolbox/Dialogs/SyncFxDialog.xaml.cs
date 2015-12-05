using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.VisualStudio.PlatformUI;
using ErikEJ.SqlCeScripting;

namespace ErikEJ.SqlCeToolbox.Dialogs
{
    public partial class SyncFxDialog : DialogWindow
    {
        public SyncFxDialog()
        {
            Telemetry.TrackPageView(nameof(SyncFxDialog));
            InitializeComponent();
            this.Background = Helpers.VSThemes.GetWindowBackground();
        }

        private List<CheckListItem> items = new List<CheckListItem>();
        private List<Column> allColumns = new List<Column>();
        private List<Column> selectedColumns = new List<Column>();
        private List<PrimaryKey> pkColumns = new List<PrimaryKey>();

        public List<string> Tables { get; set; }

        public List<Column> Columns
        {
            get
            {
                return selectedColumns;
            }
            set
            {
                allColumns = value;
            }
        }

        public List<PrimaryKey> PrimaryKeyColumns
        {
            get
            {
                return pkColumns;
            }
            set
            {
                pkColumns = value;
            }
        }
        public bool Advanced
        {
            get
            {
                return this.chkAdvanced.IsChecked.Value;
            }
        }

        public string ModelName
        {
            get
            {
                return this.txtModel.Text;
            }
            set
            {
                this.txtModel.Text = value;
            }
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            selectedColumns.AddRange(allColumns);
            foreach (string table in Tables)
            {
                items.Add(new CheckListItem { IsChecked = false, Label = table });
            }
            chkTables.ItemsSource = items;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Tables.Clear();
            foreach (object item in chkTables.Items)
            {
                var checkItem = (CheckListItem)item;
                if (checkItem.IsChecked)
                {
                    this.Tables.Add(checkItem.Label);
                }
            }
            this.Close();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void chkClear_Click(object sender, RoutedEventArgs e)
        {
            if (chkClear.IsChecked.Value)
            {
                foreach (CheckListItem item in items)
                {
                    if (!item.IsChecked)
                    {
                        item.IsChecked = true;
                    }
                }
            }
            else
            {
                foreach (CheckListItem item in items)
                {
                    if (item.IsChecked)
                    {
                        item.IsChecked = false;
                    }
                }
            }
            chkTables.ItemsSource = null;
            chkTables.ItemsSource = items;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = Keyboard.FocusedElement as CheckBox;
            if (item != null)
            {
                PopulateColumns(item.Content.ToString(), item.IsChecked.Value);
            }
        }

        private void PopulateColumns(string table, bool check)
        {
            List<CheckListItem> columns = new List<CheckListItem>();
            bool isChecked = true;
            var columnList = allColumns.Where(ac => ac.TableName == table).ToList();
            foreach (var column in columnList)
            {
                isChecked = true;
                var col = selectedColumns.Where(sc => sc != null && sc.ColumnName == column.ColumnName && sc.TableName == column.TableName).SingleOrDefault();
                if (col == null)
                    isChecked = false;
                // var isPK = pkColumns.Any(c => c.TableName == column.TableName && c.ColumnName == column.ColumnName);
                columns.Add(new CheckListItem { IsChecked = isChecked, Label = column.ColumnName, Tag = table });
            }
            chkColumns.ItemsSource = null;
            chkColumns.ItemsSource = columns;
        }

        private void chkTables_ItemSelectionChanged(object sender, Xceed.Wpf.Toolkit.Primitives.ItemSelectionChangedEventArgs e)
        {
            var item = e.Item as CheckListItem;
            if (item != null)
            {
                PopulateColumns(item.Label, item.IsChecked);
            }
        }

        private void chkColumns_ItemSelectionChanged(object sender, Xceed.Wpf.Toolkit.Primitives.ItemSelectionChangedEventArgs e)
        {
            var item = e.Item as CheckListItem;
            if (item != null)
            {
                //remove or add to/from selected columns list
                var col = selectedColumns.Where(sc => sc != null && sc.ColumnName == item.Label && sc.TableName == item.Tag).SingleOrDefault();
                if (col != null && !item.IsChecked)
                {
                    selectedColumns.Remove(col);
                }
                if (col == null && item.IsChecked)
                {
                    selectedColumns.Add(col);
                }
            }
        }
    }

}
