using System.Collections.Generic;
using System.Windows;
using ErikEJ.SqlCeToolbox.Helpers;

namespace ErikEJ.SqlCeToolbox.Dialogs
{
    public partial class EfCoreModelDialog
    {
        public EfCoreModelDialog()
        {
            Telemetry.TrackPageView(nameof(EfCoreModelDialog));
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
                Title = $"Generate EF Core Model in Project {value}";
            }
        }

        public bool UseDataAnnotations => chkDataAnnoations.IsChecked != null && chkDataAnnoations.IsChecked.Value;

        public bool UseDatabaseNames => chkUseDatabaseNames.IsChecked != null && chkUseDatabaseNames.IsChecked.Value;

        public bool UsePluralizer => chkPluralize.IsChecked != null && chkPluralize.IsChecked.Value;

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

        public string OutputPath
        {
            get
            {
                return txtOutputPath.Text;
            }
            set
            {
                txtOutputPath.Text = value;
            }
        }

        public bool InstallNuGetPackage
        {
            get
            {
                return chkInstallNuGet.IsChecked.HasValue && chkInstallNuGet.IsChecked.Value;
            }
            set
            {
                chkInstallNuGet.IsChecked = value;
            }
        }

        public int SelectedTobeGenerated => cmbLanguage.SelectedIndex;

        #endregion

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtNameSpace.Text))
            {
                EnvDteHelper.ShowMessage("Namespace is required");
                return;
            }
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                EnvDteHelper.ShowMessage("Context name is required");
                return;
            }
            DialogResult = true;
            Close();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            textBox1.Focus();
            cmbLanguage.ItemsSource = new List<string> { "EntityTypes & DbContext", "DbContext only", "EntityTypes only" };
            cmbLanguage.SelectedIndex = 0;
        }
    }
}
