namespace ErikEJ.SqlCeScripting
{
    /// <summary>
    /// TABLE_NAME, INDEX_NAME, PRIMARY_KEY, [UNIQUE], [CLUSTERED], ORDINAL_POSITION, COLUMN_NAME, COLLATION as SORT_ORDER
    /// </summary>
    public struct Index
    {
        public string TableName { get; set; }
        public string IndexName { get; set; }
        public bool Unique { get; set; }
        public bool Clustered { get; set; }
        public int OrdinalPosition { get; set; }
        public string ColumnName { get; set; }
        public SortOrderEnum SortOrder { get; set; }

    }

    public enum SortOrderEnum
    {
        ASC
        , DESC
    }
}
