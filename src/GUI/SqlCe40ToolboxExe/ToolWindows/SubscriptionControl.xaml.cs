using System;
using System.Data.SqlServerCe;
using System.Text;
using System.Windows;
using ErikEJ.SqlCeScripting;

namespace ErikEJ.SqlCeToolbox.ToolWindows
{
    /// <summary>
    /// Interaction logic for SubscriptionControl.xaml
    /// </summary>
    public partial class SubscriptionControl
    {        
        public string Database { get; set; } //This property must be set by parent window
        public string Publication { get; set; }
        public bool IsNew { get; set; }
        private SqlCeReplicationHelper replHelper;

        public SubscriptionControl()
        {
            InitializeComponent();
        }

        private void SubscriptionWindow_Loaded(object sender, RoutedEventArgs e)
        {
            PrepareUI();
        }

        private void PrepareUI()
        {
            if (this.IsNew)
            {
                btnSample.Visibility = System.Windows.Visibility.Hidden;
                txtSubscriber.Text = Environment.MachineName;
            }
            else
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(Publication))
                    {
                        using (IRepository repository = new DBRepository(Database))
                        {
                            DateTime date = repository.GetLastSuccessfulSyncTime(Publication);
                            if (date != DateTime.MinValue)
                            {
                                lblLastSync.Content = "Last sync: " + date.ToString();
                            }
                            ReplicationProperties props = SqlCeReplicationHelper.GetProperties(Database, Publication);

                            txtUrl.Text = props.InternetUrl;
                            txtPublisher.Text = props.Publisher;
                            txtPublisherDatabase.Text = props.PublisherDatabase;
                            txtPublisherUsername.Text = props.PublisherLogin;
                            txtPublication.Text = props.Publication;
                            txtSubscriber.Text = props.Subscriber;
                            txtHostname.Text = props.HostName;
                            txtInternetUsername.Text = props.InternetLogin;
                            if (props.UseNT)
                            {
                                comboBox1.SelectedIndex = 1;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Helpers.DataConnectionHelper.ShowErrors(ex));
                }
            }
        }

        #region Form events
        private void btnSync_Click(object sender, RoutedEventArgs e)
        {
            BeginSynchronize(SqlCeReplicationHelper.ReinitializeOption.None);
        }

        private void btnReinitUpload_Click(object sender, RoutedEventArgs e)
        {
            BeginSynchronize(SqlCeReplicationHelper.ReinitializeOption.UploadSubscriberChanges);
        }

        private void btnReinitNoUpload_Click(object sender, RoutedEventArgs e)
        {
            BeginSynchronize(SqlCeReplicationHelper.ReinitializeOption.DiscardSubscriberChanges);
        }

        private void btnSample_Click(object sender, RoutedEventArgs e)
        {
            txtStatus.Text = string.Empty;
            
            var fileName = System.IO.Path.GetTempFileName();
            fileName = fileName + ".cs";
            try
            {
                CreateSampleFile(fileName);
                System.Diagnostics.Process.Start(fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Helpers.DataConnectionHelper.ShowErrors(ex));
            }
        }
        #endregion

