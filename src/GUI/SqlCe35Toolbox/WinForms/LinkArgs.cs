namespace ErikEJ.SqlCeToolbox.WinForms
{
    public class LinkArgs : System.EventArgs
    {
        public LinkArgs(string id, string table, string column)
        {
            Id = id;
            Column = column;
            Table = table;
        }

        public string Id { get; set; }
        public string Table { get; set; }
        public string Column { get; set; }
    } 
}
