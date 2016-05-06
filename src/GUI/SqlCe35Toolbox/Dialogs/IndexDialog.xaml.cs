using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ErikEJ.SqlCeScripting;
using Microsoft.VisualStudio.PlatformUI;
using ErikEJ.SqlCeToolbox.Helpers;

namespace ErikEJ.SqlCeToolbox.Dialogs
{
    public partial class IndexDialog : DialogWindow
    {
        public IndexDialog(string tableName)
        {
            Telemetry.TrackPageView(nameof(IndexDialog));
            InitializeComponent();
            _tableName = tableName;
            this.Background = Helpers.VsThemes.GetWindowBackground();
        }

        #region Properties
        private string _tableName;
        private Index newIndex = new Index();
        public List<Column> Columns { get; set; }
        public Index NewIndex 
        {
            get
            {
                return newIndex;
            }
        }
        public string TableName { get; set; }
        #endregion

        #region Event Handlers
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (cmbColumns.SelectedIndex == -1)
            {
                EnvDteHelper.ShowError("Please select a column");
                return;
            }
            if (string.IsNullOrEmpty(dataSourceTextBox.Text))
            {
                EnvDteHelper.ShowError("Please enter an index name");
                return;
            }
            newIndex.IndexName = dataSourceTextBox.Text;
            newIndex.ColumnName = cmbColumns.SelectedItem.ToString();
            newIndex.TableName = _tableName;
            newIndex.Unique = chkUnique.IsChecked.Value;
            this.DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }
        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cmbColumns.ItemsSource = Columns.Select(c => c.ColumnName).ToList();
            this.dataSourceTextBox.Focus();
            this.Title = "Add Index to table " + _tableName;
        }

    }
}