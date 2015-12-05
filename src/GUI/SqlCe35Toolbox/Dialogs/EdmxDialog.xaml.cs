using System.Collections.Generic;
using System.Windows;
using Microsoft.VisualStudio.PlatformUI;

namespace ErikEJ.SqlCeToolbox.Dialogs
{
    /// <summary>
    /// Interaction logic for EdmxDialog.xaml
    /// </summary>
    public partial class EdmxDialog : DialogWindow
    {
        public EdmxDialog()
        {
            Telemetry.TrackPageView(nameof(EdmxDialog));
            InitializeComponent();
            this.Background = Helpers.VSThemes.GetWindowBackground();
        }

        private List<CheckListItem> items = new List<CheckListItem>();

        #region Properties
        public string ProjectName
        {
            get
            {
                return this.Title;
            }

            set
            {
                this.Title = string.Format("Generate EDM in Project {0}", value);
            }
        }

        public bool Pluralize 
        {
            get
            {
                return this.chkPlural.IsChecked.Value;
            }
        }
        public bool ForeignKeys
        {
            get
            {
                return this.chkFks2.IsChecked.Value;
            }
        }
        public bool SaveConfig 
        {
            get
            {
                return this.chkSave.IsChecked.Value;
            }            
        }
        public bool AddPrivateConfig
        {
            get
            {
                return this.chkDeploy.IsChecked.Value;
            }            
        
        }
        public void HideAddPrivateConfig()
        {
            this.chkDeploy.Visibility = System.Windows.Visibility.Collapsed;
        }
        public string ModelName 
        {
            get
            {
                return this.textBox1.Text;
            }
            set
            {
                this.textBox1.Text = value;
            }
        }
        public List<string> Tables { get; set; }

        #endregion

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Tables.Clear();
            foreach (object item in chkTables.Items)
            {
                var checkItem = (CheckListItem)item;
                if (checkItem.IsChecked)
                {
                    this.Tables.Add(checkItem.Label);
                }
            }
            this.Close();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.textBox1.Focus();
            bool isChecked = true;
            foreach (string table in Tables)
            { 
                isChecked = true;
                if (table.StartsWith("__"))
                {
                    isChecked = false;
                }
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
