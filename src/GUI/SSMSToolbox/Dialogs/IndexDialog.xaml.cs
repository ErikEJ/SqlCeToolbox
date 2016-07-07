using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ErikEJ.SqlCeScripting;
using ErikEJ.SqlCeToolbox.Helpers;

namespace ErikEJ.SqlCeToolbox.Dialogs
{
    public partial class IndexDialog
    {
        public IndexDialog(string tableName)
        {
            Telemetry.TrackPageView(nameof(IndexDialog));
            InitializeComponent();
            _tableName = tableName;
            Background = VsThemes.GetWindowBackground();
        }

        #region Properties
        private string _tableName;
        private Index _newIndex;
        public List<Column> Columns { get; set; }
        public Index NewIndex 
        {
            get
            {
                return _newIndex;
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
            _newIndex.IndexName = dataSourceTextBox.Text;
            _newIndex.ColumnName = cmbColumns.SelectedItem.ToString();
            _newIndex.TableName = _tableName;
            if (chkUnique.IsChecked != null) _newIndex.Unique = chkUnique.IsChecked.Value;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cmbColumns.ItemsSource = Columns.Select(c => c.ColumnName).ToList();
            dataSourceTextBox.Focus();
            Title = "Add Index to table " + _tableName;
        }

    }
}