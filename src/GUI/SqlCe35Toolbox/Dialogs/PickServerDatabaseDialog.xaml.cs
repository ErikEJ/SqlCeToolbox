using System.Windows;
using System.Collections.Generic;
using ErikEJ.SqlCeToolbox.Helpers;

namespace ErikEJ.SqlCeToolbox.Dialogs
{
    public partial class PickServerDatabaseDialog
    {
        public KeyValuePair<string, DatabaseInfo> SelectedDatabase { get; set; }

        public PickServerDatabaseDialog(Dictionary<string, DatabaseInfo> serverConnections)
        {
            Telemetry.TrackPageView(nameof(PickServerDatabaseDialog));
            InitializeComponent();
            Background = VsThemes.GetWindowBackground();
            lblCompare.Text = "Choose a database connection already connected in Server Explorer/Data Connections";
            comboBox1.DisplayMemberPath = "Value.Caption";
            comboBox1.ItemsSource = serverConnections;
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
                SelectedDatabase = (KeyValuePair<string, DatabaseInfo>)comboBox1.SelectedItem;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            comboBox1.Focus();
        }
    }
}