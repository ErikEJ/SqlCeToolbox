using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DgmlBuilder;

namespace EFCorePowerTools.Handlers
{
    internal class ModelAnalyzerHandler
    {
        private readonly EFCorePowerToolsPackage _package;
        private readonly EFCoreModelAnalyzer _modelAnalyzer = new EFCoreModelAnalyzer();

        public ModelAnalyzerHandler(EFCorePowerToolsPackage package)
        {
            _package = package;
        }

        public void GenerateDebugView(dynamic contextType)
        {
            try
            {
                var modelText = _modelAnalyzer.GenerateDebugView(contextType);
                var path = Path.GetTempFileName() + ".txt";
                File.WriteAllText(path, modelText, Encoding.UTF8);
                File.SetAttributes(path, FileAttributes.ReadOnly);
                var window = _package.Dte2.ItemOperations.OpenFile(path);
                window.Document.Activate();
            }
            catch (Exception exception)
            {
                _package.LogError(new List<string>(), exception);
            }
        }

        public void GenerateDgml(dynamic contextType)
        {
            try
            {
                var modelText = _modelAnalyzer.GenerateDgmlContent(contextType);
                var path = Path.GetTempFileName() + ".dgml";
                File.WriteAllText(path, modelText, Encoding.UTF8);
                var window = _package.Dte2.ItemOperations.OpenFile(path);
                window.Document.Activate();
            }
            catch (Exception exception)
            {
                _package.LogError(new List<string>(), exception);
            }
        }
    }
}