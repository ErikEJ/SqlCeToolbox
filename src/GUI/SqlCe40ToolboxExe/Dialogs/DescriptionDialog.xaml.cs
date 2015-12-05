using System.Windows;
namespace ErikEJ.SqlCeToolbox.Dialogs
{
    /// <summary>
    /// Interaction logic for RenameDialog.xaml
    /// </summary>
    public partial class DescriptionDialog : Window
    {
        public string Description { get; set; }

        public DescriptionDialog()
        {
            InitializeComponent();
        }

        public DescriptionDialog(string description)
        {
            InitializeComponent();
            if (!string.IsNullOrWhiteSpace(description))
                this.DescriptionText.Text = description;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            SaveSettings();
            Close();
        }

        private void SaveSettings()
        {
            Description = this.DescriptionText.Text;
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