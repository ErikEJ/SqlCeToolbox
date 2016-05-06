using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ErikEJ.SqlCeToolbox.Helpers;
using Microsoft.VisualStudio.PlatformUI;

namespace ErikEJ.SqlCeToolbox.Dialogs
{
    /// <summary>
    /// Interaction logic for DataContextDialog.xaml
    /// </summary>
    public partial class DataContextDialog : DialogWindow
    {
        public DataContextDialog()
        {
            Telemetry.TrackPageView(nameof(DataContextDialog));
            InitializeComponent();
            this.Background = Helpers.VsThemes.GetWindowBackground();
        }

        #region Properties
        public string ProjectName
        {
            get
            {
                return this.Title;
            }

            set
            {
                this.Title = string.Format("Create DataContext in Project {0}", value);
            }
        }

        public bool IsDesktop { get; set; }

        public bool Pluralize 
        {
            get
            {
                return this.chkPlural.IsChecked.Value;
            }
        }
        
        public bool AddVersionTable
        {
            get
            {
                return this.chkAddVersion.IsChecked.Value;
            }
        }

        public bool AddRowversionColumns
        {
            get
            {
                return this.chkAddRowVersion.IsChecked.Value;
            }
        }

        public bool AddConnectionStringBuilder
        {
            get
            {
                return this.chkConnStringBuilder.IsChecked.Value;
            }
        }


        public bool MultipleFiles
        {
            get
            {
                return this.chkMultipleFiles.IsChecked.Value;
            }        
        }

        public string CodeLanguage
        {
            get 
            {
                return this.cmbLanguage.SelectedValue.ToString();
            }
            set
            {
                this.cmbLanguage.SelectedValue = value;
            }
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

        public string NameSpace
        {
            get
            {
                return this.txtNameSpace.Text;
            }
            set
            {
                this.txtNameSpace.Text = value;
            }
        }
        #endregion

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsDesktop)
            {
                chkMultipleFiles.Visibility = System.Windows.Visibility.Collapsed;
                lblAdv.Visibility = System.Windows.Visibility.Collapsed;
                chkAddRowVersion.Visibility = System.Windows.Visibility.Collapsed;
                chkAddVersion.Visibility = System.Windows.Visibility.Collapsed;
                chkConnStringBuilder.Visibility = System.Windows.Visibility.Collapsed;
            }
            this.cmbLanguage.ItemsSource = new List<string> { "C#", "VB" };
            this.cmbLanguage.SelectedValue = this.CodeLanguage;
            this.textBox1.Focus();
        }

        private void chkMultipleFiles_Checked(object sender, RoutedEventArgs e)
        {
            this.txtNameSpace.Text = string.Empty;
            this.txtNameSpace.IsEnabled = false;
        }

        private void chkMultipleFiles_Unchecked(object sender, RoutedEventArgs e)
        {
            this.txtNameSpace.IsEnabled = true;
        }

        private void cmbLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            chkMultipleFiles.IsEnabled = true;
            chkConnStringBuilder.IsEnabled = true;
            if (cmbLanguage.SelectedValue.ToString() == "VB")
            {
                chkMultipleFiles.IsChecked = false;
                chkMultipleFiles.IsEnabled = false;
                chkConnStringBuilder.IsChecked = false;
                chkConnStringBuilder.IsEnabled = false;
            }
        }

    }
}
