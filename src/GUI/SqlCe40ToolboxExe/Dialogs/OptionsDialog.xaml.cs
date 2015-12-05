using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ErikEJ.SqlCeToolbox.Dialogs
{
    /// <summary>
    /// Interaction logic for OptionsDialog.xaml
    /// </summary>
    public partial class OptionsDialog : Window
    {
        private Helpers.UserOptions settings = new Helpers.UserOptions();

        public OptionsDialog()
        {
            InitializeComponent();
        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.DropTargetTables = settings.DropTargetTables;
            Properties.Settings.Default.ShowBinaryValuesInResult = settings.ShowBinaryValuesInResult;
            Properties.Settings.Default.ShowResultInGrid = settings.ShowResultInGrid;
            Properties.Settings.Default.DisplayDescriptionTable = settings.DisplayDescriptionTable;
            Properties.Settings.Default.IncludeSystemTablesInDocumentation = settings.IncludeSystemTablesInDocumentation;
            Properties.Settings.Default.MaxRowsToEdit = settings.MaxRowsToEdit;
            Properties.Settings.Default.MaxColumnWidth = settings.MaxColumnWidth;
            Properties.Settings.Default.IgnoreIdentityInInsertScript = settings.IgnoreIdentityInInsertScript;
            Properties.Settings.Default.KeepServerSchemaNames = settings.KeepServerSchemaNames;
            Properties.Settings.Default.MultiLineTextEntry = settings.MultiLineTextEntry;
            Properties.Settings.Default.Save();
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            settings.DropTargetTables = Properties.Settings.Default.DropTargetTables;
            settings.ShowBinaryValuesInResult = Properties.Settings.Default.ShowBinaryValuesInResult;
            settings.ShowResultInGrid = Properties.Settings.Default.ShowResultInGrid;
            settings.DisplayDescriptionTable = Properties.Settings.Default.DisplayDescriptionTable;
            settings.IncludeSystemTablesInDocumentation = Properties.Settings.Default.IncludeSystemTablesInDocumentation;
            settings.MaxRowsToEdit = Properties.Settings.Default.MaxRowsToEdit;
            settings.MaxColumnWidth = Properties.Settings.Default.MaxColumnWidth;
            settings.KeepServerSchemaNames = Properties.Settings.Default.KeepServerSchemaNames;
            settings.IgnoreIdentityInInsertScript = Properties.Settings.Default.IgnoreIdentityInInsertScript;
            settings.MultiLineTextEntry = Properties.Settings.Default.MultiLineTextEntry;
            Props.SelectedObject = settings;
        }
    }
}
