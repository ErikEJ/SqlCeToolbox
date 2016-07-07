using System.Collections.Generic;
using System.Windows;
using ErikEJ.SqlCeToolbox.Helpers;

namespace ErikEJ.SqlCeToolbox.Dialogs
{
    public partial class PickTablesDialog
    {
        public PickTablesDialog()
        {
            Telemetry.TrackPageView(nameof(PickTablesDialog));
            InitializeComponent();
            Background = VsThemes.GetWindowBackground();
        }

        private List<CheckListItem> items = new List<CheckListItem>();

        public List<string> Tables { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (string table in Tables)
            { 
                bool isChecked = !table.StartsWith("__");
                items.Add(new CheckListItem { IsChecked = isChecked, Label = table });                
            }
            chkTables.ItemsSource = items;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Tables.Clear();
            foreach (object item in chkTables.Items)
            {
                var checkItem = (CheckListItem)item;
                if (!checkItem.IsChecked)
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
