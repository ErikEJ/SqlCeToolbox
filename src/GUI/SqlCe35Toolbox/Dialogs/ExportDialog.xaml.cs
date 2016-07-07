using System.Windows;
using System.Collections.Generic;
using ErikEJ.SqlCeToolbox.Helpers;

namespace ErikEJ.SqlCeToolbox.Dialogs
{
    public partial class ExportDialog
    {
        public KeyValuePair<string, DatabaseInfo> TargetDatabase { get; set; }

        public ExportDialog(Dictionary<string, DatabaseInfo> connections)
        {
            Telemetry.TrackPageView(nameof(ExportDialog));
            InitializeComponent();
            Background = VsThemes.GetWindowBackground();
            lblCompare.Content = "Choose the target SQL Server database\n(connected via Server Explorer/Data Connections)";
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