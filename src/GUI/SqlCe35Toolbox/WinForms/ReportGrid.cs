using System;
using System.Data;
using System.Windows.Forms;
using System.IO;
using ErikEJ.SqlCeToolbox.Helpers;
using Microsoft.Reporting.WinForms;
using System.Security;
using System.Security.Permissions;

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

                var rdlc = RdlcHelper.BuildRDLCStream(
                    DataSet, TableName, Resources.report);

                //Fix for VS "15" permission issue
                var permissionSet = new PermissionSet(PermissionState.Unrestricted);
                var fIOPermission = new FileIOPermission(PermissionState.None);
                fIOPermission.AllLocalFiles = FileIOPermissionAccess.Read;
                permissionSet.AddPermission(fIOPermission);
                permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
                reportView.LocalReport.SetBasePermissionsForSandboxAppDomain(permissionSet);

                reportView.LocalReport.LoadReportDefinition(rdlc);
                reportView.LocalReport.DataSources.Clear();
                reportView.LocalReport.DataSources.Add(
                    new ReportDataSource(DataSet.DataSetName, DataSet.Tables[0]));
                reportView.RefreshReport();
            }
        }

    }
}
