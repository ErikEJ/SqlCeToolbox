using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ErikEJ.SqlCeToolbox.Commands;
using ErikEJ.SqlCeToolbox.ContextMenues;
using ErikEJ.SqlCeToolbox.Dialogs;
using ErikEJ.SqlCeToolbox.Helpers;
using ErikEJ.SqlCeScripting;
using System.ComponentModel;
using System.Windows.Media.Animation;

namespace ErikEJ.SqlCeToolbox.ToolWindows
{
    /// <summary>
    /// Interaction logic for ExplorerControl.xaml
    /// </summary>
    public partial class ExplorerControl : UserControl
    {
        private bool _loaded;
        //private static IRepository _repository;
        private FabTab.FabTabControl _fabTab;
        private Storyboard myStoryboard;

        public static List<DbDescription> DescriptionCache { get; set; }

        public FabTab.FabTabControl FabTab
        {
            get { return _fabTab; }
            set { _fabTab = value; }
        }

        public ExplorerControl(FabTab.FabTabControl fabTab)
        {
            _fabTab = fabTab;
            InitializeComponent();
        }

        private void MyToolWindow_Loaded(object sender, RoutedEventArgs e)
        {
            
            if (!_loaded)
            {
                Updated.Visibility = System.Windows.Visibility.Hidden;
                // Look for update async
                BackgroundWorker bw = new BackgroundWorker();
                bw.DoWork += new DoWorkEventHandler(bw_DoWork);
                bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);

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
            }

            _loaded = true;
        }

        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((Boolean)e.Result == true)
            {
                Updated.Visibility = System.Windows.Visibility.Visible;
                myStoryboard.Begin(this);
            }
            else
            {
                Updated.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = Helpers.DataConnectionHelper.CheckVersion("exe40");
        }

