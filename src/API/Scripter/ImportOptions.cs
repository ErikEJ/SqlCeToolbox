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
    public partial class ImportOptions : Form
    {
        public ImportOptions(string tableName)
        {
            InitializeComponent();
            this.Text = "Import Options for table " + tableName;
            bntOK.Enabled = false;
        }

        public string FileName 
        { 
            get
            { 
                return this.txtFilename.Text; 
            }
        }

        private List<string> sampleHeader;

        public List<string> SampleHeader 
        {
            set
            {
                this.sampleHeader = value;
                MakeSample();
            }        
        }

        public Char Separator
        {
            get
            {
                if (!string.IsNullOrEmpty(this.comboBox1.Text))
                {
                    return this.comboBox1.Text.ToCharArray(0, 1)[0];
                }
                else
                {
                    return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator.ToCharArray()[0];
                }
            }
            set
            {
                this.comboBox1.Text = value.ToString();
            }
        }

        private void MakeSample()
        {
            StringBuilder sb = new StringBuilder(200);            
            bool first = true;
            foreach (string hdr in this.sampleHeader)
            {
                if (first)
                {
                    sb.AppendFormat("{0}", hdr);
                    first = false;
                }
                else
                {
                    sb.AppendFormat("{0}{1}", this.Separator.ToString(), hdr);
                }
            }
            sb.Append(Environment.NewLine);
            first = true;
            foreach (string hdr in this.sampleHeader)
            {
                if (first)
                {
                    sb.Append("xxx");
                    first = false;
                }
                else
                {
                    sb.AppendFormat("{0}xxx", this.Separator.ToString());
                }
            }
            sb.Append(Environment.NewLine);
            first = true;
            foreach (string hdr in this.sampleHeader)
            {
                if (first)
                {
                    sb.Append("xxx");
                    first = false;
                }
                else
                {
                    sb.AppendFormat("{0}xxx", this.Separator.ToString());
                }
            }
            sb.Append(Environment.NewLine);
            this.lblSample.Text = sb.ToString();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "CSV (Comma delimited) (*.csv)|*.csv|All Files(*.*)|*.*";
                ofd.CheckFileExists = true;
                ofd.Multiselect = false;
                ofd.ValidateNames = true;
                ofd.Title = "Select Import File";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    this.txtFilename.Text = ofd.FileName;
                }
            }

        }

        private void comboBox1_TextUpdate(object sender, EventArgs e)
        {
            MakeSample();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            MakeSample();
        }

        private void txtFilename_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtFilename.Text))
            {
                bntOK.Enabled = false;
            }
            else
            {
                bntOK.Enabled = true;
            }
        }

    }
}
