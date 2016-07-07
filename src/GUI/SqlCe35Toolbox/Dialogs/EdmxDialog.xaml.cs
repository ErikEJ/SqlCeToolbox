using System.Collections.Generic;
using System.Windows;
using ErikEJ.SqlCeToolbox.Helpers;

namespace ErikEJ.SqlCeToolbox.Dialogs
{
    /// <summary>
    /// Interaction logic for EdmxDialog.xaml
    /// </summary>
    public partial class EdmxDialog
    {
        public EdmxDialog()
        {
            Telemetry.TrackPageView(nameof(EdmxDialog));
            InitializeComponent();
            Background = VsThemes.GetWindowBackground();
        }

        private List<CheckListItem> items = new List<CheckListItem>();

        #region Properties
        public string ProjectName
        {
            get
            {
                return Title;
            }

            set
            {
                Title = string.Format("Generate EDM in Project {0}", value);
            }
        }

        public bool Pluralize 
        {
            get
            {
                return chkPlural.IsChecked != null && chkPlural.IsChecked.Value;
            }
        }
        public bool ForeignKeys
        {
            get
            {
                return chkFks2.IsChecked != null && chkFks2.IsChecked.Value;
            }
        }
        public bool SaveConfig 
        {
            get
            {
                return chkSave.IsChecked != null && chkSave.IsChecked.Value;
            }            
        }
        public bool AddPrivateConfig
        {
            get
            {
                return chkDeploy.IsChecked != null && chkDeploy.IsChecked.Value;
            }            
        
        }
        public void HideAddPrivateConfig()
        {
            chkDeploy.Visibility = Visibility.Collapsed;
        }
        public string ModelName 
        {
            get
            {
                return textBox1.Text;
            }
            set
            {
                textBox1.Text = value;
            }
        }
        public List<string> Tables { get; set; }

        #endregion

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Tables.Clear();
            foreach (object item in chkTables.Items)
            {
                var checkItem = (CheckListItem)item;
                if (checkItem.IsChecked)
                {
                    Tables.Add(checkItem.Label);
                }
            }
            Close();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            textBox1.Focus();
            foreach (string table in Tables)
            { 
                bool isChecked = !table.StartsWith("__");
                items.Add(new CheckListItem { IsChecked = isChecked, Label = table });
                
            }
            chkTables.ItemsSource = items;
        }

        private void chkSave_Checked(object sender, RoutedEventArgs e)
        {
            if (chkDeploy != null)
                chkDeploy.IsEnabled = true;
        }

        private void chkSave_Unchecked(object sender, RoutedEventArgs e)
        {
            if (chkDeploy != null)
            {
                chkDeploy.IsChecked = false;
                chkDeploy.IsEnabled = false;
            }
        }

    }

    internal class CheckListItem
    {
        public string Label { get; set; }
        public bool IsChecked { get; set; }
        public string Tag { get; set; }
        public override string ToString()
        {
            return Label;
        }
    }
}
