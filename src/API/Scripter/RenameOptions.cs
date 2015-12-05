using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SqlCeScripter
{
    public partial class RenameOptions : Form
    {
        public RenameOptions(string tableName)
        {
            InitializeComponent();
            this.Text = string.Format(System.Globalization.CultureInfo.InvariantCulture, this.Text, tableName);
        }

        public string NewName 
        { 
            get
            { 
                return this.txtName.Text; 
            }
        }

    }
}
