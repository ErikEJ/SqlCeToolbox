using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Input;
using Microsoft.VisualStudio.PlatformUI;
using System.Windows.Media;
using ErikEJ.SqlCeToolbox.Helpers;

namespace ErikEJ.SqlCeToolbox.Dialogs
{
    /// <summary>
    /// Interaction logic for ImportDialog.xaml
    /// </summary>
    public partial class ConnectionDialog : DialogWindow
    {
        private bool _loading;

        private bool createDb;

        public ConnectionDialog()
        {
            Telemetry.TrackPageView(nameof(ConnectionDialog));
            _loading = true;
            InitializeComponent();
            this.Background = Helpers.VSThemes.GetWindowBackground();
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

        public DatabaseType DbType { get; set; }

        public bool CouldSupportPrivateProvider { get; set; }

        public bool ShowDDEXInfo { get; set; }

        public string InitialPath { get; set; }

        private string _connectionString;
        private string filter;
        #endregion

        #region Event Handlers
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
            sfd.Filter = filter;
            sfd.ValidateNames = true;
            sfd.Title = "Create new Database File";
            if (!string.IsNullOrEmpty(InitialPath))
            {
                sfd.InitialDirectory = InitialPath;
            }
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
            ofd.Filter = filter;
            ofd.CheckFileExists = true;
            ofd.Multiselect = false;
            ofd.ValidateNames = true;
            ofd.Title = "Select Database File";
            if (!string.IsNullOrEmpty(InitialPath))
            {
                ofd.InitialDirectory = InitialPath;
            }
            if (ofd.ShowDialog() == true)
            {
                createDb = false;
                this.dataSourceTextBox.Text = ofd.FileName;
            }
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
                        var engineHelper = Helpers.DataConnectionHelper.CreateEngineHelper(DbType);
                        engineHelper.CreateDatabase(_connectionString);
                    }
                }

                using (Helpers.DataConnectionHelper.CreateRepository(new DatabaseInfo { ConnectionString = _connectionString, DatabaseType = DbType }))
                {
                    if (showMessage)
                    {
                        EnvDTEHelper.ShowMessage("Connection OK!");
                    }
                    else
                    {
                        this.DialogResult = true;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Please upgrade using SqlCeEngine.Upgrade() method") && DbType == DatabaseType.SQLCE40)
                {
                    if (EnvDTEHelper.ShowMessageBox("This database file is from an earlier version,\n\rwould you like to Upgrade it?\n\r(A copy of the original file will be named .bak)"
                        , Microsoft.VisualStudio.Shell.Interop.OLEMSGBUTTON.OLEMSGBUTTON_YESNO
                        , Microsoft.VisualStudio.Shell.Interop.OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_SECOND
                        , Microsoft.VisualStudio.Shell.Interop.OLEMSGICON.OLEMSGICON_WARNING) == System.Windows.Forms.DialogResult.Yes)
                    {
                        string bakFile = dataSourceTextBox.Text + ".bak";
                        bool go = true; 
                        try
                        {
                            if (System.IO.File.Exists(bakFile))
                            {
                                if (EnvDTEHelper.ShowMessageBox(string.Format("{0} already exists, do you wish to overwrite it?", bakFile)
                                        , Microsoft.VisualStudio.Shell.Interop.OLEMSGBUTTON.OLEMSGBUTTON_YESNO
                                        , Microsoft.VisualStudio.Shell.Interop.OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_SECOND
                                        , Microsoft.VisualStudio.Shell.Interop.OLEMSGICON.OLEMSGICON_WARNING) == System.Windows.Forms.DialogResult.Yes)
                                {
                                    System.IO.File.Delete(bakFile);
                                    go = true;
                                }
                                else
                                {
                                    go = false;
                                }
                            }
                            if (go)
                            {
                                System.IO.File.Copy(dataSourceTextBox.Text, dataSourceTextBox.Text + ".bak");
                                SqlCeScripting.SqlCeHelper4 helper = new SqlCeScripting.SqlCeHelper4();
                                helper.UpgradeTo40(_connectionString);
                                this.DialogResult = true;
                            }
                        }
                        catch (Exception ex2)
                        {
                            Helpers.DataConnectionHelper.SendError(ex2, DbType, false);
                        }

                    }
                }
                else
                {
                    Helpers.DataConnectionHelper.SendError(ex, DbType, false);
                }
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
                        _connectionString = _connectionString + string.Concat(";Password=", password.Text);
                txtConnection.Text = _connectionString;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (ShowDDEXInfo)
            {
                txtDDEX.Visibility = System.Windows.Visibility.Visible;
            }
            if (CouldSupportPrivateProvider && !ShowDDEXInfo)
            {
                txtDDEX.Visibility = System.Windows.Visibility.Visible;
                txtDDEX.Text = "Cannot save this connection for use with EF6 Tools, make sure the SQL Server Compact DbProvider is installed, and possibly restart Visual Studio";
                txtDDEX.Foreground = new SolidColorBrush(Colors.Red);
            }
            filter = DataConnectionHelper.GetSqlCeFileFilter();
            this.dataSourceTextBox.Focus();
        }

    }
}