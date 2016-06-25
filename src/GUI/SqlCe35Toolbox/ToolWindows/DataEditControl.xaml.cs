using System.Collections.Generic;
using ErikEJ.SqlCeToolbox.WinForms;

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
        
        public DataEditControl()
        {
            InitializeComponent();
        }

        public void ShowGrid()
        {
            ResultsetGrid = new ResultsetGrid
            {
                DatabaseInfo = DatabaseInfo,
                TableName = TableName,
                ReadOnly = ReadOnly,
                ReadOnlyColumns = ReadOnlyColumns,
                SqlText = SqlText
            };
            winFormHost.Child = ResultsetGrid;
        }
    }
}