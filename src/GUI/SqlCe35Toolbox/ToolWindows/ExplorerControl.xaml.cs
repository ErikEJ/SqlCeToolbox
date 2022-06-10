using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using ErikEJ.SqlCeScripting;
using ErikEJ.SqlCeToolbox.Commands;
using ErikEJ.SqlCeToolbox.ContextMenues;
using ErikEJ.SqlCeToolbox.Dialogs;
using ErikEJ.SqlCeToolbox.Helpers;
using Microsoft.VisualStudio.Shell.Interop;
using SelectionContainer = Microsoft.VisualStudio.Shell.SelectionContainer;
using Trigger = ErikEJ.SqlCeScripting.Trigger;

namespace ErikEJ.SqlCeToolbox.ToolWindows
{
    /// <summary>
    /// Interaction logic for ExplorerControl.xaml
    /// </summary>
    public partial class ExplorerControl
    {
        private string _fatalError = string.Empty;
        private static ExplorerToolWindow _parentWindow;
        private Storyboard _myStoryboard;
        private SelectionContainer _mySelContainer;
        private System.Collections.ArrayList _mySelItems;
        private IVsWindowFrame _frame;
        public static List<DbDescription> DescriptionCache { get; set; }

        public ExplorerControl()
        {
            InitializeComponent();
        }

        public ExplorerControl(ExplorerToolWindow parentWindow)
        {
            InitializeComponent();
            _parentWindow = parentWindow;
        }

        private bool _loaded;

        private void MyToolWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (_loaded) return;
            TreeView1.Background = grid1.Background = VsThemes.GetToolWindowBackground();
            var overflowGrid = explorerToolbar.Template.FindName("OverflowGrid", explorerToolbar) as FrameworkElement;
            if (overflowGrid != null)
            {
                overflowGrid.Visibility = Visibility.Collapsed;
            }
            Updated.Visibility = Visibility.Collapsed;
#if SSMS
            AddConnections.Visibility = Visibility.Collapsed;
#endif
            // Look for update async
            var bw = new BackgroundWorker();
            bw.DoWork += bw_DoWork;
            bw.RunWorkerCompleted += (s, ea) =>
            {
                try
                {
                    PrepareTreeView("Data Connections");
                    Refresh.IsEnabled = true;
                    if ((bool)ea.Result)
                    {
                        Updated.Visibility = Visibility.Visible;
                        _myStoryboard.Begin(this);
                    }
                    else
                    {
                        Updated.Visibility = Visibility.Collapsed;
                    }
                }
                finally
                {
                    bw.Dispose();
                }
            };

            // Animate updated button
            var myDoubleAnimation = new DoubleAnimation
            {
                From = 0.1,
                To = 1.0,
                Duration = new Duration(TimeSpan.FromSeconds(5)),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            _myStoryboard = new Storyboard();
            _myStoryboard.Children.Add(myDoubleAnimation);
            Storyboard.SetTargetName(myDoubleAnimation, UpdatedText.Name);
            Storyboard.SetTargetProperty(myDoubleAnimation, new PropertyPath(OpacityProperty));

            PrepareTreeView("Loading...");
            bw.RunWorkerAsync();

            AddHandler(Keyboard.KeyDownEvent, (KeyEventHandler)HandleKeyDownEvent);
            txtConnections.Focus();
            _loaded = true;
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                BuildDatabaseTree(false);
                // ReSharper disable once RedundantAssignment
                var product = "addin35";
#if SSMS
                product = "ssmsaddin";                
#endif
                e.Result = DataConnectionHelper.CheckVersion(product);
            }
            catch
            {
                e.Result = false;
            }
        }

        public void BuildDatabaseTree()
        {
            BuildDatabaseTree(true);
        }

        private void BuildDatabaseTree(bool fromUiThread)
        {
            var databaseList = new Dictionary<string, DatabaseInfo>();
            _fatalError = string.Empty;
            try
            {
                var package = _parentWindow.Package as SqlCeToolboxPackage;
                if (package == null) return;
                
                if (fromUiThread)
                {
                    databaseList = DataConnectionHelper.GetDataConnections(package, true, false);
                }
                foreach (var info in DataConnectionHelper.GetOwnDataConnections())
                {
                    if (!databaseList.ContainsKey(info.Key))
                        databaseList.Add(info.Key, info.Value);
                }

                try
                {
                    //Boot Telemetry
                    Telemetry.Enabled = Properties.Settings.Default.ParticipateInTelemetry;
                    if (Telemetry.Enabled)
                    {
                        Telemetry.Initialize(
                            Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                            package.TelemetryVersion().ToString(),
                            "d4881a82-2247-42c9-9272-f7bc8aa29315");
                    }
                }
                catch 
                {
                    // Ignore
                }
#if SSMS
                DataConnectionHelper.LogUsage("Platform: SSMS " + package.TelemetryVersion().ToString(1));
#else
                DataConnectionHelper.LogUsage("Platform: Visual Studio " + package.TelemetryVersion().ToString(1));
#endif
            }
            catch (Exception e)
            {
                _fatalError = e.Message;
            }
            finally
            {
                if (fromUiThread)
                {
                    PrepareTreeView("Data Connections");
                    Refresh.IsEnabled = true;
                }
                var fillList = new FillDatabaseListHandler(FillDatabaseList);
                Dispatcher.BeginInvoke(fillList, databaseList); //fill the tree on the UI thread
            }
        }

