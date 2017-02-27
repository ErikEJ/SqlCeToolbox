using System;
using System.Data;
using System.Windows.Forms;
using System.Security;
using System.Security.Permissions;
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

                var rdlc = RdlcHelper.BuildRdlcStream(
                    DataSet, TableName, Resources.report);

                //Fix for VS "15" permission issue
                var permissionSet = new PermissionSet(PermissionState.Unrestricted);
                var fIoPermission = new FileIOPermission(PermissionState.None);
                fIoPermission.AllLocalFiles = FileIOPermissionAccess.Read;
                permissionSet.AddPermission(fIoPermission);
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
