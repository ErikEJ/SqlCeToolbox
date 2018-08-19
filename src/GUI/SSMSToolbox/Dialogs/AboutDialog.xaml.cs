using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System;
using System.ComponentModel;
using ErikEJ.SqlCeToolbox.Helpers;
using ErikEJ.SqlCeScripting;

namespace ErikEJ.SqlCeToolbox.Dialogs
{
    public partial class AboutDialog
    {
        public AboutDialog()
        {
            InitializeComponent();
            Telemetry.TrackPageView(nameof(AboutDialog));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var bw = new BackgroundWorker();
            bw.DoWork += bw_DoWork;
            bw.RunWorkerCompleted += (s, ea) =>
            {
                Version.Text = "Version " + Assembly.GetExecutingAssembly().GetName().Version + " " + ea.Result.ToString();
            };
            bw.RunWorkerAsync();

            Background = VsThemes.GetWindowBackground();
            Version.Text = "Version " + Assembly.GetExecutingAssembly().GetName().Version;

            txtStatus.Text = "SQL Server Compact 4.0 in GAC - ";
            try
            {
                var version = new SqlCeHelper4().IsV40Installed();
                if (version != null)
                {
                    txtStatus.Text += string.Format("Yes - {0}\n", version);
                }
                else
                {
                    txtStatus.Text += "No\n";
                }
            }
            catch
            {
                txtStatus.Text += "No\n";
            }

            txtStatus.Text += "SQL Server Compact 4.0 DbProvider - ";
            try
            {
                System.Data.Common.DbProviderFactories.GetFactory(SqlCeToolbox.Resources.SqlCompact40InvariantName);
                txtStatus.Text += "Yes\n";
            }
            catch
            {
                txtStatus.Text += "No\n";
            }

            txtStatus.Text += "\nSQL Server Compact 4.0 DDEX provider - ";
            try
            {
                if (DataConnectionHelper.DdexProviderIsInstalled(new Guid(SqlCeToolbox.Resources.SqlCompact40Provider)))
                {
                    txtStatus.Text += "Yes\n";
                }
                else
                {
                    txtStatus.Text += "No\n";
                }
            }
            catch
            {
                txtStatus.Text += "No\n";
            }

            txtStatus.Text += "SQL Server Compact 4.0 Simple DDEX provider - ";
            try
            {
                if (DataConnectionHelper.DdexProviderIsInstalled(new Guid(SqlCeToolbox.Resources.SqlCompact40PrivateProvider)))
                {
                    txtStatus.Text += "Yes\n";
                }
                else
                {
                    txtStatus.Text += "No\n";
                }
            }
            catch
            {
                txtStatus.Text += "No\n";
            }

            txtStatus.Text += "\n\nSQL Server Compact 3.5 in GAC - ";
            try
            {
                var version = new SqlCeHelper().IsV35Installed();
                if (version != null)
                {
                    txtStatus.Text += string.Format("Yes - {0}\n", version);
                }
                else
                {
                    txtStatus.Text += "No\n";
                }
            }
            catch
            {
                txtStatus.Text += "No\n";
            }

            txtStatus.Text += "SQL Server Compact 3.5 DbProvider - ";
            try
            {
                System.Data.Common.DbProviderFactories.GetFactory(SqlCeToolbox.Resources.SqlCompact35InvariantName);
                txtStatus.Text += "Yes\n";
            }
            catch
            {
                txtStatus.Text += "No\n";
            }

            txtStatus.Text += "\nSQL Server Compact 3.5 DDEX provider - ";
            try
            {
                if (DataConnectionHelper.DdexProviderIsInstalled(new Guid(SqlCeToolbox.Resources.SqlCompact35Provider)))
                {
                    txtStatus.Text += "Yes\n";
                }
                else
                {
                    txtStatus.Text += "No\n";
                }
            }
            catch
            {
                txtStatus.Text += "No\n";
            }

            txtStatus.Text += "\n\nSync Framework 2.1 SqlCe 3.5 provider - ";
            if (DataConnectionHelper.IsSyncFx21Installed())
            {
                txtStatus.Text += "Yes\n";
            }
            else
            {
                txtStatus.Text += "No\n";
            }

            txtStatus.Text += "\n\nSQLite ADO.NET Provider included: ";
            try
            {
                Assembly asm = Assembly.Load("System.Data.SQLite");
                txtStatus.Text += string.Format("{0}\n", asm.GetName().Version);
            }
            catch
            {
                txtStatus.Text += "No\n";
            }

            txtStatus.Text += "SQLite EF6 DbProvider in GAC - ";
            try
            {
                if (DataConnectionHelper.IsSqLiteDbProviderInstalled())
                {
                    txtStatus.Text += "Yes\n";
                }
                else
                {
                    txtStatus.Text += "No\n";
                }
            }
            catch
            {
                txtStatus.Text += "No\n";
            }
        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = DataConnectionHelper.GetDownloadCount();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            //Helpers.DataConnectionHelper.LogError(new Exception(Guid.NewGuid().ToString()));
            Close();
        }

        private void CodeplexLink_Click(object sender, RoutedEventArgs e)
        {
            EnvDteHelper.LaunchUrl("https://github.com/ErikEJ/SqlCeToolbox");
        }

        private void GalleryLink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://marketplace.visualstudio.com/items?itemName=ErikEJ.SQLServerCompactSQLiteToolboxforSSMS#review-details");
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var text = txtStatus.Text.Replace("\n", Environment.NewLine);
            Clipboard.SetText(Version.Text + Environment.NewLine + Environment.NewLine + text);
            MessageBox.Show("About info copied to clipboard");
        }
    }
}