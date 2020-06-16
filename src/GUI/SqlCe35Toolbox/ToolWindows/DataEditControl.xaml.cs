using System;
using System.Collections.Generic;
using ErikEJ.SqlCeToolbox.WinForms;

namespace ErikEJ.SqlCeToolbox.ToolWindows
{
    /// <summary>
    /// Interaction logic for SqlEditorControl.xaml
    /// </summary>
    public partial class DataEditControl : IDisposable
    {
        private bool disposedValue;

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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    ResultsetGrid?.Dispose();
                    winFormHost?.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}