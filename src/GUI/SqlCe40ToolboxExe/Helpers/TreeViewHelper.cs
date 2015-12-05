using System.Windows;
using System.Windows.Controls;

namespace ErikEJ.SqlCeToolbox.Helpers
{
    public class TreeViewHelper
    {
        public static DatabaseTreeViewItem CreateTreeViewItemWithImage(string name, string imageName, bool showExpander, string toolTip = null, bool isRefreshable = false)
        {
            var stackpanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new System.Windows.Thickness(2) };
            stackpanel.Children.Add(ImageHelper.GetImageFromResource(imageName));
            stackpanel.Children.Add(new TextBlock { Text = " " + name });

            var databaseTreeViewItem = new DatabaseTreeViewItem
                {
                    Header = stackpanel, 
                    MetaData = name, 
                    IsRefreshable = isRefreshable
                };

            databaseTreeViewItem.MouseRightButtonDown += DatabaseTreeViewItemMouseRightButtonDown;
            databaseTreeViewItem.ContextMenu = new ContextMenu { Visibility = Visibility.Hidden };

            if (!string.IsNullOrWhiteSpace(toolTip))
                databaseTreeViewItem.ToolTip = toolTip;

            if (showExpander)
                databaseTreeViewItem.AddLoadingChildNode(databaseTreeViewItem);

            return databaseTreeViewItem;
        }

        static void DatabaseTreeViewItemMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ((TreeViewItem)sender).IsSelected = true;
            e.Handled = true;
        }
    }
}