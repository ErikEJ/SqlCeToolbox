using System.Windows;
using ErikEJ.SqlCeToolbox.Helpers;

namespace ErikEJ.SqlCeToolbox.Dialogs
{
    public partial class PasswordDialog
    {
        public string Password { get; set; }

        public PasswordDialog()
        {
            Telemetry.TrackPageView(nameof(PasswordDialog));
            InitializeComponent();
            Background = VsThemes.GetWindowBackground();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            SaveSettings();
            Close();
        }

        private void SaveSettings()
        {
            Password = PasswordInput.Password;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PasswordInput.Focus();
        }
    }
}