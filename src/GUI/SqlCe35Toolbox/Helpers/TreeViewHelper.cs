using System.ServiceModel.Syndication;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;

namespace ErikEJ.SqlCeToolbox.Helpers
{
    public class DatabaseTreeViewItem : TreeViewItem
    {
        public string MetaData { get; set; }

        public override string ToString()
        {
            return MetaData;
        }
    }

    public sealed class TreeViewHelper
    {
        private TreeViewHelper()
        { }

        public static DatabaseTreeViewItem CreateTreeViewItemWithImage(string name, string imageName, bool showExpander)
        {
            return CreateTreeViewItemWithImageAndTooltip(name, imageName, showExpander, null);
        }

        public static Button CreateButtonWithHyperlink(string name, string url)
        {
            var button = new Button();
            button.HorizontalContentAlignment = HorizontalAlignment.Left;
            button.Content = new TextBlock { Text = " " + name, Foreground = new SolidColorBrush(Colors.SteelBlue) };
            button.BorderThickness = new Thickness(0);
            button.Background = VSThemes.GetToolWindowBackground();
            button.Tag = url;
            button.Click += new RoutedEventHandler(button_Click);
            return button;
        }

        static void button_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as Button;
            if (item != null)
            {
                EnvDTEHelper.LaunchUrl(item.Tag as string);
            }
        }

        public static DatabaseTreeViewItem CreateTreeViewItemWithImageAndTooltip(string name, string imageName, bool showExpander, string toolTip)
        {
            var stackpanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new System.Windows.Thickness(2) };
            stackpanel.Children.Add(ImageHelper.GetImageFromResource(imageName));
            // 
            stackpanel.Children.Add(new TextBlock { Text = " " + name, Foreground = Helpers.VSThemes.GetWindowText()});

