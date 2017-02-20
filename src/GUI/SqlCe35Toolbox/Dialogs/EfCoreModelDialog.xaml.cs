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
            textBox1.Focus();
        }
    }
}
