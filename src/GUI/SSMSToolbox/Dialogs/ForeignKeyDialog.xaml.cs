using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ErikEJ.SqlCeScripting;
using System;
using ErikEJ.SqlCeToolbox.Helpers;

namespace ErikEJ.SqlCeToolbox.Dialogs
{
    public partial class ForeignKeyDialog
    {
        public ForeignKeyDialog(string tableName)
        {
            Telemetry.TrackPageView(nameof(ForeignKeyDialog));
            InitializeComponent();
            _tableName = tableName;
            Background = VsThemes.GetWindowBackground();
        }

        #region Properties
        private List<string> fkActions = new List<string> { "NO ACTION", "CASCADE", "SET DEFAULT", "SET NULL" };
        private string _tableName;
        private Constraint newKey = new Constraint();
        public List<Column> AllColumns { get; set; }
        public List<PrimaryKey> AllPrimaryKeys { get; set; }
        public Constraint NewKey
        {
            get
            {
                return newKey;
            }
        }
        public string TableName { get; set; }
        #endregion

        #region Event Handlers
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(dataSourceTextBox.Text))
            {
                EnvDteHelper.ShowError("Please enter a foreign key name");
                return;
            }
            if (cmbFkColumn.SelectedIndex == -1)
            {
                EnvDteHelper.ShowError("Please select a foreign key column");
                return;
            }
            if (cmbPrimaryKeyTableAndColumn.SelectedIndex == -1)
            {
                EnvDteHelper.ShowError("Please select a primary key");
                return;                
            }
            newKey.ColumnName = cmbFkColumn.SelectedItem.ToString();
            newKey.Columns = new ColumnList { cmbFkColumn.SelectedItem.ToString() };
            newKey.ConstraintName = dataSourceTextBox.Text;
            newKey.ConstraintTableName = _tableName;
            newKey.DeleteRule = cmbDeleteAction.SelectedItem.ToString();
            newKey.UpdateRule = cmbUpdateAction.SelectedItem.ToString();

            PrimaryKey pk = (PrimaryKey)cmbPrimaryKeyTableAndColumn.SelectedValue;
            newKey.UniqueColumns = new ColumnList { pk.ColumnName };
            newKey.UniqueConstraintTableName = pk.TableName;

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
            cmbDeleteAction.ItemsSource = fkActions;
            cmbDeleteAction.SelectedIndex = 0;
            cmbUpdateAction.ItemsSource = fkActions;
            cmbUpdateAction.SelectedIndex = 0;

            cmbFkColumn.ItemsSource = AllColumns.Where(c => c.TableName == _tableName).Select(c => c.ColumnName).ToList();
            cmbPrimaryKeyTableAndColumn.DisplayMemberPath = "DisplayValue";
            cmbPrimaryKeyTableAndColumn.SelectedValuePath = "Value";

            var keys = AllPrimaryKeys.Where(c => c.TableName != _tableName);
            var displayKeys = new List<PrimaryKeyDisplay>();
            foreach (var item in keys)
            {
                displayKeys.Add(new PrimaryKeyDisplay(item));
            }

            cmbPrimaryKeyTableAndColumn.ItemsSource = displayKeys.OrderBy(d => d.DisplayValue).ToList();
            dataSourceTextBox.Focus();
            Title = "Add Foreign Key to table " + _tableName;
        }

        public class PrimaryKeyDisplay
        {
            public PrimaryKeyDisplay(PrimaryKey value)
            {
                Value = value;
                DisplayValue = string.Format("{0} ({1})", value.TableName, value.ColumnName);
            }

            // ReSharper disable once UnusedAutoPropertyAccessor.Global
            public PrimaryKey Value { get; set; }
            public String DisplayValue { get; set; }
        }


    }
}