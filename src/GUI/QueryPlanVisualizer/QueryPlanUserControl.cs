using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace ExecutionPlanVisualizer
{
    public partial class QueryPlanUserControl : UserControl
    {
        private string _planXml;

        public QueryPlanUserControl()
        {
            InitializeComponent();

            var assocQueryString = NativeMethods.AssocQueryString(NativeMethods.AssocStr.Executable, ".sqlplan");

            if (string.IsNullOrEmpty(assocQueryString))
            {
                openPlanButton.Visible = false;
            }
            else
            {
                var fileDescription = FileVersionInfo.GetVersionInfo(assocQueryString).FileDescription;
                openPlanButton.Text = $"Open with {fileDescription}";
            }
        }

        public string PlanHtml { get; set; }

        public string PlanXml { get; set; }


        private void SavePlanButtonClick(object sender, EventArgs e)
        {
            if (savePlanFileDialog.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(savePlanFileDialog.FileName, _planXml);

                planLocationLinkLabel.Text = savePlanFileDialog.FileName;
                planSavedLabel.Visible = planLocationLinkLabel.Visible = true;
            }
        }

        private void OpenPlanButtonClick(object sender, EventArgs e)
        {
            var tempFile = Path.ChangeExtension(Path.GetTempFileName(), "sqlplan");
            File.WriteAllText(tempFile, _planXml);

            try
            {
                Process.Start(tempFile);
            }
            catch (Exception exception)
            {
                MessageBox.Show($"Cannot open execution plan. {exception.Message}", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PlanLocationLinkLabelLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("explorer.exe", $"/select,\"{planLocationLinkLabel.Text}\"");
        }


        public void DisplayExecutionPlanDetails(string planXml, string planHtml)
        {
            _planXml = planXml;

            webBrowser.DocumentText = planHtml;
        }
    }
}
