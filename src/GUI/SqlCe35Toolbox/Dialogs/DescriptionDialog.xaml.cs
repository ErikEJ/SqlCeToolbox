using System.Windows;
using System.Collections.Generic;
using ErikEJ.SqlCeToolbox.Helpers;

namespace ErikEJ.SqlCeToolbox.Dialogs
{
    /// <summary>
    /// Interaction logic for RenameDialog.xaml
    /// </summary>
    public partial class DescriptionDialog 
    {
        public string TableDescription { get; set; }
        private IList<TableColumnInfo> _columnsInfo;
        
        public IList<TableColumnInfo> ColumnsInfo
        {
            get { return _columnsInfo;}
            set
            {
                if (value != null)
                {
                    dgridCols.ItemsSource = value;
                    _columnsInfo = value;
                }
            }
        }

        public bool IsDatabase
        {
            get
            {
                return false;
            }
            set
            {
                if (value)
                {
                    Title = "Edit Description";
                    lblTable.Content = "Database Description:";
                }
            }
        }

        public DescriptionDialog(string tableDescription)
        {
            Telemetry.TrackPageView(nameof(DescriptionDialog));
            InitializeComponent();
            Background = VsThemes.GetWindowBackground();
            if (!string.IsNullOrWhiteSpace(tableDescription))
            {
                txtTableDesc.Text = tableDescription;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            SaveSettings();
            Close();
        }

        private void SaveSettings()
        {
            TableDescription = txtTableDesc.Text;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtTableDesc.Focus();
        }

    }
}