        private void PrepareTreeView(string label)
        {
            Refresh.IsEnabled = false;
            txtHelp.Foreground = VsThemes.GetWindowText();
            RootItem.Foreground = VsThemes.GetWindowText();
            txtConnections.Text = label;
            txtConnections.Focus();
            RootItem.ContextMenu = new DatabasesContextMenu(new DatabaseMenuCommandParameters
            {
                ExplorerControl = this
            }, _parentWindow);
            RootItem.Foreground = VsThemes.GetWindowText();
        }

        private delegate void FillDatabaseListHandler(Dictionary<string, DatabaseInfo> databaseList);

        private void FillDatabaseList(Dictionary<string, DatabaseInfo> databaseList)
        {
            txtConnections.Text = "Data Connections";
            txtConnections.Focus();
            if (!string.IsNullOrWhiteSpace(_fatalError))
            {
                var errorItem = new TreeViewItem
                {
                    Header = _fatalError,
                    Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red)
                };
                RootItem.Items.Add(errorItem);
                return;
            }
            var sortedList = new SortedList<string, KeyValuePair<string, DatabaseInfo>>();

            try
            {
                foreach (var databaseInfo in databaseList)
                {
                    var x = 0;
                    var key = databaseInfo.Value.Caption;
                    if (sortedList.ContainsKey(key))
                    {
                        while (sortedList.ContainsKey(key))
                        {
                            x++;
                            key = string.Format("{0} ({1})", key, x.ToString());
                        }
                    }
                    sortedList.Add(key, databaseInfo);
                }
                RootItem.Items.Clear();

                foreach (var databaseInfo in sortedList)
                {
                    var databaseTreeViewItem = AddDatabaseToTreeView(databaseInfo.Value);
                    databaseTreeViewItem.Tag = databaseInfo.Value.Value;
                    RootItem.Items.Add(databaseTreeViewItem);
                }

                RootItem.Items.Add(TreeViewHelper.GetTypesItem(RootItem));
                RootItem.IsExpanded = true;
                //TreeViewHelper.GetInfoItems(InfoStack);
            }
            catch (Exception ex)
            {
                var errorItem = new TreeViewItem
                {
                    Header = ex.Message,
                    Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red)
                };
                RootItem.Items.Add(errorItem);
            }
        }

        private TreeViewItem AddDatabaseToTreeView(KeyValuePair<string, DatabaseInfo> database)
        {
            var caption = database.Value.Caption;
            switch (database.Value.DatabaseType)
            {
                case DatabaseType.SQLCE35:
                    caption = caption + " (Compact 3.5)";
                    break;
                case DatabaseType.SQLCE40:
                    caption = caption + " (Compact 4.0)";
                    break;
                case DatabaseType.SQLServer:
                    caption = caption + " (SQL Server)";
                    break;
                case DatabaseType.SQLite:
                    caption = caption + " (SQLite)";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var databaseTreeViewItem = TreeViewHelper.CreateTreeViewItemWithImage(caption, "../Resources/database_16xLG.png", true);
            if (database.Value.FromServerExplorer == false)
            {
                databaseTreeViewItem = TreeViewHelper.CreateTreeViewItemWithImage(caption, "../Resources/database_16xLG_Own.png", true);
            }
            if (database.Value.FileIsMissing)
            {
                databaseTreeViewItem = TreeViewHelper.CreateTreeViewItemWithImage(caption, "../Resources/database_16xLG_Broken.png", true);
            }
            databaseTreeViewItem.ToolTip = database.Value.ConnectionString;

            if (database.Value.DatabaseType == DatabaseType.SQLServer)
            {
                databaseTreeViewItem.ContextMenu = new SqlServerDatabaseContextMenu(new DatabaseMenuCommandParameters
                {
                    ExplorerControl = this,
                    DatabaseInfo = database.Value
                }, _parentWindow);
                databaseTreeViewItem.Items.Clear();
                return databaseTreeViewItem;
            }

            if (database.Value.DatabaseType == DatabaseType.SQLCE35
                || database.Value.DatabaseType == DatabaseType.SQLCE40)
            {
                databaseTreeViewItem.ContextMenu = new SqlCeDatabaseContextMenu(new DatabaseMenuCommandParameters
                {
                    ExplorerControl = this,
                    DatabaseInfo = database.Value
                }, _parentWindow);
            }

            if (database.Value.DatabaseType == DatabaseType.SQLite)
            {
                databaseTreeViewItem.ContextMenu = new SqliteDatabaseContextMenu(new DatabaseMenuCommandParameters
                {
                    ExplorerControl = this,
                    DatabaseInfo = database.Value
                }, _parentWindow);
            }

            databaseTreeViewItem.Items.Clear();

            var tables = TreeViewHelper.CreateTreeViewItemWithImage("Tables", "../Resources/folder_Closed_16xLG.png", true);
            tables.ContextMenu = new TablesContextMenu(new DatabaseMenuCommandParameters
            {
                DatabaseInfo = database.Value,
                ExplorerControl = this
            }, _parentWindow);

            tables.Expanded += (sender, args) => new GetTableItemsHandler(GetTableItems).BeginInvoke(sender, args, database, null, null);
            databaseTreeViewItem.Items.Add(tables);

            if (database.Value.DatabaseType == DatabaseType.SQLCE35)
            {
                var subscriptions = TreeViewHelper.CreateTreeViewItemWithImage("Subscriptions", "../Resources/folder_Closed_16xLG.png", true);
                subscriptions.ContextMenu = new SubscriptionsContextMenu(new MenuCommandParameters
                {
                    MenuItemType = MenuType.Function,
                    DatabaseInfo = database.Value
                }, _parentWindow);
                subscriptions.Expanded += (sender, args) => new GetSubsItemsHandler(GetSubscriptions).BeginInvoke(sender, args, database, null, null);
                databaseTreeViewItem.Items.Add(subscriptions);
            }

            if (database.Value.DatabaseType == DatabaseType.SQLCE35)
            {
                var scopes = TreeViewHelper.CreateTreeViewItemWithImage("Scopes", "../Resources/folder_Closed_16xLG.png", true);
                scopes.Expanded += (sender, args) => new GetScopesItemsHandler(GetScopes).BeginInvoke(sender, args, database, null, null);
                databaseTreeViewItem.Items.Add(scopes);
            }
            if (database.Value.DatabaseType == DatabaseType.SQLite)
            {
                var views = TreeViewHelper.CreateTreeViewItemWithImage("Views", "../Resources/folder_Closed_16xLG.png", true);
                views.ContextMenu = new ViewsContextMenu(new DatabaseMenuCommandParameters
                {
                    DatabaseInfo = database.Value,
                    ExplorerControl = this
                }, _parentWindow);
                views.Expanded += (sender, args) => new GetViewsItemsHandler(GetViews).BeginInvoke(sender, args, database, null, null);
                databaseTreeViewItem.Items.Add(views);
            }
            return databaseTreeViewItem;
        }

        private delegate void GetSubsItemsHandler(object sender, RoutedEventArgs args, KeyValuePair<string, DatabaseInfo> database);

        private void GetSubscriptions(object sender, RoutedEventArgs args, KeyValuePair<string, DatabaseInfo> database)
        {
            var viewItem = sender as DatabaseTreeViewItem;
            // Prevent loading again and again
            if (viewItem != null && (viewItem.Items.Count > 0 && viewItem.Items[0].ToString() == "Loading..."))
            {
                List<string> nameList;
                try
                {
                    using (var repository = Helpers.RepositoryHelper.CreateRepository(database.Value))
                    {
                        nameList = repository.GetAllSubscriptionNames();
                    }
                }
                catch (Exception e)
                {
                    DataConnectionHelper.SendError(e, DatabaseType.SQLCE35, false);
                    return;
                }
                Dispatcher.BeginInvoke(new FillSubscriptionItemsHandler(FillSubItems), database, viewItem, nameList);
            }
            args.Handled = true;
        }

        private delegate void FillSubscriptionItemsHandler(KeyValuePair<string, DatabaseInfo> database, DatabaseTreeViewItem parentItem, IList<string> childItems);

        private static void FillSubItems(KeyValuePair<string, DatabaseInfo> database, DatabaseTreeViewItem parentItem, IList<string> nameList)
        {
            parentItem.Items.Clear();
            foreach (var sub in nameList)
            {
                var item = TreeViewHelper.CreateTreeViewItemWithImage(sub, "../Resources/arrow_Sync_16xLG.png", false);
                item.ContextMenu = new SubscriptionsContextMenu(new MenuCommandParameters { DatabaseInfo = database.Value, Name = sub, MenuItemType = MenuType.Manage }, _parentWindow);
                parentItem.Items.Add(item);
            }
        }

        private delegate void GetScopesItemsHandler(object sender, RoutedEventArgs args, KeyValuePair<string, DatabaseInfo> database);

        private void GetScopes(object sender, RoutedEventArgs args, KeyValuePair<string, DatabaseInfo> database)
        {
            var viewItem = sender as DatabaseTreeViewItem;
            // Prevent loading again and again
            if (viewItem != null && (viewItem.Items.Count > 0 && viewItem.Items[0].ToString() == "Loading..."))
            {
                var scopeList = new List<string>();
                try
                {
                    if (SyncFxHelper.IsProvisioned(database.Value))
                        scopeList = SyncFxHelper.GetSqlCeScopes(database.Value.ConnectionString).OrderBy(s => s).ToList();
                }
                catch (Exception e)
                {
                    DataConnectionHelper.SendError(e, DatabaseType.SQLCE35);
                    return;
                }
                Dispatcher.BeginInvoke(new FillScopeItemsHandler(FillScopeItems), database, viewItem, scopeList);
            }
            args.Handled = true;
        }

        private delegate void FillScopeItemsHandler(KeyValuePair<string, DatabaseInfo> database, DatabaseTreeViewItem parentItem, IList<string> childItems);

        private static void FillScopeItems(KeyValuePair<string, DatabaseInfo> database, DatabaseTreeViewItem parentItem, IList<string> scopeList)
        {
            parentItem.Items.Clear();
            foreach (var scope in scopeList)
            {
                var item = TreeViewHelper.CreateTreeViewItemWithImage(scope, "../Resources/Synchronize_16xLG.png", true);
                item.ContextMenu = new ScopesContextMenu(new MenuCommandParameters { DatabaseInfo = database.Value, Name = scope, MenuItemType = MenuType.Manage }, _parentWindow);
                parentItem.Items.Add(item);
                var tables = SyncFxHelper.GetSqlCeScopeDefinition(database.Value.ConnectionString, scope).Select(s => s.TableName).Distinct();
                item.Expanded += (s, e) => GetScopeTables(s, e, tables, database);
            }
        }

        private static void GetScopeTables(object sender, RoutedEventArgs args, IEnumerable<string> tables, KeyValuePair<string, DatabaseInfo> database)
        {
            var viewItem = sender as DatabaseTreeViewItem;
            // Prevent loading again and again
            if (viewItem != null && (viewItem.Items.Count > 0 && viewItem.Items[0].ToString() == "Loading..."))
            {
                viewItem.Items.Clear();
                var scopeName = viewItem.MetaData;

                foreach (var table in tables)
                {
                    var item = TreeViewHelper.CreateTreeViewItemWithImage(table, "../Resources/table_16xLG.png", true);
                    item.ToolTip = table;
                    viewItem.Items.Add(item);

                    var columns = SyncFxHelper.GetSqlCeScopeDefinition(database.Value.ConnectionString, scopeName);
                    item.Expanded += (s, e) => GetScopeTableColumns(s, e, columns);
                }
            }
            args.Handled = true;
        }

        private static void GetScopeTableColumns(object sender, RoutedEventArgs args, IEnumerable<Column> columns)
        {
            var viewItem = sender as DatabaseTreeViewItem;
            // Prevent loading again and again
            if (viewItem != null && (viewItem.Items.Count > 0 && viewItem.Items[0].ToString() == "Loading..."))
            {
                viewItem.Items.Clear();
                var tableName = viewItem.MetaData;

                foreach (var column in columns.Where(c => c.TableName == tableName))
                {
                    var item = TreeViewHelper.CreateTreeViewItemWithImage(column.ColumnName, "../Resources/column_16xLG.png", false);
                    item.ToolTip = column;
                    viewItem.Items.Add(item);
                }
            }
            args.Handled = true;
        }

        private delegate void GetViewsItemsHandler(object sender, RoutedEventArgs args, KeyValuePair<string, DatabaseInfo> database);

        private void GetViews(object sender, RoutedEventArgs args, KeyValuePair<string, DatabaseInfo> database)
        {
            var viewItem = sender as DatabaseTreeViewItem;
            // Prevent loading again and again
            if (viewItem != null && (viewItem.Items.Count > 0 && viewItem.Items[0].ToString() == "Loading...")
                || args == null && viewItem != null)
            {
                List<View> viewList;
                try
                {
                    using (var repository = Helpers.RepositoryHelper.CreateRepository(database.Value))
                    {
                        viewList = repository.GetAllViews();
                    }
                }
                catch (Exception e)
                {
                    DataConnectionHelper.SendError(e, database.Value.DatabaseType);
                    return;
                }
                Dispatcher.BeginInvoke(new FillViewItemsHandler(FillViewItems), database, viewItem, viewList);
            }
            if (args != null) args.Handled = true;
        }

        private delegate void FillViewItemsHandler(KeyValuePair<string, DatabaseInfo> database, DatabaseTreeViewItem parentItem, IList<View> childItems);

        private static void FillViewItems(KeyValuePair<string, DatabaseInfo> database, DatabaseTreeViewItem parentItem, IList<View> viewList)
        {
            parentItem.Items.Clear();
            foreach (var view in viewList)
            {
                var item = TreeViewHelper.CreateTreeViewItemWithImage(view.ViewName, "../Resources/table_16xLG.png", false);
                item.ContextMenu = new ViewContextMenu(new MenuCommandParameters { DatabaseInfo = database.Value, Name = view.ViewName, MenuItemType = MenuType.Manage, Description = view.Definition }, _parentWindow);
                parentItem.Items.Add(item);
            }
        }

        private delegate void GetTableItemsHandler(object sender, RoutedEventArgs args, KeyValuePair<string, DatabaseInfo> database);

        private void GetTableItems(object sender, RoutedEventArgs args, KeyValuePair<string, DatabaseInfo> database)
        {
            var viewItem = sender as DatabaseTreeViewItem;

            Exception ex = null;

            // Prevent loading again and again
            var doShow = args != null && viewItem != null && (viewItem.Items.Count > 0 && viewItem.Items[0].ToString() == "Loading...")
                || args == null && viewItem != null;
            if (doShow)
            {
                try
                {
                    using (var repository = Helpers.RepositoryHelper.CreateRepository(database.Value))
                    {
                        repository.ExecuteSql("SELECT 1" + Environment.NewLine + "GO");
                    }
                }
                catch (Exception e)
                {
                    ex = e;
                }
                Dispatcher.BeginInvoke(new FillTableItemsHandler(FillTableItems), database, viewItem, ex, args);
            }
            if (args != null) args.Handled = true;
        }

        private delegate void FillTableItemsHandler(KeyValuePair<string, DatabaseInfo> database, DatabaseTreeViewItem parentItem, Exception ex, RoutedEventArgs args);

        private void FillTableItems(KeyValuePair<string, DatabaseInfo> database, DatabaseTreeViewItem parentItem, Exception ex, RoutedEventArgs args)
        {
            try
            {
                if (ex != null)
                {
                    var error = Helpers.RepositoryHelper.CreateEngineHelper(database.Value.DatabaseType).FormatError(ex);
                    if (error.Contains("Minor Err.: 25028"))
                    {
                        var pwd = new PasswordDialog();
                        pwd.ShowModal();
                        if (pwd.DialogResult.HasValue && pwd.DialogResult.Value && !string.IsNullOrWhiteSpace(pwd.Password))
                        {
                            database.Value.ConnectionString = database.Value.ConnectionString + ";Password=" + pwd.Password;
                            GetTableItems(parentItem, args, database);
                        }
                    }
                    else
                    {
                        DataConnectionHelper.SendError(ex, database.Value.DatabaseType, false);
                    }
                    return;
                }

                parentItem.Items.Clear();

                using (var repository = Helpers.RepositoryHelper.CreateRepository(database.Value))
                {
                    DescriptionCache = new Helpers.DescriptionHelper().GetDescriptions(database.Value);
                    var dbdesc = DescriptionCache.Where(dc => dc.Parent == null && dc.Object == null).Select(dc => dc.Description).SingleOrDefault();
                    if (!string.IsNullOrWhiteSpace(dbdesc))
                    {
                        var item = parentItem.Parent as DatabaseTreeViewItem;
                        if (item != null)
                        {
                            var dbItem = item;
                            dbItem.ToolTip = dbItem.ToolTip + Environment.NewLine + dbdesc;
                        }
                    }
                    var parent = parentItem.Parent as DatabaseTreeViewItem;
                    if (parent != null && Properties.Settings.Default.DisplayObjectProperties)
                    {
                        var dbItem = parent;
                        var dbInfo = dbItem.Tag as DatabaseInfo;
                        if (dbInfo != null)
                        {
                            foreach (var values in repository.GetDatabaseInfo())
                            {
                                if (values.Key.ToLowerInvariant() == "locale identifier")
                                    dbInfo.LCID = int.Parse(values.Value);
                                if (values.Key.ToLowerInvariant() == "encryption mode")
                                    dbInfo.EncryptionMode = values.Value;
                                if (string.IsNullOrWhiteSpace(dbInfo.EncryptionMode))
                                    dbInfo.EncryptionMode = "None";
                                if (values.Key.ToLowerInvariant() == "case sensitive")
                                    dbInfo.CaseSensitive = bool.Parse(values.Value);
                                if (values.Key == "DatabaseSize")
                                    dbInfo.Size = values.Value;
                                if (values.Key == "SpaceAvailable")
                                    dbInfo.SpaceAvailable = values.Value;
                                if (values.Key == "Created")
                                    dbInfo.Created = values.Value;
                                if (values.Key == "ServerVersion")
                                    dbInfo.ServerVersion = values.Value;
                            }
                            TrackSelection(dbInfo);
                        }
                    }
                    var tables = repository.GetAllTableNames();
                    var columns = repository.GetAllColumns();
                    var primaryKeys = repository.GetAllPrimaryKeys();
                    var foreignKeys = repository.GetAllForeignKeys();
                    var indexes = repository.GetAllIndexes();
                    var triggers = repository.GetAllTriggers();

                    foreach (var table in tables)
                    {
                        if (!Properties.Settings.Default.DisplayDescriptionTable && table.Equals("__ExtendedProperties"))
                        {
                            continue;
                        }
                        var item = TreeViewHelper.CreateTreeViewItemWithImage(table, "../Resources/table_16xLG.png", true);
                        item.ContextMenu = new TableContextMenu(new MenuCommandParameters { DatabaseInfo = database.Value, Name = table, MenuItemType = MenuType.Table }, _parentWindow);
                        item.ToolTip = table;
                        item.Tag = new TableInfo { Name = table, RowCount = repository.GetRowCount(table) };
                        if (DescriptionCache != null)
                        {
                            var desc = DescriptionCache.Where(dc => dc.Parent == null && dc.Object == table).Select(dc => dc.Description).SingleOrDefault();
                            if (!string.IsNullOrWhiteSpace(desc))
                            {
                                item.ToolTip = desc;
                            }
                        }

                        var tableColumns = (from col in columns where col.TableName == table select col).ToList();
                        var tablePrimaryKeys = primaryKeys.Where(pk => pk.TableName == table).ToList();
                        var tableForeignKeys = foreignKeys.Where(fk => fk.ConstraintTableName == table).ToList();
                        var tableIndexes = indexes.Where(i => i.TableName == table).ToList();
                        var tableTriggers = triggers.Where(t => t.TableName == table).ToList();
                        parentItem.Items.Add(item);
                        item.Expanded += (s, e) => GetTableColumns(s, e, tableColumns, tableForeignKeys, tablePrimaryKeys, tableIndexes, tableTriggers, database);
                    }
                }
            }
            catch (Exception ex2)
            {
                DataConnectionHelper.SendError(ex2, database.Value.DatabaseType, false);
            }
        }

        private static void GetTableColumns(object sender, RoutedEventArgs args, List<Column> columns, List<Constraint> fkList, List<PrimaryKey> pkList, List<Index> indexes, List<Trigger> triggers, KeyValuePair<string, DatabaseInfo> database)
        {
            var viewItem = sender as DatabaseTreeViewItem;
            // Prevent loading again and again
            if (viewItem != null && (viewItem.Items.Count > 0 && viewItem.Items[0].ToString() == "Loading..."))
            {
                viewItem.Items.Clear();
                var tableName = viewItem.MetaData;


                var pks = (from pk in pkList select pk.ColumnName).ToList();

                foreach (var column in columns)
                {
                    var display = column.ShortType;
                    var image = "../Resources/column_16xLG.png";

                    var constraints = (from fk in fkList where fk.Columns.Contains(column.ColumnName) select fk);
                    if (constraints.Any())
                    {
                        display = "FK, " + display;
                        image = "../Resources/KeyDownFk_8461.png";
                    }
                    if (pks.Contains(column.ColumnName))
                    {
                        display = "PK, " + display;
                        image = "../Resources/KeyDown_8461.png";
                    }

                    string nullable = " not null)";
                    if (column.IsNullable == YesNoOption.YES)
                    {
                        nullable = " null)";
                    }
                    display = column.ColumnName + " (" + display + nullable;
                    var i = TreeViewHelper.CreateTreeViewItemWithImage(display, image, false);
                    if (database.Value.DatabaseType != DatabaseType.SQLite)
                        i.ContextMenu = new ColumnContextMenu(new MenuCommandParameters { Description = tableName, DatabaseInfo = database.Value, Name = column.ColumnName, MenuItemType = MenuType.Table }, _parentWindow);
                    i.ToolTip = column.ColumnName;
                    if (DescriptionCache != null)
                    {
                        var desc = DescriptionCache.Where(dc => dc.Parent == tableName && dc.Object == column.ColumnName).Select(dc => dc.Description).SingleOrDefault();
                        if (!string.IsNullOrWhiteSpace(desc))
                        {
                            i.ToolTip = desc;
                        }
                    }
                    viewItem.Items.Add(i);
                }

                var indexesItem = TreeViewHelper.CreateTreeViewItemWithImage("Indexes", "../Resources/folder_Closed_16xLG.png", true);
                indexesItem.Items.Clear();

                string oldName = string.Empty;
                foreach (var primaryKey in pkList)
                {
                    if (oldName != primaryKey.KeyName)
                    {
                        var display = primaryKey.KeyName + " (Primary Key)";
                        var indexItem = TreeViewHelper.CreateTreeViewItemWithImage(display, "../Resources/Index_8287_16x.png", false);
                        if (database.Value.DatabaseType != DatabaseType.SQLite)
                            indexItem.ContextMenu = new IndexContextMenu(new MenuCommandParameters { Description = primaryKey.KeyName, DatabaseInfo = database.Value, Name = viewItem.MetaData, MenuItemType = MenuType.Table }, _parentWindow);
                        indexItem.ToolTip = primaryKey.KeyName;
                        indexesItem.Items.Add(indexItem);
                        oldName = primaryKey.KeyName;
                    }
                }

                oldName = string.Empty;

                foreach (var index in indexes)
                {
                    if (oldName == index.IndexName) continue;
                    string display;
                    if (index.Unique)
                    {
                        display = index.IndexName + " (Unique)";
                    }
                    else
                    {
                        display = index.IndexName + " (Non-Unique)";
                    }
                    var indexItem = TreeViewHelper.CreateTreeViewItemWithImage(display, "../Resources/Index_8287_16x.png", false);
                    indexItem.ContextMenu = new IndexContextMenu(new MenuCommandParameters { Description = index.IndexName, DatabaseInfo = database.Value, Name = viewItem.MetaData, MenuItemType = MenuType.Table }, _parentWindow);
                    indexItem.ToolTip = index.IndexName;
                    indexesItem.Items.Add(indexItem);
                    oldName = index.IndexName;
                }
                viewItem.Items.Add(indexesItem);

                var keysItem = TreeViewHelper.CreateTreeViewItemWithImage("Keys", "../Resources/folder_Closed_16xLG.png", true);
                keysItem.Items.Clear();

                foreach (var primaryKey in pkList)
                {
                    if (oldName == primaryKey.KeyName) continue;
                    var display = primaryKey.KeyName;
                    var keyItem = TreeViewHelper.CreateTreeViewItemWithImage(display, "../Resources/KeyDown_8461.png", false);
                    if (database.Value.DatabaseType != DatabaseType.SQLite)
                        keyItem.ContextMenu = new KeyContextMenu(new MenuCommandParameters { Description = primaryKey.KeyName, DatabaseInfo = database.Value, Name = viewItem.MetaData, MenuItemType = MenuType.Pk }, _parentWindow);
                    keyItem.ToolTip = primaryKey.KeyName;
                    keysItem.Items.Add(keyItem);
                    oldName = primaryKey.KeyName;
                }

                foreach (var fk in fkList)
                {
                    var display = fk.ConstraintName;
                    var keyItem = TreeViewHelper.CreateTreeViewItemWithImage(display, "../Resources/KeyDownFk_8461.png", false);
                    if (database.Value.DatabaseType != DatabaseType.SQLite)
                        keyItem.ContextMenu = new KeyContextMenu(new MenuCommandParameters { Description = fk.ConstraintName, DatabaseInfo = database.Value, Name = viewItem.MetaData, MenuItemType = MenuType.Fk }, _parentWindow);
                    keyItem.ToolTip = fk.ConstraintName;
                    keysItem.Items.Add(keyItem);
                }
                viewItem.Items.Add(keysItem);


                if (database.Value.DatabaseType == DatabaseType.SQLite)
                {
                    var triggersItem = TreeViewHelper.CreateTreeViewItemWithImage("Triggers",
                        "../Resources/folder_Closed_16xLG.png", true);
                    triggersItem.Items.Clear();

                    foreach (var trigger in triggers)
                    {
                        var triggerItem = TreeViewHelper.CreateTreeViewItemWithImage(trigger.TriggerName,
                            "../Resources/RunOutline.png", false);
                        triggerItem.ContextMenu = new TriggerContextMenu(
                                new MenuCommandParameters
                                {
                                    DatabaseInfo = database.Value,
                                    Name = trigger.TriggerName,
                                    MenuItemType = MenuType.Manage,
                                    Description = trigger.Definition
                                }, _parentWindow);
                        triggersItem.Items.Add(triggerItem);
                    }
                    viewItem.Items.Add(triggersItem);
                }
            }
            args.Handled = true;
        }

        public void RefreshTables(DatabaseInfo databaseInfo)
        {
            var item = FindItem(databaseInfo, "Tables");
            if (item != null)
                GetTableItems(item, null, new KeyValuePair<string, DatabaseInfo>(databaseInfo.Caption, databaseInfo));
        }

        public void RefreshViews(DatabaseInfo databaseInfo)
        {
            var item = FindItem(databaseInfo, "Views");
            if (item != null)
                GetViews(item, null, new KeyValuePair<string, DatabaseInfo>(databaseInfo.Caption, databaseInfo));
        }

        private DatabaseTreeViewItem FindItem(DatabaseInfo databaseInfo, string label)
        {
            if (RootItem.HasItems)
            {
                foreach (var item in RootItem.Items)
                {
                    var dbItem = item as DatabaseTreeViewItem;
                    if (dbItem != null)
                    {
                        var dbInfo = dbItem.Tag as DatabaseInfo;
                        if (dbInfo == null) continue;
                        if (dbInfo.ConnectionString != databaseInfo.ConnectionString) continue;
                        if (!dbItem.HasItems) continue;
                        foreach (var tabItem in dbItem.Items)
                        {
                            var tabFoundItem = tabItem as DatabaseTreeViewItem;
                            if (tabFoundItem != null && tabFoundItem.ToString() == label)
                                return tabFoundItem;
                        }
                    }
                }
            }
            return null;
        }

        private void TreeViewItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((TreeViewItem)sender).IsSelected = true;
            e.Handled = true;
        }

        private void ToolbarRefresh_Click(object sender, RoutedEventArgs e)
        {
            BuildDatabaseTree();
            DataConnectionHelper.LogUsage("ToolbarRefresh");
        }

        private void ToolbarAbout_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AboutDialog();
            dialog.ShowModal();
            DataConnectionHelper.LogUsage("ToolbarAbout");
        }

        private void Feedback_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://marketplace.visualstudio.com/items?itemName=ErikEJ.SQLServerCompactSQLiteToolbox#review-details");
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/ErikEJ/SqlCeToolbox");
            DataConnectionHelper.LogUsage("ToolbarUpdate");
        }

        private void Options_Click(object sender, RoutedEventArgs e)
        {
            var package = _parentWindow.Package as SqlCeToolboxPackage;
            if (package == null) return;
            package.ShowOptionPage(typeof(OptionsPageGeneral));
            DataConnectionHelper.LogUsage("ToolbarOptions");
        }

        #region Properties Windows
        private void TreeView1_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            NewQuery.IsEnabled = false;
            var image = NewQuery.Content as Image;
            if (image != null) image.Opacity = 0.5;
            var dbInfo = TryGetDatabaseInfo(TreeView1);
            if (dbInfo != null && dbInfo.DatabaseType != DatabaseType.SQLServer)
            {
                NewQuery.IsEnabled = true;
                NewQuery.Tag = dbInfo;
                if (image != null) image.Opacity = 1;
            }
            if (Properties.Settings.Default.DisplayObjectProperties)
            {
                var databaseTreeviewItem = TreeView1.SelectedItem as DatabaseTreeViewItem;
                if (databaseTreeviewItem?.Tag != null)
                {
                    if (databaseTreeviewItem.Tag is DatabaseInfo)
                        TrackSelection(databaseTreeviewItem.Tag as DatabaseInfo);
                    else if (databaseTreeviewItem.Tag is TableInfo)
                        TrackSelection(databaseTreeviewItem.Tag as TableInfo);
                    else
                        TrackSelection(null);
                }
                else
                {
                    try
                    {
                        var treeItem = TreeView1.SelectedItem as TreeViewItem;
                        if (treeItem != null)
                        {
                            databaseTreeviewItem = (treeItem).Parent as DatabaseTreeViewItem;
                            if (databaseTreeviewItem?.Tag != null)
                            {
                                TrackSelection(databaseTreeviewItem.Tag as DatabaseInfo);
                            }
                            else
                            {
                                TrackSelection(null);
                            }
                        }
                        else
                        {
                            TrackSelection(null);
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }
                var focusItem = TreeView1.SelectedItem as DatabaseTreeViewItem;
                focusItem?.Focus();
            }
            else
            {
                TrackSelection(null);
            }
        }

        private DatabaseInfo TryGetDatabaseInfo(TreeView treeView)
        {
            var databaseTreeviewItem = treeView.SelectedItem as DatabaseTreeViewItem;
            if (databaseTreeviewItem?.Tag != null && databaseTreeviewItem.Tag.GetType() == typeof(DatabaseInfo))
            {
                return databaseTreeviewItem.Tag as DatabaseInfo;
            }
            try
            {
                var treeItem = treeView.SelectedItem as TreeViewItem;
                if (treeItem != null)
                {
                    databaseTreeviewItem = (treeItem).Parent as DatabaseTreeViewItem;
                    if (databaseTreeviewItem?.Tag != null && databaseTreeviewItem.Tag.GetType() == typeof(DatabaseInfo))
                    {
                        return databaseTreeviewItem.Tag as DatabaseInfo;
                    }
                    var item = (treeItem).Parent as TreeViewItem;

                    var parent = item?.Parent as TreeViewItem;
                    if (parent?.Tag != null && parent.Tag.GetType() == typeof(DatabaseInfo))
                    {
                        return parent.Tag as DatabaseInfo;
                    }

                    var grandParent = parent?.Parent as TreeViewItem;
                    if (grandParent?.Tag != null && grandParent.Tag.GetType() == typeof(DatabaseInfo))
                    {
                        return grandParent.Tag as DatabaseInfo;
                    }

                    var greatGrandParent = grandParent?.Parent as TreeViewItem;
                    if (greatGrandParent?.Tag != null && greatGrandParent.Tag.GetType() == typeof(DatabaseInfo))
                    {
                        return greatGrandParent.Tag as DatabaseInfo;
                    }
                }
            }
            catch
            {
                // ignored
            }
            return null;
        }

        // http://msdn.microsoft.com/en-us/library/cc138529.aspx
        private void TrackSelection(object info)
        {
            ShowPropertiesFrame();

            if (_mySelContainer == null)
            {
                _mySelContainer = new SelectionContainer();
            }

            _mySelItems = new System.Collections.ArrayList();

            if (info != null)
            {
                _mySelItems.Add(info);
            }

            _mySelContainer.SelectedObjects = _mySelItems;

            //Must use the GetService of the Window to get the ITrackSelection reference
            var track = _parentWindow.GetServiceHelper(typeof(STrackSelection)) as ITrackSelection;
            track?.OnSelectChange(_mySelContainer);
        }

        private void ShowPropertiesFrame()
        {
            if (!Properties.Settings.Default.DisplayObjectProperties) return;
            var package = _parentWindow.Package as SqlCeToolboxPackage;
            if (package == null) return;
            if (_frame != null) return;
            var shell = package.GetServiceHelper(typeof(SVsUIShell)) as IVsUIShell;
            if (shell == null) return;
            var guidPropertyBrowser = new Guid(ToolWindowGuids.PropertyBrowser);
            shell.FindToolWindow((uint)__VSFINDTOOLWIN.FTW_fForceCreate, ref guidPropertyBrowser, out _frame);
            _frame.Show();
        }

        #endregion

        private void HandleKeyDownEvent(object sender, KeyEventArgs e)
        {
            var item = TreeView1.SelectedItem as DatabaseTreeViewItem;
            if (item == null) return;
            try
            {
                if (e.Key == Key.C && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    var databaseInfo = item.Tag as DatabaseInfo;
                    if (databaseInfo != null)
                    {
                        var helper = Helpers.RepositoryHelper.CreateEngineHelper(databaseInfo.DatabaseType);
                        Clipboard.Clear();
                        Clipboard.SetData(DataFormats.FileDrop, new[] { helper.PathFromConnectionString(databaseInfo.ConnectionString) });
                    }
                }
            }
            catch (Exception ex)
            {
                DataConnectionHelper.SendError(ex, DatabaseType.SQLServer);
            }
        }

        private void AddConnections_OnClick(object sender, RoutedEventArgs e)
        {
            var dbsCommandHandler = new DatabasesMenuCommandsHandler(_parentWindow);
            dbsCommandHandler.ScanConnections(null, null);
        }

        private void AddSqliteDb_OnClick(object sender, RoutedEventArgs e)
        {
            var dbsCommandHandler = new DatabasesMenuCommandsHandler(_parentWindow);
            dbsCommandHandler.AddSqLiteDatabase(null, null);
        }

        private void AddSqlCeDb_Click(object sender, RoutedEventArgs e)
        {
            if (!Helpers.RepositoryHelper.IsV40Installed())
            {
                EnvDteHelper.ShowMessage("The SQL Server Compact 4.0 runtime must be installed in order to add connections");
                return;
            }
            var dbsCommandHandler = new DatabasesMenuCommandsHandler(_parentWindow);
            dbsCommandHandler.AddPrivateCe40Database(null, null);
        }

        private void AddSqlServerDb_Click(object sender, RoutedEventArgs e)
        {
            var dbsCommandHandler = new DatabasesMenuCommandsHandler(_parentWindow);
            dbsCommandHandler.AddPrivateSqlServerDatabase(null, null);
        }

        private void NewQuery_OnClick(object sender, RoutedEventArgs e)
        {
            var dbCommandHandler = new DatabaseMenuCommandsHandler(_parentWindow);
            var button = sender as Button;
            if (button?.Tag is DatabaseInfo)
            {
                var parameter = new DatabaseMenuCommandParameters
                {
                    DatabaseInfo = button.Tag as DatabaseInfo,
                    ExplorerControl = this
                };
                var theSender = new MenuItem
                {
                    CommandParameter = parameter
                };
                dbCommandHandler.SpawnSqlEditorWindow(theSender, null);
            }

        }
    }
}
