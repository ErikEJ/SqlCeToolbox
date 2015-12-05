using System;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Input;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.Win32;
using ErikEJ.SqlCeToolbox.Helpers;

namespace ErikEJ.SqlCeToolbox.Dialogs
{
    public partial class SQLiteConnectionDialog : DialogWindow
    {
        private SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder();

        private bool createDb;

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
            Props.SelectedObject = builder;
        }

        private void btn_Click(object sender, RoutedEventArgs e)
        {
            TestConnection(true);
        }

        private void TestConnection(bool showMessage)
        {
            if (string.IsNullOrWhiteSpace(builder.DataSource))
            {
                return;
            }
            try
            {
                if (createDb)
                {
                    if (!System.IO.File.Exists(builder.DataSource))
                    {
                        var engineHelper = Helpers.DataConnectionHelper.CreateEngineHelper(DatabaseType.SQLite);
                        engineHelper.CreateDatabase(builder.ConnectionString);
                    }
                }

                using (var conn = new SQLiteConnection(builder.ConnectionString))
                {
                    conn.Open();
                    this.ConnectionString = builder.ConnectionString;
                    if (showMessage)
                    {
                        EnvDTEHelper.ShowMessage("Test succeeded!");
                    }
                    else
                    {
                        this.DialogResult = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, DatabaseType.SQLite, false);
            }
        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            TestConnection(false);
            this.Close();
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
                createDb = true;
                this.builder.DataSource = sfd.FileName;
                txtDataSource.Text = sfd.FileName;
                Props.SelectedObject = builder;
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
                createDb = false;
                this.builder.DataSource = ofd.FileName;
                txtDataSource.Text = ofd.FileName;
                Props.SelectedObject = builder;
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
