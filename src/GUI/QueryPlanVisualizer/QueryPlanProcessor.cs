using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Xsl;
using ExecutionPlanVisualizer.Properties;

namespace ExecutionPlanVisualizer
{
    class QueryPlanProcessor
    {
        private readonly string _planXml;
        private static readonly XNamespace PlanXmlNamespace = "http://schemas.microsoft.com/sqlserver/2004/07/showplan";

        public QueryPlanProcessor(string planXml)
        {
            _planXml = planXml;
        }

        public string ConvertPlanToHtml()
        {
            var schema = new XmlSchemaSet();
            using (var planSchemaReader = XmlReader.Create(new StringReader(Resources.showplanxml)))
            {
                schema.Add(PlanXmlNamespace.NamespaceName, planSchemaReader);
            }

            var transform = new XslCompiledTransform(true);

            using (var xsltReader = XmlReader.Create(new StringReader(Resources.qpXslt)))
            {
                transform.Load(xsltReader);
            }

            var planHtml = new StringBuilder();

            var settings = new XmlReaderSettings
            {
                ValidationType = ValidationType.Schema,
                Schemas = schema,
            };

            using (var queryPlanReader = XmlReader.Create(new StringReader(_planXml), settings))
            {
                using (var writer = XmlWriter.Create(planHtml, transform.OutputSettings))
                {
                    transform.Transform(queryPlanReader, writer);
                }
            }
            return planHtml.ToString();
        }
    }
}