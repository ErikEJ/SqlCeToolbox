using System.Windows;
namespace ErikEJ.SqlCeToolbox.Dialogs
{
    /// <summary>
    /// Interaction logic for RenameDialog.xaml
    /// </summary>
    public partial class PasswordDialog : Window
    {
        public string Password { get; set; }

        public PasswordDialog()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            SaveSettings();
            Close();
        }

        private void SaveSettings()
        {
            Password = this.DescriptionText.Password;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DescriptionText.Focus();
        }

    }
}