        #region private methods
        private void BeginSynchronize(SqlCeReplicationHelper.ReinitializeOption option)
        {
            try
            {
                txtStatus.Text = string.Empty;
                btnSync.IsEnabled = false;

                bool useNT = false;
                if (comboBox1.SelectedIndex == 1)
                    useNT = true;

                //txtUrl.Text = "http://erik-pc/ssce35sync/sqlcesa35.dll";
                //txtPublisher.Text = "Erik-PC\\SQL2008R2";
                //txtPublisherDatabase.Text = "PostCodes";
                //txtPublication.Text = "PubPostCodes";
                //txtSubscriber.Text = "ERIK-PC";

                replHelper = new SqlCeReplicationHelper(this.Database, txtUrl.Text, txtPublisher.Text, txtPublisherDatabase.Text, txtPublication.Text, txtSubscriber.Text, txtHostname.Text, useNT, txtInternetUsername.Text, txtInternetPassword.Password, txtPublisherUsername.Text, txtPublisherPassword.Password, IsNew, option);
                replHelper.Completed += new SqlCeReplicationHelper.CompletedHandler(replHelper_Completed);
                replHelper.Progress += new SqlCeReplicationHelper.ProgressHandler(replHelper_Progress);
                replHelper.Synchronize();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Helpers.DataConnectionHelper.ShowErrors(ex));
            }
        }


        void replHelper_Progress(object sender, ErikEJ.SqlCeScripting.SqlCeReplicationHelper.SyncArgs ca)
        {
            if (!txtStatus.Dispatcher.CheckAccess())
            {
                txtStatus.Dispatcher.Invoke(
                  System.Windows.Threading.DispatcherPriority.Normal,
                  new Action(
                    delegate()
                    {
                        txtStatus.Text += ca.Message() + Environment.NewLine;
                    }
                ));
            }
            else
            {
                txtStatus.Text += ca.Message() + Environment.NewLine;
            }
        }

        void replHelper_Completed(object sender, ErikEJ.SqlCeScripting.SqlCeReplicationHelper.SyncArgs ca)
        {
            if (!btnSync.Dispatcher.CheckAccess())
            {
                btnSync.Dispatcher.Invoke(
                  System.Windows.Threading.DispatcherPriority.Normal,
                  new Action(
                    delegate()
                    {
                        btnSync.IsEnabled = true;
                    }
                ));
            }
            else
            {
                btnSync.IsEnabled = true;
            }

            if (ca.Exception() != null)
            {
                if (!txtStatus.Dispatcher.CheckAccess())
                {
                    txtStatus.Dispatcher.Invoke(
                        System.Windows.Threading.DispatcherPriority.Normal,
                        new Action(
                        delegate()
                        {
                            txtStatus.Text += Helpers.DataConnectionHelper.ShowErrors(ca.Exception()) + Environment.NewLine;
                        }
                    ));
                }
                else
                {
                    txtStatus.Text += Helpers.DataConnectionHelper.ShowErrors(ca.Exception()) + Environment.NewLine;
                }
            }
            else
            {
                this.IsNew = false;
            }
            if (!txtStatus.Dispatcher.CheckAccess())
            {
                txtStatus.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(
                    delegate()
                    {
                        txtStatus.Text += ca.Message() + Environment.NewLine;
                    }
                ));
            }
            else
            {
                txtStatus.Text += ca.Message() + Environment.NewLine;
            }                    
        }
        #endregion

        private void CreateSampleFile(string fileName)
        {
            string csharpCode = ErikEJ.SqlCeToolbox.Resources.ClassTemplateCsharp;
            StringBuilder code = new StringBuilder();

            code.AppendFormat("\t\trepl.InternetUrl = @\"{0}\";{1}", txtUrl.Text, Environment.NewLine);
            code.AppendFormat("\t\trepl.Publisher = @\"{0}\";{1}", txtPublisher.Text, Environment.NewLine);
            code.AppendFormat("\t\trepl.PublisherDatabase = @\"{0}\";{1}", txtPublisherDatabase.Text, Environment.NewLine);
            code.AppendFormat("\t\trepl.Publication = @\"{0}\";{1}", txtPublication.Text, Environment.NewLine);
            string secMode = "DBAuthentication";
            if (comboBox1.SelectedIndex == 1)
                secMode = "NTAuthentication";
            code.AppendFormat("\t\trepl.PublisherSecurityMode = SecurityType.{0};{1}", secMode, Environment.NewLine);
            code.AppendFormat("\t\trepl.PublisherLogin = @\"{0}\";{1}", txtPublisherUsername.Text, Environment.NewLine);
            code.AppendFormat("\t\trepl.PublisherPassword = @\"{0}\";{1}", "<...>", Environment.NewLine);
            code.AppendFormat("\t\trepl.Subscriber = @\"{0}\";{1}", txtSubscriber.Text, Environment.NewLine);
            code.AppendFormat("\t\trepl.SubscriberConnectionString = @\"{0}\";{1}", Database.Replace("\"", string.Empty), Environment.NewLine);

            if (!string.IsNullOrWhiteSpace(txtInternetUsername.Text))
            {
                code.AppendFormat("\t\trepl.InternetLogin = @\"{0}\";{1}", txtInternetUsername.Text, Environment.NewLine);
                code.AppendFormat("\t\trepl.InternetPassword = @\"{0}\";{1}", "<...>", Environment.NewLine);
            }
            if (!string.IsNullOrWhiteSpace(txtHostname.Text))
            {
                code.AppendFormat("\t\trepl.HostName = @\"{0}\";{1}", txtHostname.Text, Environment.NewLine);
            }

            csharpCode = csharpCode.Replace("#ReplParams#", code.ToString());
            System.IO.File.WriteAllText(fileName, csharpCode);
        }


    }
}