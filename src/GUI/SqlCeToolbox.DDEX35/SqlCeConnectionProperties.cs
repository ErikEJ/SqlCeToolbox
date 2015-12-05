using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                this["Data Source"] = (object)value;
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
                this["Password"] = (object)value;
            }
        }

        [Name("Max Database Size")]
        public int MaxDatabaseSize
        {
            get
            {
                object obj = this["Max Database Size"];
                if (obj is int)
                    return (int)obj;
                else
                    return 0;
            }
            set
            {
                this["Max Database Size"] = (object)value;
            }
        }
    }
}
