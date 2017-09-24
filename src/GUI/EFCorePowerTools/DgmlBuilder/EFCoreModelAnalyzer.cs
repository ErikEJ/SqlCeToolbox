using System;
using System.IO;
using System.Reflection;

namespace DgmlBuilder
{
    public class EFCoreModelAnalyzer
    {
        public string GenerateDebugView(dynamic context)
        {
            return CreateDebugView(context);
        }

        public string GenerateDgmlContent(dynamic context)
        {
            Type type = context.GetType();
            var dgmlBuilder = new DgmlBuilder();

            var debugView = CreateDebugView(context);
            var dgml = dgmlBuilder.Build(debugView, type.Name, GetTemplate());

            return dgml;
        }

        private string CreateDebugView(dynamic context)
        {
            var model = context.Model;
            string view = model.DebugView.View;

            return view;
        }

        private string GetTemplate()
        {
            var resourceName = "EFCorePowerTools.DgmlBuilder.template.dgml";

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