            var databaseTreeViewItem = new DatabaseTreeViewItem { Header = stackpanel, MetaData = name };
            databaseTreeViewItem.MouseRightButtonDown += DatabaseTreeViewItemMouseRightButtonDown;
            databaseTreeViewItem.ContextMenu = new ContextMenu { Visibility = Visibility.Hidden };
            if (!string.IsNullOrWhiteSpace(toolTip))
            {
                databaseTreeViewItem.ToolTip = toolTip;
            }
            if (showExpander) databaseTreeViewItem.Items.Add("Loading...");
            return databaseTreeViewItem;
        }

        static void DatabaseTreeViewItemMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ((TreeViewItem)sender).IsSelected = true;
            e.Handled = true;
        }

        public static void GetInfoItems(StackPanel infoItem)
        {
            if (infoItem.Children.Count > 0)
                return;
            try
            {
                XmlReader reader = XmlReader.Create("http://sqlcompact.dk/VSAddinFeed.xml");
                SyndicationFeed feed = SyndicationFeed.Load(reader);
                foreach (var item in feed.Items)
                {
                    infoItem.Children.Add(CreateButtonWithHyperlink(item.Title.Text, item.Links[0].Uri.OriginalString));
                }
            }
            catch { }
            infoItem.Children.Add(CreateButtonWithHyperlink("Everything SQL Server Compact blog", "http://erikej.blogspot.com/"));
        }

        public static TreeViewItem GetTypesItem(TreeViewItem viewItem)
        {
            var types = CreateTreeViewItemWithImage("SQL Server Compact Data Types", "../Resources/folder_Closed_16xLG.png", false);

            var numbersItem = CreateTreeViewItemWithImage("Exact Numerics", "../Resources/folder_Closed_16xLG.png", false);
            numbersItem.Items.Add(CreateTreeViewItemWithImageAndTooltip("bit", "../Resources/TypeDefinition_521.png", false, SqlCeToolbox.Resources.bit));
            numbersItem.Items.Add(CreateTreeViewItemWithImageAndTooltip("tinyint", "../Resources/TypeDefinition_521.png", false, SqlCeToolbox.Resources.tinyint));
            numbersItem.Items.Add(CreateTreeViewItemWithImageAndTooltip("smallint", "../Resources/TypeDefinition_521.png", false, SqlCeToolbox.Resources.smallint));
            numbersItem.Items.Add(CreateTreeViewItemWithImageAndTooltip("int", "../Resources/TypeDefinition_521.png", false, SqlCeToolbox.Resources.integer));
            numbersItem.Items.Add(CreateTreeViewItemWithImageAndTooltip("bigint", "../Resources/TypeDefinition_521.png", false, SqlCeToolbox.Resources.bigint));
            numbersItem.Items.Add(CreateTreeViewItemWithImageAndTooltip("numeric", "../Resources/TypeDefinition_521.png", false, SqlCeToolbox.Resources.numeric));
            numbersItem.Items.Add(CreateTreeViewItemWithImageAndTooltip("money", "../Resources/TypeDefinition_521.png", false, SqlCeToolbox.Resources.money));
            types.Items.Add(numbersItem);

            var floatItem = CreateTreeViewItemWithImage("Approximate Numerics", "../Resources/folder_Closed_16xLG.png", false);
            floatItem.Items.Add(CreateTreeViewItemWithImageAndTooltip("float", "../Resources/TypeDefinition_521.png", false, SqlCeToolbox.Resources.floating));
            floatItem.Items.Add(CreateTreeViewItemWithImageAndTooltip("real", "../Resources/TypeDefinition_521.png", false, SqlCeToolbox.Resources.real));
            types.Items.Add(floatItem);

            var dateItem = CreateTreeViewItemWithImage("Date and Time", "../Resources/folder_Closed_16xLG.png", false);
            dateItem.Items.Add(CreateTreeViewItemWithImageAndTooltip("datetime", "../Resources/TypeDefinition_521.png", false, SqlCeToolbox.Resources.datetime));
            types.Items.Add(dateItem);

            var stringItem = CreateTreeViewItemWithImage("Unicode Character Strings", "../Resources/folder_Closed_16xLG.png", false);
            stringItem.Items.Add(CreateTreeViewItemWithImageAndTooltip("nchar", "../Resources/TypeDefinition_521.png", false, SqlCeToolbox.Resources.nchar));
            stringItem.Items.Add(CreateTreeViewItemWithImageAndTooltip("nvarchar", "../Resources/TypeDefinition_521.png", false, SqlCeToolbox.Resources.nvarchar));
            stringItem.Items.Add(CreateTreeViewItemWithImageAndTooltip("ntext", "../Resources/TypeDefinition_521.png", false, SqlCeToolbox.Resources.ntext));
            types.Items.Add(stringItem);

            var binaryItem = CreateTreeViewItemWithImage("Binary Values", "../Resources/folder_Closed_16xLG.png", false);
            binaryItem.Items.Add(CreateTreeViewItemWithImageAndTooltip("binary", "../Resources/TypeDefinition_521.png", false, SqlCeToolbox.Resources.binary));
            binaryItem.Items.Add(CreateTreeViewItemWithImageAndTooltip("varbinary", "../Resources/TypeDefinition_521.png", false, SqlCeToolbox.Resources.varbinary));
            binaryItem.Items.Add(CreateTreeViewItemWithImageAndTooltip("image", "../Resources/TypeDefinition_521.png", false, SqlCeToolbox.Resources.image));
            types.Items.Add(binaryItem);

            var otherItem = CreateTreeViewItemWithImage("Other Data Types", "../Resources/folder_Closed_16xLG.png", false);
            otherItem.Items.Add(CreateTreeViewItemWithImageAndTooltip("rowversion", "../Resources/TypeDefinition_521.png", false, SqlCeToolbox.Resources.rowversion));
            otherItem.Items.Add(CreateTreeViewItemWithImageAndTooltip("uniqueidentifier", "../Resources/TypeDefinition_521.png", false, SqlCeToolbox.Resources.uniqueidentifier));
            types.Items.Add(otherItem);

            return types;
        }

    }
}