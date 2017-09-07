using System.Text;
using System.Windows;
using ErikEJ.SqlCeScripting;
using System;

namespace ErikEJ.SqlCeToolbox.ToolWindows
{
    /// <summary>
    /// Interaction logic for SubscriptionControl.xaml
    /// </summary>
    public partial class SubscriptionControl
    {
        private SubscriptionWindow _parentWindow;
        public DatabaseInfo DatabaseInfo { get; set; } //This property must be set by parent window
        public string Publication { get; set; }
        public bool IsNew { get; set; }
        private SqlCeReplicationHelper _replHelper;
        public SubscriptionControl()
        {
            InitializeComponent();
        }

        public SubscriptionControl(SubscriptionWindow parentWindow)
        {
            InitializeComponent();
            _parentWindow = parentWindow;
        }


        private void SubscriptionWindow_Loaded(object sender, RoutedEventArgs e)
        {
            PrepareUi();
        }

        private void PrepareUi()
        {
            if (IsNew)
            {
                btnSample.Visibility = Visibility.Hidden;
                txtSubscriber.Text = Environment.MachineName;
            }
            else
            {
                {
                    if (!string.IsNullOrWhiteSpace(Publication))
                    {
                        try
                        {
                            using (IRepository repository = Helpers.RepositoryHelper.CreateRepository(DatabaseInfo))
                            {
                                DateTime date = repository.GetLastSuccessfulSyncTime(Publication);
                                if (date != DateTime.MinValue)
                                {
                                    lblLastSync.Content = "Last sync: " + date;
                                }
                                ReplicationProperties props = SqlCeReplicationHelper.GetProperties(DatabaseInfo.ConnectionString, Publication);

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
                        catch (Exception ex)
                        {
                            Helpers.DataConnectionHelper.SendError(ex, DatabaseInfo.DatabaseType, false);
                        }
                    }
                }
            }
        }
        #region Form events

        private void btnSync_Click(object sender, RoutedEventArgs e)
        {
            BeginSynchronize(SqlCeReplicationHelper.ReinitializeOption.None);
        }

        private void btnSample_Click(object sender, RoutedEventArgs e)
        {
            txtStatus.Text = string.Empty;
            var package = _parentWindow.Package as SqlCeToolboxPackage;
            if (package == null) return;
            var dte = package.GetServiceHelper(typeof(EnvDTE.DTE)) as EnvDTE.DTE;

            var fileName = System.IO.Path.GetTempFileName();
            fileName = fileName + ".cs";
            try
            {
                CreateSampleFile(fileName);

                if (dte != null)
                {
                    dte.ItemOperations.OpenFile(fileName);
                    dte.ActiveDocument.Activate();
                }
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, DatabaseInfo.DatabaseType);
            }

        }

        private void btnReinitUpload_Click(object sender, RoutedEventArgs e)
        {
            BeginSynchronize(SqlCeReplicationHelper.ReinitializeOption.UploadSubscriberChanges);
        }

        private void btnReinitNoUpload_Click(object sender, RoutedEventArgs e)
        {
            BeginSynchronize(SqlCeReplicationHelper.ReinitializeOption.DiscardSubscriberChanges);
        }

        #endregion

        #region private methods
        private void BeginSynchronize(SqlCeReplicationHelper.ReinitializeOption option)
        {
            try
            {
                txtStatus.Text = string.Empty;
                btnSync.IsEnabled = false;

                bool useNt = comboBox1.SelectedIndex == 1;
//#if DEBUG
//                txtUrl.Text = "http://erik-pc/ssce35sync/sqlcesa35.dll";
//                txtPublisher.Text = "Erik-PC";
//                txtPublisherDatabase.Text = "PostCodes";
//                txtPublication.Text = "PubPostCodes";
//                txtSubscriber.Text = "ERIK-PC";
//#endif
                _replHelper = new SqlCeReplicationHelper(DatabaseInfo.ConnectionString, txtUrl.Text, txtPublisher.Text, txtPublisherDatabase.Text, txtPublication.Text, txtSubscriber.Text, txtHostname.Text, useNt, txtInternetUsername.Text, txtInternetPassword.Password, txtPublisherUsername.Text, txtPublisherPassword.Password, IsNew, option);

                _replHelper.Completed += replHelper_Completed;
                _replHelper.Progress += replHelper_Progress;
                _replHelper.Synchronize();
            }
            catch (Exception ex)
            {
                btnSync.IsEnabled = true;
                Helpers.DataConnectionHelper.SendError(ex, DatabaseInfo.DatabaseType, false);
            }
        }

        void replHelper_Progress(object sender, SqlCeReplicationHelper.SyncArgs ca)
        {
            if (!txtStatus.Dispatcher.CheckAccess())
            {
                txtStatus.Dispatcher.Invoke(
                  System.Windows.Threading.DispatcherPriority.Normal,
                  new Action(
                    delegate
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

        void replHelper_Completed(object sender, SqlCeReplicationHelper.SyncArgs ca)
        {
            if (!btnSync.Dispatcher.CheckAccess())
            {
                btnSync.Dispatcher.Invoke(
                  System.Windows.Threading.DispatcherPriority.Normal,
                  new Action(
                    delegate
                    {
                        PrepareUi();
                        btnSync.IsEnabled = true;
                    }
                ));
            }
            else
            {
                PrepareUi();
                btnSync.IsEnabled = true;
            }

            if (ca.Exception() != null)
            {
                if (!txtStatus.Dispatcher.CheckAccess())
                {
                    txtStatus.Dispatcher.Invoke(
                        System.Windows.Threading.DispatcherPriority.Normal,
                        new Action(
                        delegate
                        {
                            txtStatus.Text += Helpers.RepositoryHelper.CreateEngineHelper(DatabaseType.SQLCE35).FormatError(ca.Exception()) + Environment.NewLine;
                        }
                    ));
                }
                else
                {
                    txtStatus.Text += Helpers.RepositoryHelper.CreateEngineHelper(DatabaseType.SQLCE35).FormatError(ca.Exception()) + Environment.NewLine;
                }
            }
            else
            {
                IsNew = false;
            }
            if (!txtStatus.Dispatcher.CheckAccess())
            {
                txtStatus.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(
                    delegate
                    {
                        txtStatus.Text += ca.Message() + Environment.NewLine;
                    }
                ));
            }
            else
            {
                txtStatus.Text += ca.Message() + Environment.NewLine;
            }
            _replHelper.Dispose();
        }
        #endregion

        private void CreateSampleFile(string fileName)
        {
            string csharpCode = SqlCeToolbox.Resources.ClassTemplateCsharp;
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
            code.AppendFormat("\t\trepl.SubscriberConnectionString = @\"{0}\";{1}", DatabaseInfo.ConnectionString.Replace("\"", string.Empty), Environment.NewLine);

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