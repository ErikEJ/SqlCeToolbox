using System.Windows;
using System.Collections.Generic;
using ErikEJ.SqlCeToolbox.Helpers;
using Microsoft.VisualStudio.PlatformUI;

namespace ErikEJ.SqlCeToolbox.Dialogs
{
    public partial class ExportDialog : DialogWindow
    {
        public KeyValuePair<string, DatabaseInfo> TargetDatabase { get; set; }

        public ExportDialog(string caption, Dictionary<string, DatabaseInfo> connections)
        {
            Telemetry.TrackPageView(nameof(ExportDialog));
            InitializeComponent();
            this.Background = Helpers.VsThemes.GetWindowBackground();
            this.lblCompare.Content = "Choose the target SQL Server database\n(connected via Server Explorer/Data Connections)";
            comboBox1.DisplayMemberPath = "Value.Caption";
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
                this.TargetDatabase = (KeyValuePair<string, DatabaseInfo>)comboBox1.SelectedItem;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.comboBox1.Focus();
        }
    }
}