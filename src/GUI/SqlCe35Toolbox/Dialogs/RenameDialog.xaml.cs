using System.Windows;
using ErikEJ.SqlCeToolbox.Helpers;
using Microsoft.VisualStudio.PlatformUI;
namespace ErikEJ.SqlCeToolbox.Dialogs
{
    /// <summary>
    /// Interaction logic for RenameDialog.xaml
    /// </summary>
    public partial class RenameDialog : DialogWindow
    {
        public string NewName { get; set; }

        public bool DbRename { get; set; }

        public RenameDialog(string tableName)
        {
            Telemetry.TrackPageView(nameof(RenameDialog));
            InitializeComponent();
            this.Background = Helpers.VsThemes.GetWindowBackground();
            this.Title = "Rename " + tableName;
            this.ServerName.Text = tableName;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            SaveSettings();
            Close();
        }

        private void SaveSettings()
        {
            NewName = this.ServerName.Text;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.ServerName.Focus();
            if (DbRename)
            {
                this.Title = "Rename Connection";
            }
        }

    }
}