using ErikEJ.SqlCeToolbox;
using ErikEJ.SqlCeToolbox.Dialogs;
using ErikEJ.SqlCeToolbox.Helpers;
using ErikEJ.SqlCeToolbox.ToolWindows;

using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

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
                Telemetry.Initialize(Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                    "d4881a82-2247-42c9-9272-f7bc8aa29315");

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
