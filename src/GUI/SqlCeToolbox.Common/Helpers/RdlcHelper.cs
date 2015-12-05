using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.Xml;
using System.Xml.Xsl;

namespace ErikEJ.SqlCeToolbox.Helpers
{
    public class RdlcHelper
    {
        public static Stream BuildRDLCStream(
            DataSet data, string name, string reportResource)
        {
            using (MemoryStream schemaStream = new MemoryStream())
            {
                // save the schema to a stream
                data.WriteXmlSchema(schemaStream);
                schemaStream.Seek(0, SeekOrigin.Begin);

                // load it into a Document and set the Name variable
                XmlDocument xmlDomSchema = new XmlDocument();
                xmlDomSchema.Load(schemaStream);
                xmlDomSchema.DocumentElement.SetAttribute("Name", data.DataSetName);

                // Prepare XSL transformation
                using (var sr = new StringReader(reportResource))
                using (var xr = XmlReader.Create(sr))
                {
                    // load the report's XSL file (that's the magic)
                    XslCompiledTransform xform = new XslCompiledTransform();
                    xform.Load(xr);

                    // do the transform
                    MemoryStream rdlcStream = new MemoryStream();
                    XmlWriter writer = XmlWriter.Create(rdlcStream);
                    xform.Transform(xmlDomSchema, writer);
                    writer.Close();
                    rdlcStream.Seek(0, SeekOrigin.Begin);

                    // send back the RDLC
                    return rdlcStream;
                }
            }
        }

    }
}
