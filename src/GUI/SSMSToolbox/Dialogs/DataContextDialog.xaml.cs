using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ErikEJ.SqlCeToolbox.Helpers;

namespace ErikEJ.SqlCeToolbox.Dialogs
{
    /// <summary>
    /// Interaction logic for DataContextDialog.xaml
    /// </summary>
    public partial class DataContextDialog
    {
        public DataContextDialog()
        {
            Telemetry.TrackPageView(nameof(DataContextDialog));
            InitializeComponent();
            Background = VsThemes.GetWindowBackground();
        }

        #region Properties
        public string ProjectName
        {
            get
            {
                return Title;
            }

            set
            {
                Title = string.Format("Create DataContext in Project {0}", value);
            }
        }

        public bool IsDesktop { get; set; }

        public bool Pluralize 
        {
            get
            {
                return chkPlural.IsChecked != null && chkPlural.IsChecked.Value;
            }
        }
        
        public bool AddVersionTable
        {
            get
            {
                return chkAddVersion.IsChecked != null && chkAddVersion.IsChecked.Value;
            }
        }

        public bool AddRowversionColumns
        {
            get
            {
                return chkAddRowVersion.IsChecked != null && chkAddRowVersion.IsChecked.Value;
            }
        }

        public bool AddConnectionStringBuilder
        {
            get
            {
                return chkConnStringBuilder.IsChecked != null && chkConnStringBuilder.IsChecked.Value;
            }
        }


        public bool MultipleFiles
        {
            get
            {
                return chkMultipleFiles.IsChecked != null && chkMultipleFiles.IsChecked.Value;
            }        
        }

        public string CodeLanguage
        {
            get 
            {
                return cmbLanguage.SelectedValue.ToString();
            }
            set
            {
                cmbLanguage.SelectedValue = value;
            }
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

        public string NameSpace
        {
            get
            {
                return txtNameSpace.Text;
            }
            set
            {
                txtNameSpace.Text = value;
            }
        }
        #endregion

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsDesktop)
            {
                chkMultipleFiles.Visibility = Visibility.Collapsed;
                lblAdv.Visibility = Visibility.Collapsed;
                chkAddRowVersion.Visibility = Visibility.Collapsed;
                chkAddVersion.Visibility = Visibility.Collapsed;
                chkConnStringBuilder.Visibility = Visibility.Collapsed;
            }
            cmbLanguage.ItemsSource = new List<string> { "C#", "VB" };
            cmbLanguage.SelectedValue = CodeLanguage;
            textBox1.Focus();
        }

        private void chkMultipleFiles_Checked(object sender, RoutedEventArgs e)
        {
            txtNameSpace.Text = string.Empty;
            txtNameSpace.IsEnabled = false;
        }

        private void chkMultipleFiles_Unchecked(object sender, RoutedEventArgs e)
        {
            txtNameSpace.IsEnabled = true;
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
