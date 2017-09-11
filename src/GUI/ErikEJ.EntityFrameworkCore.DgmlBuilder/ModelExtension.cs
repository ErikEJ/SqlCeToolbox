using System;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ReverseEngineer20.ModelAnalyzer;

namespace Microsoft.EntityFrameworkCore
{
    public static class ModelExtension
    {
        public static string AsDgml(this DbContext context)
        {
            Type type = context.GetType();
            var dgmlBuilder = new DgmlBuilder();

            var debugView = CreateDebugView(context);
            var dgml = dgmlBuilder.Build(debugView, type.Name);

            return dgml;
        }

        private static string CreateDebugView(DbContext context)
        {
            var model = context.Model;
            return model.AsModel().DebugView.View;
        }
    }
}
