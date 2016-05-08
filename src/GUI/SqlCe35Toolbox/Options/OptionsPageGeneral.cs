using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace ErikEJ.SqlCeToolbox
{
    [Guid(GuidList.GuidPageGeneral)]
    public class OptionsPageGeneral : DialogPage
    {
        protected override void OnActivate(CancelEventArgs e)
        {
            ShowBinaryValuesInResult = Properties.Settings.Default.ShowBinaryValuesInResult;
            ShowResultInGrid = Properties.Settings.Default.ShowResultInGrid;
            DisplayDescriptionTable = Properties.Settings.Default.DisplayDescriptionTable;
            DisplayObjectProperties = Properties.Settings.Default.DisplayObjectProperties;
            RemoveInvalidConnectionsOnStart = Properties.Settings.Default.ValidateConnectionsOnStart;
            PreferDDEX = Properties.Settings.Default.PreferDDEX;
            PromptToSaveChangedScript = Properties.Settings.Default.PromptToSaveChangedScript;
            FileFilterSqlCe = Properties.Settings.Default.FileFilterSqlCe;
            FileFilterSqlite = Properties.Settings.Default.FileFilterSqlite;
            ShowNullValuesAsNULL = Properties.Settings.Default.ShowNullValuesAsNULL;
            DisableKeyboardShortcuts = Properties.Settings.Default.DisableEditorKeyboardShortcuts;
            UseClassicGrid = Properties.Settings.Default.UseClassicGrid;
            base.OnActivate(e);
        }

        [Category("Query Editor"),
        DisplayName("Show binary values in Result"),
        Description("If true, will show binary values a binary string in results (slower)"),
        DefaultValue(false)]
        public bool ShowBinaryValuesInResult { get; set; }

        [Category("Query Editor"),
        DisplayName("Show Result As Grid"),
        Description("If true, will show the query results as grid by default (slower)"),
        DefaultValue(false)]
        public bool ShowResultInGrid { get; set; }

        [Category("Query Editor"),
        DisplayName("Show null values as NULL"),
        Description("If true, will show NULL values as 'NULL'"),
        DefaultValue(true)]
        public bool ShowNullValuesAsNULL { get; set; }

        [Category("Query Editor"),
        DisplayName("Prompt to save changes"),
        Description("If true, will ask you to save changed scripts"),
        DefaultValue(false)]
        public bool PromptToSaveChangedScript { get; set; }

        [Category("Query Editor"),
        DisplayName("Use classic (plain) grid"),
        Description("If true, will use the classic grid for results, else use the advanced grib with sorting and grouping"),
        DefaultValue(true)]
        public bool UseClassicGrid { get; set; }

        [Category("Object Tree"),
        DisplayName("Display the description table"),
        Description("If true, will show the __ExtendedProperties metadata table"),
        DefaultValue(false)]
        public bool DisplayDescriptionTable { get; set; }

        [Category("Object Tree"),
        DisplayName("Display Properties Window"),
        Description("If true, will show database and table properties in Properties window"),
        DefaultValue(true)]
        public bool DisplayObjectProperties { get; set; }

        [Category("Object Tree"),
        DisplayName("Remove invalid connections on launch"),
        Description("If true, will remove invalid connections on startup"),
        DefaultValue(true)]
        public bool RemoveInvalidConnectionsOnStart { get; set; }

        [Category("Object Tree"),
        DisplayName("Prefer Server Explorer"),
        Description("Use the Visual Studio Connection dialog and save connections in Server Explorer, if possible"),
        DefaultValue(true)]
        public bool PreferDDEX { get; set; }

        [Category("Object Tree"),
        DisplayName("File filter for SQL Server Compact"),
        Description("File filter use for SQL Server Compact database files"),
        DefaultValue("*.sdf")]
        public string FileFilterSqlCe { get; set; }

        [Category("Object Tree"),
        DisplayName("File filter for SQLite"),
        Description("File filter use for SQLite database files"),
        DefaultValue("*.db;*.db3;*.sqlite;*.sqlite3;*.dat")]
        public string FileFilterSqlite { get; set; }

        [Category("Query Editor"),
        DisplayName("Disable keyboard shortcuts"),
        Description("Disable keyboard shortcuts"),
        DefaultValue(false)]
        public bool DisableKeyboardShortcuts { get; set; }

        protected override void OnApply(PageApplyEventArgs e)
        {
            Properties.Settings.Default.ShowBinaryValuesInResult = ShowBinaryValuesInResult;
            Properties.Settings.Default.ShowResultInGrid = ShowResultInGrid;
            Properties.Settings.Default.DisplayDescriptionTable = DisplayDescriptionTable;
            Properties.Settings.Default.DisplayObjectProperties = DisplayObjectProperties;
            Properties.Settings.Default.ValidateConnectionsOnStart = RemoveInvalidConnectionsOnStart;
            Properties.Settings.Default.PreferDDEX = PreferDDEX;
            Properties.Settings.Default.PromptToSaveChangedScript = PromptToSaveChangedScript;
            Properties.Settings.Default.FileFilterSqlCe = FileFilterSqlCe;
            Properties.Settings.Default.FileFilterSqlite = FileFilterSqlite;
            Properties.Settings.Default.ShowNullValuesAsNULL = ShowNullValuesAsNULL;
            Properties.Settings.Default.UseClassicGrid = UseClassicGrid;
            Properties.Settings.Default.DisableEditorKeyboardShortcuts = DisableKeyboardShortcuts;
            Properties.Settings.Default.Save();
            base.OnApply(e);
        }
    }
}
