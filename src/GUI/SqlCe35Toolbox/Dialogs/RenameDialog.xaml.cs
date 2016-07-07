using System.Windows;
using ErikEJ.SqlCeToolbox.Helpers;

namespace ErikEJ.SqlCeToolbox.Dialogs
{
    public partial class RenameDialog
    {
        public string NewName { get; set; }

        public bool DbRename { get; set; }

        public RenameDialog(string tableName)
        {
            Telemetry.TrackPageView(nameof(RenameDialog));
            InitializeComponent();
            Background = VsThemes.GetWindowBackground();
            Title = "Rename " + tableName;
            ServerName.Text = tableName;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            SaveSettings();
            Close();
        }

        private void SaveSettings()
        {
            NewName = ServerName.Text;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ServerName.Focus();
            if (DbRename)
            {
                Title = "Rename Connection";
            }
        }

    }
}