using System;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using System.Linq;
using ErikEJ.SqlCeToolbox.ToolWindows;

namespace ErikEJ.SqlCeToolbox
{
    //Lifted and ported to SqlCe from http://sqldbdoc.codeplex.com/
    internal class SqlCeDbDoc
    {
        private static readonly string[] FORMATS = { "html", "wikiplex", "xml" };
        private static readonly string[] HTML_EXTENSIONS = { ".htm", ".html", ".xhtml" };
        private static readonly string[] WIKI_EXTENSIONS = { ".txt", ".wiki" };

        public void CreateDocumentation(
            DatabaseInfo info,
            string fileName,
            bool overwrite,
            //[Optional(null, "f", Description = "output format: html, wikiplex, xml (autodetected when omitted)")] 
            string format
            )
        {
            // Validate arguments
            if (info == null) throw new ArgumentNullException("info");
            if (fileName == null) throw new ArgumentNullException("fileName");
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "fileName");

            // Validate output file
            if (File.Exists(fileName) && !overwrite)
            {
                throw new ArgumentException("File already exists");
            }

            // Get output format
            if (string.IsNullOrWhiteSpace(format))
            {
                Console.WriteLine("Autodetecting output format...");
                if (Array.IndexOf(HTML_EXTENSIONS, Path.GetExtension(fileName)) > -1)
                {
                    format = "html";
                }
                else if (Array.IndexOf(WIKI_EXTENSIONS, Path.GetExtension(fileName)) > -1)
                {
                    format = "wikiplex";
                }
                else
                {
                    format = "xml";
                }
            }
            else
            {
                format = format.ToLower().Trim();
                if (Array.IndexOf(FORMATS, format) == -1) throw new ArgumentOutOfRangeException("format", "Unknown format string.");
            }

            // Prepare XML document
            var doc = new XmlDocument();

