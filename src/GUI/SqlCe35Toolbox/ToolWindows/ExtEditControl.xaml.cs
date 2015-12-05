using System.ComponentModel;
using System.Data;
using System.Windows.Controls;

namespace ErikEJ.SqlCeToolbox.ToolWindows
{
    public partial class ExtEditControl : INotifyPropertyChanged
    {
        DataTable _sourceTable;
        public DataTable SourceTable
        {
            get
            {
                return _sourceTable;
            }
            set
            {
                _sourceTable = value;
                NotifyPropertyChanged("SourceTable");
            }
        }

        public ExtEditControl()
        {
            DataContext = this;
            InitializeComponent();
        }

        private void ExtEditWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var fontFamiliy = new System.Windows.Media.FontFamily("Consolas");
            grid.FontFamily = fontFamiliy;
            grid.Theme = ExtendedGrid.ExtendedGridControl.ExtendedDataGrid.Themes.System;
            grid.Background = Helpers.VSThemes.GetCommandBackground();
            grid.IsReadOnly = true;
        }
 
        void grid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var pos = e.PropertyName.IndexOf("_");
            if (pos > 0 && e.Column.Header != null)
            {
                e.Column.Header = e.Column.Header.ToString().Replace("_", "__");
            }
            if (Properties.Settings.Default.ShowNullValuesAsNULL)
            {
                ((DataGridBoundColumn)e.Column).Binding.TargetNullValue = "NULL";
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}