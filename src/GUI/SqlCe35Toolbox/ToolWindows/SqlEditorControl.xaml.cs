using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using ErikEJ.SqlCeToolbox.Dialogs;
using ErikEJ.SqlCeToolbox.Helpers;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Search;
using Microsoft.Win32;

namespace ErikEJ.SqlCeToolbox.ToolWindows
{
    /// <summary>
    /// Interaction logic for SqlEditorControl.xaml
    /// </summary>
    public partial class SqlEditorControl
    {
        public DatabaseInfo DatabaseInfo 
        {
            get
            {
                return _dbInfo;
            }
            set
            {
                if (value != null)
                {
                    _dbInfo = value;
                    _parentWindow.Caption = _dbInfo.Caption;
                }
            }
        } 
        //This property must be set by parent window
        private SqlEditorWindow _parentWindow;
        private DatabaseInfo _dbInfo;
        private string _savedFileName;
        private FontFamily fontFamiliy = new System.Windows.Media.FontFamily("Consolas");
        private double fontSize = 14;
        private EnvDTE.DTE dte = null;
        private bool ignoreDdlErrors;
        private bool showResultInGrid;
        private bool showBinaryValuesInResult;
        private bool showNullValuesAsNULL;
        private bool useClassicGrid;

        public SqlEditorControl(SqlEditorWindow parentWindow)
        {
            InitializeComponent();
            _parentWindow = parentWindow;
        }

        //TODO For intellisense
        //public List<string> TableNames { get; set; }
        //public List<Column> Columns { get; set; }

        public ExplorerControl ExplorerControl { get; set; }

        public string SqlText
        {
            get
            {
                return SqlTextBox.Text;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (value.Length > 10000)
                        SqlTextBox.SyntaxHighlighting = null;
                    SqlTextBox.Text = value;
                    isDirty = false;
                    if (value.Length <= 10000 && SqlTextBox.SyntaxHighlighting == null)
                        LoadHighlighter();                    
                    this.Resultspanel.Children.Clear();
                }
                else
                {
                    SqlTextBox.Clear();
                }
            }
        }

        private bool isDirty;

        public bool IsDirty
        {
            get
            {
                return isDirty;
            }
            set
            {
                isDirty = value;
                if (value == true)
                {
                    _parentWindow.Caption = _dbInfo.Caption + "*";
                }
                else
                {
                    _parentWindow.Caption = _dbInfo.Caption;
                }
                if (!string.IsNullOrEmpty(_savedFileName))
                {
                    _parentWindow.Caption = Path.GetFileName(_savedFileName) + " - " + _parentWindow.Caption;
                }
            }
        }

        private void SqlEditorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var overflowGrid = toolBar1.Template.FindName("OverflowGrid", toolBar1) as FrameworkElement;
                if (overflowGrid != null)
                {
                    overflowGrid.Visibility = Visibility.Collapsed;
                }
                var package = _parentWindow.Package as SqlCeToolboxPackage;
                if (package == null) return;
                dte = package.GetServiceHelper(typeof(EnvDTE.DTE)) as EnvDTE.DTE;

                toolBar1.Background = toolTray.Background = Helpers.VSThemes.GetCommandBackground();
                dock1.Background = Helpers.VSThemes.GetWindowBackground();
                sep4.Background = Helpers.VSThemes.GetToolbarSeparatorBackground();
                txtSaveAs.Foreground = Helpers.VSThemes.GetWindowText();
                if (this.DatabaseInfo != null)
                    this.txtVersion.Text = this.DatabaseInfo.ServerVersion;
                LoadDefaultOptions();
                ConfigureOptions();
                LoadHighlighter();
                SqlTextBox.TextChanged += new EventHandler(SqlTextBox_TextChanged);
                //TODO Entry point for Intellisense
                //SqlTextBox.TextArea.TextEntering += SqlTextBox_TextArea_TextEntering;
                //SqlTextBox.TextArea.TextEntered += SqlTextBox_TextArea_TextEntered;

