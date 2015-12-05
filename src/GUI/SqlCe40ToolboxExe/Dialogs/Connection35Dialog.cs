using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Input;
using Xceed.Wpf.Toolkit;
using ErikEJ.SqlCeScripting;
using ErikEJ.SqlCeToolbox.Helpers;

namespace ErikEJ.SqlCeToolbox.Dialogs
{
    public partial class Connection35Dialog : Window
    {
        private bool _loading;

        private bool createDb;

        public Connection35Dialog()
        {
            _loading = true;
            InitializeComponent();
            this.SaveButton.IsEnabled = false;
            this.TestButton.IsEnabled = false;
            _loading = false;
        }

        #region Properties
        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
        }

        private string _connectionString;
        #endregion

        #region Event Handlers

        private void browseButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (File.Exists(dataSourceTextBox.Text))
                openFileDialog.FileName = dataSourceTextBox.Text;
            if (Path.IsPathRooted(dataSourceTextBox.Text) && Directory.Exists(Path.GetDirectoryName(dataSourceTextBox.Text)))
                openFileDialog.InitialDirectory = Path.GetDirectoryName(dataSourceTextBox.Text);

            bool? result = openFileDialog.ShowDialog();
            if (result.HasValue && result == true)
                dataSourceTextBox.Text = openFileDialog.FileName;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            TestConnection(false);
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "SQL Server Compact Database (*.sdf)|*.sdf|All Files(*.*)|*.*";
            sfd.ValidateNames = true;
            sfd.Title = "Create new Database File";
            if (sfd.ShowDialog() == true)
            {
                createDb = true;
                this.dataSourceTextBox.Text = sfd.FileName;
            }
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            TestConnection(true);
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "SQL Server Compact Database File (*.sdf)|*.sdf|All Files (*.*)|*.*";
            ofd.CheckFileExists = true;
            ofd.Multiselect = false;
            ofd.ValidateNames = true;
            ofd.Title = "Select Database File";
            if (ofd.ShowDialog() == true)
            {
                createDb = false;
                this.dataSourceTextBox.Text = ofd.FileName;
            }
        }

        private void textBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            UpdateBuilder();
        }

        private void password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdateBuilder();
        }

        private void maxSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            UpdateBuilder();
        }

        private void txtConnection_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtConnection.Text))
            {
                _connectionString = txtConnection.Text;
                SaveButton.IsEnabled = true;
                TestButton.IsEnabled = true;
            }
        }

        #endregion

        private void TestConnection(bool showMessage)
        {
            try
            {
                if (createDb)
                {
                    if (!System.IO.File.Exists(dataSourceTextBox.Text))
                    {
                        var engineHelper = new SqlCeHelper();
                        engineHelper.CreateDatabase(_connectionString);
                    }
                }

                using (var repository = new DBRepository(_connectionString))
                {
                    if (showMessage)
                    {
                        System.Windows.MessageBox.Show("Connection OK!");
                    }
                    else
                    {
                        this.DialogResult = true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(DataConnectionHelper.ShowErrors(ex));
            }
        }

        private void UpdateBuilder()
        {
            if (!_loading)
            {
                _connectionString = string.Empty;
                if (!string.IsNullOrWhiteSpace(dataSourceTextBox.Text))
                    _connectionString = string.Format("Data Source={0}", dataSourceTextBox.Text);
                if (!string.IsNullOrWhiteSpace(maxSize.Text) && maxSize.Text != "256")
                    _connectionString = _connectionString + string.Format(";Max Database Size={0}", maxSize.Text);
                if (password != null)
                    if (!string.IsNullOrWhiteSpace(password.Text))
                        _connectionString = _connectionString + string.Format(";Password={0}", password.Text);
                txtConnection.Text = _connectionString;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.dataSourceTextBox.Focus();
        }

    }
}