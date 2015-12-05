using System.Windows;
namespace ErikEJ.SqlCeToolbox.Dialogs
{
    /// <summary>
    /// Interaction logic for RenameDialog.xaml
    /// </summary>
    public partial class RenameDialog : Window
    {
        public string NewName { get; set; }

        public RenameDialog()
        {
            InitializeComponent();
        }

        public RenameDialog(string tableName)
        {
            InitializeComponent();
            this.Title = "Rename Table " + tableName;
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
    }
}