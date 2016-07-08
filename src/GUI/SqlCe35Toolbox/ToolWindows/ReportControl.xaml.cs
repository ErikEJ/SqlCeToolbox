using System.Data;

namespace ErikEJ.SqlCeToolbox.ToolWindows
{
    public partial class ReportControl
    {
        public DatabaseInfo  DatabaseInfo { get; set; } //This property must be set by parent window
        public string TableName { get; set; }
        public DataSet DataSet { get; set; }

        public ReportControl()
        {
            InitializeComponent();
        }

        public void ShowReport()
        {
            var grid = new ReportGrid
            {
                TableName = TableName,
                DataSet = DataSet
            };
            winFormHost.Child = grid;
        }
    }
}