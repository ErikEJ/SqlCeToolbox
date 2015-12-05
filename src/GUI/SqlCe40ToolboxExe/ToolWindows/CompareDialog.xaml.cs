using System.Windows;
using System.Collections.Generic;
using ErikEJ.SqlCeToolbox.Helpers;

namespace ErikEJ.SqlCeToolbox.Dialogs
{
    /// <summary>
    /// Interaction logic for CompareDialog.xaml
    /// </summary>
    public partial class CompareDialog : Window
    {
        public KeyValuePair<string, string> TargetDatabase { get; set; }

        public CompareDialog(string caption, SortedDictionary<string, string> connections)
        {
            InitializeComponent();
            this.lblCompare.Content = string.Format("Choose the target database\nto compare {0} (source) with:", caption);
            comboBox1.DisplayMemberPath = "Key";
            comboBox1.ItemsSource = connections;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }

        private void comboBox1_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (comboBox1.SelectedItem != null)
                this.TargetDatabase = (KeyValuePair<string, string>)comboBox1.SelectedItem;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.comboBox1.Focus();
        }
    }
}