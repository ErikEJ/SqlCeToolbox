using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ErikEJ.SqlCeToolbox.ToolWindows;
using SqlCeToolboxExe;
using ErikEJ.SqlCeToolbox.Helpers;
using ErikEJ.SqlCeScripting;
using ErikEJ.SqlCeToolbox.Dialogs;
using ErikEJ.SqlCeToolbox;
using System.Reflection;

namespace SqlCeToolbox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        

        private bool _loaded;
        private ExplorerControl _explorerControl;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_loaded)
            {
                DataConnectionHelper.Monitor = EQATEC.Analytics.Monitor.AnalyticsMonitorFactory.CreateMonitor("C244D8923C7C4235A1A24AB1127BD521");
                DataConnectionHelper.Monitor.Start();

                ExtractDll("QuickGraph.dll");
                ExtractDll("QuickGraph.Data.dll");
                
                if (DataConnectionHelper.Argument != null)
                {
                    string filePath = DataConnectionHelper.Argument.ToLowerInvariant();
                    if (System.IO.File.Exists(filePath))
                    {
                        var connStr = string.Format("Data Source={0};Max Database Size=4091", filePath);
                        var databaseList = DataConnectionHelper.GetDataConnections();
                        var item = databaseList.Where(d => d.Value.StartsWith(connStr)).FirstOrDefault();
                        if (item.Value == null)
                        {
                            try
                            {
                                TrySave(connStr);
                            }
                            catch (Exception ex)
                            {
                                string error = DataConnectionHelper.ShowErrors(ex);
                                if (error.Contains("Minor Err.: 25028"))
                                {
                                    PasswordDialog pwd = new PasswordDialog();
                                    pwd.ShowDialog();
                                    if (pwd.DialogResult.HasValue && pwd.DialogResult.Value == true && !string.IsNullOrWhiteSpace(pwd.Password))
                                    {   
                                        connStr = connStr + ";Password=" + pwd.Password;
                                    }
                                    try
                                    {
                                        TrySave(connStr);
                                    }
                                    catch (Exception ex1)
                                    {
                                        MessageBox.Show(DataConnectionHelper.ShowErrors(ex1));
                                    }
                                }
                                else
                                {
                                    MessageBox.Show(DataConnectionHelper.ShowErrors(ex));
                                }
                            }
                        }
                    }
                }
                this.Title = "SQL Server Compact Toolbox for runtime " + RepoHelper.apiVer;
                _explorerControl = new ExplorerControl(fabTab);
                MainGrid.Children.Add(_explorerControl);
            }
            _loaded = true;
        }

        private static void ExtractDll(string dllName)
        {
            var fileName = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), dllName);
            if (!System.IO.File.Exists(fileName))
            {
                using (System.IO.Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ErikEJ.SqlCeToolbox." + dllName))
                {
                    using (System.IO.FileStream fileStream = System.IO.File.Create(fileName, (int)stream.Length))
                    {
                        byte[] bytesInStream = new byte[stream.Length];
                        stream.Read(bytesInStream, 0, (int)bytesInStream.Length);
                        fileStream.Write(bytesInStream, 0, bytesInStream.Length);
                    }
                }
            }
        }

        private static void TrySave(string connStr)
        {
            using (var _repository = RepoHelper.CreateRepository(connStr))
            {
            }
            DataConnectionHelper.SaveDataConnection(connStr);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.F8)
                _explorerControl.FocusSelectedItem();

            base.OnKeyDown(e);
        }
    }
}
