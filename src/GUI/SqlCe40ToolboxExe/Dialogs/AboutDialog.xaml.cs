using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System;
using System.Windows.Input;

namespace ErikEJ.SqlCeToolbox.Dialogs
{
    /// <summary>
    /// Interaction logic for CreateLoginDialog.xaml
    /// </summary>
    public partial class AboutDialog : Window
    {
        public AboutDialog()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Version.Text = "Version " + Assembly.GetExecutingAssembly().GetName().Version;
            txtStatus.Text = string.Format("SQL Server Compact {0} in GAC - ", RepoHelper.apiVer);
            try
            {
#if V35
                Assembly asm4 = System.Reflection.Assembly.Load("System.Data.SqlServerCe, Version=3.5.1.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91");
#else
                Assembly asm4 = System.Reflection.Assembly.Load("System.Data.SqlServerCe, Version=4.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91");
#endif
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm4.Location); 
                string version = fvi.FileVersion;
                txtStatus.Text += string.Format("Yes - {0}\n", version.ToString());
            }
            catch (System.IO.FileNotFoundException)
            {
                txtStatus.Text += "No\n";
            }

            txtStatus.Text += string.Format("SQL Server Compact {0} DbProvider - ", RepoHelper.apiVer);
            try
            {
                var factory = System.Data.Common.DbProviderFactories.GetFactory(string.Format("System.Data.SqlServerCe.{0}", RepoHelper.apiVer));
                txtStatus.Text += "Yes\n";
            }
            catch (System.Configuration.ConfigurationException)
            {
                txtStatus.Text += "No\n";
            }

            catch (ArgumentException)
            {
                txtStatus.Text += "No\n";
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CodeplexLink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://sqlcetoolbox.codeplex.com");
        }

        private void AboutDialog_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}