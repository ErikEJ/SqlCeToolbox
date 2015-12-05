using System.Data;

namespace ErikEJ.SqlCeToolbox.ToolWindows
{
    public partial class ReportControl
    {
        public DatabaseInfo  DatabaseInfo { get; set; } //This property must be set by parent window
        private ReportWindow _parentWindow;
        public string TableName { get; set; }
        public DataSet DataSet { get; set; }
        
        public ReportControl(ReportWindow parentWindow)
        {
            InitializeComponent();
            _parentWindow = parentWindow;
        }

        public void ShowReport()
        {
            var grid = new ReportGrid();
            grid.TableName = this.TableName;
            grid.DataSet = this.DataSet;
            this.winFormHost.Child = grid;
        }

    }
}