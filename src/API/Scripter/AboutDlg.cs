using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using System;

namespace SqlCeScripter
{
    internal partial class AboutDlg : Form
    {
        private string downloadUri;

        internal AboutDlg()
        {
            InitializeComponent();
            this.lblVersion.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            if (CheckVersion("ssms35"))
            {
                linkLabelNewVersion.Visible = true;
                this.downloadUri = "http://exportsqlce.codeplex.com/";
            }
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LinkLabel label = (LinkLabel)sender;
            Process.Start(label.Text);
        }

        private void linkLabelNewVersion_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.downloadUri))
            {
                Process.Start(this.downloadUri);    
            }
        }

        private bool CheckVersion(string lookingFor)
        {
            try
            {
                using (var wc = new System.Net.WebClient())
                {
                    var xDoc = new System.Xml.XmlDocument();                    
                    xDoc.Load(@"http://www.sqlcompact.dk/SqlCeToolboxVersions.xml");

                    string newVersion = xDoc.DocumentElement.Attributes[lookingFor].Value;

                    Version vN = new Version(newVersion);
                    if (vN > Assembly.GetExecutingAssembly().GetName().Version)
                    {
                        return true;
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return false;
        }

    }
}
