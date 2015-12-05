using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using ErikEJ.SqlCeScripting;
using ErikEJ.SqlCeToolbox.Commands;
using ErikEJ.SqlCeToolbox.ContextMenues;
using ErikEJ.SqlCeToolbox.Dialogs;
using ErikEJ.SqlCeToolbox.Helpers;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Windows.Media;

namespace ErikEJ.SqlCeToolbox.ToolWindows
{
    /// <summary>
    /// Interaction logic for ExplorerControl.xaml
    /// </summary>
    public partial class ExplorerControl : UserControl
    {
        private string _fatalError = string.Empty;
        private static ExplorerToolWindow _parentWindow;
        private Storyboard myStoryboard;
        private SelectionContainer mySelContainer;
        private System.Collections.ArrayList mySelItems;
        private IVsWindowFrame frame = null;
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
            var overflowGrid = explorerToolbar.Template.FindName("OverflowGrid", explorerToolbar) as FrameworkElement;
            if (overflowGrid != null)
            {
                overflowGrid.Visibility = Visibility.Collapsed;
            }
            //explorerToolbar.Foreground = Helpers.VSThemes.GetWindowText();
            //explorerToolbar.Background = Helpers.VSThemes.GetCommandBackground();
            //toolTray.Background = explorerToolbar.Background;
            //sep2.Background = sep1.Background = Helpers.VSThemes.GetToolbarSeparatorBackground();
            TreeView1.Background = grid1.Background = Helpers.VSThemes.GetToolWindowBackground();
            Updated.Visibility = System.Windows.Visibility.Collapsed;

            // Look for update async
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            bw.RunWorkerCompleted += (s, ea) =>
            {
                try
                {
                    if ((Boolean)ea.Result == true)
                    {
                        Updated.Visibility = System.Windows.Visibility.Visible;
                        myStoryboard.Begin(this);
                    }
                    else
                    {
                        Updated.Visibility = System.Windows.Visibility.Collapsed;
                    }
                }
                finally
                {
                    bw.Dispose();
                }
            };

            // Animate updated button
            DoubleAnimation myDoubleAnimation = new DoubleAnimation();
            myDoubleAnimation.From = 0.1;
            myDoubleAnimation.To = 1.0;
            myDoubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(5));
            myDoubleAnimation.AutoReverse = true;
            myDoubleAnimation.RepeatBehavior = RepeatBehavior.Forever;

