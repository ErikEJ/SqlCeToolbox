using System.Windows;
using Microsoft.VisualStudio.PlatformUI;
namespace ErikEJ.SqlCeToolbox.Dialogs
{
    /// <summary>
    /// Interaction logic for RenameDialog.xaml
    /// </summary>
    public partial class PasswordDialog : DialogWindow
    {
        public string Password { get; set; }

        public PasswordDialog()
        {
            Telemetry.TrackPageView(nameof(PasswordDialog));
            InitializeComponent();
            this.Background = Helpers.VSThemes.GetWindowBackground();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            SaveSettings();
            Close();
        }

        private void SaveSettings()
        {
            Password = this.PasswordInput.Password;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.PasswordInput.Focus();
        }

    }
}