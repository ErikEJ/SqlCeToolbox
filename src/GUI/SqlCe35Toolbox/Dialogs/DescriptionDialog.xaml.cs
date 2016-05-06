using System.Windows;
using Microsoft.VisualStudio.PlatformUI;
using System.Collections.Generic;
using ErikEJ.SqlCeToolbox.Helpers;

namespace ErikEJ.SqlCeToolbox.Dialogs
{
    /// <summary>
    /// Interaction logic for RenameDialog.xaml
    /// </summary>
    public partial class DescriptionDialog : DialogWindow
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
                    this.Title = "Edit Description";
                    this.lblTable.Content = "Database Description:";
                }
            }
        }

        public DescriptionDialog(string tableDescription)
        {
            Telemetry.TrackPageView(nameof(DescriptionDialog));
            InitializeComponent();
            this.Background = Helpers.VsThemes.GetWindowBackground();
            if (!string.IsNullOrWhiteSpace(tableDescription))
            {
                this.txtTableDesc.Text = tableDescription;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            SaveSettings();
            Close();
        }

        private void SaveSettings()
        {
            TableDescription = this.txtTableDesc.Text;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.txtTableDesc.Focus();
        }

    }
}