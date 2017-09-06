using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ReverseEngineer20;

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
                _package.Dte2.ItemOperations.OpenFile(path);
            }
            catch (Exception exception)
            {
                _package.LogError(new List<string>(), exception);
            }
        }
    }
}