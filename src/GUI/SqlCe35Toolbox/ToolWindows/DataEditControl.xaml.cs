using System.Collections.Generic;

namespace ErikEJ.SqlCeToolbox.ToolWindows
{
    /// <summary>
    /// Interaction logic for SqlEditorControl.xaml
    /// </summary>
    public partial class DataEditControl
    {
        public DatabaseInfo  DatabaseInfo { get; set; } //This property must be set by parent window
        public ResultsetGrid ResultsetGrid { get; set; }
        public string TableName { get; set; }
        public bool ReadOnly { get; set; }
        public List<int> ReadOnlyColumns { get; set; }
        public string SqlText { get; set; }
        
        public DataEditControl(DataGridViewWindow parentWindow)
        {
            InitializeComponent();
        }

        public void ShowGrid()
        {
            ResultsetGrid = new ResultsetGrid();
            ResultsetGrid.DatabaseInfo = this.DatabaseInfo;
            ResultsetGrid.TableName = this.TableName;
            ResultsetGrid.ReadOnly = this.ReadOnly;
            ResultsetGrid.ReadOnlyColumns = this.ReadOnlyColumns;
            ResultsetGrid.SqlText = this.SqlText;
            this.winFormHost.Child = ResultsetGrid;
        }
    }
}