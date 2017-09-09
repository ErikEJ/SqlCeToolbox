using System;
using ReverseEngineer20.ModelAnalyzer;

namespace ReverseEngineer20
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
            var dgml = dgmlBuilder.Build(debugView, type.Name);

            return dgml;
        }

        private string CreateDebugView(dynamic context)
        {
            var model = context.Model;
            string view = model.DebugView.View;

            return view;
        }
    }
}
