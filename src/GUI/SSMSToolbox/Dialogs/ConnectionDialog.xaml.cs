using System;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Media;
using ErikEJ.SqlCeToolbox.Helpers;

namespace ErikEJ.SqlCeToolbox.Dialogs
{
    public partial class ConnectionDialog 
    {
        private readonly bool _loading;
        private bool _createDb;
        private string _connectionString;
        private string _filter;

        public ConnectionDialog()
        {
            Telemetry.TrackPageView(nameof(ConnectionDialog));
            _loading = true;
            InitializeComponent();
            Background = VsThemes.GetWindowBackground();
            SaveButton.IsEnabled = false;
            TestButton.IsEnabled = false;
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

        public DatabaseType DbType { get; set; }

        public bool CouldSupportPrivateProvider { get; set; }

        public bool ShowDdexInfo { get; set; }

        public string InitialPath { get; set; }
        #endregion

        #region Event Handlers
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (TestConnection(false))
            {
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = _filter;
            sfd.ValidateNames = true;
            sfd.Title = "Create new Database File";
            if (!string.IsNullOrEmpty(InitialPath))
            {
                sfd.InitialDirectory = InitialPath;
            }
            if (sfd.ShowDialog() != true) return;
            _createDb = true;
            dataSourceTextBox.Text = sfd.FileName;
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            TestConnection(true);
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = _filter;
            ofd.CheckFileExists = true;
            ofd.Multiselect = false;
            ofd.ValidateNames = true;
            ofd.Title = "Select Database File";
            if (!string.IsNullOrEmpty(InitialPath))
            {
                ofd.InitialDirectory = InitialPath;
            }
            if (ofd.ShowDialog() != true) return;
            _createDb = false;
            dataSourceTextBox.Text = ofd.FileName;
        }

        private void maxSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            UpdateBuilder();
        }

        private void textBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            UpdateBuilder();
        }

        private void password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdateBuilder();
        }

        private void txtConnection_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtConnection.Text)) return;
            _connectionString = txtConnection.Text;
            SaveButton.IsEnabled = true;
            TestButton.IsEnabled = true;
        }

        #endregion

        private bool TestConnection(bool showMessage)
        {
            try
            {
                if (_createDb)
                {
                    if (!System.IO.File.Exists(dataSourceTextBox.Text))
                    {
                        var engineHelper = Helpers.RepositoryHelper.CreateEngineHelper(DbType);
                        engineHelper.CreateDatabase(_connectionString);
                    }
                }
                using (Helpers.RepositoryHelper.CreateRepository(new DatabaseInfo { ConnectionString = _connectionString, DatabaseType = DbType }))
                {
                    if (showMessage)
                    {
                        EnvDteHelper.ShowMessage("Connection OK!");
                    }
                    else
                    {
                        DialogResult = true;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Please upgrade using SqlCeEngine.Upgrade() method") && DbType == DatabaseType.SQLCE40)
                {
                    if (EnvDteHelper.ShowMessageBox("This database file is from an earlier version,\n\rwould you like to Upgrade it?\n\r(A copy of the original file will be named .bak)"
                        , Microsoft.VisualStudio.Shell.Interop.OLEMSGBUTTON.OLEMSGBUTTON_YESNO
                        , Microsoft.VisualStudio.Shell.Interop.OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_SECOND
                        , Microsoft.VisualStudio.Shell.Interop.OLEMSGICON.OLEMSGICON_WARNING) == System.Windows.Forms.DialogResult.Yes)
                    {
                        var bakFile = dataSourceTextBox.Text + ".bak";
                        var go = true; 
                        try
                        {
                            if (System.IO.File.Exists(bakFile))
                            {
                                if (EnvDteHelper.ShowMessageBox(string.Format("{0} already exists, do you wish to overwrite it?", bakFile)
                                        , Microsoft.VisualStudio.Shell.Interop.OLEMSGBUTTON.OLEMSGBUTTON_YESNO
                                        , Microsoft.VisualStudio.Shell.Interop.OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_SECOND
                                        , Microsoft.VisualStudio.Shell.Interop.OLEMSGICON.OLEMSGICON_WARNING) == System.Windows.Forms.DialogResult.Yes)
                                {
                                    System.IO.File.Delete(bakFile);
                                }
                                else
                                {
                                    go = false;
                                }
                            }
                            if (go)
                            {
                                System.IO.File.Copy(dataSourceTextBox.Text, dataSourceTextBox.Text + ".bak");
                                var helper = new SqlCeScripting.SqlCeHelper4();
                                helper.UpgradeTo40(_connectionString);
                                DialogResult = true;
                            }
                        }
                        catch (Exception ex2)
                        {
                            DataConnectionHelper.SendError(ex2, DbType, false);
                            return false;
                        }
                    }
                }
                else
                {
                    DataConnectionHelper.SendError(ex, DbType, false);
                    return false;
                }
            }
            return true;
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
                        _connectionString = _connectionString + string.Concat(";Password=", password.Text);
                txtConnection.Text = _connectionString;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (ShowDdexInfo)
            {
                txtDDEX.Visibility = Visibility.Visible;
            }
            if (CouldSupportPrivateProvider && !ShowDdexInfo)
            {
                txtDDEX.Visibility = Visibility.Visible;
                txtDDEX.Text = "Cannot save this connection for use with EF6 Tools, make sure the SQL Server Compact DbProvider is installed, and possibly restart Visual Studio";
                txtDDEX.Foreground = new SolidColorBrush(Colors.Red);
            }
            _filter = DataConnectionHelper.GetSqlCeFileFilter();
            dataSourceTextBox.Focus();
        }
    }
}