        public void BuildDatabaseTree()
        {
            SortedDictionary<string, string> databaseList = new SortedDictionary<string, string>();
            try
            {
                Refresh.IsEnabled = false;
                this.txtConnections.Text = "SQL Server Compact Data Connections";
                ItemDatabases.ContextMenu = new DatabasesContextMenu(new DatabasesMenuCommandParameters
                {
                    ExplorerControl = this,
                    DatabasesTreeViewItem = ItemDatabases
                }, this);
                ItemDatabases.Items.Clear();

                ItemDatabases.IsExpanded = true;

                if (!DataConnectionHelper.IsRuntimeInstalled())
                {
                    var errorItem = new TreeViewItem();
                    errorItem.Header = string.Format("The SQL Server Compact {0}\r\nprovider could not be found,\r\ninstall or repair SQL Server Compact {1}", RepoHelper.apiVer, RepoHelper.apiVer);
                    errorItem.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Yellow);
                    ItemDatabases.Items.Add(errorItem);
                    return;
                }

                ItemDatabases.Items.Add("Loading...");
                ItemDatabases.IsExpanded = true;
                databaseList = DataConnectionHelper.GetDataConnections();
                if (databaseList.Count == 0)
                {
                    this.txtConnections.Text = "No SQL Server Compact Data Connections found,\r\n select Add from the context menu to add one.";
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error getting list of databases from Data Connections, make sure to create one. " + e.Message,
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            finally
            {
                var fillList = new FillDatabaseListHandler(FillDatabaseList);
                Dispatcher.BeginInvoke(fillList, databaseList); //fill the tree on the UI thread
                Refresh.IsEnabled = true;
            }

            FocusSelectedItem();
        }


        private delegate void FillDatabaseListHandler(SortedDictionary<string, string> databaseList);

        private void FillDatabaseList(SortedDictionary<string, string> databaseList)
        {
            ItemDatabases.Items.Clear();
            foreach (var databaseName in databaseList)
            {
                var databaseTreeViewItem = AddDatabaseToTreeView(databaseName);
                ItemDatabases.Items.Add(databaseTreeViewItem);
            }
            ItemDatabases.Items.Add(GetTypesItem(ItemDatabases));
            ItemDatabases.IsExpanded = true;
        }

        private TreeViewItem AddDatabaseToTreeView(KeyValuePair<string, string> database)
        {
            var databaseTreeViewItem = TreeViewHelper.CreateTreeViewItemWithImage(database.Key, "../Resources/database.png", false);
            databaseTreeViewItem.ToolTip = database.Value;
            databaseTreeViewItem.ContextMenu = new DatabaseContextMenu(new DatabaseMenuCommandParameters
            {
                ExplorerControl = this,
                Connectionstring = database.Value,
                Caption = database.Key,
            }, this);
            databaseTreeViewItem.Items.Clear();

            var tables = TreeViewHelper.CreateTreeViewItemWithImage("Tables", "../Resources/folder.png", true, null, true);
            tables.Expanded += (sender, args) => new GetTableItemsHandler(GetTableItems).BeginInvoke(sender, args, database, null, null);
            databaseTreeViewItem.Items.Add(tables);

#if V35
            var subscriptions = TreeViewHelper.CreateTreeViewItemWithImage("Subscriptions", "../Resources/folder.png", true);
            subscriptions.ContextMenu = new SubscriptionsContextMenu(new MenuCommandParameters
            {
                MenuItemType = MenuCommandParameters.MenuType.Function,
                Connectionstring = database.Key,
                Caption = database.Value
            }, this);
            subscriptions.Expanded += (sender, args) => new GetSubsItemsHandler(GetSubscriptions).BeginInvoke(sender, args, database, null, null);
            databaseTreeViewItem.Items.Add(subscriptions);
#endif
            return databaseTreeViewItem;
        }
#if V35
        private delegate void GetSubsItemsHandler(object sender, RoutedEventArgs args, KeyValuePair<string, string> database);

        private void GetSubscriptions(object sender, RoutedEventArgs args, KeyValuePair<string, string> database)
        {
            var viewItem = sender as DatabaseTreeViewItem;
            // Prevent loading again and again
            if (viewItem != null && (viewItem.Items.Count > 0 && viewItem.Items[0].ToString() == "Loading..."))
            {
                List<string> nameList;
                try
                {
                    using (var _repository = new DBRepository(database.Value))
                    {
                        nameList = _repository.GetAllSubscriptionNames();
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("Exception getting subscription list: " + e.Message,
                                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                Dispatcher.BeginInvoke(new FillSubscriptionItemsHandler(FillSubItems), database, viewItem, nameList);
            }
            args.Handled = true;
        }

        private delegate void FillSubscriptionItemsHandler(KeyValuePair<string, string> database, DatabaseTreeViewItem parentItem, IList<string> childItems);

        private void FillSubItems(KeyValuePair<string, string> database, DatabaseTreeViewItem parentItem, IList<string> nameList)
        {
            parentItem.Items.Clear();
            foreach (var sub in nameList)
            {
                var item = TreeViewHelper.CreateTreeViewItemWithImage(sub, "../Resources/subs.png", false);
                item.ContextMenu = new SubscriptionsContextMenu(new MenuCommandParameters { Connectionstring = database.Value, Name = sub, MenuItemType = MenuCommandParameters.MenuType.Manage, Caption = database.Key }, this);
                parentItem.Items.Add(item);
            }
        }

#endif
        private delegate void GetTableItemsHandler(object sender, RoutedEventArgs args, KeyValuePair<string, string> database);

        private void GetTableItems(object sender, RoutedEventArgs args, KeyValuePair<string, string> database)
        {
            var viewItem = sender as DatabaseTreeViewItem;
            // Prevent loading again and again
            if (viewItem != null && (viewItem.Items.Count > 0 && viewItem.Items[0].ToString() == "Loading..."))
            {
                IList<string> nameList;
                try
                {
                    using (var _repository = RepoHelper.CreateRepository(database.Value))
                    {
                        nameList = _repository.GetAllTableNames();
                    }
                    DescriptionCache = new Helpers.DescriptionHelper().GetDescriptions(database.Value);

                }
                catch (Exception e)
                {
                    MessageBox.Show("Exception getting table list: " + e.Message,
                                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                Dispatcher.BeginInvoke(new FillTableItemsHandler(FillTableItems), database, viewItem, nameList);
            }
            args.Handled = true;
        }

        private delegate void FillTableItemsHandler(KeyValuePair<string, string> database, DatabaseTreeViewItem parentItem, IList<string> childItems);

        private void FillTableItems(KeyValuePair<string, string> database, DatabaseTreeViewItem parentItem, IList<string> nameList)
        {
            parentItem.Items.Clear();

            if (DescriptionCache != null)
            {
                var dbdesc = DescriptionCache.Where(dc => dc.Parent == null && dc.Object == null).Select(dc => dc.Description).SingleOrDefault();
                if (!string.IsNullOrWhiteSpace(dbdesc))
                {
                    if (parentItem.Parent is DatabaseTreeViewItem)
                    {
                        var dbItem = (DatabaseTreeViewItem)parentItem.Parent;
                        dbItem.ToolTip = dbItem.ToolTip.ToString() + Environment.NewLine + dbdesc;
                    }
                }
            }

            using (var _repository = RepoHelper.CreateRepository(database.Value))
            {
                var columns = _repository.GetAllColumns();
                foreach (var table in nameList)
                {
                    if (!Properties.Settings.Default.DisplayDescriptionTable && table.Equals("__ExtendedProperties"))
                    {
                        continue;
                    }
                    var item = TreeViewHelper.CreateTreeViewItemWithImage(table, "../Resources/table.png", true, null, true);
                    item.ContextMenu = new TableContextMenu(new MenuCommandParameters { Connectionstring = database.Value, Name = table, MenuItemType = MenuCommandParameters.MenuType.Table, Caption = database.Key }, this);
                    item.ToolTip = table;
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
                    parentItem.Items.Add(item);
                    item.Expanded += (s, e) => GetTableColumns(s, e, tableColumns, database);
                }
            }
        }

        private void GetTableColumns(object sender, RoutedEventArgs args, List<Column> columns, KeyValuePair<string, string> database)
        {
            var viewItem = sender as DatabaseTreeViewItem;

            // Prevent loading again and again
            if (viewItem != null && (viewItem.Items.Count > 0 && viewItem.Items[0].ToString() == "Loading..."))
            {
                var tableName = viewItem.MetaData;

                viewItem.Items.Clear();

                using (var _repository =  RepoHelper.CreateRepository(database.Value))
                {
                    // If the node is being refreshed by the user, make sure to reload the columns instead of using the cached ones from the 
                    // previous load of the entire database tree
                    if (viewItem.IsRefreshing)
                    {
                        columns = _repository.GetAllColumns().Where(x => x.TableName == tableName).ToList();
                    }
                    
                    var pkList = _repository.GetAllPrimaryKeys().Where(p => p.TableName == tableName);
                    var pks = (from pk in pkList
                               select pk.ColumnName).ToList<string>();

                    var fkList = _repository.GetAllForeignKeys().Where(fk => fk.ConstraintTableName == tableName).ToList();

                    foreach (var column in columns)
                    {
                        var display = column.ShortType;
                        var image = "../Resources/column.png";

                        var constraints = (from fk in fkList
                                           where fk.Columns.Contains(column.ColumnName)
                                           select fk);
                        if (constraints.Count() > 0)
                        {
                            display = "FK, " + display;
                            image = "../Resources/fk.png";
                        }
                        if (pks.Contains(column.ColumnName))
                        {
                            display = "PK, " + display;
                            image = "../Resources/key.png";
                        }

                        string nullable = " not null)";
                        if (column.IsNullable == YesNoOption.YES)
                        {
                            nullable = " null)";
                        }
                        display = column.ColumnName + " (" + display + nullable;
                        var i = TreeViewHelper.CreateTreeViewItemWithImage(display, image, false);
                        i.ContextMenu = new ColumnContextMenu(new MenuCommandParameters { Description = tableName, Connectionstring = database.Value, Name = column.ColumnName, MenuItemType = MenuCommandParameters.MenuType.Table }, this);
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
                    var indexesItem = TreeViewHelper.CreateTreeViewItemWithImage("Indexes", "../Resources/folder.png", true);
                    
                    indexesItem.Items.Clear();
                    string oldName = string.Empty;
                    foreach (var primaryKey in pkList)
                    {
                        if (oldName != primaryKey.KeyName)
                        {
                            var display = primaryKey.KeyName + " (Primary Key)";
                            var indexItem = TreeViewHelper.CreateTreeViewItemWithImage(display, "../Resources/index.png", false);
                            
                            indexItem.ContextMenu = new IndexContextMenu(new MenuCommandParameters { Connectionstring = database.Value, Name = viewItem.MetaData, MenuItemType = MenuCommandParameters.MenuType.Table, Caption = primaryKey.KeyName }, this);
                            indexItem.ToolTip = primaryKey.KeyName;
                            indexesItem.Items.Add(indexItem);
                            oldName = primaryKey.KeyName;
                        }
                    }

                    oldName = string.Empty;
                    var indexes = _repository.GetIndexesFromTable(viewItem.MetaData);

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
                            var indexItem = TreeViewHelper.CreateTreeViewItemWithImage(display, "../Resources/index.png", false);
                            indexItem.ContextMenu = new IndexContextMenu(new MenuCommandParameters { Connectionstring = database.Value, Name = viewItem.MetaData, MenuItemType = MenuCommandParameters.MenuType.Table, Caption = index.IndexName }, this);
                            indexItem.ToolTip = index.IndexName;
                            indexesItem.Items.Add(indexItem);
                            oldName = index.IndexName;
                        }
                    }
                    viewItem.Items.Add(indexesItem);
                }
            }
            args.Handled = true;
        }

        private TreeViewItem GetTypesItem(TreeViewItem viewItem)
        {
            var types = TreeViewHelper.CreateTreeViewItemWithImage("Data Types", "../Resources/folder.png", false);

            var numbersItem = TreeViewHelper.CreateTreeViewItemWithImage("Exact Numerics", "../Resources/folder.png", false);
            numbersItem.Items.Add(TreeViewHelper.CreateTreeViewItemWithImage("bit", "../Resources/sp.png", false, SqlCeToolbox.Resources.bit));
            numbersItem.Items.Add(TreeViewHelper.CreateTreeViewItemWithImage("tinyint", "../Resources/sp.png", false, SqlCeToolbox.Resources.tinyint));
            numbersItem.Items.Add(TreeViewHelper.CreateTreeViewItemWithImage("smallint", "../Resources/sp.png", false, SqlCeToolbox.Resources.smallint));
            numbersItem.Items.Add(TreeViewHelper.CreateTreeViewItemWithImage("int", "../Resources/sp.png", false, SqlCeToolbox.Resources.integer));
            numbersItem.Items.Add(TreeViewHelper.CreateTreeViewItemWithImage("bigint", "../Resources/sp.png", false, SqlCeToolbox.Resources.bigint));
            numbersItem.Items.Add(TreeViewHelper.CreateTreeViewItemWithImage("numeric", "../Resources/sp.png", false, SqlCeToolbox.Resources.numeric));
            numbersItem.Items.Add(TreeViewHelper.CreateTreeViewItemWithImage("money", "../Resources/sp.png", false, SqlCeToolbox.Resources.money));
            types.Items.Add(numbersItem);

            var floatItem = TreeViewHelper.CreateTreeViewItemWithImage("Approximate Numerics", "../Resources/folder.png", false);
            floatItem.Items.Add(TreeViewHelper.CreateTreeViewItemWithImage("float", "../Resources/sp.png", false, SqlCeToolbox.Resources.floating));
            floatItem.Items.Add(TreeViewHelper.CreateTreeViewItemWithImage("real", "../Resources/sp.png", false, SqlCeToolbox.Resources.real));
            types.Items.Add(floatItem);

            var dateItem = TreeViewHelper.CreateTreeViewItemWithImage("Date and Time", "../Resources/folder.png", false);
            dateItem.Items.Add(TreeViewHelper.CreateTreeViewItemWithImage("datetime", "../Resources/sp.png", false, SqlCeToolbox.Resources.datetime));
            types.Items.Add(dateItem);

            var stringItem = TreeViewHelper.CreateTreeViewItemWithImage("Unicode Character Strings", "../Resources/folder.png", false);
            stringItem.Items.Add(TreeViewHelper.CreateTreeViewItemWithImage("nchar", "../Resources/sp.png", false, SqlCeToolbox.Resources.nchar));
            stringItem.Items.Add(TreeViewHelper.CreateTreeViewItemWithImage("nvarchar", "../Resources/sp.png", false, SqlCeToolbox.Resources.nvarchar));
            stringItem.Items.Add(TreeViewHelper.CreateTreeViewItemWithImage("ntext", "../Resources/sp.png", false, SqlCeToolbox.Resources.ntext));
            types.Items.Add(stringItem);

            var binaryItem = TreeViewHelper.CreateTreeViewItemWithImage("Binary Values", "../Resources/folder.png", false);
            binaryItem.Items.Add(TreeViewHelper.CreateTreeViewItemWithImage("binary", "../Resources/sp.png", false, SqlCeToolbox.Resources.binary));
            binaryItem.Items.Add(TreeViewHelper.CreateTreeViewItemWithImage("varbinary", "../Resources/sp.png", false, SqlCeToolbox.Resources.varbinary));
            binaryItem.Items.Add(TreeViewHelper.CreateTreeViewItemWithImage("image", "../Resources/sp.png", false, SqlCeToolbox.Resources.image));
            types.Items.Add(binaryItem);

            var otherItem = TreeViewHelper.CreateTreeViewItemWithImage("Other Data Types", "../Resources/folder.png", false);
            otherItem.Items.Add(TreeViewHelper.CreateTreeViewItemWithImage("rowversion", "../Resources/sp.png", false, SqlCeToolbox.Resources.rowversion));
            otherItem.Items.Add(TreeViewHelper.CreateTreeViewItemWithImage("uniqueidentifier", "../Resources/sp.png", false, SqlCeToolbox.Resources.uniqueidentifier));
            types.Items.Add(otherItem);

            return types;
        }


        private void TreeViewItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((TreeViewItem)sender).IsSelected = true;
            e.Handled = true;
        }

        private void ToolbarRefresh_Click(object sender, RoutedEventArgs e)
        {
            BuildDatabaseTree();
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://sqlcetoolbox.codeplex.com/");
        }

        private void ToolbarAbout_Click(object sender, RoutedEventArgs e)
        {
            ShowDialog(new AboutDialog());
        }

        private void Options_Click(object sender, RoutedEventArgs e)
        {
            ShowDialog(new OptionsDialog());
        }

        private static void ShowDialog(Window window)
        {
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
        }

        private void ExplorerControl_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                var selectedItem = TreeView1.SelectedItem as DatabaseTreeViewItem;

                bool wasRefreshed = false;

                if (selectedItem != null)
                    wasRefreshed = selectedItem.Refresh();

                if (!wasRefreshed)
                    BuildDatabaseTree();
            }
        }

        public void FocusSelectedItem()
        {
            var selectedItem = TreeView1.SelectedItem as TreeViewItem;

            if (selectedItem != null)
                selectedItem.Focus();
            else
                ItemDatabases.Focus();
        }
    }
}