using System;
using System.IO;
using System.Reflection;

namespace ReverseEngineer20.ModelAnalyzer
{
    public class DgmlBuilder
    {
        public string Build(string debugView, string contextName)
        {
            var parser = new DebugViewParser();
            var result = parser.Parse(debugView.Split(new [] { Environment.NewLine }, StringSplitOptions.None), contextName);

            var template = GetTemplate();

            var nodes = string.Join(Environment.NewLine, result.Nodes);
            var links = string.Join(Environment.NewLine, result.Links);

            return template.Replace("{Links}", links).Replace("{Nodes}", nodes);
        }

        private string GetTemplate()
        {
            var resourceName = "ReverseEngineer20.ModelAnalyzer.template.dgml";

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                using (var reader = new StreamReader(stream ?? throw new InvalidOperationException()))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
