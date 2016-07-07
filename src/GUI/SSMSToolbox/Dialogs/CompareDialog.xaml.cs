using System.Windows;
using System.Collections.Generic;
using ErikEJ.SqlCeToolbox.Helpers;

namespace ErikEJ.SqlCeToolbox.Dialogs
{
    /// <summary>
    /// Interaction logic for CompareDialog.xaml
    /// </summary>
    public partial class CompareDialog
    {
        public KeyValuePair<string, DatabaseInfo> TargetDatabase { get; set; }

        public bool SwapTarget 
        { 
            get
            {
                return checkBox1.IsChecked != null && checkBox1.IsChecked.Value;
            }
        }

        public CompareDialog(string caption, Dictionary<string, DatabaseInfo> connections, string tableName = null)
        {
            Telemetry.TrackPageView(nameof(CompareDialog));
            InitializeComponent();
            Background = VsThemes.GetWindowBackground();
            lblCompare.Text = string.Format("Choose the target database to compare {0} (source) with:", caption);
            if (!string.IsNullOrEmpty(tableName))
            {
                lblCompare.Text = string.Format("Choose the database to compare table {0} in {1} (source) with:", tableName, caption);
                Title = "Script Table Data Diff";
            }
            comboBox1.DisplayMemberPath = "Value.Caption";
            comboBox1.ItemsSource = connections;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void comboBox1_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (comboBox1.SelectedItem != null)
                TargetDatabase = (KeyValuePair<string, DatabaseInfo>)comboBox1.SelectedItem;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            comboBox1.Focus();
        }
    }
}