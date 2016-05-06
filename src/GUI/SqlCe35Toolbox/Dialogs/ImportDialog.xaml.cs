using System.Windows;
using System.Text;
using System.Collections.Generic;
using System;
using ErikEJ.SqlCeToolbox.Helpers;
using Microsoft.Win32;
using Microsoft.VisualStudio.PlatformUI;
namespace ErikEJ.SqlCeToolbox.Dialogs
{
    /// <summary>
    /// Interaction logic for ImportDialog.xaml
    /// </summary>
    public partial class ImportDialog : DialogWindow
    {
        public string NewName { get; set; }

        public ImportDialog()
        {
            Telemetry.TrackPageView(nameof(ImportDialog));
            InitializeComponent();
            this.Background = Helpers.VsThemes.GetWindowBackground();
            this.SaveButton.IsEnabled = false;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }


        public string File
        {
            get
            {
                return this.FileName.Text;
            }
        }

        private List<string> sampleHeader = new List<string>();

        public List<string> SampleHeader
        {
            set
            {
                this.sampleHeader = value;
                MakeSample();
            }
        }

        public Char Separator
        {
            get
            {
                if (!string.IsNullOrEmpty(this.comboBox1.Text))
                {
                    return this.comboBox1.Text.ToCharArray(0, 1)[0];
                }
                else
                {
                    return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator.ToCharArray()[0];
                }
            }
            set
            {
                this.comboBox1.Text = value.ToString();
            }
        }

        private void MakeSample()
        {
            StringBuilder sb = new StringBuilder(200);
            bool first = true;
            foreach (string hdr in this.sampleHeader)
            {
                if (first)
                {
                    sb.AppendFormat("{0}", hdr);
                    first = false;
                }
                else
                {
                    sb.AppendFormat("{0}{1}", this.Separator.ToString(), hdr);
                }
            }
            MakeSampleHeaderLine(sb);
            MakeSampleHeaderLine(sb);
            sb.Append(Environment.NewLine);
            this.txtSample.Text = sb.ToString();
        }

        private void MakeSampleHeaderLine(StringBuilder sb)
        {
            sb.Append(Environment.NewLine);
            bool first = true;
            foreach (string hdr in this.sampleHeader)
            {
                if (first)
                {
                    sb.Append("xxx");
                    first = false;
                }
                else
                {
                    sb.AppendFormat("{0}xxx", this.Separator.ToString());
                }
            }
        }

        private void comboBox1_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            MakeSample();
        }

        private void FileName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.FileName.Text))
            {
                this.SaveButton.IsEnabled = false;
            }
            else
            {
                this.SaveButton.IsEnabled = true;
            }
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "CSV (Comma delimited) (*.csv)|*.csv|All Files(*.*)|*.*";
            ofd.CheckFileExists = true;
            ofd.Multiselect = false;
            ofd.ValidateNames = true;
            ofd.Title = "Select Import File";
            if (ofd.ShowDialog() == true)
            {
                this.FileName.Text = ofd.FileName;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.FileName.Focus();
        }
    }
}