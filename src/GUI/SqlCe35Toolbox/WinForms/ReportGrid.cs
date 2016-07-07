using System;
using System.Data;
using System.Windows.Forms;
using System.IO;
using ErikEJ.SqlCeToolbox.Helpers;
using Microsoft.Reporting.WinForms;

namespace ErikEJ.SqlCeToolbox.ToolWindows
{
    public partial class ReportGrid : UserControl
    {
        public ReportGrid()
        {
            InitializeComponent();
        }

        public DataSet DataSet { get; set; }

        public string TableName { get; set; }

        private void ReportGrid_Load(object sender, EventArgs e)
        {
            if (DataSet != null)
            {
                DataSet.DataSetName = TableName;

                Stream rdlc = RdlcHelper.BuildRDLCStream(
                    DataSet, TableName, Resources.report);

                reportView.LocalReport.LoadReportDefinition(rdlc);
                reportView.LocalReport.DataSources.Clear();
                reportView.LocalReport.DataSources.Add(
                    new ReportDataSource(DataSet.DataSetName, DataSet.Tables[0]));
                reportView.RefreshReport();
            }
        }

    }
}
