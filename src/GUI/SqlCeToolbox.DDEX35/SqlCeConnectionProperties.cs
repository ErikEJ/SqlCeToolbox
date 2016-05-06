using Microsoft.VisualStudio.Data.Framework;
using System.ComponentModel;

namespace ErikEJ.SqlCeToolbox.DDEX35
{
    [DefaultProperty("Data Source")]
    internal class SqlCeConnectionProperties : DataConnectionProperties
    {
        [Name("Data Source")]
        public string DataSource
        {
            get
            {
                return this["Data Source"] as string;
            }
            set
            {
                this["Data Source"] = value;
            }
        }

        public string Password
        {
            get
            {
                return this["Password"] as string;
            }
            set
            {
                this["Password"] = value;
            }
        }

        [Name("Max Database Size")]
        public int MaxDatabaseSize
        {
            get
            {
                var obj = this["Max Database Size"];
                if (obj is int)
                    return (int)obj;
                return 0;
            }
            set
            {
                this["Max Database Size"] = value;
            }
        }
    }
}