            using (var repository = Helpers.RepositoryHelper.CreateRepository(info))
            {
                // Get latest descriptions
                ExplorerControl.DescriptionCache = new Helpers.DescriptionHelper().GetDescriptions(info);

                // Process database info
                var parent = doc.AppendChild(doc.CreateElement("database"));
                doc.DocumentElement.SetAttribute("dateCreated", XmlConvert.ToString(DateTime.Now, XmlDateTimeSerializationMode.RoundtripKind));
                doc.DocumentElement.SetAttribute("name", info.Caption);
                doc.DocumentElement.SetAttribute("dateGenerated", XmlConvert.ToString(DateTime.Now, XmlDateTimeSerializationMode.RoundtripKind));

                var e = parent.AppendChild(parent.OwnerDocument.CreateElement("schema")) as XmlElement;
                e.SetAttribute("name", "SqlCe");

                string desc = ExplorerControl.DescriptionCache.Where(d => d.Object == null && d.Parent == null).Select(d => d.Description).SingleOrDefault();
                if (!string.IsNullOrWhiteSpace(desc))
                {
                    var pop = parent.AppendChild(parent.OwnerDocument.CreateElement("dbProperty")) as XmlElement;
                    pop.SetAttribute("key", "Description");
                    pop.SetAttribute("value", desc);
                }
                var properties = repository.GetDatabaseInfo();
                foreach (var prop in properties)
                {
                    var pop = parent.AppendChild(parent.OwnerDocument.CreateElement("dbProperty")) as XmlElement;
                    pop.SetAttribute("key", prop.Key);
                    pop.SetAttribute("value", prop.Value);
                }

                var tables = repository.GetAllTableNames();
                var columns = repository.GetAllColumns();
                var primaryKeys = repository.GetAllPrimaryKeys();

                // Create object elements
                foreach (var table in tables)
                {
                    if (!Properties.Settings.Default.IncludeSystemTablesInDocumentation && table.StartsWith("__"))
                        continue;

                    var foreignKeys = repository.GetAllForeignKeys().Where(f => f.ConstraintTableName == table).ToList();

                    e = parent.AppendChild(parent.OwnerDocument.CreateElement("object")) as XmlElement;
                    e.SetAttribute("id", table);
                    e.SetAttribute("schema", "SqlCe");
                    e.SetAttribute("name", table);
                    e.SetAttribute("type", "USER_TABLE");
                    e.SetAttribute("dateCreated", XmlConvert.ToString(DateTime.Now, XmlDateTimeSerializationMode.RoundtripKind));
                    e.SetAttribute("dateModified", XmlConvert.ToString(DateTime.Now, XmlDateTimeSerializationMode.RoundtripKind));
                    desc = ExplorerControl.DescriptionCache.Where(d => d.Object == table && d.Parent == null).Select(d => d.Description).SingleOrDefault();
                    if (!string.IsNullOrWhiteSpace(desc))
                        e.SetAttribute("description", desc);

                    var columnList = columns.Where(c => c.TableName == table).ToList();
                    foreach (var col in columnList)
                    {
                        var c = e.AppendChild(parent.OwnerDocument.CreateElement("column")) as XmlElement;
                        c.SetAttribute("name", col.ColumnName);
                        c.SetAttribute("type", col.DataType);
                        c.SetAttribute("length", XmlConvert.ToString(col.CharacterMaxLength));
                        c.SetAttribute("precision", XmlConvert.ToString(col.NumericPrecision));
                        c.SetAttribute("scale", XmlConvert.ToString(col.NumericScale));
                        bool isNullable = col.IsNullable == SqlCeScripting.YesNoOption.YES;
                        c.SetAttribute("nullable", XmlConvert.ToString(isNullable));
                        bool isIdentity = col.AutoIncrementBy > 0;
                        c.SetAttribute("identity", XmlConvert.ToString(isIdentity));
                        if (isIdentity)
                        {
                            c.SetAttribute("identitySeed", "(" + col.AutoIncrementSeed.ToString() + "," + col.AutoIncrementBy.ToString() + ")");
                        }
                        c.SetAttribute("computed", XmlConvert.ToString(false));
                        c.SetAttribute("rowguidcol", XmlConvert.ToString(col.RowGuidCol));

                        desc = ExplorerControl.DescriptionCache.Where(d => d.Object == col.ColumnName && d.Parent == table).Select(d => d.Description).SingleOrDefault();
                        if (!string.IsNullOrWhiteSpace(desc))
                            c.SetAttribute("description", desc);

                        if (col.ColumnHasDefault)
                        {
                            var def = c.AppendChild(parent.OwnerDocument.CreateElement("default")) as XmlElement;
                            def.SetAttribute("value", col.ColumnDefault);
                        }

                        var pkList = primaryKeys.Where(pk => pk.TableName == table && pk.ColumnName == col.ColumnName);
                        foreach (var pk in pkList)
                        {
                            var p = c.AppendChild(parent.OwnerDocument.CreateElement("primaryKey")) as XmlElement;
                            p.SetAttribute("refId", pk.KeyName);
                        }

                        var fkList = foreignKeys.Where(fk => fk.ColumnName == col.ColumnName).ToList();
                        foreach (var fk in fkList)
                        {
                            var p = c.AppendChild(parent.OwnerDocument.CreateElement("foreignKey")) as XmlElement;
                            p.SetAttribute("refId", fk.ConstraintName);
                            p.SetAttribute("tableId", fk.UniqueConstraintTableName);
                            p.SetAttribute("column", fk.UniqueColumnName);
                        }

                    }

                    var allPks = primaryKeys.Where(pk => pk.TableName == table).Select(pk => pk.KeyName).Distinct().ToList();
                    foreach (var pk in allPks)
                    {
                        var p = e.AppendChild(parent.OwnerDocument.CreateElement("object")) as XmlElement;
                        p.SetAttribute("id", pk);
                        p.SetAttribute("schema", "SqlCe");
                        p.SetAttribute("name", pk);
                        p.SetAttribute("type", "PRIMARY_KEY_CONSTRAINT");
                        p.SetAttribute("dateCreated", XmlConvert.ToString(DateTime.Now, XmlDateTimeSerializationMode.RoundtripKind));
                        p.SetAttribute("dateModified", XmlConvert.ToString(DateTime.Now, XmlDateTimeSerializationMode.RoundtripKind));
                    }

                    foreach (var fk in foreignKeys)
                    {
                        var f = e.AppendChild(parent.OwnerDocument.CreateElement("object")) as XmlElement;
                        f.SetAttribute("id", fk.ConstraintName);
                        f.SetAttribute("schema", "SqlCe");
                        f.SetAttribute("name", fk.ConstraintName);
                        f.SetAttribute("type", "FOREIGN_KEY_CONSTRAINT");
                        f.SetAttribute("dateCreated", XmlConvert.ToString(DateTime.Now, XmlDateTimeSerializationMode.RoundtripKind));
                        f.SetAttribute("dateModified", XmlConvert.ToString(DateTime.Now, XmlDateTimeSerializationMode.RoundtripKind));
                    }
                }
            }

            if (format.Equals("xml"))
            {
                // Save raw XML
                doc.Save(fileName);
                return;
            }

            // Read XSL template code
            string xslt;
            if (format.Equals("html"))
            {
                xslt = Resources.Html;
            }
            else
            {
                xslt = Resources.WikiPlex;
            }

            // Prepare XSL transformation
            StringReader sr = null;
            XmlReader xr = null;
            try
            {
                sr = new StringReader(xslt);
                xr = XmlReader.Create(sr);
                var tran = new XslCompiledTransform();
                tran.Load(xr);
                sr = null;
                xr.Close();
                using (var fw = File.CreateText(fileName))
                {
                    tran.Transform(doc, null, fw);
                }
            }
            finally
            {
                if (sr != null)
                    sr.Dispose();
            }
        }
    }
}
