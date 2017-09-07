using System;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using ErikEJ.SqlCeToolbox.Helpers;

namespace ErikEJ.SqlCeToolbox.Dialogs
{
    // ReSharper disable once InconsistentNaming
    public partial class SQLiteConnectionDialog
    {
        private readonly SQLiteConnectionStringBuilder _builder = new SQLiteConnectionStringBuilder();

        private bool _createDb;

        public string ConnectionString { get; set; }

        public string InitialPath { get; set; }

        public SQLiteConnectionDialog()
        {
            Telemetry.TrackPageView(nameof(SQLiteConnectionDialog));
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Props.ShowSummary = true;
            Props.SelectedObject = _builder;
        }

        private void btn_Click(object sender, RoutedEventArgs e)
        {
            TestConnection(true);
        }

        private void TestConnection(bool showMessage)
        {
            if (string.IsNullOrWhiteSpace(_builder.DataSource))
            {
                return;
            }
            try
            {
                if (_createDb)
                {
                    if (!System.IO.File.Exists(_builder.DataSource))
                    {
                        var engineHelper = Helpers.RepositoryHelper.CreateEngineHelper(DatabaseType.SQLite);
                        engineHelper.CreateDatabase(_builder.ConnectionString);
                    }
                }

                using (var conn = new SQLiteConnection(_builder.ConnectionString))
                {
                    conn.Open();
                    ConnectionString = _builder.ConnectionString;
                    if (showMessage)
                    {
                        EnvDteHelper.ShowMessage("Test succeeded!");
                    }
                    else
                    {
                        DialogResult = true;
                    }
                }
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, DatabaseType.SQLite, false);
            }
        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            TestConnection(false);
            Close();
        }

        private void create_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = DataConnectionHelper.GetSqliteFileFilter();
            sfd.ValidateNames = true;
            if (!string.IsNullOrEmpty(InitialPath))
            {
                sfd.InitialDirectory = InitialPath;
            }
            sfd.Title = "Create new Database File";
            if (sfd.ShowDialog() == true && !string.IsNullOrEmpty(sfd.FileName))
            {
                _createDb = true;
                _builder.DataSource = sfd.FileName;
                txtDataSource.Text = sfd.FileName;
                Props.SelectedObject = _builder;
            }
        }

        private void browse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = DataConnectionHelper.GetSqliteFileFilter();
            ofd.CheckFileExists = true;
            ofd.Multiselect = false;
            ofd.ValidateNames = true;
            if (!string.IsNullOrEmpty(InitialPath))
            {
                ofd.InitialDirectory = InitialPath;
            }
            ofd.Title = "Select Database File to Open";
            if (ofd.ShowDialog() == true && !string.IsNullOrEmpty(ofd.FileName))
            {
                _createDb = false;
                _builder.DataSource = ofd.FileName;
                txtDataSource.Text = ofd.FileName;
                Props.SelectedObject = _builder;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();

            base.OnKeyDown(e);
        }
    }
}
