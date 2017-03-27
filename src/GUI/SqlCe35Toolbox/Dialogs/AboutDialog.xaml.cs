using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System;
using System.ComponentModel;
using ErikEJ.SqlCeToolbox.Helpers;
using System.IO;

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
            if (SqlCeToolboxPackage.VisualStudioVersion < new Version(11, 0))
            {
                DDEXButton.Visibility = Visibility.Collapsed;
            }
            BackgroundWorker bw = new BackgroundWorker();
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
                var asm4 = Assembly.Load("System.Data.SqlServerCe, Version=4.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91");
                if (asm4 != null && asm4.Location != null)
                {
                    var fvi = FileVersionInfo.GetVersionInfo(asm4.Location); 
                    string version = fvi.FileVersion;
                    txtStatus.Text += string.Format("Yes - {0}\n", version);
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
            var sqlce35Ver = new Version(0,0,0);
            try
            {
                var asm35 = Assembly.Load("System.Data.SqlServerCe, Version=3.5.1.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91");
                if (asm35 != null && asm35.Location != null)
                {
                    var fvi = FileVersionInfo.GetVersionInfo(asm35.Location);
                    sqlce35Ver = new Version(fvi.FileVersion);
                }
                txtStatus.Text += string.Format("Yes - {0}\n", sqlce35Ver);
            }
            catch
            {
                txtStatus.Text += "No\n";
            }
            if (sqlce35Ver > new Version(0,0,0) && sqlce35Ver < new Version(3, 5, 8080))
            {
                txtStatus.Text += "(Too old version installed, update to 3.5 SP2)\n";
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

            txtStatus.Text += "SQL Server Compact 3.5 Simple DDEX provider - ";
            try
            {
                if (DataConnectionHelper.DdexProviderIsInstalled(new Guid(SqlCeToolbox.Resources.SqlCompact35PrivateProvider)))
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

            try
            {
                //if (File.Exists(tempFile40))
                //{
                //    File.Delete(tempFile40);
                //}
                //if (File.Exists(tempFile35))
                //{
                //    File.Delete(tempFile35);
                //}
            }
            catch
            {
                // ignored
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

        private void DDEXButton_Click(object sender, RoutedEventArgs e)
        {
            DataConnectionHelper.RegisterDdexProviders();
            EnvDteHelper.ShowMessage("Providers registered, you may have to restart Visual Studio");
        }

        private void GalleryLink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://marketplace.visualstudio.com/items?itemName=ErikEJ.SQLServerCompactSQLiteToolbox#review-details");
        }
    }
}