using System.Collections.Generic;
namespace ErikEJ.SqlCeScripting
{
    public class Trigger
    {
        public string TableName { get; set; }
        public string Definition { get; set; }
        public string TriggerName { get; set; }
    }
}