            myStoryboard = new Storyboard();
            myStoryboard.Children.Add(myDoubleAnimation);
            Storyboard.SetTargetName(myDoubleAnimation, UpdatedText.Name);
            Storyboard.SetTargetProperty(myDoubleAnimation, new PropertyPath(TextBlock.OpacityProperty));
            BuildDatabaseTree();
            bw.RunWorkerAsync();
            AddHandler(Keyboard.KeyDownEvent, (KeyEventHandler)HandleKeyDownEvent);
            txtConnections.Focus();
            _loaded = true;
        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                e.Result = Helpers.DataConnectionHelper.CheckVersion("addin35");
            }
            catch { }
        }

        public void BuildDatabaseTree()
        {
            Dictionary<string, DatabaseInfo> databaseList = new Dictionary<string, DatabaseInfo>();
            try
            {
                Refresh.IsEnabled = false;
                _fatalError = string.Empty;
                this.txtConnections.Text = "Data Connections";
                this.txtHelp.Foreground = Helpers.VSThemes.GetWindowText();
                txtConnections.Focus();
                ItemDatabases.ContextMenu = new DatabasesContextMenu(new DatabaseMenuCommandParameters
                {
                    ExplorerControl = this
                }, _parentWindow);
                ItemDatabases.Foreground = Helpers.VSThemes.GetWindowText();
                ItemDatabases.ToolTip = "Right click to Manage Connections (and many other features)";
                ItemDatabases.Items.Clear();
                ItemDatabases.IsExpanded = true;

                var package = _parentWindow.Package as SqlCeToolboxPackage;
                if (package == null) return;
                if (!DataConnectionHelper.IsV35Installed() && !DataConnectionHelper.IsV40Installed())
                {
                    RuntimeMissing.Visibility = System.Windows.Visibility.Visible;
                }
                ItemDatabases.Items.Add("Loading...");
                if (Properties.Settings.Default.ValidateConnectionsOnStart)
                {
                    try
                    {
                        new DataConnectionHelper().ValidateConnections(package);
                    }
                    catch { }
                }
                databaseList = DataConnectionHelper.GetDataConnections(package, true, false);
                foreach (KeyValuePair<string, DatabaseInfo> info in DataConnectionHelper.GetOwnDataConnections())
                {
                    if (!databaseList.ContainsKey(info.Key))
                        databaseList.Add(info.Key, info.Value);
                }
            }
            catch (Exception e)
            {
                //Helpers.DataConnectionHelper.SendError(e, DatabaseType.SQLServer);
                _fatalError = e.Message;
            }
            finally
            {
                var fillList = new FillDatabaseListHandler(FillDatabaseList);
                Dispatcher.BeginInvoke(fillList, databaseList); //fill the tree on the UI thread
                Refresh.IsEnabled = true;
            }
        }

        private delegate void FillDatabaseListHandler(Dictionary<string, DatabaseInfo> databaseList);

        private void FillDatabaseList(Dictionary<string, DatabaseInfo> databaseList)
        {
            SortedList<string, KeyValuePair<string, DatabaseInfo>> sortedList = new SortedList<string, KeyValuePair<string, DatabaseInfo>>();

            try
            {
                foreach (var databaseInfo in databaseList)
                {
                    int x = 0;
                    string key = databaseInfo.Value.Caption;
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
                ItemDatabases.Items.Clear();

                foreach (var databaseInfo in sortedList)
                {
                    var databaseTreeViewItem = AddDatabaseToTreeView(databaseInfo.Value);
                    databaseTreeViewItem.Tag = databaseInfo.Value.Value;
                    ItemDatabases.Items.Add(databaseTreeViewItem);
                }

                if (!string.IsNullOrWhiteSpace(_fatalError))
                {
                    var errorItem = new TreeViewItem();
                    errorItem.Header = _fatalError;
                    errorItem.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
                    ItemDatabases.Items.Add(errorItem);
                    return;
                }
                ItemDatabases.Items.Add(TreeViewHelper.GetTypesItem(ItemDatabases));
                ItemDatabases.IsExpanded = true;
                //TreeViewHelper.GetInfoItems(InfoStack);
            }
            catch (Exception ex)
            {
                var errorItem = new TreeViewItem();
                errorItem.Header = ex.Message;
                errorItem.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
                ItemDatabases.Items.Add(errorItem);
                return;                
            }
        }

        private TreeViewItem AddDatabaseToTreeView(KeyValuePair<string, DatabaseInfo> database)
        {
            string caption = database.Value.Caption;
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
                    break;
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

            databaseTreeViewItem.ContextMenu = new DatabaseContextMenu(new DatabaseMenuCommandParameters
            {
                ExplorerControl = this,
                DatabaseInfo = database.Value
            }, _parentWindow);

            databaseTreeViewItem.Items.Clear();

            var tables = TreeViewHelper.CreateTreeViewItemWithImage("Tables", "../Resources/folder_Closed_16xLG.png", true);
            tables.ContextMenu = new TablesContextMenu(new DatabaseMenuCommandParameters
            {
                DatabaseInfo = database.Value,
                ExplorerControl = this
            },
            _parentWindow);

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
                views.Expanded += (sender, args) => new GetViewsItemsHandler(GetViews).BeginInvoke(sender, args, database, null, null);
                databaseTreeViewItem.Items.Add(views);

                var triggers = TreeViewHelper.CreateTreeViewItemWithImage("Triggers", "../Resources/folder_Closed_16xLG.png", true);
                triggers.Expanded += (sender, args) => new GetTriggersItemsHandler(GetTriggers).BeginInvoke(sender, args, database, null, null);
                databaseTreeViewItem.Items.Add(triggers);
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
                    using (var _repository = Helpers.DataConnectionHelper.CreateRepository(database.Value))
                    {
                        nameList = _repository.GetAllSubscriptionNames();
                    }
                }
                catch (Exception e)
                {
                    Helpers.DataConnectionHelper.SendError(e, DatabaseType.SQLCE35, false);
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
                    Helpers.DataConnectionHelper.SendError(e, DatabaseType.SQLCE35);
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

                foreach (var column in columns.Where(c=>c.TableName == tableName))
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
            if (viewItem != null && (viewItem.Items.Count > 0 && viewItem.Items[0].ToString() == "Loading..."))
            {
                var viewList = new List<View>();
                try
                {
                    using (var _repository = Helpers.DataConnectionHelper.CreateRepository(database.Value))
                    {
                        viewList = _repository.GetAllViews();
                    }
                }
                catch (Exception e)
                {
                    Helpers.DataConnectionHelper.SendError(e, database.Value.DatabaseType);
                    return;
                }
                Dispatcher.BeginInvoke(new FillViewItemsHandler(FillViewItems), database, viewItem, viewList);
            }
            args.Handled = true;
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

        private delegate void GetTriggersItemsHandler(object sender, RoutedEventArgs args, KeyValuePair<string, DatabaseInfo> database);
        private void GetTriggers(object sender, RoutedEventArgs args, KeyValuePair<string, DatabaseInfo> database)
        {
            var viewItem = sender as DatabaseTreeViewItem;
            // Prevent loading again and again
            if (viewItem != null && (viewItem.Items.Count > 0 && viewItem.Items[0].ToString() == "Loading..."))
            {
                var triggerList = new List<ErikEJ.SqlCeScripting.Trigger>();
                try
                {
                    using (var _repository = Helpers.DataConnectionHelper.CreateRepository(database.Value))
                    {
                        triggerList = _repository.GetAllTriggers();
                    }
                }
                catch (Exception e)
                {
                    Helpers.DataConnectionHelper.SendError(e, database.Value.DatabaseType);
                    return;
                }
                Dispatcher.BeginInvoke(new FillTriggerItemsHandler(FillTriggerItems), database, viewItem, triggerList);
            }
            args.Handled = true;
        }

        private delegate void FillTriggerItemsHandler(KeyValuePair<string, DatabaseInfo> database, DatabaseTreeViewItem parentItem, IList<ErikEJ.SqlCeScripting.Trigger> childItems);
        private static void FillTriggerItems(KeyValuePair<string, DatabaseInfo> database, DatabaseTreeViewItem parentItem, IList<ErikEJ.SqlCeScripting.Trigger> triggerList)
        {
            parentItem.Items.Clear();
            foreach (var trigger in triggerList)
            {
                var item = TreeViewHelper.CreateTreeViewItemWithImage(trigger.TriggerName, "../Resources/RunOutline.png", false);
                item.ContextMenu = new TriggerContextMenu(new MenuCommandParameters { DatabaseInfo = database.Value, Name = trigger.TriggerName, MenuItemType = MenuType.Manage, Description = trigger.Definition }, _parentWindow);
                parentItem.Items.Add(item);
            }
        }

        private delegate void GetTableItemsHandler(object sender, RoutedEventArgs args, KeyValuePair<string, DatabaseInfo> database);
        private void GetTableItems(object sender, RoutedEventArgs args, KeyValuePair<string, DatabaseInfo> database)
        {
            var viewItem = sender as DatabaseTreeViewItem;

            Exception ex = null;

            // Prevent loading again and again
            bool doShow = false;
            if (args != null && viewItem != null && (viewItem.Items.Count > 0 && viewItem.Items[0].ToString() == "Loading..."))
            {
                doShow = true;
            }
            if (args == null && viewItem != null)
            {
                doShow = true;
            }
            if (doShow)
            {
                try
                {
                    using (var _repository = Helpers.DataConnectionHelper.CreateRepository(database.Value))
                    {
                        var test = _repository.ExecuteSql("SELECT 1" + Environment.NewLine + "GO");
                    }
                }
                catch (Exception e)
                {
                    ex = e;
                }
                Dispatcher.BeginInvoke(new FillTableItemsHandler(FillTableItems), database, viewItem, ex, args);
            }
            if (args != null)
            {
                args.Handled = true;
            }
        }

        private delegate void FillTableItemsHandler(KeyValuePair<string, DatabaseInfo> database, DatabaseTreeViewItem parentItem, Exception ex, RoutedEventArgs args);
        private void FillTableItems(KeyValuePair<string, DatabaseInfo> database, DatabaseTreeViewItem parentItem, Exception ex, RoutedEventArgs args)
        {
            if (ex != null)
            {
                string error = Helpers.DataConnectionHelper.CreateEngineHelper(database.Value.DatabaseType).FormatError(ex);
                if (error.Contains("Minor Err.: 25028"))
                {
                    PasswordDialog pwd = new PasswordDialog();
                    pwd.ShowModal();
                    if (pwd.DialogResult.HasValue && pwd.DialogResult.Value == true && !string.IsNullOrWhiteSpace(pwd.Password))
                    {
                        database.Value.ConnectionString = database.Value.ConnectionString + ";Password=" + pwd.Password;
                        GetTableItems(parentItem, args, database);
                    }
                }
                else
                {
                    Helpers.DataConnectionHelper.SendError(ex, database.Value.DatabaseType, false);
                }
                return;
            }
            try
            {
                parentItem.Items.Clear();

                using (var _repository = Helpers.DataConnectionHelper.CreateRepository(database.Value))
                {
                    DescriptionCache = new Helpers.DescriptionHelper().GetDescriptions(database.Value);
                    var dbdesc = DescriptionCache.Where(dc => dc.Parent == null && dc.Object == null).Select(dc => dc.Description).SingleOrDefault();
                    if (!string.IsNullOrWhiteSpace(dbdesc))
                    {
                        if (parentItem.Parent is DatabaseTreeViewItem)
                        {
                            var dbItem = (DatabaseTreeViewItem)parentItem.Parent;
                            dbItem.ToolTip = dbItem.ToolTip.ToString() + Environment.NewLine + dbdesc;
                        }
                    }
                    if (parentItem.Parent is DatabaseTreeViewItem && Properties.Settings.Default.DisplayObjectProperties)
                    {
                        var dbItem = (DatabaseTreeViewItem)parentItem.Parent;
                        var dbInfo = dbItem.Tag as DatabaseInfo;
                        if (dbInfo != null)
                        {
                            foreach (var values in _repository.GetDatabaseInfo())
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
                    var tables = _repository.GetAllTableNames();
                    var columns = _repository.GetAllColumns();
                    var primaryKeys = _repository.GetAllPrimaryKeys();
                    var foreignKeys = _repository.GetAllForeignKeys();
                    var indexes = _repository.GetAllIndexes();

                    foreach (var table in tables)
                    {
                        if (!Properties.Settings.Default.DisplayDescriptionTable && table.Equals("__ExtendedProperties"))
                        {
                            continue;
                        }
                        var item = TreeViewHelper.CreateTreeViewItemWithImage(table, "../Resources/table_16xLG.png", true);
                        item.ContextMenu = new TableContextMenu(new MenuCommandParameters { DatabaseInfo = database.Value, Name = table, MenuItemType = MenuType.Table }, _parentWindow);
                        item.ToolTip = table;
                        item.Tag = new TableInfo { Name = table, RowCount = _repository.GetRowCount(table) };
                        if (DescriptionCache != null)
                        {
                            var desc = DescriptionCache.Where(dc => dc.Parent == null && dc.Object == table).Select(dc => dc.Description).SingleOrDefault();
                            if (!string.IsNullOrWhiteSpace(desc))
                            {
                                item.ToolTip = desc;
                            }
                        }

                        var tableColumns = (from col in columns
                                            where col.TableName == table
                                            select col).ToList<Column>();
                        var tablePrimaryKeys = primaryKeys.Where(pk => pk.TableName == table).ToList();
                        var tableForeignKeys = foreignKeys.Where(fk => fk.ConstraintTableName == table).ToList();
                        var tableIndexes = indexes.Where(i => i.TableName == table).ToList();
                        parentItem.Items.Add(item);
                        item.Expanded += (s, e) => GetTableColumns(s, e, tableColumns, tableForeignKeys, tablePrimaryKeys, tableIndexes, database);
                    }
                }
            }
            catch (Exception ex2)
            {
                Helpers.DataConnectionHelper.SendError(ex2, database.Value.DatabaseType, false);
            }
        }

        private static void GetTableColumns(object sender, RoutedEventArgs args, List<Column> columns, List<Constraint> fkList, List<PrimaryKey> pkList, List<Index> indexes, KeyValuePair<string, DatabaseInfo> database)
        {
            var viewItem = sender as DatabaseTreeViewItem;
            // Prevent loading again and again
            if (viewItem != null && (viewItem.Items.Count > 0 && viewItem.Items[0].ToString() == "Loading..."))
            {
                viewItem.Items.Clear();
                var tableName = viewItem.MetaData;

        
                var pks = (from pk in pkList
                            select pk.ColumnName).ToList<string>();

                foreach (var column in columns)
                {
                    var display = column.ShortType;
                    var image = "../Resources/column_16xLG.png";

                    var constraints = (from fk in fkList
                                        where fk.Columns.Contains(column.ColumnName)
                                        select fk);
                    if (constraints.Count() > 0)
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
                    if (oldName != index.IndexName)
                    {
                        var display = string.Empty;
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
                }
                viewItem.Items.Add(indexesItem);

                var keysItem = TreeViewHelper.CreateTreeViewItemWithImage("Keys", "../Resources/folder_Closed_16xLG.png", true);

                keysItem.Items.Clear();

                foreach (var primaryKey in pkList)
                {
                    if (oldName != primaryKey.KeyName)
                    {
                        var display = primaryKey.KeyName;
                        var keyItem = TreeViewHelper.CreateTreeViewItemWithImage(display, "../Resources/KeyDown_8461.png", false);
                        if (database.Value.DatabaseType != DatabaseType.SQLite)
                            keyItem.ContextMenu = new KeyContextMenu(new MenuCommandParameters { Description = primaryKey.KeyName, DatabaseInfo = database.Value, Name = viewItem.MetaData, MenuItemType = MenuType.PK }, _parentWindow);
                        keyItem.ToolTip = primaryKey.KeyName;
                        keysItem.Items.Add(keyItem);
                        oldName = primaryKey.KeyName;
                    }
                }

                foreach (var fk in fkList)
                {
                    var display = fk.ConstraintName;
                    var keyItem = TreeViewHelper.CreateTreeViewItemWithImage(display, "../Resources/KeyDownFk_8461.png", false);
                    if (database.Value.DatabaseType != DatabaseType.SQLite)
                        keyItem.ContextMenu = new KeyContextMenu(new MenuCommandParameters { Description = fk.ConstraintName, DatabaseInfo = database.Value, Name = viewItem.MetaData, MenuItemType = MenuType.FK }, _parentWindow);
                    keyItem.ToolTip = fk.ConstraintName;
                    keysItem.Items.Add(keyItem);
                }

                viewItem.Items.Add(keysItem);

            }
            args.Handled = true;
        }

        public void RefreshTables(DatabaseInfo databaseInfo)
        {
            var item = FindTablesItem(databaseInfo);
            if (item != null)
                GetTableItems(item, null, new KeyValuePair<string, DatabaseInfo>(databaseInfo.Caption, databaseInfo));
        }

        private DatabaseTreeViewItem FindTablesItem(DatabaseInfo databaseInfo)
        {
            if (ItemDatabases.HasItems)
            {
                foreach (var item in ItemDatabases.Items)
                {
                    var dbItem = item as DatabaseTreeViewItem;
                    if (dbItem != null)
                    {
                        var dbInfo = dbItem.Tag as DatabaseInfo;
                        if (dbInfo != null)
                        {
                            if (dbInfo.ConnectionString == databaseInfo.ConnectionString)
                            {
                                if (dbItem.HasItems)
                                {
                                    foreach (var tabItem in dbItem.Items)
                                    {
                                        var tabFoundItem = tabItem as DatabaseTreeViewItem;
                                        if (tabFoundItem != null && tabFoundItem.ToString() == "Tables")
                                            return tabFoundItem;
                                    }
                                }
                            }
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
            Helpers.DataConnectionHelper.LogUsage("ToolbarRefresh");
        }

        private void ToolbarAbout_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AboutDialog();
            dialog.ShowModal();
            Helpers.DataConnectionHelper.LogUsage("ToolbarAbout");
        }

        private void Feedback_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://visualstudiogallery.msdn.microsoft.com/0e313dfd-be80-4afb-b5e9-6e74d369f7a1/view/Reviews");
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://sqlcetoolbox.codeplex.com/");
            Helpers.DataConnectionHelper.LogUsage("ToolbarUpdate");
        }

        private void Options_Click(object sender, RoutedEventArgs e)
        {
            var package = _parentWindow.Package as SqlCeToolboxPackage;
            if (package == null) return;
            package.ShowOptionPage(typeof(OptionsPageGeneral));
            Helpers.DataConnectionHelper.LogUsage("ToolbarOptions");
        }

        private void RuntimeMissing_Click(object sender, RoutedEventArgs e)
        {
            if (!DataConnectionHelper.IsV35Installed() && !DataConnectionHelper.IsV40Installed())
            {
                EnvDTEHelper.ShowMessage("The SQL Server Compact 3.5 SP2 and 4.0 runtimes are not properly installed,\r\nso many features are not available,\r\ninstall or repair SQL Server Compact 3.5 SP2 or 4.0 Desktop to remedy");
            }
        }

        #region Properties Windows
        private void TreeView1_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (Properties.Settings.Default.DisplayObjectProperties)
            {
                var databaseTreeviewItem = TreeView1.SelectedItem as DatabaseTreeViewItem;
                if (databaseTreeviewItem != null && databaseTreeviewItem.Tag != null)
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
                            if (databaseTreeviewItem != null && databaseTreeviewItem.Tag != null)
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
                    catch { }
                }
                var focusItem = TreeView1.SelectedItem as DatabaseTreeViewItem;
                if (focusItem != null)
                {
                    focusItem.Focus();
                }
            }
            else
            {
                TrackSelection(null);
            }
        }

        // http://msdn.microsoft.com/en-us/library/cc138529.aspx
        private void TrackSelection(object info)
        {
            ShowPropertiesFrame();

            if (mySelContainer == null)
            {
                mySelContainer = new SelectionContainer();
            }

            mySelItems = new System.Collections.ArrayList();

            if (info != null)
            {
                mySelItems.Add(info);
            }

            mySelContainer.SelectedObjects = mySelItems;

            //Must use the GetService of the Window to get the ITrackSelection reference
            var track = _parentWindow.GetServiceHelper(typeof(STrackSelection)) as ITrackSelection;
            if (track != null)
            {
                track.OnSelectChange(mySelContainer);
            }
        }

        private void ShowPropertiesFrame()
        {
            if (Properties.Settings.Default.DisplayObjectProperties)
            {
                var package = _parentWindow.Package as SqlCeToolboxPackage;
                if (package == null) return;
                if (frame == null)
                {
                    var shell = package.GetServiceHelper(typeof(SVsUIShell)) as IVsUIShell;
                    if (shell != null)
                    {
                        var guidPropertyBrowser = new
                            Guid(ToolWindowGuids.PropertyBrowser);
                        shell.FindToolWindow((uint)__VSFINDTOOLWIN.FTW_fForceCreate,
                            ref guidPropertyBrowser, out frame);
                        if (frame != null)
                        {
                            frame.Show();
                            //IVsWindowFrame thisFrame = (IVsWindowFrame)_parentWindow.Frame;
                            //thisFrame.Show();
                        }
                    }
                }
            }
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
                        var helper = DataConnectionHelper.CreateEngineHelper(databaseInfo.DatabaseType);
                        Clipboard.Clear();
                        Clipboard.SetData(DataFormats.FileDrop, new String[] { helper.PathFromConnectionString(databaseInfo.ConnectionString) });
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.DataConnectionHelper.SendError(ex, DatabaseType.SQLServer, true);
            }
        }       
    }
}