                SqlTextBox.Focus();
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, DatabaseInfo.DatabaseType, true);
            }
        }

        private void LoadDefaultOptions()
        {
            showResultInGrid = Properties.Settings.Default.ShowResultInGrid;
            showBinaryValuesInResult = Properties.Settings.Default.ShowBinaryValuesInResult;
            showNullValuesAsNULL = Properties.Settings.Default.ShowNullValuesAsNULL;
            useClassicGrid = Properties.Settings.Default.UseClassicGrid;
        }

        private List<CheckListItem> items = new List<CheckListItem>();

        private void ConfigureOptions()
        {
            items.Clear();

            items.Add(new CheckListItem
                {
                    IsChecked = showResultInGrid,
                    Label = "Show Result in Grid",
                    Tag = "ShowResultInGrid"
                });            
            items.Add(new CheckListItem
                {
                    IsChecked = showBinaryValuesInResult,
                    Label = "Show Binary Values in Result",
                    Tag = "ShowBinaryValuesInResult"
                });

            items.Add(new CheckListItem
            {
                IsChecked = showNullValuesAsNULL,
                Label = "Show null Values as NULL",
                Tag = "ShowNullValuesAsNULL"
            });
            items.Add(new CheckListItem
            {
                IsChecked = ignoreDdlErrors,
                Label = "Ignore DDL Errors",
                Tag = "ignoreDdlErrors"
            });
            items.Add(new CheckListItem
            {
                IsChecked = useClassicGrid,
                Label = "Use classic (plain) grid",
                Tag = "useClassicGrid"
            });
            chkOptions.ItemsSource = null;
            chkOptions.ItemsSource = items;
        }

        private void chkOptions_ItemSelectionChanged(object sender, Xceed.Wpf.Toolkit.Primitives.ItemSelectionChangedEventArgs e)
        {
            var item = e.Item as CheckListItem;
            if (item != null)
            {
                switch (item.Tag)
                {
                    case "ignoreDdlErrors":
                        ignoreDdlErrors = item.IsChecked;
                        break;
                    case "ShowResultInGrid":
                        showResultInGrid = item.IsChecked;
                        break;
                    case "ShowBinaryValuesInResult":
                        showBinaryValuesInResult = item.IsChecked;
                        break;
                    case "ShowNullValuesAsNULL":
                        showNullValuesAsNULL = item.IsChecked;
                        break;
                    case "useClassicGrid":
                        useClassicGrid = item.IsChecked;
                        break;
                }
            }
        }

        private void ddButton_Click(object sender, RoutedEventArgs e)
        {
            ConfigureOptions();
        }

        void SqlTextBox_TextChanged(object sender, EventArgs e)
        {
            IsDirty = true;
        }

        private void LoadHighlighter()
        {
            MemoryStream ms = null;
            try
            {
                ms = new MemoryStream(SqlCeToolbox.Resources.SqlCeSyntax);
                SqlTextBox.SyntaxHighlighting = HighlightingLoader.Load(new XmlTextReader(ms),
                HighlightingManager.Instance);
            }
            finally
            {
                if (ms != null)
                    ms.Dispose();
            }
        }

        //CompletionWindow completionWindow;
        //void SqlTextBox_TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        //{
        //    var line = SqlTextBox.Document.GetLineByOffset(SqlTextBox.CaretOffset);
        //    if (line == null)
        //        return;
        //    var segment = new TextSegment { StartOffset = line.Offset, EndOffset = SqlTextBox.CaretOffset };
        //    if (segment == null)
        //        return;
        //    string text = SqlTextBox.Document.GetText(segment);
        //    if (string.IsNullOrWhiteSpace(text))
        //        return;
        //    //if (e.Text.Length > 0 && char.IsLetterOrDigit(e.Text[0]))
        //    if (e.Text.Length > 0)
        //    {
        //        if (text.ToLowerInvariant().EndsWith("from " + e.Text.ToLowerInvariant()))
        //        {
        //            // Open code completion after the user has pressed dot:
        //            completionWindow = new CompletionWindow(SqlTextBox.TextArea);
        //            completionWindow.StartOffset = SqlTextBox.CaretOffset - 1;
        //            IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
        //            //foreach (var item in TableNames)
        //            //{
        //            //    data.Add(new MyCompletionData(item));    
        //            //}
        //            completionWindow.Show();
        //            completionWindow.Closed += delegate
        //            {
        //                completionWindow = null;
        //            };
        //        }
        //        else if (e.Text == ".")
        //        {
        //            // Open code completion after the user has pressed dot:
        //            completionWindow = new CompletionWindow(SqlTextBox.TextArea);                    
        //            IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
        //            //foreach (var item in TableNames)
        //            //{
        //            //    data.Add(new MyCompletionData(item));
        //            //}
        //            completionWindow.Show();
        //            completionWindow.Closed += delegate
        //            {
        //                completionWindow = null;
        //            };
        //        }

        //    }
        //}

        //void SqlTextBox_TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        //{
        //    if (e.Text.Length > 0 && completionWindow != null)
        //    {
        //        if (!char.IsLetterOrDigit(e.Text[0]))
        //        {
        //            // Whenever a non-letter is typed while the completion window is open,
        //            // insert the currently selected element.
        //            completionWindow.CompletionList.RequestInsertion(e);
        //        }
        //    }
        //    // Do not set e.Handled=true.
        //    // We still want to insert the character that was typed.
        //}


        ///// Implements AvalonEdit ICompletionData interface to provide the entries in the
        ///// completion drop down.
        //private class MyCompletionData : ICompletionData
        //{
        //    public MyCompletionData(string text)
        //    {
        //        this.Text = text;
        //    }

        //    public System.Windows.Media.ImageSource Image
        //    {
        //        get { return null; }
        //    }

        //    public string Text { get; private set; }

        //    // Use this property if you want to show a fancy UIElement in the list.
        //    public object Content
        //    {
        //        get { return this.Text; }
        //    }

        //    public object Description
        //    {
        //        get { return "Description for " + this.Text; }
        //    }

        //    public void Complete(TextArea textArea, ISegment completionSegment,
        //        EventArgs insertionRequestEventArgs)
        //    {
        //        textArea.Document.Replace(completionSegment, "[" + this.Text + "]");
        //    }

        //    public double Priority
        //    {
        //        get { return 0; }
        //    }
        //}

        private void SqlEditorControl_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (DatabaseInfo != null && DatabaseInfo.DatabaseType == DatabaseType.SQLite)
            {
                ParseButton.Visibility = Visibility.Collapsed;
                ExecuteWithPlanButton.Visibility = Visibility.Collapsed;
            }
        }

        #region Toolbar Button events

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            Helpers.DataConnectionHelper.LogUsage("EditorNew");
            OpenSqlEditorToolWindow();
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            Helpers.DataConnectionHelper.LogUsage("EditorOpen");
            OpenScript();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Helpers.DataConnectionHelper.LogUsage("EditorSave");
            SaveScript(false);
        }

        private void SaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            Helpers.DataConnectionHelper.LogUsage("EditorSave");
            SaveScript(true);
        }

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            ExecuteScript();
        }

        public void ExecuteScript()
        {
            if (string.IsNullOrWhiteSpace(SqlText))
                return;
            Helpers.DataConnectionHelper.LogUsage("EditorExecute");
            ExecuteSqlScriptInEditor();
        }

        public static RoutedCommand ExecuteCommand = new RoutedCommand();
        public void ExecutedExecuteCommand(object sender, ExecutedRoutedEventArgs e)
        {
            ExecuteScript();
        }

        private void ExecuteWithPlanButton_Click(object sender, RoutedEventArgs e)
        {
            Helpers.DataConnectionHelper.LogUsage("EditorExecuteWithPlan");
            if (string.IsNullOrWhiteSpace(SqlText))
                return;
            try
            {
                using (var repository = Helpers.DataConnectionHelper.CreateRepository(DatabaseInfo))
                {
                    var textBox = new TextBox();
                    textBox.FontFamily = fontFamiliy;
                    textBox.FontSize = fontSize;
                    string sql = GetSqlFromSqlEditorTextBox();
                    string showPlan = string.Empty;
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    var dataset = repository.ExecuteSql(sql, out showPlan);
                    sw.Stop();
                    FormatTime(sw);
                    if (dataset != null)
                    {
                        ParseDataSetResultsToResultsBox(dataset);
                    }
                    try
                    {
                        TryLaunchSqlplan(showPlan);
                    }
                    catch (System.ComponentModel.Win32Exception)
                    {
                        EnvDTEHelper.ShowError("This feature requires Visual Studio 2010 Premium / SQL Server Management Studio to be installed");
                    }
                    catch (Exception ex)
                    {
                        Helpers.DataConnectionHelper.SendError(ex, DatabaseType.SQLCE35);
                    }

                }
            }
            catch (Exception ex)
            {
                ParseSqlErrorToResultsBox(Helpers.DataConnectionHelper.CreateEngineHelper(DatabaseInfo.DatabaseType).FormatError(ex));
            }
        }

        private void TryLaunchSqlplan(string showPlan)
        {
            if (!string.IsNullOrWhiteSpace(showPlan))
            {
                if (DatabaseInfo.DatabaseType == DatabaseType.SQLite)
                {
                    var textBox = new TextBox();
                    textBox.FontFamily = fontFamiliy;
                    textBox.FontSize = fontSize;
                    textBox.Text = showPlan;
                    ClearResults();
                    this.Resultspanel.Children.Add(textBox);
                    this.tab1.Visibility = System.Windows.Visibility.Collapsed;
                    resultsTabControl.SelectedIndex = 1;
                }
                else
                {
                    var fileName = System.IO.Path.GetTempFileName();
                    fileName = fileName + ".sqlplan";
                    System.IO.File.WriteAllText(fileName, showPlan);
                    // If Data Dude is available
                    var pkg = _parentWindow.Package as SqlCeToolboxPackage;
                    if (pkg.VSSupportsSqlPlan())
                    {
                        dte.ItemOperations.OpenFile(fileName);
                        dte.ActiveDocument.Activate();
                    }
                    else
                    {
                        // Just try to start SSMS
                        using (RegistryKey rkRoot = Registry.ClassesRoot)
                        {
                            RegistryKey rkFileType = rkRoot.OpenSubKey(".sqlplan");
                            if (rkFileType != null)
                            {
                                System.Diagnostics.Process.Start(fileName);
                            }
                            else
                            {
                                EnvDTEHelper.ShowError("No application that can open .sqlplan files is installed, you could install SSMS 2012 SP1 Express");
                            }
                        }
                    }
                }
            }
        }

        private void ParseButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SqlText))
                return;
            Helpers.DataConnectionHelper.LogUsage("EditorParse");
            try
            {
                using (var repository = Helpers.DataConnectionHelper.CreateRepository(DatabaseInfo))
                {
                    var textBox = new TextBox();
                    textBox.FontFamily = fontFamiliy;
                    textBox.FontSize = fontSize;
                    string sql = GetSqlFromSqlEditorTextBox();
                    repository.ParseSql(sql);
                    textBox.Text = "Statement(s) in script parsed and seems OK!";
                    ClearResults();
                    this.Resultspanel.Children.Add(textBox);
                }
            }
            catch (Exception ex)
            {
                ParseSqlErrorToResultsBox(Helpers.DataConnectionHelper.CreateEngineHelper(DatabaseInfo.DatabaseType).FormatError(ex));
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchPanel sPanel = new SearchPanel();
            sPanel.Attach(SqlTextBox.TextArea);
        }

        private void ClearResults()
        {
            tab1.Visibility = System.Windows.Visibility.Visible;
            tab2.Visibility = System.Windows.Visibility.Visible;
            tab2.Header = "Messages";
            this.GridPanel.Children.Clear();
            this.Resultspanel.Children.Clear();
        }

        private void ShowPlanButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SqlText))
                return;
            Helpers.DataConnectionHelper.LogUsage("EditorShowPlan");
            try
            {
                using (var repository = Helpers.DataConnectionHelper.CreateRepository(DatabaseInfo))
                {
                    var textBox = new TextBox();
                    textBox.FontFamily = fontFamiliy;
                    textBox.FontSize = fontSize;
                    string sql = GetSqlFromSqlEditorTextBox();
                    string showPlan = repository.ParseSql(sql);
                    try
                    {
                        TryLaunchSqlplan(showPlan);
                    }
                    catch (System.ComponentModel.Win32Exception)
                    {
                        EnvDTEHelper.ShowError("This feature requires Visual Studio 2010 Premium / SQL Server Management Studio to be installed");
                    }
                    catch (Exception ex)
                    {
                        Helpers.DataConnectionHelper.SendError(ex, DatabaseType.SQLCE35);
                    }
                }
            }
            catch (Exception sqlException)
            {
                ParseSqlErrorToResultsBox(Helpers.DataConnectionHelper.CreateEngineHelper(DatabaseInfo.DatabaseType).FormatError(sqlException));
            }
        }

        #endregion

        private void FormatTime(Stopwatch sw)
        {
            var ts = new TimeSpan(sw.ElapsedTicks);
            this.txtTime.Text = string.Format("Duration: {0:00}:{1:00}.{2:000}", ts.Minutes, ts.Seconds, ts.Milliseconds);
        }

        public void OpenScript()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "SQL Server Compact Script (*.sqlce;*.sql)|*.sqlce;*.sql|All Files(*.*)|*.*";
            if (DatabaseInfo.DatabaseType == DatabaseType.SQLite)
            {
                ofd.Filter = "SQLite Script (*.sql)|*.sql|All Files(*.*)|*.*";
            }
            ofd.CheckFileExists = true;
            ofd.Multiselect = false;
            ofd.ValidateNames = true;
            ofd.Title = "Select Script to Open";
            if (ofd.ShowDialog() == true)
            {
                this.SqlText = System.IO.File.ReadAllText(ofd.FileName);
                _savedFileName = ofd.FileName;
                IsDirty = false;
            }
        }

        public void SaveScript(bool promptForName)
        {
            if (promptForName || string.IsNullOrEmpty(_savedFileName))
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "SQL Server Compact Script (*.sqlce;*.sql)|*.sqlce;*.sql|All Files(*.*)|*.*";
                if (DatabaseInfo.DatabaseType == DatabaseType.SQLite)
                {
                    sfd.Filter = "SQLite Script (*.sql)|*.sql|All Files(*.*)|*.*";
                }
                sfd.ValidateNames = true;
                sfd.Title = "Save script as";
                if (sfd.ShowDialog() == true)
                {
                    System.IO.File.WriteAllText(sfd.FileName, this.SqlText);
                    _savedFileName = sfd.FileName;
                    IsDirty = false;
                }
            }
            else
            {
                System.IO.File.WriteAllText(_savedFileName, this.SqlText);
                IsDirty = false;
            }
        }

        private void ExecuteSqlScriptInEditor()
        {
            try
            {

                using (var repository = Helpers.DataConnectionHelper.CreateRepository(DatabaseInfo))
                {
                    var sql = GetSqlFromSqlEditorTextBox();
                    bool schemaChanged = false;
                    if (sql.Length == 0) return;
                    sql = sql.Replace("\r", " \r");
                    sql = sql.Replace("GO  \r", "GO\r");
                    sql = sql.Replace("GO \r", "GO\r");
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    var dataset = repository.ExecuteSql(sql, out schemaChanged, ignoreDdlErrors);
                    sw.Stop();
                    FormatTime(sw);
                    if (dataset != null)
                    {
                        ParseDataSetResultsToResultsBox(dataset);
                        if (schemaChanged)
                        {
                            if (ExplorerControl != null)
                            {
                                ExplorerControl.RefreshTables(DatabaseInfo);
                            }
                        }
                    }
                }
            }
            catch (Exception sqlException)
            {
                ParseSqlErrorToResultsBox(Helpers.DataConnectionHelper.CreateEngineHelper(DatabaseInfo.DatabaseType).FormatError(sqlException));
            }
        }

        private string GetSqlFromSqlEditorTextBox()
        {
            var sql = SqlText.Trim();
            if (!string.IsNullOrWhiteSpace(SqlTextBox.SelectedText))
            {
                sql = SqlTextBox.SelectedText;
            }

            if (!sql.EndsWith("\r\nGO"))
                sql = sql + "\r\nGO";
            return sql;
        }

        private void ParseSqlErrorToResultsBox(string sqlException)
        {
            ClearResults();
            var textBox = new TextBox();
            textBox.Foreground = Brushes.Red;
            textBox.FontFamily = fontFamiliy;
            textBox.FontSize = fontSize;
            textBox.Text = sqlException;
            this.Resultspanel.Children.Add(textBox);
            this.tab1.Visibility = System.Windows.Visibility.Collapsed;
            resultsTabControl.SelectedIndex = 1;
        }

        private void ParseDataSetResultsToResultsBox(DataSet dataset)
        {
            ClearResults();

            foreach (DataTable table in dataset.Tables)
            {
                this.txtTime.Text = this.txtTime.Text + " / " + table.Rows.Count.ToString() + " rows ";
                var textBox = new TextBox();
                textBox.FontFamily = fontFamiliy;
                textBox.FontSize = fontSize;
                textBox.Foreground = Brushes.Black;
                DockPanel.SetDock(textBox, Dock.Top);
                if (table.Rows.Count == 0)
                {
                    textBox.Text = string.Format("{0} rows affected", table.MinimumCapacity);
                    this.Resultspanel.Children.Add(textBox);
                    resultsTabControl.SelectedIndex = 1;
                }
                else
                {
                    if (showResultInGrid)
                    {
                        if (useClassicGrid)
                        {
                            var grid = BuildPlainGrid(table);
                            DockPanel.SetDock(grid, Dock.Top);
                            this.GridPanel.Children.Add(grid);
                        }
                        else
                        {
                            var grid = new ExtEditControl();
                            grid.SourceTable = table;
                            DockPanel.SetDock(grid, Dock.Top);
                            this.GridPanel.Children.Add(grid);
                        }
                        resultsTabControl.SelectedIndex = 0;
                    }
                    else
                    {
                        this.tab1.Visibility = System.Windows.Visibility.Collapsed;
                        this.tab2.Header = "Results";
                        this.resultsTabControl.SelectedIndex = 1;
                        var results = new StringBuilder();
                        foreach (var column in table.Columns)
                        {
                            results.Append(column + "\t");
                        }
                        results.Remove(results.Length - 1, 1);
                        results.Append(Environment.NewLine);

                        foreach (DataRow row in table.Rows)
                        {
                            foreach (var item in row.ItemArray)
                            {
                                if (item == DBNull.Value)
                                {
                                    if (showNullValuesAsNULL)
                                    {
                                        results.Append("NULL\t");
                                    }
                                    else
                                    {
                                        results.Append("\t");
                                    }
                                }
                                //This formatting is optional (causes perf degradation)
                                else if (item.GetType() == typeof(byte[]) && showBinaryValuesInResult)
                                {
                                    var buffer = (byte[])item;
                                    results.Append("0x");
                                    for (int i = 0; i < buffer.Length; i++)
                                    {
                                        results.Append(buffer[i].ToString("X2", System.Globalization.CultureInfo.InvariantCulture));
                                    }
                                    results.Append("\t");
                                }
                                else if (item is DateTime)
                                {
                                    results.Append(((DateTime)item).ToString("O") + "\t");
                                }
                                else if (item is double || item is float)
                                {
                                    string intString = Convert.ToDouble(item).ToString("R", System.Globalization.CultureInfo.InvariantCulture);
                                    results.Append(intString + "\t");
                                }
                                else
                                {
                                    results.Append(item + "\t");
                                }
                            }
                            results.Remove(results.Length - 1, 1);
                            results.Append(Environment.NewLine);
                        }
                        textBox.Text = results.ToString();
                        this.Resultspanel.Children.Add(textBox);
                    }
                }
            }

            if (showResultInGrid && this.GridPanel.Children.Count > 0)
            {
                resultsTabControl.SelectedIndex = 0;
            }
        }

        private DataGrid BuildPlainGrid(DataTable table)
        {
            var grid = new DataGrid();
            grid.AutoGenerateColumns = true;
            grid.AutoGeneratingColumn += grid_AutoGeneratingColumn;
            grid.IsReadOnly = true;
            grid.FontSize = fontSize;
            grid.FontFamily = fontFamiliy;
            grid.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            grid.ItemsSource = ((IListSource)table).GetList();
            return grid;
        }

        void grid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {   
            var pos = e.PropertyName.IndexOf("_", StringComparison.Ordinal);   
            if (pos > 0 && e.Column.Header != null)   
            {   
                e.Column.Header = e.Column.Header.ToString().Replace("_", "__");   
            }   
            if (showNullValuesAsNULL)   
            {   
                ((DataGridBoundColumn)e.Column).Binding.TargetNullValue = "NULL";   
            }   
        }  

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.GridPanel.Children.Count > 0)
                {
                    DataGrid dataGrid = FindDataGrid();
                    if (dataGrid == null) return;
                    SaveFileDialog sfd = new SaveFileDialog();
                    sfd.Filter = "CSV file (*.csv)|*.csv|All Files(*.*)|*.*";
                    sfd.ValidateNames = true;
                    sfd.Title = "Save result as CSV";
                    if (sfd.ShowDialog() == true)
                    {
                        dataGrid.SelectAllCells();
                        dataGrid.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
                        ApplicationCommands.Copy.Execute(null, dataGrid);
                        dataGrid.UnselectAllCells();
                        var result = (string)Clipboard.GetData(DataFormats.CommaSeparatedValue);
                        Clipboard.Clear();
                        File.WriteAllText(sfd.FileName, result);
                    }
                    return;
                }
                if (this.Resultspanel.Children.Count > 0)
                {
                    var textBox = Resultspanel.Children[0] as TextBox;
                    if (textBox == null) return;
                    SaveFileDialog sfd = new SaveFileDialog();
                    sfd.Filter = "CSV file (*.csv)|*.csv|All Files(*.*)|*.*";
                    sfd.ValidateNames = true;
                    sfd.Title = "Save result as CSV";
                    if (sfd.ShowDialog() == true)
                    {
                        var separator = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator;
                        var result = textBox.Text.Replace("\t", separator);
                        File.WriteAllText(sfd.FileName, result);
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, DatabaseInfo.DatabaseType, true);
            }
        }

        private DataGrid FindDataGrid()
        {
            var dataGrid = GridPanel.Children[0] as DataGrid;
            if (dataGrid != null)
            {
                return dataGrid;
            }
            var control = GridPanel.Children[0] as ExtEditControl;
            if (control == null) return null;
            var grid = control.FindName("masterGrid") as Grid;
            if (grid == null) return null;
            return grid.Children[0] as DataGrid;
        }

        public void OpenSqlEditorToolWindow()
        {
            if (DatabaseInfo == null)  return;
            if (ExplorerControl == null) return;

            try
            {
                var pkg = _parentWindow.Package as SqlCeToolboxPackage;
                Debug.Assert(pkg != null, "Package property of the Explorere Tool Window should never be null, have you tried to create it manually and not through FindToolWindow()?");
                var sqlEditorWindow = pkg.CreateWindow<SqlEditorWindow>();
                var control = sqlEditorWindow.Content as SqlEditorControl;
                control.DatabaseInfo = DatabaseInfo;
                control.ExplorerControl = ExplorerControl;
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, DatabaseInfo.DatabaseType);
            }
        }

        private void Options_Click(object sender, RoutedEventArgs e)
        {
            var package = _parentWindow.Package as SqlCeToolboxPackage;
            if (package == null) return;
            package.ShowOptionPage(typeof(OptionsPageGeneral));
            Helpers.DataConnectionHelper.LogUsage("ToolbarOptions");
        }
    }
}