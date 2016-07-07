using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace ErikEJ.SqlCeToolbox
{
    [Guid(GuidList.GuidPageAdvanced)]
    public class OptionsPageAdvanced : DialogPage
    {
        protected override void OnActivate(CancelEventArgs e)
        {
            MaxRowsToEdit = Properties.Settings.Default.MaxRowsToEdit;
            MaxColumnWidth = Properties.Settings.Default.MaxColumnWidth;
            MultiLineTextEntry = Properties.Settings.Default.MultiLineTextEntry;
            DropTargetTables = Properties.Settings.Default.DropTargetTables;
            IncludeSystemTablesInDocumentation = Properties.Settings.Default.IncludeSystemTablesInDocumentation;
            IgnoreIdentityInInsertScript = Properties.Settings.Default.IgnoreIdentityInInsertScript;
            KeepServerSchemaNames = Properties.Settings.Default.KeepSchemaNames;
            PreserveSqlDates = Properties.Settings.Default.PreserveSqlDates;
            TruncateSQLiteStrings = Properties.Settings.Default.TruncateSQLiteStrings;
            ParticipateInTelemetry = Properties.Settings.Default.ParticipateInTelemetry;
            MakeSQLiteDatetimeReadOnly = Properties.Settings.Default.MakeSQLiteDatetimeReadOnly;
            base.OnActivate(e);
        }

        [Category("Other"),
        DisplayName(@"Participate in Telemetry"),
        Description("Help improve the Toolbox by providing anynonymous usage data and crash reports"),
        DefaultValue(true)]
        public bool ParticipateInTelemetry { get; set; }

        [Category("Schema Diff"),
        DisplayName(@"Drop Target Tables"),
        Description("If true, will add DROP TABLE statements for tables only in target"),
        DefaultValue(false)]
        public bool DropTargetTables { get; set; }

        [Category("Edit Table Data"),
        DisplayName(@"Max number of rows to edit"),
        Description("Sets number of rows loaded by Edit Table grid"),
        DefaultValue(200)]
        public int MaxRowsToEdit { get; set; }

        [Category("Edit Table Data"),
        DisplayName(@"Allow multi-line text entry"),
        Description("Allow entry of multi line text using Shift+Enter (may affect performance)"),
        DefaultValue(false)]
        public bool MultiLineTextEntry { get; set; }

        [Category("Edit Table Data"),
        DisplayName(@"Max edit grid column width"),
        Description("Sets the desired column width in pixels"),
        DefaultValue(0)]
        public int MaxColumnWidth { get; set; }

        [Category("Edit Table Data"),
        DisplayName(@"Make SQLite datetime read-only"),
        Description("Work around the inability to edit SQLite datetime columns"),
        DefaultValue(false)]
        public bool MakeSQLiteDatetimeReadOnly { get; set; }

        [Category("Documentation"),
        DisplayName(@"Include system tables"),
        Description("If true, will also include tables beginning with __ "),
        DefaultValue(false)]
        public bool IncludeSystemTablesInDocumentation { get; set; }

        [Category("Scripting"),
        DisplayName(@"Ignore IDENTITY in data scripts"),
        Description("Ignore IDENTITY column when scripting Data as INSERTs (table level)"),
        DefaultValue(false)]
        public bool IgnoreIdentityInInsertScript { get; set; }

        [Category("Scripting"),
        DisplayName(@"Keep server schema names"),
        Description("Keep schema names with Server export/scripting"),
        DefaultValue(false)]
        public bool KeepServerSchemaNames { get; set; }

        [Category("Scripting"),
        DisplayName(@"Preserve values of unsupported SQL Server data types"),
        Description("Script SQL Server date, datetime2 and datetimeoffset as nvarchar(x)"),
        DefaultValue(false)]
        public bool PreserveSqlDates { get; set; }

        [Category("Scripting"),
        DisplayName(@"Truncate SQLite string values"),
        Description("Enabling this option will truncate SQLite strings, that are longer than the defined size, when exporting to SQL Server. Truncations will be logged to: %temp%\\SQLiteTruncates.log"),
        DefaultValue(false)]
        public bool TruncateSQLiteStrings { get; set; }

        protected override void OnApply(PageApplyEventArgs e)
        {
            Properties.Settings.Default.MaxRowsToEdit = MaxRowsToEdit;
            Properties.Settings.Default.MaxColumnWidth = MaxColumnWidth;
            Properties.Settings.Default.MultiLineTextEntry = MultiLineTextEntry;
            Properties.Settings.Default.DropTargetTables = DropTargetTables;
            Properties.Settings.Default.IncludeSystemTablesInDocumentation = IncludeSystemTablesInDocumentation;
            Properties.Settings.Default.IgnoreIdentityInInsertScript = IgnoreIdentityInInsertScript;
            Properties.Settings.Default.KeepSchemaNames = KeepServerSchemaNames;
            Properties.Settings.Default.PreserveSqlDates = PreserveSqlDates;
            Properties.Settings.Default.TruncateSQLiteStrings = TruncateSQLiteStrings;
            Properties.Settings.Default.ParticipateInTelemetry = ParticipateInTelemetry;
            Properties.Settings.Default.MakeSQLiteDatetimeReadOnly = MakeSQLiteDatetimeReadOnly;
            Properties.Settings.Default.Save();
            base.OnApply(e);
        }
    }
}
