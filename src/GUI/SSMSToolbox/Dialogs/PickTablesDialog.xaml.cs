using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using ErikEJ.SqlCeToolbox.Helpers;
using Microsoft.Win32;
using System.Linq;

namespace ErikEJ.SqlCeToolbox.Dialogs
{
    public partial class PickTablesDialog
    {
        public PickTablesDialog(bool allowWindow)
        {
            Telemetry.TrackPageView(nameof(PickTablesDialog));
            InitializeComponent();
            Background = VsThemes.GetWindowBackground();
            if (!allowWindow)
            {
                button1.Content = "OK";
                btnWindow.Visibility = Visibility.Collapsed;
            };
        }

        private List<CheckListItem> items = new List<CheckListItem>();

        public List<string> Tables { get; set; }

        public bool ToWindow { get; set; } = false;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (string table in Tables)
            { 
                bool isChecked = !table.StartsWith("__");
                isChecked = !table.StartsWith("dbo.__");
                items.Add(new CheckListItem { IsChecked = isChecked, Label = table });                
            }
            chkTables.ItemsSource = items;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            AddTables();
            Close();
        }

        private void AddTables()
        {
            Tables.Clear();
            foreach (object item in chkTables.Items)
            {
                var checkItem = (CheckListItem)item;
                if (!checkItem.IsChecked)
                {
                    Tables.Add(checkItem.Label);
                }
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnWindow_Click(object sender, RoutedEventArgs e)
        {
            ToWindow = true;
            DialogResult = true;
            AddTables();
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

        private void BtnSaveSelection_OnClick(object sender, RoutedEventArgs e)
        {
            var tableList = string.Empty;
            foreach (var item in chkTables.Items)
            {
                var checkItem = (CheckListItem)item;
                if ((checkItem.IsChecked))
                {
                    tableList += checkItem.Label + Environment.NewLine;
                }
            }

            var sfd = new SaveFileDialog
            {
                Filter = "Text file (*.txt)|*.txt|All Files(*.*)|*.*",
                ValidateNames = true,
                Title = "Save list of tables as"
            };
            if (sfd.ShowDialog() != true) return;
            File.WriteAllText(sfd.FileName, tableList, Encoding.UTF8);
        }

        private void BtnLoadSelection_OnClick(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Text file (*.txt)|*.txt|All Files(*.*)|*.*",
                CheckFileExists = true,
                Multiselect = false,
                Title = "Select list of tables to load"
            };
            if (ofd.ShowDialog() != true) return;

            var lines = File.ReadAllLines(ofd.FileName);
            foreach (var item in items)
            {
                item.IsChecked = lines.Contains(item.Label);
            }
            chkTables.ItemsSource = null;
            chkTables.ItemsSource = items;
        }
    }
}
