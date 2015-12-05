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

namespace ErikEJ.SqlCeToolbox.Dialogs
{
    public partial class PickTablesDialog : Window
    {
        public PickTablesDialog()
        {
            InitializeComponent();
        }

        private List<CheckListItem> items = new List<CheckListItem>();

        public List<string> Tables { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            bool isChecked = true;
            foreach (string table in Tables)
            { 
                isChecked = true;
                if (table.StartsWith("__"))
                {
                    isChecked = false;
                }
                items.Add(new CheckListItem { IsChecked = isChecked, Label = table });                
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
                if (!checkItem.IsChecked)
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
        
    
    }

}
