using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ErikEJ.SqlCeToolbox.WinForms
{
    public class LinkArgs : System.EventArgs
    {
        public LinkArgs(string id, string table, string column)
        {
            this.Id = id;
            this.Column = column;
            this.Table = table;
        }

        public string Id { get; set; }
        public string Table { get; set; }
        public string Column { get; set; }
    } 
}
