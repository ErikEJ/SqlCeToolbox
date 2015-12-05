using System;
using System.Data.SqlServerCe;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace ErikEJ.SqlCeToolbox.Dialogs
{
    /// <summary>
    /// Interaction logic for ConnectionDialog.xaml
    /// </summary>
    public partial class ConnectionDialog : Window
    {
        private SqlCeConnectionStringBuilder builder = new SqlCeConnectionStringBuilder();

        private bool createDb;

        public string ConnectionString { get; set; }

        public ConnectionDialog()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Props.SelectedObject = builder;
        }

        private void btn_Click(object sender, RoutedEventArgs e)
        {
            TestConnection(true);
        }

        private void TestConnection(bool showMessage)
        {
            try
            {
                if (createDb)
                {
                    if (!System.IO.File.Exists(builder.DataSource))
                    {
                        using (var eng = new SqlCeEngine(builder.ConnectionString))
                        {
                            eng.CreateDatabase();
                        }
                    }
                }

                using (var conn = new SqlCeConnection(builder.ConnectionString))
                {
                    conn.Open();
                    this.ConnectionString = builder.ConnectionString;
                    if (showMessage)
                    {
                        MessageBox.Show("Test succeeded!");
                    }
                    else
                    {
                        this.DialogResult = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Helpers.DataConnectionHelper.ShowErrors(ex));
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
            sfd.Filter = "SQL Server Compact Database (*.sdf)|*.sdf|All Files(*.*)|*.*";
            sfd.ValidateNames = true;
            sfd.Title = "Create new Database File";
            if (sfd.ShowDialog() == true)
            {
                createDb = true;
                this.builder.DataSource = sfd.FileName;
                Props.SelectedObject = builder;
            }
        }

        private void browse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "SQL Server Compact Database (*.sdf)|*.sdf|All Files(*.*)|*.*";
            ofd.CheckFileExists = true;
            ofd.Multiselect = false;
            ofd.ValidateNames = true;
            ofd.Title = "Select Database File to Open";
            if (ofd.ShowDialog() == true)
            {
                createDb = false;
                this.builder.DataSource = ofd.FileName;
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
